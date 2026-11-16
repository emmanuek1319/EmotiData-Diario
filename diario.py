from flask import Flask, request, jsonify, g
from flask_cors import CORS
from pymongo import MongoClient, DESCENDING
from pymongo.errors import DuplicateKeyError
from bson.objectid import ObjectId
from werkzeug.security import generate_password_hash, check_password_hash
from datetime import datetime, timedelta, timezone
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
TZ_MX = timezone(timedelta(hours=-6))


def ahora_mx():
    return datetime.now(TZ_MX)


def a_entero(valor, defecto):
    try:
        if valor is None or str(valor).strip() == "":
            return defecto
        return int(float(valor))
    except (ValueError, TypeError):
        return defecto


def a_flotante(valor, defecto=0.0):
    try:
        if valor is None or str(valor).strip() == "":
            return defecto
        return float(valor)
    except (ValueError, TypeError):
        return defecto


def a_lista(valor):
    return valor if isinstance(valor, list) else []


def obtener_detalles(doc):
    if not isinstance(doc, dict):
        return {}
    raw = doc.get("detalles_estudiante")
    return raw if isinstance(raw, dict) else {}


try:
    client = MongoClient(MONGO_URI, serverSelectionTimeoutMS=5000)
    db = client["diario_inteligente"]
    client.admin.command("ping")
    tipo_conexion = "Atlas (Nube)" if "mongodb+srv" in MONGO_URI else "Local"
    print(f"✅ Conectado a MongoDB: {tipo_conexion}")
except Exception as e:
    print(f"❌ Error al conectar a MongoDB: {e}")
    raise

import emoti_auth
import emoti_motor
from emoti_auth import requiere_sesion, requiere_rol, propio, error_interno
emoti_auth.init(app, db)

# Catálogo de las 27 emociones: (emocion, puntuacion, keywords, respuesta_empatica)
EMOCIONES_27 = [
    ("Ansiedad", -0.8, ["ansiedad", "ansioso", "ansiosa", "nervioso", "nerviosa", "panico", "pánico", "agobiado", "estres", "estrés", "preocupado", "inquieto", "taquicardia", "estresado", "estresada", "estresante", "agobiada", "preocupada", "inquieta", "angustia", "angustiado", "angustiada"], "Respira despacio: inhala 4 segundos y exhala 6. No tienes que resolver todo hoy mismo."),
    ("Miedo", -0.8, ["miedo", "asustado", "asustada", "temor", "terror", "inseguro", "amenaza", "sustos"], "Sentir miedo es una señal de alerta natural. Da un paso pequeño a la vez y apóyate en alguien de confianza."),
    ("Tristeza", -0.7, ["triste", "tristeza", "deprimido", "llorar", "llorando", "apagado", "desanimado", "vacio", "vacío", "dolor", "pena", "mal", "fatal", "pésimo", "de la chingada", "deprimida", "desanimada", "decaido", "decaida", "lloro", "llore"], "Es válido sentirte así. Date permiso de ir más despacio hoy y recuerda que no estás solo."),
    ("Enojo", -0.7, ["enojado", "enojada", "enojo", "coraje", "rabia", "molesto", "frustrado", "harto", "furioso", "ira", "molesta", "frustrada", "harta", "furiosa"], "Tu enojo señala que algo cruzó un límite importante. Haz una pausa antes de responder o decidir."),
    ("Impacto", -0.7, ["horror", "impactado", "shock", "espantado", "aterrado", "terrible", "horrible"], "Cuando algo nos sacude fuerte, volver al cuerpo ayuda: toma agua y enfócate en tu respiración."),
    ("Rechazo", -0.6, ["asco", "repulsion", "repulsión", "rechazo", "desagrado", "hartazgo"], "Reconocer lo que te genera rechazo también te ayuda a proteger tus límites y tu bienestar."),
    ("Incomodidad", -0.5, ["incomodo", "incómodo", "verguenza", "vergüenza", "culpa", "culpable", "pena ajena", "torpe"], "Todos cometemos errores o pasamos momentos incómodos; trátate con la misma paciencia que a un amigo."),
    ("Envidia", -0.4, ["envidia", "celos", "comparando", "compararme", "quisiera tener"], "Compararte desgasta tu energía. Usa esa señal para identificar qué meta personal te gustaría construir."),
    ("Aburrimiento", -0.3, ["aburrido", "aburrida", "aburrimiento", "monotonia", "monotonía", "sin nada que hacer", "tedio"], "El aburrimiento es una invitación a cambiar de actividad por 15 minutos y probar algo distinto."),
    ("Confusión", -0.3, ["confundido", "confundida", "confusion", "confusión", "perdido", "no se que hacer", "no sé qué hacer", "duda"], "No tener todas las respuestas hoy es normal; escribe las opciones en papel para aclarar tu mente."),
    ("Nostalgia", -0.2, ["nostalgia", "nostalgico", "extrano", "extraño", "recuerdos"], "Extrañar etapas o personas habla de lo valioso que fue para ti. Agradece ese recuerdo y vuelve al presente."),
    ("Empatía", -0.2, ["empatia", "empatía", "compasion", "compasión", "me duele ver", "solidaridad"], "Tu capacidad de conectar con otros es valiosa; cuida también tus propios límites emocionales."),
    ("Anhelo", 0.1, ["anhelo", "deseo", "quisiera", "ojala", "ojalá", "sueno con", "sueño con"], "Tener anhelos claros te da dirección. Convierte ese deseo en una micro-acción concreta hoy."),
    ("Asombro", 0.4, ["asombro", "asombrado", "sorprendido", "increible", "increíble", "maravillado"], "Mantener la capacidad de asombro mantiene tu mente abierta y receptiva a nuevas ideas."),
    ("Interés", 0.4, ["interes", "interés", "interesado", "curiosidad", "curioso", "aprender", "investigando"], "Tu curiosidad es un gran motor de crecimiento. Aprovecha ese enfoque para avanzar en algo que te apasione."),
    ("Alivio", 0.5, ["alivio", "aliviado", "por fin", "ya paso", "ya pasó", "descanso", "respiro"], "Qué bueno que esa carga disminuyó. Aprovecha este respiro para recargar energía."),
    ("Calma", 0.6, ["calma", "calmado", "tranquilo", "tranquila", "paz", "sereno", "relajado"], "Disfruta este estado de serenidad; es una base excelente para tomar buenas decisiones."),
    ("Satisfacción", 0.6, ["satisfecho", "satisfaccion", "satisfacción", "logre", "logré", "cumpli", "cumplí", "orgulloso"], "Reconoce tu propio esfuerzo: cada avance cumplido fortalece tu confianza."),
    ("Admiración", 0.6, ["admiracion", "admiración", "admiro"], "Admirar cualidades en otros refleja valores que también puedes cultivar en ti."),
    ("Inspiración", 0.7, ["inspirado", "inspiracion", "inspiración", "embeleso", "creativo", "arte", "musica", "música"], "Aprovecha esta chispa creativa para anotar tus ideas o dar forma a un proyecto."),
    ("Gratitud", 0.7, ["gratitud", "agradecido", "agradecida", "gracias", "afortunado"], "La gratitud entrena tu atención hacia lo que sí funciona en tu vida."),
    ("Afecto", 0.7, ["afecto", "carino", "cariño", "querer", "quiero mucho", "aprecio"], "Cuidar tus vínculos afectivos es uno de los pilares más sólidos del bienestar emocional."),
    ("Ilusión", 0.7, ["ilusion", "ilusión", "romance", "enamorado", "enamorada", "pareja", "cita"], "Disfruta esta etapa con autenticidad, comunicación clara y equilibrio personal."),
    ("Diversión", 0.8, ["diversion", "diversión", "divertido", "risa", "riendo", "gracioso", "jugando"], "Reír y disfrutar el momento libera tensión y renueva tu ánimo."),
    ("Motivación", 0.8, ["motivado", "motivacion", "motivación", "pasion", "pasión", "enfocado", "determinado"], "Esa determinación es oro: canalízala ahora mismo en tu tarea más importante."),
    ("Entusiasmo", 0.9, ["entusiasmo", "entusiasmado", "emocionado", "emocionada", "euforia", "genial"], "¡Qué gran impulso! Aprovecha esa energía para dar un paso firme hacia tus metas."),
    ("Alegría", 0.9, ["feliz", "alegre", "alegria", "alegría", "contento", "contenta", "bien", "excelente", "maravilloso", "chido", "chida"], "¡Qué gusto leer esto! Guarda este momento como recordatorio de lo bien que puedes sentirte."),
    ("Neutral", 0.0, [], "Gracias por registrar tu día. La constancia de escribir fortalece tu autoconocimiento.")
]

RETOS_RESPALDO = [
    {"emocion_trigger": "Alegría", "tipo": "mejorar", "titulo": "Anclar un buen momento", "micro_accion": "Escribir 3 detalles de por qué hoy fue un buen día", "duracion_dias": 14},
    {"emocion_trigger": "Tristeza", "tipo": "construir", "titulo": "Un buen momento al día", "micro_accion": "Anotar al final del día una cosa pequeña que salió bien", "duracion_dias": 14},
    {"emocion_trigger": "Ansiedad", "tipo": "construir", "titulo": "Higiene del sueño", "micro_accion": "Apagar pantallas 30 minutos antes de dormir", "duracion_dias": 21},
    {"emocion_trigger": "Miedo", "tipo": "construir", "titulo": "Paso valiente de 10 minutos", "micro_accion": "Dividir una tarea que me intimida y hacer solo los primeros 10 minutos", "duracion_dias": 14},
    {"emocion_trigger": "Enojo", "tipo": "soltar", "titulo": "Pausa antes de reaccionar", "micro_accion": "Respirar 3 veces antes de responder cuando sienta coraje", "duracion_dias": 14},
    {"emocion_trigger": "Neutral", "tipo": "construir", "titulo": "Estudio sin distracciones", "micro_accion": "25 minutos de estudio o lectura sin celular", "duracion_dias": 20}
]


def analizar_texto_27(texto):
    return emoti_motor.analizar(texto, EMOCIONES_27)

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
                "duracion_dias": a_entero(r.get("duracion_dias"), 15)
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


@app.route('/', methods=['GET'], strict_slashes=False)
@app.route('/health', methods=['GET'], strict_slashes=False)
def health():
    return jsonify({"estado": "ok", "servicio": "EmotiData API", "emociones_soportadas": 27}), 200


@app.route('/api/usuario/resumen', methods=['GET'], strict_slashes=False)
@requiere_sesion
def obtener_resumen_usuario_default():
    return obtener_resumen_usuario(g.uid)


@app.route('/api/usuario/resumen/<usuario_id>', methods=['GET'], strict_slashes=False)
@app.route('/api/usuarios/perfil/<usuario_id>', methods=['GET'], strict_slashes=False)
@requiere_sesion
def obtener_resumen_usuario(usuario_id):
    _x = propio(usuario_id)
    if _x:
        return _x
    try:
        usuario = db.usuarios.find_one({"_id": usuario_id}) or db.usuarios.find_one({"_id": "USR_EMMANUEL_01"})
        detalles = obtener_detalles(usuario)

        ultimo_log = db.diario_logs.find_one({"usuario_id": usuario_id}, sort=[("fecha", DESCENDING)])
        total_usuario = db.diario_logs.count_documents({"usuario_id": usuario_id})

        racha = emoti_auth.calcular_racha(usuario_id)
        ultima_emocion = str(ultimo_log.get("emocion", "Neutral")) if ultimo_log else "Sin registros"
        ultima_fecha = str(ultimo_log.get("fecha", "--")) if ultimo_log else "--"

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
        return error_interno(e)


@app.route('/api/usuario/logros', methods=['GET'], strict_slashes=False)
@requiere_sesion
def obtener_logros_usuario_default():
    return obtener_logros_usuario(g.uid)


@app.route('/api/usuario/logros/<usuario_id>', methods=['GET'], strict_slashes=False)
@requiere_sesion
def obtener_logros_usuario(usuario_id):
    _x = propio(usuario_id)
    if _x:
        return _x
    try:
        usuario = db.usuarios.find_one({"_id": usuario_id}) or db.usuarios.find_one({"_id": "USR_EMMANUEL_01"})
        detalles = obtener_detalles(usuario)
        total_logs = db.diario_logs.count_documents({"usuario_id": usuario_id})

        racha = emoti_auth.calcular_racha(usuario_id)
        nivel = max(1, (total_logs // 3) + 1)
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
                {"titulo": "Primer Paso", "descripcion": "Registraste tu primera emoción en el diario.", "desbloqueado": total_logs >= 1, "icono": "🏅"},
                {"titulo": "Constancia (7 Días)", "descripcion": "Mantuviste una racha de registro por una semana.", "desbloqueado": racha >= 7, "icono": "🔥"},
                {"titulo": "Autoconocimiento", "descripcion": "Alcanzaste el Nivel 3 de introspección.", "desbloqueado": nivel >= 3, "icono": "🧠"},
                {"titulo": "Atrévete al Cambio", "descripcion": "Iniciaste tu primera meta de hábitos saludables.", "desbloqueado": metas_activas > 0, "icono": "🌱"}
            ]
        }), 200
    except Exception as e:
        return error_interno(e)


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


@app.route('/api/diario/reporte_psicologica', methods=['GET'], strict_slashes=False)
@requiere_rol("psicologa")
def obtener_reporte_psicologica_default():
    return obtener_reporte_psicologica('USR_EMMANUEL_01')


@app.route('/api/diario/reporte_psicologica/<usuario_id>', methods=['GET'], strict_slashes=False)
@app.route('/api/psicologa/reporte/<usuario_id>', methods=['GET'], strict_slashes=False)
@requiere_rol("psicologa")
def obtener_reporte_psicologica(usuario_id):
    if not emoti_auth.estudiante_visible(usuario_id):
        return jsonify({"error": "Este estudiante no ha compartido su información contigo."}), 403
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
                p = a_flotante(l.get("puntuacion"), 0.0)
                suma_punt += p
                if p < -0.5:
                    negativos_seguidos += 1

            predominante = max(conteo_emo, key=conteo_emo.get)
            promedio = round(suma_punt / total_entradas, 2)
            nivel_alerta = "ALTA" if negativos_seguidos >= 3 else ("MODERADA" if promedio < -0.2 else "ESTABLE")

            obs = (
                f"Resumen estadístico ({total_entradas} registros recientes): "
                f"Emoción predominante '{predominante}', índice emocional promedio {promedio}, "
                f"nivel de alerta preventiva: {nivel_alerta}."
            )
        else:
            nivel_alerta = "SIN DATOS"
            obs = f"El estudiante {usuario_id} aún no cuenta con entradas registradas en su diario."

        nota = (
            db["notas_psicológicas"].find_one({"$or": [{"estudiante_id": usuario_id}, {"usuario_id": usuario_id}]}, sort=[("fecha", DESCENDING)])
            or db["notas_psicologica"].find_one({"$or": [{"estudiante_id": usuario_id}, {"usuario_id": usuario_id}]}, sort=[("fecha", DESCENDING)])
        )
        nota_clinica = (nota.get("notas") or nota.get("observaciones")) if isinstance(nota, dict) else ""

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
        return error_interno(e)


@app.route('/api/diario/compartir/<log_id>', methods=['POST'], strict_slashes=False)
@requiere_sesion
def compartir_entrada_psicologa(log_id):
    data = request.json or {}
    usuario_id = g.uid
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
        return jsonify({"mensaje": "Privacidad actualizada.", "compartido_psicologa": autorizar}), 200
    except Exception as e:
        return error_interno(e)


@app.route('/api/diario/historial', methods=['GET'], strict_slashes=False)
@requiere_sesion
def obtener_historial_default():
    return obtener_historial(g.uid)


@app.route('/api/diario/historial/<usuario_id>', methods=['GET'], strict_slashes=False)
@requiere_sesion
def obtener_historial(usuario_id):
    _x = propio(usuario_id)
    if _x:
        return _x
    try:
        logs = list(db.diario_logs.find({"usuario_id": usuario_id}).sort("fecha", DESCENDING).limit(500))
        historial_lista = []
        for log in logs:
            historial_lista.append({
                "id": str(log.get("_id", "1")),
                "fecha": str(log.get("fecha", "")),
                "dia": str(log.get("fecha", ""))[:10],
                "emocion": str(log.get("emocion", "")),
                "estado_animo": str(log.get("emocion", "")),
                "texto_original": str(log.get("texto_original", "")),
                "pensamiento": str(log.get("texto_original", "")),
                "texto": str(log.get("texto_original", "")),
                "respuesta_enviada": str(log.get("respuesta_enviada", "Sigue adelante")),
                "puntuacion": a_flotante(log.get("puntuacion"), 0.0),
                "compartido_psicologa": bool(log.get("compartido_psicologa", False))
            })

        return jsonify({"entradas": historial_lista}), 200
    except Exception as e:
        return error_interno(e)


@app.route('/api/diario/registrar', methods=['POST'], strict_slashes=False)
@app.route('/api/diario/guardar', methods=['POST'], strict_slashes=False)
@requiere_sesion
def registrar_emocion():
    data = request.get_json(silent=True) or {}
    texto = emoti_auth.limpiar(data.get("texto"), 5000)
    if not texto:
        return jsonify({"error": "Escribe algo antes de guardar."}), 400
    if len(texto) > 1000:
        return jsonify({"error": "Tu entrada es muy larga (máximo 1000 caracteres)."}), 400
    cid = str(data.get("cid") or "")
    if not re.fullmatch(r"[A-Za-z0-9_-]{8,64}", cid):
        cid = ""

    def respuesta(log, codigo, duplicado=False):
        out = {"mensaje": "Guardado con éxito", "log_id": str(log["_id"]), "emocion": log.get("emocion"),
               "mensaje_empatico": log.get("respuesta_enviada"), "compartido_psicologa": bool(log.get("compartido_psicologa")),
               "reto_sugerido": log.get("reto_sugerido"), "duplicado": duplicado}
        if log.get("senal_riesgo"):
            out["ayuda"] = emoti_motor.AYUDA
        return jsonify(out), codigo

    if cid:
        previo = db.diario_logs.find_one({"usuario_id": g.uid, "cid": cid})
        if previo:
            return respuesta(previo, 200, True)  # reintento o doble clic: no se duplica
    emocion, puntuacion, resp = analizar_texto_27(texto)
    riesgo = emoti_motor.detectar_riesgo(texto)
    if riesgo:
        resp = emoti_motor.RESPUESTA_RIESGO
    ahora = ahora_mx()
    log = {
        "usuario_id": g.uid, "fecha": ahora.strftime("%Y-%m-%d %H:%M:%S"), "dia": ahora.strftime("%Y-%m-%d"),
        "ts": datetime.now(timezone.utc), "texto_original": texto, "emocion": emocion, "puntuacion": puntuacion,
        "respuesta_enviada": resp, "reto_sugerido": obtener_reto_sugerido(emocion), "compartido_psicologa": False,
        "precision_psicologa": None, "senal_riesgo": riesgo
    }
    if cid:
        log["cid"] = cid
    try:
        db.diario_logs.insert_one(log)
    except DuplicateKeyError:
        ex = db.diario_logs.find_one({"usuario_id": g.uid, "cid": cid})
        if ex:
            return respuesta(ex, 200, True)
        return jsonify({"error": "No pudimos guardar tu entrada. Inténtalo de nuevo."}), 503
    except Exception as e:
        print("❌ no se pudo guardar una entrada:", type(e).__name__)
        return jsonify({"error": "No pudimos guardar tu entrada. Inténtalo de nuevo."}), 503
    return respuesta(log, 201)


@app.route('/api/habitos/catalogo', methods=['GET'], strict_slashes=False)
@requiere_sesion
def obtener_catalogo_habitos():
    emocion = request.args.get("emocion")
    try:
        filtro = {"activo": {"$ne": False}}
        if emocion:
            filtro["emocion_trigger"] = emocion
        docs = list(db.retos_catalogo.find(filtro))
        if docs:
            if not emocion and len(docs) > 12:
                docs = random.sample(docs, 12)
            catalogo = [{
                "id": str(d["_id"]),
                "emocion_trigger": d.get("emocion_trigger", "Neutral"),
                "tipo": d.get("tipo", "construir"),
                "titulo": d.get("titulo", ""),
                "micro_accion": d.get("micro_accion", ""),
                "duracion_dias": a_entero(d.get("duracion_dias"), 15)
            } for d in docs]
        else:
            catalogo = [r for r in RETOS_RESPALDO if not emocion or r["emocion_trigger"] == emocion]
        return jsonify({"catalogo": catalogo}), 200
    except Exception as e:
        return error_interno(e)


@app.route('/api/habitos/<usuario_id>', methods=['GET'], strict_slashes=False)
@requiere_sesion
def obtener_metas_habitos(usuario_id):
    _x = propio(usuario_id)
    if _x:
        return _x
    mes_actual = ahora_mx().strftime("%Y-%m")
    hoy_str = ahora_mx().strftime("%Y-%m-%d")
    try:
        marca = db.usuarios.update_one({"_id": usuario_id, "habitos_iniciados": {"$ne": True}}, {"$set": {"habitos_iniciados": True}})
        if marca.modified_count == 1 and db.metas_habitos.count_documents({"usuario_id": usuario_id}) == 0:
            iniciales = []
            for tipo in ("construir", "mejorar", "soltar"):
                cand = list(db.retos_catalogo.find({"tipo": tipo, "activo": {"$ne": False}}))
                if cand:
                    r = random.choice(cand)
                    iniciales.append({
                        "usuario_id": usuario_id,
                        "reto_origen_id": str(r["_id"]),
                        "tipo": tipo,
                        "titulo": r.get("titulo", ""),
                        "micro_accion": r.get("micro_accion", ""),
                        "mes": mes_actual,
                        "meta_dias": a_entero(r.get("duracion_dias"), 20),
                        "dias_cumplidos": [],
                        "estado": "activo",
                        "fecha_creacion": ahora_mx().strftime("%Y-%m-%d %H:%M:%S")
                    })
            if iniciales:
                db.metas_habitos.insert_many(iniciales)

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
            punt_por_dia.setdefault(dia_f, []).append(a_flotante(l.get("puntuacion"), 0.0))

        lista_metas = []
        for m in metas_db:
            dias = sorted(list(set(a_lista(m.get("dias_cumplidos")))))
            meta_dias = a_entero(m.get("meta_dias"), 20)
            porcentaje = min(100, round((len(dias) / max(1, meta_dias)) * 100))

            racha = 0
            cursor = ahora_mx().date()
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

        todas_metas = list(db.metas_habitos.find({"usuario_id": usuario_id, "estado": {"$ne": "eliminada"}}))
        total_victorias = sum(len(a_lista(x.get("dias_cumplidos"))) for x in todas_metas)
        alguna_7 = any(len(a_lista(x.get("dias_cumplidos"))) >= 7 for x in todas_metas)
        alguna_cumplida = any(len(a_lista(x.get("dias_cumplidos"))) >= a_entero(x.get("meta_dias"), 20) for x in todas_metas)
        mes_oro = len(lista_metas) >= 3 and all(x["porcentaje"] >= 100 for x in lista_metas)

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
        return error_interno(e)


@app.route('/api/habitos/asignar', methods=['POST'], strict_slashes=False)
@requiere_sesion
def asignar_meta_habito():
    data = request.json or {}
    usuario_id = g.uid
    titulo = emoti_auth.limpiar(data.get("titulo"), 80)
    micro_accion = emoti_auth.limpiar(data.get("micro_accion"), 160)

    if not usuario_id or not titulo or not micro_accion:
        return jsonify({"error": "Faltan datos obligatorios (usuario_id, titulo, micro_accion)."}), 400

    mes_actual = ahora_mx().strftime("%Y-%m")
    try:
        activas = db.metas_habitos.count_documents({"usuario_id": usuario_id, "mes": mes_actual, "estado": "activo"})
        if activas >= 5:
            return jsonify({"error": "Ya tienes 5 metas activas este mes. Quita una antes de agregar otra."}), 400

        nueva_meta = {
            "usuario_id": usuario_id,
            "reto_origen_id": data.get("reto_id"),
            "tipo": data.get("tipo") if data.get("tipo") in ("construir", "mejorar", "soltar") else "construir",
            "titulo": titulo,
            "micro_accion": micro_accion,
            "mes": mes_actual,
            "meta_dias": max(5, min(31, a_entero(data.get("meta_dias"), 20))),
            "dias_cumplidos": [],
            "estado": "activo",
            "fecha_creacion": ahora_mx().strftime("%Y-%m-%d %H:%M:%S")
        }
        res = db.metas_habitos.insert_one(nueva_meta)
        return jsonify({"mensaje": "Meta de hábito guardada en tu cuenta", "id": str(res.inserted_id)}), 201
    except Exception as e:
        return error_interno(e)


@app.route('/api/habitos/repetir_mes_anterior', methods=['POST'], strict_slashes=False)
@requiere_sesion
def repetir_metas_mes_anterior():
    mes_actual = ahora_mx().strftime("%Y-%m")
    try:
        previo = db.metas_habitos.find_one({"usuario_id": g.uid, "mes": {"$lt": mes_actual}, "estado": "activo"}, sort=[("mes", DESCENDING)])
        if not previo:
            return jsonify({"error": "No se encontraron metas previas."}), 404
        origen = list(db.metas_habitos.find({"usuario_id": g.uid, "mes": previo["mes"], "estado": "activo"}).limit(5))
        actuales = list(db.metas_habitos.find({"usuario_id": g.uid, "mes": mes_actual, "estado": "activo"}))
        ya = {str(m.get("titulo", "")).strip().lower() for m in actuales}
        creadas = 0
        for p in origen:
            titulo = str(p.get("titulo", "")).strip()
            if len(actuales) + creadas >= 5:
                break
            if titulo.lower() in ya:
                continue
            try:
                db.metas_habitos.insert_one({
                    "usuario_id": g.uid, "tipo": p.get("tipo", "construir"), "titulo": titulo,
                    "micro_accion": p.get("micro_accion", ""), "mes": mes_actual, "meta_dias": a_entero(p.get("meta_dias"), 20),
                    "dias_cumplidos": [], "estado": "activo", "fecha_creacion": ahora_mx().strftime("%Y-%m-%d %H:%M:%S")
                })
                ya.add(titulo.lower())
                creadas += 1
            except DuplicateKeyError:
                pass
        return jsonify({"mensaje": f"Se repitieron {creadas} metas para este mes."}), 201
    except Exception as e:
        return error_interno(e)


@app.route('/api/habitos/checkin', methods=['POST'], strict_slashes=False)
@requiere_sesion
def checkin_meta_habito():
    data = request.json or {}
    meta_id = data.get("meta_id")
    usuario_id = g.uid
    fecha_dia = emoti_auth.dia_valido(data.get("fecha"))

    if not meta_id or not usuario_id:
        return jsonify({"error": "Se requiere meta_id y usuario_id."}), 400

    try:
        meta = db.metas_habitos.find_one({"_id": ObjectId(meta_id), "usuario_id": usuario_id})
        if not meta:
            return jsonify({"error": "Meta no encontrada."}), 404

        dias = a_lista(meta.get("dias_cumplidos"))
        if fecha_dia in dias:
            db.metas_habitos.update_one({"_id": ObjectId(meta_id)}, {"$pull": {"dias_cumplidos": fecha_dia}})
            hecho = False
        else:
            db.metas_habitos.update_one({"_id": ObjectId(meta_id)}, {"$addToSet": {"dias_cumplidos": fecha_dia}})
            hecho = True

        return jsonify({"mensaje": "Seguimiento actualizado", "hecho": hecho, "fecha": fecha_dia}), 200
    except Exception as e:
        return error_interno(e)


@app.route('/api/habitos/<meta_id>', methods=['DELETE'], strict_slashes=False)
@requiere_sesion
def eliminar_meta_habito(meta_id):
    try:
        res = db.metas_habitos.update_one({"_id": ObjectId(meta_id), "usuario_id": g.uid}, {"$set": {"estado": "eliminada"}})
        if res.matched_count == 0:
            return jsonify({"error": "Meta no encontrada."}), 404
        return jsonify({"mensaje": "Meta eliminada"}), 200
    except Exception as e:
        return error_interno(e)


@app.route('/api/usuarios/configuracion', methods=['GET'], strict_slashes=False)
@app.route('/api/usuarios/configuracion/<id_usuario>', methods=['GET'], strict_slashes=False)
def obtener_configuracion_tema(id_usuario='USR_EMMANUEL_01'):
    return jsonify({"tema": "OscuroRojo", "tema_color": "Verde Menta", "modo_oscuro": True}), 200


@app.route('/api/psicologa/estudiantes', methods=['GET'], strict_slashes=False)
@requiere_rol("psicologa")
def obtener_estudiantes():
    try:
        estudiantes_db = list(db.usuarios.find({"rol": "estudiante", **emoti_auth.FILTRO_VISIBLES}))
        lista_estudiantes = []
        for est in estudiantes_db:
            id_est = str(est.get("_id", "")).strip()
            detalles = obtener_detalles(est)
            ultimo_log = db.diario_logs.find_one({"usuario_id": id_est}, sort=[("fecha", DESCENDING)])
            puntuacion = a_flotante(ultimo_log.get("puntuacion"), 0.0) if ultimo_log else 0.0
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
        return error_interno(e)


@app.route('/api/psicologa/estudio/<usuario_id>', methods=['GET'], strict_slashes=False)
@requiere_rol("psicologa")
def obtener_estudio_mensual_estudiante(usuario_id):
    if not emoti_auth.estudiante_visible(usuario_id):
        return jsonify({"error": "Este estudiante no ha compartido su información contigo."}), 403
    mes_solicitado = request.args.get("mes")
    try:
        est = db.usuarios.find_one({"_id": usuario_id})
        detalles = obtener_detalles(est)

        todos_logs = list(db.diario_logs.find({"usuario_id": usuario_id}).sort("fecha", DESCENDING))
        meses_set = sorted(list({str(l.get("fecha", ""))[:7] for l in todos_logs if len(str(l.get("fecha", ""))) >= 7}), reverse=True)
        mes_actual = ahora_mx().strftime("%Y-%m")
        if mes_actual not in meses_set:
            meses_set.insert(0, mes_actual)

        mes = mes_solicitado if mes_solicitado else meses_set[0]
        logs_mes = [l for l in todos_logs if str(l.get("fecha", "")).startswith(mes)]

        conteo = {}
        suma_punt = 0.0
        negativos = 0
        por_dia = {}

        for l in logs_mes:
            em = str(l.get("emocion", "Neutral"))
            conteo[em] = conteo.get(em, 0) + 1
            p = a_flotante(l.get("puntuacion"), 0.0)
            suma_punt += p
            if p < -0.5:
                negativos += 1
            fecha_str = str(l.get("fecha", ""))[:10]
            por_dia.setdefault(fecha_str, []).append({"p": p, "e": em})

        total_mes = len(logs_mes)
        promedio = round(suma_punt / total_mes, 2) if total_mes > 0 else 0.0
        nivel_alerta = "SIN DATOS" if total_mes == 0 else ("ALTA" if negativos >= 3 else ("MODERADA" if promedio < -0.2 else "ESTABLE"))

        if conteo:
            distribucion = [
                {"emocion": e, "conteo": c, "porcentaje": round((c / total_mes) * 100)}
                for e, c in sorted(conteo.items(), key=lambda x: x[1], reverse=True)
            ]
        else:
            distribucion = [{"emocion": e, "conteo": 0, "porcentaje": 0} for e in ["Alegría", "Calma", "Ansiedad", "Tristeza", "Neutral"]]

        try:
            anio, num_mes = map(int, mes.split("-"))
            dias_en_mes = calendar.monthrange(anio, num_mes)[1]
        except Exception:
            anio, num_mes, dias_en_mes = ahora_mx().year, ahora_mx().month, 30

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

        metas_db = list(db.metas_habitos.find({"usuario_id": usuario_id, "mes": mes, "estado": "activo"}))
        habitos_lista = []
        for m in metas_db:
            dias_c = a_lista(m.get("dias_cumplidos"))
            meta_d = a_entero(m.get("meta_dias"), 20)
            habitos_lista.append({
                "titulo": m.get("titulo", ""),
                "tipo": m.get("tipo", "construir"),
                "micro_accion": m.get("micro_accion", ""),
                "cumplidos": len(dias_c),
                "meta_dias": meta_d,
                "porcentaje": min(100, round((len(dias_c) / max(1, meta_d)) * 100))
            })

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
                "puntuacion": a_flotante(l.get("puntuacion"), 0.0),
                "respuesta_enviada": str(l.get("respuesta_enviada", "")),
                "reto_sugerido": l.get("reto_sugerido") or obtener_reto_sugerido(str(l.get("emocion", "Neutral"))),
                "precision_psicologa": prec,
                "comentario_precision": str(l.get("comentario_precision", "")),
                "compartido_psicologa": compartido,
                "texto_compartido": str(l.get("texto_original", "")) if compartido else None
            })

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
                "nombre": str(est.get("nombre_completo", usuario_id)).strip() if isinstance(est, dict) else usuario_id,
                "correo": str(est.get("correo", "")).strip() if isinstance(est, dict) else "",
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
        return error_interno(e)


@app.route('/api/psicologa/evaluar_precision', methods=['POST'], strict_slashes=False)
@requiere_rol("psicologa")
def evaluar_precision_recomendacion():
    data = request.json or {}
    log_id = data.get("log_id")
    precision = data.get("precision")
    comentario = str(data.get("comentario", "")).strip()
    psicologa_id = g.uid

    if not log_id or precision not in ("precisa", "no_precisa"):
        return jsonify({"error": "Datos inválidos para evaluar precisión."}), 400

    try:
        res = db.diario_logs.update_one(
            {"_id": ObjectId(log_id)},
            {"$set": {
                "precision_psicologa": precision,
                "comentario_precision": comentario,
                "evaluado_por": psicologa_id,
                "fecha_evaluacion": ahora_mx().strftime("%Y-%m-%d %H:%M:%S")
            }}
        )
        if res.matched_count == 0:
            return jsonify({"error": "Registro no encontrado."}), 404

        return jsonify({
            "mensaje": "Evaluación guardada",
            "precision_global": calcular_precision_global()
        }), 200
    except Exception as e:
        return error_interno(e)


@app.route('/api/psicologa/notas', methods=['POST'], strict_slashes=False)
@requiere_rol("psicologa")
def guardar_nota_psicologa():
    data = request.json or {}
    usuario_id = str(data.get("usuario_id", "")).strip()
    texto_nota = str(data.get("nota", "")).strip()
    psicologa_id = g.uid
    autor = emoti_auth.nombre_de(g.uid)

    if not usuario_id or not texto_nota:
        return jsonify({"error": "Escribe una observación antes de guardar."}), 400

    if not emoti_auth.estudiante_visible(usuario_id):
        return jsonify({"error": "Este estudiante no ha compartido su información contigo."}), 403
    try:
        doc = {
            "estudiante_id": usuario_id,
            "usuario_id": usuario_id,
            "psicologa_id": psicologa_id,
            "autor": autor,
            "notas": texto_nota,
            "observaciones": texto_nota,
            "fecha": ahora_mx().strftime("%Y-%m-%d %H:%M:%S")
        }
        res = db["notas_psicológicas"].insert_one(doc)
        return jsonify({"mensaje": "Nota clínica guardada en Atlas.", "id": str(res.inserted_id)}), 201
    except Exception as e:
        return error_interno(e)


@app.route('/api/psicologa/registrar_estudiante', methods=['POST'], strict_slashes=False)
@app.route('/api/usuarios/registrar', methods=['POST'], strict_slashes=False)
@requiere_rol("psicologa")
def registrar_usuario():
    data = request.json or {}
    nombre = str(data.get("nombre_completo", "")).strip()
    correo = str(data.get("correo", "")).strip().lower()
    matricula = str(data.get("matricula", "")).strip()
    carrera = str(data.get("carrera", "")).strip()
    password_plana = str(data.pop("password", None) or data.pop("contrasena", None) or "").strip()

    if not nombre or not correo:
        return jsonify({"error": "Nombre completo y correo son obligatorios."}), 400
    if not emoti_auth.RE_CORREO.match(correo):
        return jsonify({"error": "El correo no es válido."}), 400
    if password_plana and emoti_auth.validar_password(password_plana):
        return jsonify({"error": emoti_auth.validar_password(password_plana)}), 400

    try:
        existente = db.usuarios.find_one({"correo": correo})
        if existente:
            return jsonify({"error": "Ya existe un usuario registrado con ese correo."}), 400

        nuevo_id = f"USR_{matricula or secrets.token_hex(3).upper()}".strip()
        nuevo_doc = {
            "_id": nuevo_id,
            "nombre_completo": nombre,
            "correo": correo,
            "rol": "estudiante",
            "canalizado": True,
            "verificado": True,
            "creado_por": g.uid,
            "consentimientos": {"compartir_con_psicologa": True, "origen": "alta_por_psicologa"},
            "activo": True,
            "fecha_registro": ahora_mx().strftime("%Y-%m-%d %H:%M:%S"),
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
        return error_interno(e)


if __name__ == '__main__':
    print("🚀 Servidor Flask (API EmotiData) Completo (27 emociones)...")
    app.run(host='127.0.0.1', port=5000, debug=True)