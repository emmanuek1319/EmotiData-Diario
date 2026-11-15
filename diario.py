from flask import Flask, request, jsonify
from flask_cors import CORS
from pymongo import MongoClient, DESCENDING
from bson.objectid import ObjectId
from werkzeug.security import generate_password_hash, check_password_hash
from datetime import datetime, timedelta
import calendar
import secrets
import random
import re
import os
from dotenv import load_dotenv

load_dotenv()

app = Flask(__name__)

CORS_ORIGINS = [o.strip() for o in os.environ.get("CORS_ORIGINS", "").split(",") if o.strip()]
CORS(app, resources={r"/api/*": {"origins": CORS_ORIGINS}})

MONGO_URI = os.environ.get("MONGO_URI", "mongodb://localhost:27017/")

try:
    client = MongoClient(MONGO_URI, serverSelectionTimeoutMS=5000)
    db = client["diario_inteligente"]
    client.admin.command("ping")
    tipo_conexion = "Atlas (Nube)" if "mongodb+srv" in MONGO_URI else "Local"
    print(f"✅ Conectado a MongoDB: {tipo_conexion}")
except Exception as e:
    print(f"❌ Error al conectar a MongoDB: {e}")
    raise

RETOS_RESPALDO = [
    {"emocion_trigger": "Tristeza", "tipo": "construir", "titulo": "Un buen momento al día", "micro_accion": "Escribir al final del día una cosa pequeña que salió bien", "duracion_dias": 14},
    {"emocion_trigger": "Miedo / Ansiedad", "tipo": "construir", "titulo": "Higiene del sueño", "micro_accion": "Apagar pantallas 30 minutos antes de dormir", "duracion_dias": 21},
    {"emocion_trigger": "Enojo", "tipo": "soltar", "titulo": "Pausa antes de reaccionar", "micro_accion": "Respirar 3 veces antes de responder cuando sienta coraje", "duracion_dias": 14},
    {"emocion_trigger": "Neutral", "tipo": "construir", "titulo": "Estudio sin distracciones", "micro_accion": "25 minutos de estudio sin celular", "duracion_dias": 20},
    {"emocion_trigger": "Alegría", "tipo": "mejorar", "titulo": "Consolidar mi ritmo", "micro_accion": "Avanzar 20 minutos en mi proyecto principal", "duracion_dias": 20}
]

def obtener_reto_sugerido(emocion):
    try:
        retos = list(db.retos_catalogo.find({"emocion_trigger": emocion, "activo": {"$ne": False}}))
        if retos:
            r = random.choice(retos)
            return {
                "id": str(r["_id"]),
                "emocion_trigger": r.get("emocion_trigger", emocion),
                "tipo": r.get("tipo", "construir"),
                "titulo": r.get("titulo", ""),
                "micro_accion": r.get("micro_accion", ""),
                "duracion_dias": int(r.get("duracion_dias", 15))
            }
    except Exception:
        pass
    coincidentes = [r for r in RETOS_RESPALDO if r["emocion_trigger"] == emocion] or RETOS_RESPALDO
    return random.choice(coincidentes)


def calcular_precision_global():
    try:
        evaluadas = db.diario_logs.count_documents({"precision_psicologa": {"$in": ["precisa", "no_precisa"]}})
        precisas = db.diario_logs.count_documents({"precision_psicologa": "precisa"})
        porcentaje = round((precisas / evaluadas) * 100) if evaluadas > 0 else None
        return {"evaluadas": evaluadas, "precisas": precisas, "porcentaje": porcentaje}
    except Exception:
        return {"evaluadas": 0, "precisas": 0, "porcentaje": None}


# -------------------------------------------------------------------
# 0. SALUD DEL SERVICIO
# -------------------------------------------------------------------
@app.route('/', methods=['GET'], strict_slashes=False)
@app.route('/health', methods=['GET'], strict_slashes=False)
def health():
    return jsonify({"estado": "ok", "servicio": "EmotiData API"}), 200


# -------------------------------------------------------------------
# 1. RESUMEN DEL MENÚ PRINCIPAL (ESTUDIANTE)
# -------------------------------------------------------------------
@app.route('/api/usuario/resumen', defaults={'usuario_id': 'USR_EMMANUEL_01'}, methods=['GET'], strict_slashes=False)
@app.route('/api/usuario/resumen/<usuario_id>', methods=['GET'], strict_slashes=False)
@app.route('/api/usuarios/perfil/<usuario_id>', methods=['GET'], strict_slashes=False)
def obtener_resumen_usuario(usuario_id):
    try:
        usuario = db.usuarios.find_one({"_id": usuario_id}) or db.usuarios.find_one({"_id": "USR_EMMANUEL_01"})
        detalles = usuario.get("detalles_estudiante", {}) if usuario else {}

        ultimo_log = db.diario_logs.find_one({"usuario_id": usuario_id}, sort=[("fecha", DESCENDING)])
        if not ultimo_log:
            ultimo_log = db.diario_logs.find_one({"usuario_id": "USR_EMMANUEL_01"}, sort=[("fecha", DESCENDING)])

        racha_db = detalles.get("racha_dias")
        racha = int(racha_db) if racha_db is not None else 22

        ultima_emocion = str(ultimo_log.get("emocion", "Alegría")) if ultimo_log else "Alegría"
        ultima_fecha = str(ultimo_log.get("fecha", "2026-10-30")) if ultimo_log else "2026-10-30"

        return jsonify({
            "racha_dias": racha,
            "racha": racha,
            "Racha": racha,
            "emocion": ultima_emocion,
            "ultima_emocion": ultima_emocion,
            "fecha": ultima_fecha,
            "ultima_fecha": ultima_fecha,
            "fecha_ultima": ultima_fecha
        }), 200
    except Exception as e:
        return jsonify({"error": str(e)}), 500


# -------------------------------------------------------------------
# 2. LOGROS Y MEDALLAS
# -------------------------------------------------------------------
@app.route('/api/usuario/logros', defaults={'usuario_id': 'USR_EMMANUEL_01'}, methods=['GET'], strict_slashes=False)
@app.route('/api/usuario/logros/<usuario_id>', methods=['GET'], strict_slashes=False)
def obtener_logros_usuario(usuario_id):
    try:
        usuario = db.usuarios.find_one({"_id": usuario_id}) or db.usuarios.find_one({"_id": "USR_EMMANUEL_01"})
        detalles = usuario.get("detalles_estudiante", {}) if usuario else {}

        racha_db = detalles.get("racha_dias")
        racha = int(racha_db) if racha_db is not None else 22

        nivel_db = detalles.get("nivel")
        nivel = int(nivel_db) if nivel_db is not None else 4

        metas_activas = db.metas_habitos.count_documents({"usuario_id": usuario_id, "estado": "activo"})

        return jsonify({
            "racha_actual": racha,
            "desbloqueados_conteo": nivel,
            "total_logros": 10,
            "metas_habitos_activas": metas_activas,
            "proximo_logro": {
                "leyenda": f"🎯 Próximo logro: Nivel {nivel + 1} en progreso..."
            },
            "logros": [
                {"titulo": "Primer Paso", "descripcion": "Registraste tu primera emoción en el diario.", "desbloqueado": True, "icono": "🏅"},
                {"titulo": "Constancia (7 Días)", "descripcion": "Mantuviste una racha de registro por una semana.", "desbloqueado": racha >= 7, "icono": "🔥"},
                {"titulo": "Autoconocimiento", "descripcion": "Alcanzaste el Nivel 3 de introspección.", "desbloqueado": nivel >= 3, "icono": "🧠"},
                {"titulo": "Atrévete al Cambio", "descripcion": "Iniciaste tu primera meta de hábitos saludables.", "desbloqueado": metas_activas > 0, "icono": "🌱"}
            ]
        }), 200
    except Exception as e:
        return jsonify({"error": str(e)}), 500


# -------------------------------------------------------------------
# 3. ZONA RELAX / FRASES
# -------------------------------------------------------------------
@app.route('/api/relax/frases', methods=['GET'], strict_slashes=False)
@app.route('/api/frases', methods=['GET'], strict_slashes=False)
@app.route('/api/frases/aleatoria', methods=['GET'], strict_slashes=False)
def obtener_frases_relax():
    try:
        frases_db = list(db.frases_motivacionales.aggregate([{"$sample": {"size": 5}}]))
        lista_frases = []
        if frases_db:
            for f in frases_db:
                lista_frases.append({"texto": f.get("texto", ""), "autor": f.get("autor", "Anónimo")})
        else:
            lista_frases = [
                {"texto": "El impedimento a la acción avanza la acción.", "autor": "Marco Aurelio"},
                {"texto": "La disciplina formará tu destino.", "autor": "Ryan Holiday"},
                {"texto": "No es lo que te pasa, sino cómo reaccionas.", "autor": "Epicteto"}
            ]
        return jsonify({"frases": lista_frases}), 200
    except Exception:
        return jsonify({"frases": [{"texto": "Respira y mantén la calma.", "autor": "Buda"}]}), 200


# -------------------------------------------------------------------
# 4. REPORTE PSICOLÓGICO (COMPATIBLE CON C# + PRIVACIDAD BLINDADA)
# -------------------------------------------------------------------
@app.route('/api/diario/reporte_psicologica', defaults={'usuario_id': 'USR_EMMANUEL_01'}, methods=['GET'], strict_slashes=False)
@app.route('/api/diario/reporte_psicologica/<usuario_id>', methods=['GET'], strict_slashes=False)
@app.route('/api/psicologa/reporte/<usuario_id>', methods=['GET'], strict_slashes=False)
def obtener_reporte_psicologica(usuario_id):
    try:
        logs_recientes = list(db.diario_logs.find({"usuario_id": usuario_id}).sort("fecha", DESCENDING).limit(14))
        total_entradas = len(logs_recientes)

        if total_entradas > 0:
            conteo_emo = {}
            suma_punt = 0.0
            negativos_seguidos = 0
            for l in logs_recientes:
                em = str(l.get("emocion", "Neutral"))
                conteo_emo[em] = conteo_emo.get(em, 0) + 1
                p = float(l.get("puntuacion", 0.0))
                suma_punt += p
                if p < -0.5:
                    negativos_seguidos += 1

            predominante = max(conteo_emo, key=conteo_emo.get)
            promedio = round(suma_punt / total_entradas, 2)
            nivel_alerta = "ALTA" if negativos_seguidos >= 3 else ("MODERADA" if promedio < -0.2 else "ESTABLE")

            obs = (
                f"Resumen estadístico ({total_entradas} registros recientes): "
                f"Emoción predominante '{predominante}', índice emocional promedio {promedio}, "
                f"nivel de alerta preventiva: {nivel_alerta}. "
                f"(Los escritos privados están protegidos y ocultos)."
            )
        else:
            nivel_alerta = "SIN DATOS"
            obs = f"El estudiante {usuario_id} aún no cuenta con entradas registradas en su diario."

        nota = (
            db["notas_psicológicas"].find_one({"$or": [{"estudiante_id": usuario_id}, {"usuario_id": usuario_id}]}, sort=[("fecha", DESCENDING)])
            or db["notas_psicologica"].find_one({"$or": [{"estudiante_id": usuario_id}, {"usuario_id": usuario_id}]}, sort=[("fecha", DESCENDING)])
        )
        nota_clinica = (nota.get("notas") or nota.get("observaciones")) if nota else ""

        return jsonify({
            "observaciones": nota_clinica if nota_clinica else obs,
            "resumen_metricas": obs,
            "nivel_alerta": nivel_alerta,
            "nota": obs,
            "puntos_ciegos": "Privacidad activa: solo se muestran indicadores agregados y entradas autorizadas por el alumno.",
            "sugerencia": "💡 Sugerencia: Revisar el estudio mensual y evaluar la precisión de las recomendaciones.",
            "recomendacion": "💡 Sugerencia: Revisar el estudio mensual y evaluar la precisión de las recomendaciones."
        }), 200
    except Exception as e:
        return jsonify({"error": str(e)}), 500


@app.route('/api/diario/compartir/<log_id>', methods=['POST'], strict_slashes=False)
def compartir_entrada_psicologa(log_id):
    data = request.json or {}
    usuario_id = data.get("usuario_id")
    autorizar = bool(data.get("compartir", True))

    if not usuario_id:
        return jsonify({"error": "Se requiere usuario_id para confirmar la autorización."}), 400

    try:
        res = db.diario_logs.update_one(
            {"_id": ObjectId(log_id), "usuario_id": usuario_id},
            {"$set": {"compartido_psicologa": autorizar}}
        )
        if res.matched_count == 0:
            return jsonify({"error": "Entrada no encontrada o no pertenece al usuario."}), 404
        estado_txt = "compartida con tutoría" if autorizar else "marcada como privada nuevamente"
        return jsonify({"mensaje": f"Entrada {estado_txt}.", "compartido_psicologa": autorizar}), 200
    except Exception as e:
        return jsonify({"error": str(e)}), 500


# -------------------------------------------------------------------
# 5. HISTORIAL DEL ESTUDIANTE
# -------------------------------------------------------------------
@app.route('/api/diario/historial', defaults={'usuario_id': 'USR_EMMANUEL_01'}, methods=['GET'], strict_slashes=False)
@app.route('/api/diario/historial/<usuario_id>', methods=['GET'], strict_slashes=False)
def obtener_historial(usuario_id):
    try:
        logs = list(db.diario_logs.find({"usuario_id": usuario_id}).sort("fecha", DESCENDING))
        historial_lista = []
        for log in logs:
            historial_lista.append({
                "id": str(log.get("_id", "1")),
                "fecha": str(log.get("fecha", "")),
                "emocion": str(log.get("emocion", "")),
                "estado_animo": str(log.get("emocion", "")),
                "texto_original": str(log.get("texto_original", "")),
                "pensamiento": str(log.get("texto_original", "")),
                "texto": str(log.get("texto_original", "")),
                "respuesta_enviada": str(log.get("respuesta_enviada", "Sigue adelante")),
                "puntuacion": float(log.get("puntuacion", 0.0)),
                "compartido_psicologa": bool(log.get("compartido_psicologa", False))
            })

        return jsonify({"entradas": historial_lista}), 200
    except Exception as e:
        return jsonify({"error": str(e)}), 500


# -------------------------------------------------------------------
# 6. LOGIN CON CONTRASEÑA CIFRADA + TRANSICIÓN SEGURA PARA C#
# -------------------------------------------------------------------
@app.route('/api/login', methods=['POST'], strict_slashes=False)
def login():
    data = request.json or {}
    usuario_input = str(data.get("usuario") or data.get("correo") or data.get("matricula", "")).strip()
    password_input = str(data.get("password") or data.get("contrasena") or "").strip()

    if not usuario_input:
        return jsonify({"error": "Ingresa tu correo, matrícula o ID."}), 400

    try:
        patron = {"$regex": f"^\\s*{re.escape(usuario_input)}\\s*$", "$options": "i"}
        user_db = db.usuarios.find_one({
            "$or": [
                {"correo": patron},
                {"detalles_estudiante.matricula": patron},
                {"_id": patron}
            ]
        })

        if not user_db:
            return jsonify({"error": "Usuario no encontrado."}), 401

        if user_db.get("activo") is False:
            return jsonify({"error": "Esta cuenta se encuentra desactivada."}), 403

        password_hash = user_db.get("password_hash")
        password_inicializada = False

        if password_hash:
            if not password_input or not check_password_hash(password_hash, password_input):
                return jsonify({"error": "Contraseña incorrecta."}), 401
        else:
            # Cuenta migrada sin password_hash todavía: si el usuario ingresa contraseña en la web, se cifra y guarda
            if password_input:
                if len(password_input) < 4:
                    return jsonify({"error": "La contraseña debe tener al menos 4 caracteres."}), 400
                db.usuarios.update_one(
                    {"_id": user_db["_id"]},
                    {"$set": {"password_hash": generate_password_hash(password_input)}}
                )
                password_inicializada = True

        token = secrets.token_hex(24)
        return jsonify({
            "mensaje": "Login exitoso",
            "password_inicializada": password_inicializada,
            "token": token,
            "id": str(user_db["_id"]).strip(),
            "usuario_id": str(user_db["_id"]).strip(),
            "rol": str(user_db.get("rol", "estudiante")).strip(),
            "Rol": str(user_db.get("rol", "estudiante")).strip(),
            "nombre": str(user_db.get("nombre_completo", "Usuario")).strip(),
            "nombre_completo": str(user_db.get("nombre_completo", "Usuario")).strip()
        }), 200

    except Exception as e:
        return jsonify({"error": str(e)}), 500


@app.route('/api/usuario/cambiar_password', methods=['POST'], strict_slashes=False)
def cambiar_password():
    data = request.json or {}
    usuario_id = str(data.get("usuario_id", "")).strip()
    actual = str(data.get("actual", "")).strip()
    nueva = str(data.get("nueva", "")).strip()

    if not usuario_id or len(nueva) < 4:
        return jsonify({"error": "La nueva contraseña debe tener al menos 4 caracteres."}), 400

    try:
        user_db = db.usuarios.find_one({"_id": usuario_id})
        if not user_db:
            return jsonify({"error": "Usuario no encontrado."}), 404

        if user_db.get("password_hash") and not check_password_hash(user_db["password_hash"], actual):
            return jsonify({"error": "La contraseña actual no coincide."}), 401

        db.usuarios.update_one({"_id": usuario_id}, {"$set": {"password_hash": generate_password_hash(nueva)}})
        return jsonify({"mensaje": "Contraseña actualizada correctamente."}), 200
    except Exception as e:
        return jsonify({"error": str(e)}), 500


# -------------------------------------------------------------------
# 7. REGISTRO DE DIARIO + PRIVACIDAD POR DEFECTO + RETO VINCULADO
# -------------------------------------------------------------------
@app.route('/api/diario/registrar', methods=['POST'], strict_slashes=False)
@app.route('/api/diario/guardar', methods=['POST'], strict_slashes=False)
def registrar_emocion():
    data = request.json or {}
    id_usuario = data.get("id_usuario") or data.get("usuario_id") or "USR_EMMANUEL_01"
    texto = str(data.get("texto", "")).strip()
    if not texto:
        return jsonify({"error": "El texto no puede estar vacío."}), 400

    texto_lower = texto.lower()

    if any(word in texto_lower for word in ["feliz", "bien", "alegre", "excelente", "genial", "motivado"]):
        emocion_detectada = "Alegría"
        puntuacion = 0.8
        respuesta_ia = "¡Esa actitud es fantástica! Canaliza toda esa energía positiva para seguir avanzando."
    elif any(word in texto_lower for word in ["triste", "tristeza", "mal", "deprimido", "llorar", "apagado"]):
        emocion_detectada = "Tristeza"
        puntuacion = -0.7
        respuesta_ia = "Es completamente válido sentirse así. Tómate tu tiempo, respira y recuerda que no estás solo."
    elif any(word in texto_lower for word in ["asustado", "miedo", "ansiedad", "nervioso", "estrés", "estres", "preocupado"]):
        emocion_detectada = "Miedo / Ansiedad"
        puntuacion = -0.8
        respuesta_ia = "El impedimento a la acción avanza la acción. Intenta hacer el ejercicio de respiración en la Zona Relax."
    elif any(word in texto_lower for word in ["enojado", "coraje", "rabia", "molesto", "frustrado", "harto"]):
        emocion_detectada = "Enojo"
        puntuacion = -0.6
        respuesta_ia = "La frustración es señal de que algo te importa. Date un respiro profundo antes de reaccionar."
    else:
        emocion_detectada = "Neutral"
        puntuacion = 0.0
        respuesta_ia = "Gracias por compartir tus pensamientos de hoy. Mantener este hábito te ayuda a reflexionar."

    reto_sugerido = obtener_reto_sugerido(emocion_detectada)

    nuevo_log = {
        "usuario_id": id_usuario,
        "fecha": datetime.now().strftime("%Y-%m-%d %H:%M:%S"),
        "texto_original": texto,
        "emocion": emocion_detectada,
        "puntuacion": puntuacion,
        "respuesta_enviada": respuesta_ia,
        "reto_sugerido": reto_sugerido,
        "compartido_psicologa": False,
        "precision_psicologa": None
    }

    log_id = ""
    try:
        res = db.diario_logs.insert_one(nuevo_log)
        log_id = str(res.inserted_id)
    except Exception as e:
        print(f"❌ Error insertando log: {e}")

    return jsonify({
        "mensaje": "Guardado con éxito",
        "log_id": log_id,
        "emocion": emocion_detectada,
        "mensaje_empatico": respuesta_ia,
        "compartido_psicologa": False,
        "reto_sugerido": reto_sugerido
    }), 201


# -------------------------------------------------------------------
# 8. MODIFICACIÓN DE CONDUCTA: HÁBITOS SINCRONIZADOS EN MONGODB
# -------------------------------------------------------------------
@app.route('/api/habitos/catalogo', methods=['GET'], strict_slashes=False)
def obtener_catalogo_habitos():
    emocion = request.args.get("emocion")
    try:
        filtro = {"activo": {"$ne": False}}
        if emocion:
            filtro["emocion_trigger"] = emocion
        docs = list(db.retos_catalogo.find(filtro))
        if docs:
            catalogo = [{
                "id": str(d["_id"]),
                "emocion_trigger": d.get("emocion_trigger", "Neutral"),
                "tipo": d.get("tipo", "construir"),
                "titulo": d.get("titulo", ""),
                "micro_accion": d.get("micro_accion", ""),
                "duracion_dias": int(d.get("duracion_dias", 15))
            } for d in docs]
        else:
            catalogo = [r for r in RETOS_RESPALDO if not emocion or r["emocion_trigger"] == emocion]
        return jsonify({"catalogo": catalogo}), 200
    except Exception as e:
        return jsonify({"error": str(e)}), 500


@app.route('/api/habitos/<usuario_id>', methods=['GET'], strict_slashes=False)
def obtener_metas_habitos(usuario_id):
    mes_actual = datetime.now().strftime("%Y-%m")
    hoy_str = datetime.now().strftime("%Y-%m-%d")
    try:
        metas_db = list(db.metas_habitos.find({
            "usuario_id": usuario_id,
            "mes": mes_actual,
            "estado": "activo"
        }))

        logs_mes = list(db.diario_logs.find({
            "usuario_id": usuario_id,
            "fecha": {"$regex": f"^{mes_actual}"}
        }))
        punt_por_dia = {}
        for l in logs_mes:
            dia_f = str(l.get("fecha", ""))[:10]
            punt_por_dia.setdefault(dia_f, []).append(float(l.get("puntuacion", 0.0)))

        lista_metas = []
        for m in metas_db:
            dias = sorted(list(set(m.get("dias_cumplidos", []))))
            meta_dias = int(m.get("meta_dias", 20))
            porcentaje = min(100, round((len(dias) / max(1, meta_dias)) * 100))

            racha = 0
            cursor = datetime.now().date()
            if cursor.strftime("%Y-%m-%d") not in dias:
                cursor -= timedelta(days=1)
            while cursor.strftime("%Y-%m-%d") in dias:
                racha += 1
                cursor -= timedelta(days=1)

            si_cumplio, no_cumplio = [], []
            for dia_f, punts in punt_por_dia.items():
                prom = sum(punts) / len(punts)
                if dia_f in dias:
                    si_cumplio.append(prom)
                else:
                    no_cumplio.append(prom)

            if len(si_cumplio) >= 3 and len(no_cumplio) >= 3:
                p_si = round(((sum(si_cumplio) / len(si_cumplio)) + 1) * 50)
                p_no = round(((sum(no_cumplio) / len(no_cumplio)) + 1) * 50)
                pista = f"Los días que cumples esta meta, tu bienestar promedia {p_si}/100 frente a {p_no}/100 los días que no."
            else:
                pista = "Sigue registrando tu ánimo y tus victorias diarias para descubrir cómo este hábito impacta tu bienestar."

            lista_metas.append({
                "id": str(m["_id"]),
                "tipo": m.get("tipo", "construir"),
                "titulo": m.get("titulo", ""),
                "micro_accion": m.get("micro_accion", ""),
                "mes": m.get("mes", mes_actual),
                "meta_dias": meta_dias,
                "dias_cumplidos": dias,
                "hecho_hoy": hoy_str in dias,
                "porcentaje": porcentaje,
                "racha": racha,
                "correlacion_animo": pista
            })

        # Estadísticas históricas globales del usuario para medallas
        todas_metas = list(db.metas_habitos.find({"usuario_id": usuario_id, "estado": {"$ne": "eliminada"}}))
        total_victorias = sum(len(x.get("dias_cumplidos", [])) for x in todas_metas)
        alguna_7 = any(len(x.get("dias_cumplidos", [])) >= 7 for x in todas_metas)
        alguna_cumplida = any(len(x.get("dias_cumplidos", [])) >= int(x.get("meta_dias", 20)) for x in todas_metas)
        mes_oro = len(lista_metas) >= 3 and all(x["porcentaje"] >= 100 for x in lista_metas)

        # Revisar si tiene metas del mes anterior para ofrecer repetir
        previas_count = db.metas_habitos.count_documents({
            "usuario_id": usuario_id,
            "mes": {"$ne": mes_actual},
            "estado": "activo"
        })

        return jsonify({
            "mes": mes_actual,
            "metas": lista_metas,
            "tiene_previas": previas_count > 0,
            "stats_medallas": {
                "total_metas_historicas": len(todas_metas),
                "total_victorias": total_victorias,
                "alguna_7": alguna_7,
                "alguna_cumplida": alguna_cumplida,
                "mes_oro": mes_oro
            }
        }), 200
    except Exception as e:
        return jsonify({"error": str(e)}), 500


@app.route('/api/habitos/asignar', methods=['POST'], strict_slashes=False)
def asignar_meta_habito():
    data = request.json or {}
    usuario_id = data.get("usuario_id")
    titulo = str(data.get("titulo", "")).strip()
    micro_accion = str(data.get("micro_accion", "")).strip()

    if not usuario_id or not titulo or not micro_accion:
        return jsonify({"error": "Faltan datos obligatorios (usuario_id, titulo, micro_accion)."}), 400

    mes_actual = datetime.now().strftime("%Y-%m")
    try:
        activas = db.metas_habitos.count_documents({"usuario_id": usuario_id, "mes": mes_actual, "estado": "activo"})
        if activas >= 5:
            return jsonify({"error": "Ya tienes 5 metas activas este mes. Quita una antes de agregar otra."}), 400

        nueva_meta = {
            "usuario_id": usuario_id,
            "reto_origen_id": data.get("reto_id"),
            "tipo": data.get("tipo", "construir"),
            "titulo": titulo,
            "micro_accion": micro_accion,
            "mes": mes_actual,
            "meta_dias": max(5, min(31, int(data.get("meta_dias", 20)))),
            "dias_cumplidos": [],
            "estado": "activo",
            "fecha_creacion": datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        }
        res = db.metas_habitos.insert_one(nueva_meta)
        return jsonify({"mensaje": "Meta de hábito guardada en tu cuenta", "id": str(res.inserted_id)}), 201
    except Exception as e:
        return jsonify({"error": str(e)}), 500


@app.route('/api/habitos/repetir_mes_anterior', methods=['POST'], strict_slashes=False)
def repetir_metas_mes_anterior():
    data = request.json or {}
    usuario_id = data.get("usuario_id")
    if not usuario_id:
        return jsonify({"error": "Falta usuario_id"}), 400

    mes_actual = datetime.now().strftime("%Y-%m")
    try:
        previas = list(db.metas_habitos.find({
            "usuario_id": usuario_id,
            "mes": {"$ne": mes_actual},
            "estado": "activo"
        }).sort("mes", DESCENDING).limit(5))

        if not previas:
            return jsonify({"error": "No se encontraron metas previas."}), 404

        creadas = 0
        for p in previas:
            db.metas_habitos.insert_one({
                "usuario_id": usuario_id,
                "tipo": p.get("tipo", "construir"),
                "titulo": p.get("titulo", ""),
                "micro_accion": p.get("micro_accion", ""),
                "mes": mes_actual,
                "meta_dias": int(p.get("meta_dias", 20)),
                "dias_cumplidos": [],
                "estado": "activo",
                "fecha_creacion": datetime.now().strftime("%Y-%m-%d %H:%M:%S")
            })
            creadas += 1
        return jsonify({"mensaje": f"Se repitieron {creadas} metas para este mes."}), 201
    except Exception as e:
        return jsonify({"error": str(e)}), 500


@app.route('/api/habitos/checkin', methods=['POST'], strict_slashes=False)
def checkin_meta_habito():
    data = request.json or {}
    meta_id = data.get("meta_id")
    usuario_id = data.get("usuario_id")
    fecha_dia = str(data.get("fecha") or datetime.now().strftime("%Y-%m-%d"))[:10]

    if not meta_id or not usuario_id:
        return jsonify({"error": "Se requiere meta_id y usuario_id."}), 400

    try:
        meta = db.metas_habitos.find_one({"_id": ObjectId(meta_id), "usuario_id": usuario_id})
        if not meta:
            return jsonify({"error": "Meta no encontrada."}), 404

        dias = meta.get("dias_cumplidos", [])
        if fecha_dia in dias:
            db.metas_habitos.update_one({"_id": ObjectId(meta_id)}, {"$pull": {"dias_cumplidos": fecha_dia}})
            hecho = False
        else:
            db.metas_habitos.update_one({"_id": ObjectId(meta_id)}, {"$addToSet": {"dias_cumplidos": fecha_dia}})
            hecho = True

        return jsonify({"mensaje": "Seguimiento actualizado", "hecho": hecho, "fecha": fecha_dia}), 200
    except Exception as e:
        return jsonify({"error": str(e)}), 500


@app.route('/api/habitos/<meta_id>', methods=['DELETE'], strict_slashes=False)
def eliminar_meta_habito(meta_id):
    try:
        db.metas_habitos.update_one({"_id": ObjectId(meta_id)}, {"$set": {"estado": "eliminada"}})
        return jsonify({"mensaje": "Meta eliminada"}), 200
    except Exception as e:
        return jsonify({"error": str(e)}), 500


# -------------------------------------------------------------------
# 9. PANEL COMPLETO DE PSICOLOGÍA (ESTUDIO MENSUAL, GRÁFICAS, PRECISIÓN Y NOTAS)
# -------------------------------------------------------------------
@app.route('/api/usuarios/configuracion', defaults={'id_usuario': 'USR_EMMANUEL_01'}, methods=['GET'], strict_slashes=False)
@app.route('/api/usuarios/configuracion/<id_usuario>', methods=['GET'], strict_slashes=False)
def obtener_configuracion_tema(id_usuario):
    return jsonify({"tema": "OscuroRojo", "tema_color": "Verde Menta", "modo_oscuro": True}), 200


@app.route('/api/psicologa/estudiantes', methods=['GET'], strict_slashes=False)
def obtener_estudiantes():
    try:
        estudiantes_db = list(db.usuarios.find({"rol": "estudiante"}))
        lista_estudiantes = []
        for est in estudiantes_db:
            id_est = str(est.get("_id", "")).strip()
            detalles = est.get("detalles_estudiante", {}) or {}
            ultimo_log = db.diario_logs.find_one({"usuario_id": id_est}, sort=[("fecha", DESCENDING)])
            puntuacion = float(ultimo_log.get("puntuacion", 0)) if ultimo_log else 0.0
            total_logs = db.diario_logs.count_documents({"usuario_id": id_est})
            metas_activas = db.metas_habitos.count_documents({"usuario_id": id_est, "estado": "activo"})

            lista_estudiantes.append({
                "id": id_est,
                "nombre": str(est.get("nombre_completo", "Sin Nombre")).strip(),
                "correo": str(est.get("correo", "")).strip(),
                "matricula": str(detalles.get("matricula", "")).strip(),
                "carrera": str(detalles.get("carrera", "")).strip(),
                "ultima_emocion": ultimo_log.get("emocion", "Sin registros") if ultimo_log else "Sin registros",
                "fecha_ultima": str(ultimo_log.get("fecha", "--"))[:10] if ultimo_log else "--",
                "total_registros": total_logs,
                "metas_activas": metas_activas,
                "alerta": True if puntuacion < -0.5 else False
            })

        return jsonify({
            "estudiantes": lista_estudiantes,
            "precision_global": calcular_precision_global()
        }), 200
    except Exception as e:
        return jsonify({"error": str(e)}), 500


@app.route('/api/psicologa/estudio/<usuario_id>', methods=['GET'], strict_slashes=False)
def obtener_estudio_mensual_estudiante(usuario_id):
    mes_solicitado = request.args.get("mes")
    try:
        est = db.usuarios.find_one({"_id": usuario_id})
        detalles = (est.get("detalles_estudiante") or {}) if est else {}

        todos_logs = list(db.diario_logs.find({"usuario_id": usuario_id}).sort("fecha", DESCENDING))
        meses_set = sorted(list({str(l.get("fecha", ""))[:7] for l in todos_logs if len(str(l.get("fecha", ""))) >= 7}), reverse=True)
        mes_actual = datetime.now().strftime("%Y-%m")
        if mes_actual not in meses_set:
            meses_set.insert(0, mes_actual)

        mes = mes_solicitado if mes_solicitado else meses_set[0]
        logs_mes = [l for l in todos_logs if str(l.get("fecha", "")).startswith(mes)]

        # 1. Distribución emocional del mes
        emociones_base = ["Alegría", "Tristeza", "Miedo / Ansiedad", "Enojo", "Neutral"]
        conteo = {e: 0 for e in emociones_base}
        suma_punt = 0.0
        negativos = 0
        por_dia = {}

        for l in logs_mes:
            em = str(l.get("emocion", "Neutral"))
            if em not in conteo:
                conteo[em] = 0
            conteo[em] += 1
            p = float(l.get("puntuacion", 0.0))
            suma_punt += p
            if p < -0.5:
                negativos += 1
            fecha_str = str(l.get("fecha", ""))[:10]
            por_dia.setdefault(fecha_str, []).append({"p": p, "e": em})

        total_mes = len(logs_mes)
        promedio = round(suma_punt / total_mes, 2) if total_mes > 0 else 0.0
        nivel_alerta = "SIN DATOS" if total_mes == 0 else ("ALTA" if negativos >= 3 else ("MODERADA" if promedio < -0.2 else "ESTABLE"))

        distribucion = [
            {
                "emocion": e,
                "conteo": conteo[e],
                "porcentaje": round((conteo[e] / total_mes) * 100) if total_mes > 0 else 0
            }
            for e in emociones_base
        ]

        # 2. Serie diaria del mes (para gráfica de evolución y calendario)
        try:
            anio, num_mes = map(int, mes.split("-"))
            dias_en_mes = calendar.monthrange(anio, num_mes)[1]
        except Exception:
            anio, num_mes, dias_en_mes = datetime.now().year, datetime.now().month, 30

        serie_diaria = []
        for dia in range(1, dias_en_mes + 1):
            f_dia = f"{anio:04d}-{num_mes:02d}-{dia:02d}"
            items = por_dia.get(f_dia, [])
            if items:
                prom_d = round(sum(x["p"] for x in items) / len(items), 2)
                emo_d = items[0]["e"]
            else:
                prom_d = None
                emo_d = None
            serie_diaria.append({
                "dia": dia,
                "fecha": f_dia,
                "puntuacion": prom_d,
                "emocion": emo_d,
                "registros": len(items)
            })

        # 3. Hábitos del alumno en ese mes
        metas_db = list(db.metas_habitos.find({"usuario_id": usuario_id, "mes": mes, "estado": "activo"}))
        habitos_lista = []
        for m in metas_db:
            dias_c = m.get("dias_cumplidos", [])
            meta_d = int(m.get("meta_dias", 20))
            habitos_lista.append({
                "titulo": m.get("titulo", ""),
                "tipo": m.get("tipo", "construir"),
                "micro_accion": m.get("micro_accion", ""),
                "cumplidos": len(dias_c),
                "meta_dias": meta_d,
                "porcentaje": min(100, round((len(dias_c) / max(1, meta_d)) * 100))
            })

        # 4. Recomendaciones generadas por EmotiData + Evaluación de Precisión (respetando privacidad)
        recomendaciones = []
        eval_total, eval_precisas = 0, 0
        for l in logs_mes:
            prec = l.get("precision_psicologa")
            if prec in ("precisa", "no_precisa"):
                eval_total += 1
                if prec == "precisa":
                    eval_precisas += 1
            compartido = bool(l.get("compartido_psicologa", False))
            recomendaciones.append({
                "id": str(l["_id"]),
                "fecha": str(l.get("fecha", "")),
                "emocion": str(l.get("emocion", "")),
                "puntuacion": float(l.get("puntuacion", 0.0)),
                "respuesta_enviada": str(l.get("respuesta_enviada", "")),
                "reto_sugerido": l.get("reto_sugerido") or obtener_reto_sugerido(str(l.get("emocion", "Neutral"))),
                "precision_psicologa": prec,
                "comentario_precision": str(l.get("comentario_precision", "")),
                "compartido_psicologa": compartido,
                "texto_compartido": str(l.get("texto_original", "")) if compartido else None
            })

        # 5. Notas clínicas de la psicóloga (colección notas_psicológicas)
        notas_raw = list(db["notas_psicológicas"].find({
            "$or": [{"estudiante_id": usuario_id}, {"usuario_id": usuario_id}]
        }).sort("fecha", DESCENDING))
        notas_legacy = list(db["notas_psicologica"].find({
            "$or": [{"estudiante_id": usuario_id}, {"usuario_id": usuario_id}]
        }).sort("fecha", DESCENDING))

        notas_clinicas = []
        for n in (notas_raw + notas_legacy):
            notas_clinicas.append({
                "id": str(n.get("_id", "")),
                "fecha": str(n.get("fecha", "")),
                "autor": str(n.get("autor", "Psicología / Tutoría")),
                "nota": str(n.get("notas") or n.get("observaciones") or "")
            })

        return jsonify({
            "estudiante": {
                "id": usuario_id,
                "nombre": str(est.get("nombre_completo", usuario_id)).strip() if est else usuario_id,
                "correo": str(est.get("correo", "")).strip() if est else "",
                "matricula": str(detalles.get("matricula", "")).strip(),
                "carrera": str(detalles.get("carrera", "")).strip()
            },
            "mes": mes,
            "meses_disponibles": meses_set,
            "resumen": {
                "total_entradas": total_mes,
                "promedio_puntuacion": promedio,
                "nivel_alerta": nivel_alerta,
                "metas_activas": len(habitos_lista)
            },
            "distribucion": distribucion,
            "serie_diaria": serie_diaria,
            "habitos": habitos_lista,
            "recomendaciones": recomendaciones,
            "precision_alumno": {
                "evaluadas": eval_total,
                "precisas": eval_precisas,
                "porcentaje": round((eval_precisas / eval_total) * 100) if eval_total > 0 else None
            },
            "notas_clinicas": notas_clinicas
        }), 200
    except Exception as e:
        return jsonify({"error": str(e)}), 500


@app.route('/api/psicologa/evaluar_precision', methods=['POST'], strict_slashes=False)
def evaluar_precision_recomendacion():
    data = request.json or {}
    log_id = data.get("log_id")
    precision = data.get("precision")  # "precisa" o "no_precisa"
    comentario = str(data.get("comentario", "")).strip()
    psicologa_id = str(data.get("psicologa_id", "USR_DULCE_02")).strip()

    if not log_id or precision not in ("precisa", "no_precisa"):
        return jsonify({"error": "Datos inválidos para evaluar precisión."}), 400

    try:
        res = db.diario_logs.update_one(
            {"_id": ObjectId(log_id)},
            {"$set": {
                "precision_psicologa": precision,
                "comentario_precision": comentario,
                "evaluado_por": psicologa_id,
                "fecha_evaluacion": datetime.now().strftime("%Y-%m-%d %H:%M:%S")
            }}
        )
        if res.matched_count == 0:
            return jsonify({"error": "Registro no encontrado."}), 404

        return jsonify({
            "mensaje": "Evaluación guardada",
            "precision_global": calcular_precision_global()
        }), 200
    except Exception as e:
        return jsonify({"error": str(e)}), 500


@app.route('/api/psicologa/notas', methods=['POST'], strict_slashes=False)
def guardar_nota_psicologa():
    data = request.json or {}
    usuario_id = str(data.get("usuario_id", "")).strip()
    texto_nota = str(data.get("nota", "")).strip()
    psicologa_id = str(data.get("psicologa_id", "USR_DULCE_02")).strip()
    autor = str(data.get("autor", "Lic. Dulce Contreras")).strip()

    if not usuario_id or not texto_nota:
        return jsonify({"error": "Escribe una observación antes de guardar."}), 400

    try:
        doc = {
            "estudiante_id": usuario_id,
            "usuario_id": usuario_id,
            "psicologa_id": psicologa_id,
            "autor": autor,
            "notas": texto_nota,
            "observaciones": texto_nota,
            "fecha": datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        }
        res = db["notas_psicológicas"].insert_one(doc)
        return jsonify({"mensaje": "Nota clínica guardada en Atlas.", "id": str(res.inserted_id)}), 201
    except Exception as e:
        return jsonify({"error": str(e)}), 500


@app.route('/api/psicologa/registrar_estudiante', methods=['POST'], strict_slashes=False)
@app.route('/api/usuarios/registrar', methods=['POST'], strict_slashes=False)
def registrar_usuario():
    data = request.json or {}
    nombre = str(data.get("nombre_completo", "")).strip()
    correo = str(data.get("correo", "")).strip().lower()
    matricula = str(data.get("matricula", "")).strip()
    carrera = str(data.get("carrera", "")).strip()
    password_plana = str(data.pop("password", None) or data.pop("contrasena", None) or "").strip()

    if not nombre or not correo:
        return jsonify({"error": "Nombre completo y correo son obligatorios."}), 400

    try:
        existente = db.usuarios.find_one({"correo": correo})
        if existente:
            return jsonify({"error": "Ya existe un usuario registrado con ese correo."}), 400

        nuevo_id = str(data.get("_id") or f"USR_{matricula or secrets.token_hex(3).upper()}").strip()
        nuevo_doc = {
            "_id": nuevo_id,
            "nombre_completo": nombre,
            "correo": correo,
            "rol": data.get("rol", "estudiante"),
            "activo": True,
            "fecha_registro": datetime.now().strftime("%Y-%m-%d %H:%M:%S"),
            "detalles_estudiante": {
                "matricula": matricula,
                "carrera": carrera,
                "racha_dias": 0,
                "nivel": 1
            }
        }
        if password_plana:
            nuevo_doc["password_hash"] = generate_password_hash(password_plana)

        db.usuarios.insert_one(nuevo_doc)
        return jsonify({"mensaje": f"Estudiante '{nombre}' registrado con éxito.", "id": nuevo_id}), 201
    except Exception as e:
        return jsonify({"error": str(e)}), 500


if __name__ == '__main__':
    print("🚀 Servidor Flask (API EmotiData) Completo...")
    app.run(host='127.0.0.1', port=5000, debug=True)