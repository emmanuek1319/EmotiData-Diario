"""EmotiData · sesión real (JWT), registro con confirmación por correo, consentimientos y límites de uso.

Variables de entorno (Render → Environment):
  JWT_SECRET        obligatoria (mínimo 32 caracteres aleatorios)
  FRONTEND_URL      dirección de la web, p. ej. https://emmoti-data.netlify.app
  EMAIL_PROVIDER    appsscript | brevo | resend   (Render gratis bloquea SMTP: se envía por HTTPS)
  -- appsscript (Gmail de EmotiData, ~100 correos/día en cuentas gmail.com):
  MAIL_WEBHOOK_URL  dirección /exec de tu Aplicación web de Apps Script
  MAIL_WEBHOOK_KEY  la misma CLAVE secreta que guardaste en el script
  -- brevo / resend:
  EMAIL_API_KEY     clave del proveedor
  EMAIL_FROM_ADDR   remitente verificado, p. ej. no-reply@tudominio.mx
  EMAIL_FROM_NAME   opcional, por defecto "EmotiData"
  ENTORNO           "dev" solo en tu PC (permite EMAIL_PROVIDER=log y AUTO_VERIFICAR=1)
"""
import os
import re
import json
import time
import hashlib
import secrets
import html as _html
import urllib.request
from datetime import datetime, timedelta, timezone
from functools import wraps

import jwt
from bson.errors import InvalidId
from flask import Blueprint, request, jsonify, g
from pymongo.errors import DuplicateKeyError
from werkzeug.exceptions import HTTPException
from werkzeug.middleware.proxy_fix import ProxyFix
from werkzeug.security import generate_password_hash, check_password_hash

TZ_MX = timezone(timedelta(hours=-6))
ENTORNO = os.environ.get("ENTORNO", "prod").lower()
SECRET = os.environ.get("JWT_SECRET", "")
FRONTEND_URL = os.environ.get("FRONTEND_URL", "https://emmoti-data.netlify.app").rstrip("/")
AVISO_VERSION = "2026-11"
SESION_HORAS, VERIF_HORAS, RESET_HORAS, PENDIENTE_DIAS = 12, 48, 1, 7

auth_bp = Blueprint("auth", __name__)
_db = None
_hits = {}
_DUMMY = generate_password_hash("emotidata-dummy")
RE_CORREO = re.compile(r"^[^@\s]{1,64}@[^@\s]{1,120}\.[^@\s]{2,}$")
COMUNES = {"12345678", "123456789", "1234567890", "password", "contrasena", "qwertyui", "11111111", "abcd1234", "password1", "emotidata"}

# Estudiantes que la psicóloga puede ver: los que ella dio de alta (salvo que lo revoquen) o los que lo autorizaron.
FILTRO_VISIBLES = {"$or": [
    {"consentimientos.compartir_con_psicologa": True},
    {"canalizado": True, "consentimientos.compartir_con_psicologa": {"$ne": False}},
]}


def ahora_mx():
    return datetime.now(TZ_MX)


# ---------- límites de intentos (en memoria; se reinician si Render duerme el servicio) ----------
def _purga(clave, ventana):
    ahora = time.time()
    lista = [t for t in _hits.get(clave, []) if ahora - t < ventana]
    _hits[clave] = lista
    return lista


def excedido(clave, maximo, ventana):
    return len(_purga(clave, ventana)) >= maximo


def anotar(clave):
    if len(_hits) > 20000:
        _hits.clear()
    _hits.setdefault(clave, []).append(time.time())


def limitar(clave, maximo, ventana):
    if excedido(clave, maximo, ventana):
        return False
    anotar(clave)
    return True


# ---------- validaciones ----------
def limpiar(valor, n):
    t = re.sub(r"[\x00-\x08\x0b\x0c\x0e-\x1f\x7f]", "", str(valor or "")).strip()
    return t[:n]


def validar_password(p):
    if not isinstance(p, str) or len(p) < 8:
        return "La contraseña debe tener al menos 8 caracteres."
    if len(p) > 128:
        return "La contraseña es demasiado larga."
    if p.lower() in COMUNES or not re.search(r"[A-Za-z]", p) or not re.search(r"\d", p):
        return "Usa letras y números, y evita contraseñas comunes."
    return None


def dia_valido(d, atras=7, adelante=0):
    hoy = ahora_mx().date()
    try:
        x = datetime.strptime(str(d)[:10], "%Y-%m-%d").date()
    except Exception:
        return hoy.isoformat()
    if x < hoy - timedelta(days=atras) or x > hoy + timedelta(days=adelante):
        return hoy.isoformat()
    return x.isoformat()


# ---------- sesión (JWT) ----------
def emitir_token(uid, proposito, horas, extra=None):
    ahora = datetime.now(timezone.utc)
    carga = {"sub": str(uid), "pur": proposito, "iat": ahora, "exp": ahora + timedelta(hours=horas)}
    carga.update(extra or {})
    return jwt.encode(carga, SECRET, algorithm="HS256")


def leer_token(token, proposito):
    try:
        p = jwt.decode(token, SECRET, algorithms=["HS256"])
    except jwt.PyJWTError:
        return None
    return p if p.get("pur") == proposito else None


def requiere_sesion(f):
    @wraps(f)
    def w(*a, **k):
        h = request.headers.get("Authorization", "")
        p = leer_token(h[7:], "sesion") if h.startswith("Bearer ") else None
        u = _db.usuarios.find_one({"_id": p["sub"]}, {"rol": 1, "activo": 1}) if p else None
        if not u or u.get("activo") is False:
            return jsonify({"error": "Tu sesión expiró. Inicia sesión de nuevo."}), 401
        g.uid, g.rol = p["sub"], u.get("rol", "estudiante")
        return f(*a, **k)
    return w


def requiere_rol(*roles):
    def deco(f):
        @wraps(f)
        def w(*a, **k):
            if g.rol not in roles:
                return jsonify({"error": "No tienes permiso para esta acción."}), 403
            return f(*a, **k)
        return requiere_sesion(w)
    return deco


def propio(usuario_id):
    if str(usuario_id or "").strip() != g.uid:
        return jsonify({"error": "No tienes permiso para ver estos datos."}), 403
    return None


def estudiante_visible(uid):
    q = {"_id": str(uid), "rol": "estudiante"}
    q.update(FILTRO_VISIBLES)
    return _db.usuarios.count_documents(q) == 1


def nombre_de(uid):
    u = _db.usuarios.find_one({"_id": uid}, {"nombre_completo": 1}) or {}
    return str(u.get("nombre_completo") or "Psicología / Tutoría")


def error_interno(e):
    if isinstance(e, InvalidId):
        return jsonify({"error": "Identificador inválido."}), 400
    if isinstance(e, DuplicateKeyError):
        return jsonify({"error": "Ese registro ya existe."}), 409
    print("❌", request.path, type(e).__name__)  # sin detalles: pueden incluir datos personales
    return jsonify({"error": "Ocurrió un problema. Inténtalo de nuevo."}), 500


def calcular_racha(uid):
    dias = {str(l.get("fecha", ""))[:10] for l in _db.diario_logs.find({"usuario_id": uid}, {"fecha": 1})}
    c = ahora_mx().date()
    if c.isoformat() not in dias:
        c -= timedelta(days=1)
    n = 0
    while c.isoformat() in dias:
        n += 1
        c -= timedelta(days=1)
    return n


# ---------- correo (API HTTPS; Render gratis bloquea SMTP) ----------
def correo_habilitado():
    prov = os.environ.get("EMAIL_PROVIDER", "").lower()
    if prov == "log":
        return ENTORNO == "dev"
    if prov == "appsscript":
        return os.environ.get("MAIL_WEBHOOK_URL", "").startswith("https://") and len(os.environ.get("MAIL_WEBHOOK_KEY", "")) >= 16
    return prov in ("brevo", "resend") and bool(os.environ.get("EMAIL_API_KEY")) and bool(os.environ.get("EMAIL_FROM_ADDR"))


def autoverificar():
    return ENTORNO == "dev" and os.environ.get("AUTO_VERIFICAR") == "1"


def registro_habilitado():
    return correo_habilitado() or autoverificar()


def enviar_correo(para, asunto, texto, html):
    prov = os.environ.get("EMAIL_PROVIDER", "").lower()
    if prov == "log" and ENTORNO == "dev":
        print("📧 [dev]", asunto, "→", para, "\n", texto)
        return True
    if not correo_habilitado():
        return False
    if prov == "appsscript":  # el script responde 200 aunque rechace: hay que leer el "ok"
        cuerpo = {"clave": os.environ["MAIL_WEBHOOK_KEY"], "para": para, "asunto": asunto, "texto": texto, "html": html}
        req = urllib.request.Request(os.environ["MAIL_WEBHOOK_URL"], json.dumps(cuerpo).encode(), {"Content-Type": "application/json"}, method="POST")
        try:
            with urllib.request.urlopen(req, timeout=20) as r:
                return bool(json.loads(r.read().decode("utf-8", "ignore")).get("ok"))
        except Exception as e:
            print("❌ envío de correo (puente):", type(e).__name__)
            return False
    key, de = os.environ["EMAIL_API_KEY"], os.environ["EMAIL_FROM_ADDR"]
    nombre = os.environ.get("EMAIL_FROM_NAME", "EmotiData")
    if prov == "brevo":
        url, cab = "https://api.brevo.com/v3/smtp/email", {"api-key": key}
        cuerpo = {"sender": {"name": nombre, "email": de}, "to": [{"email": para}], "subject": asunto, "htmlContent": html, "textContent": texto}
    else:
        url, cab = "https://api.resend.com/emails", {"Authorization": "Bearer " + key}
        cuerpo = {"from": f"{nombre} <{de}>", "to": [para], "subject": asunto, "html": html, "text": texto}
    req = urllib.request.Request(url, json.dumps(cuerpo).encode(), {"Content-Type": "application/json", "Accept": "application/json", "User-Agent": "EmotiData/1.0", **cab}, method="POST")
    try:
        urllib.request.urlopen(req, timeout=10)
        return True
    except Exception as e:
        print("❌ envío de correo:", getattr(e, "code", ""), type(e).__name__)
        return False


def _plantilla(titulo, parrafos, boton, url):
    p = "".join(f"<p style='margin:0 0 14px'>{_html.escape(x)}</p>" for x in parrafos)
    u = _html.escape(url)
    return (f"<div style=\"font-family:system-ui,Segoe UI,Arial,sans-serif;max-width:480px;margin:auto;color:#1f2a44\">"
            f"<h2 style='margin:0 0 12px'>{_html.escape(titulo)}</h2>{p}"
            f"<p><a href=\"{u}\" style=\"background:#e8772e;color:#1f1a10;padding:12px 22px;border-radius:99px;text-decoration:none;font-weight:600;display:inline-block\">{_html.escape(boton)}</a></p>"
            f"<p style='font-size:13px;color:#6b6f80'>Si el botón no funciona, copia este enlace en tu navegador:<br>{u}</p>"
            f"<p style='font-size:13px;color:#6b6f80'>Si no fuiste tú, ignora este mensaje: no se activará nada.</p></div>")


def _correo_verificacion(user):
    url = f"{FRONTEND_URL}/#verificar=" + emitir_token(user["_id"], "verificar", VERIF_HORAS)
    pars = [f"Hola {user.get('nombre_completo', '')}: recibimos una solicitud para crear tu cuenta en EmotiData.",
            "Confirma que fuiste tú para activarla. El enlace es válido por 48 horas."]
    return enviar_correo(user["correo"], "Confirma tu cuenta de EmotiData", "\n".join(pars) + "\n" + url,
                         _plantilla("Confirma tu cuenta", pars, "Confirmar mi cuenta", url))


def _huella(u):
    return hashlib.sha256(str(u.get("password_hash") or "none").encode()).hexdigest()[:16]


def _correo_reset(user):
    url = f"{FRONTEND_URL}/#restablecer=" + emitir_token(user["_id"], "reset", RESET_HORAS, {"fp": _huella(user)})
    pars = ["Recibimos una solicitud para crear o cambiar tu contraseña de EmotiData.", "El enlace es válido por 1 hora y solo funciona una vez."]
    return enviar_correo(user["correo"], "Crea o cambia tu contraseña de EmotiData", "\n".join(pars) + "\n" + url,
                         _plantilla("Tu contraseña", pars, "Elegir contraseña", url))


# ---------- rutas ----------
GENERICO = {"mensaje": "Si los datos son válidos, te enviamos un mensaje. Revisa tu correo y también la carpeta de spam."}


@auth_bp.route("/api/auth/config", methods=["GET"])
def config():
    return jsonify({"registro": registro_habilitado(), "aviso_version": AVISO_VERSION}), 200


@auth_bp.route("/api/auth/registro", methods=["POST"])
def registro():
    d = request.get_json(silent=True) or {}
    ip = request.remote_addr or "?"
    if not limitar("reg:" + ip, 8, 3600):
        return jsonify({"error": "Demasiados intentos. Inténtalo más tarde."}), 429
    nombre = limpiar(d.get("nombre"), 60)
    correo = str(d.get("correo") or "").strip().lower()
    pwd = d.get("password")
    if not nombre or len(correo) > 120 or not RE_CORREO.match(correo):
        return jsonify({"error": "Revisa tu nombre y tu correo."}), 400
    err = validar_password(pwd)
    if err:
        return jsonify({"error": err}), 400
    if d.get("acepta_aviso") is not True or d.get("mayor_edad") is not True:
        return jsonify({"error": "Debes aceptar el aviso de privacidad y confirmar tu edad o la autorización de tu madre, padre o tutor."}), 400
    if not registro_habilitado():
        return jsonify({"error": "El registro por correo aún no está disponible. Inténtalo más tarde."}), 503
    if not limitar("regc:" + correo, 3, 3600):
        return jsonify(GENERICO), 202
    ahora = datetime.now(timezone.utc)
    previo = _db.usuarios.find_one({"correo": correo}, {"verificado": 1, "nombre_completo": 1, "correo": 1})
    if previo:
        if previo.get("verificado") is False:
            _correo_verificacion(previo)
        return jsonify(GENERICO), 202
    doc = {
        "_id": "USR_" + secrets.token_hex(5).upper(), "nombre_completo": nombre, "correo": correo, "rol": "estudiante",
        "activo": True, "verificado": autoverificar(), "password_hash": generate_password_hash(pwd),
        "fecha_registro": ahora_mx().strftime("%Y-%m-%d %H:%M:%S"),
        "consentimientos": {"aviso_privacidad": {"version": AVISO_VERSION, "aceptado": ahora.isoformat()},
                            "mayor_edad_o_autorizacion": True, "compartir_con_psicologa": d.get("compartir_psicologa") is True},
        "detalles_estudiante": {},
    }
    if not doc["verificado"]:
        doc["expira_pendiente"] = ahora + timedelta(days=PENDIENTE_DIAS)
    try:
        _db.usuarios.insert_one(doc)
    except DuplicateKeyError:
        return jsonify(GENERICO), 202
    if not doc["verificado"] and not _correo_verificacion(doc):
        _db.usuarios.delete_one({"_id": doc["_id"]})
        return jsonify({"error": "No pudimos enviar el correo de confirmación. Inténtalo de nuevo."}), 503
    return jsonify(GENERICO), 202


@auth_bp.route("/api/auth/verificar", methods=["POST"])
def verificar():
    p = leer_token(str((request.get_json(silent=True) or {}).get("token") or ""), "verificar")
    if not p:
        return jsonify({"error": "El enlace no es válido o venció. Solicita uno nuevo."}), 400
    _db.usuarios.update_one({"_id": p["sub"]}, {"$set": {"verificado": True, "fecha_verificacion": ahora_mx().strftime("%Y-%m-%d %H:%M:%S")}, "$unset": {"expira_pendiente": ""}})
    return jsonify({"mensaje": "Cuenta confirmada. Ya puedes iniciar sesión."}), 200


@auth_bp.route("/api/auth/reenviar", methods=["POST"])
def reenviar():
    correo = str((request.get_json(silent=True) or {}).get("correo") or "").strip().lower()[:120]
    if RE_CORREO.match(correo) and limitar("rev:" + correo, 3, 3600) and limitar("revip:" + (request.remote_addr or "?"), 10, 3600):
        u = _db.usuarios.find_one({"correo": correo, "verificado": False})
        if u:
            _correo_verificacion(u)
    return jsonify(GENERICO), 202


@auth_bp.route("/api/auth/olvide", methods=["POST"])
def olvide():
    correo = str((request.get_json(silent=True) or {}).get("correo") or "").strip().lower()[:120]
    if RE_CORREO.match(correo) and limitar("olv:" + correo, 3, 3600) and limitar("olvip:" + (request.remote_addr or "?"), 10, 3600):
        u = _db.usuarios.find_one({"correo": correo, "activo": {"$ne": False}})
        if u and correo_habilitado():
            _correo_reset(u)
    return jsonify(GENERICO), 202


@auth_bp.route("/api/auth/restablecer", methods=["POST"])
def restablecer():
    d = request.get_json(silent=True) or {}
    p = leer_token(str(d.get("token") or ""), "reset")
    u = _db.usuarios.find_one({"_id": p["sub"]}) if p else None
    if not u or p.get("fp") != _huella(u):
        return jsonify({"error": "El enlace no es válido o ya se usó. Solicita uno nuevo."}), 400
    err = validar_password(d.get("password"))
    if err:
        return jsonify({"error": err}), 400
    _db.usuarios.update_one({"_id": u["_id"]}, {"$set": {"password_hash": generate_password_hash(d["password"]), "verificado": True}, "$unset": {"expira_pendiente": ""}})
    return jsonify({"mensaje": "Contraseña actualizada. Ya puedes iniciar sesión."}), 200


@auth_bp.route("/api/auth/login", methods=["POST"])
@auth_bp.route("/api/login", methods=["POST"])
def login():
    d = request.get_json(silent=True) or {}
    ident = str(d.get("usuario") or d.get("correo") or d.get("matricula") or "").strip()[:120]
    pwd = str(d.get("password") or d.get("contrasena") or "")[:200]
    if not ident or not pwd:
        return jsonify({"error": "Escribe tu correo o matrícula y tu contraseña."}), 400
    ip, k = "ip:" + (request.remote_addr or "?"), "lg:" + ident.lower()
    if excedido(ip, 40, 900) or excedido(k, 6, 900):
        return jsonify({"error": "Demasiados intentos. Espera unos minutos e inténtalo de nuevo."}), 429
    u = _db.usuarios.find_one({"$or": [{"correo": ident.lower()}, {"detalles_estudiante.matricula": ident}, {"_id": ident}]})
    hash_ = (u or {}).get("password_hash")
    ok = check_password_hash(hash_ or _DUMMY, pwd) and bool(hash_)
    if not ok:
        anotar(k)
        anotar(ip)
        return jsonify({"error": "Correo o contraseña incorrectos."}), 401
    if u.get("activo") is False:
        return jsonify({"error": "Esta cuenta se encuentra desactivada."}), 403
    if u.get("verificado") is False:
        return jsonify({"error": "Confirma tu correo para entrar. Si no te llegó, pide un nuevo enlace.", "pendiente_verificacion": True}), 403
    _hits.pop(k, None)
    nombre = str(u.get("nombre_completo", "Usuario")).strip()
    rol = str(u.get("rol", "estudiante")).strip()
    return jsonify({"mensaje": "Login exitoso", "token": emitir_token(u["_id"], "sesion", SESION_HORAS), "id": str(u["_id"]), "usuario_id": str(u["_id"]),
                    "rol": rol, "Rol": rol, "nombre": nombre, "nombre_completo": nombre}), 200


@auth_bp.route("/api/auth/yo", methods=["GET"])
@requiere_sesion
def yo():
    u = _db.usuarios.find_one({"_id": g.uid}) or {}
    c = u.get("consentimientos") or {}
    return jsonify({"id": g.uid, "nombre": u.get("nombre_completo"), "rol": g.rol, "canalizado": bool(u.get("canalizado")),
                    "compartir_con_psicologa": c.get("compartir_con_psicologa", bool(u.get("canalizado")))}), 200


@auth_bp.route("/api/usuario/consentimiento", methods=["POST"])
@requiere_sesion
def consentimiento():
    v = (request.get_json(silent=True) or {}).get("compartir_con_psicologa")
    if not isinstance(v, bool):
        return jsonify({"error": "Dato inválido."}), 400
    _db.usuarios.update_one({"_id": g.uid}, {"$set": {"consentimientos.compartir_con_psicologa": v, "consentimientos.actualizado": datetime.now(timezone.utc).isoformat()}})
    return jsonify({"mensaje": "Preferencia guardada.", "compartir_con_psicologa": v}), 200


@auth_bp.route("/api/usuario/cambiar_password", methods=["POST"])
@requiere_sesion
def cambiar_password():
    d = request.get_json(silent=True) or {}
    if not limitar("cp:" + g.uid, 6, 900):
        return jsonify({"error": "Demasiados intentos. Espera unos minutos."}), 429
    err = validar_password(d.get("nueva"))
    if err:
        return jsonify({"error": err}), 400
    u = _db.usuarios.find_one({"_id": g.uid}) or {}
    if not u.get("password_hash") or not check_password_hash(u["password_hash"], str(d.get("actual") or "")):
        return jsonify({"error": "La contraseña actual no coincide."}), 401
    _db.usuarios.update_one({"_id": g.uid}, {"$set": {"password_hash": generate_password_hash(d["nueva"])}})
    return jsonify({"mensaje": "Contraseña actualizada correctamente."}), 200


# ---------- arranque ----------
def asegurar_indices():
    def ix(col, claves, **kw):
        try:
            _db[col].create_index(claves, **kw)
        except Exception as e:
            print(f"⚠️ índice en {col}: {type(e).__name__} (revisa duplicados con gestionar_usuarios.py revisar)")
    ix("usuarios", "correo", unique=True, partialFilterExpression={"correo": {"$type": "string"}}, name="correo_unico")
    ix("usuarios", "expira_pendiente", expireAfterSeconds=0, name="pendientes_ttl")
    ix("diario_logs", [("usuario_id", 1), ("cid", 1)], unique=True, partialFilterExpression={"cid": {"$type": "string"}}, name="entrada_unica")
    ix("diario_logs", [("usuario_id", 1), ("fecha", -1)], name="historial")
    ix("metas_habitos", [("usuario_id", 1), ("mes", 1), ("titulo", 1)], unique=True, partialFilterExpression={"estado": "activo"}, name="meta_activa_unica")


def init(app, db):
    global _db, SECRET
    _db = db
    if len(SECRET) < 32:
        if ENTORNO == "dev":
            SECRET = secrets.token_hex(32)
            print("⚠️ JWT_SECRET ausente: se usa una clave temporal (solo ENTORNO=dev)")
        else:
            raise RuntimeError("Falta JWT_SECRET (mínimo 32 caracteres) en las variables de entorno.")
    app.wsgi_app = ProxyFix(app.wsgi_app, x_for=1)
    app.register_blueprint(auth_bp)
    asegurar_indices()

    @app.after_request
    def cabeceras(r):
        if request.path.startswith("/api/"):
            r.headers["Cache-Control"] = "public, max-age=60" if request.path == "/api/relax/frases" else "no-store"
            r.headers["X-Content-Type-Options"] = "nosniff"
        return r

    @app.errorhandler(HTTPException)
    def http_error(e):
        msg = {400: "Solicitud inválida.", 404: "No encontrado.", 405: "Método no permitido.", 413: "Solicitud demasiado grande.", 415: "Formato no admitido."}
        return jsonify({"error": msg.get(e.code, "Error.")}), e.code
