"""EmotiData · motor emocional por reglas (sin servicios externos).

Mejoras frente a la búsqueda por subcadenas:
  - ignora acentos y mayúsculas, y compara palabras completas ("inspirado" ya no activa "ira");
  - entiende negaciones cercanas ("no me siento bien" no es alegría);
  - detecta lenguaje de riesgo y permite mostrar líneas de ayuda.
Es un apoyo heurístico: no diagnostica ni sustituye la valoración de una persona.
"""
import re
import unicodedata

NEGADORES = {"no", "nunca", "jamas", "ni", "nada", "tampoco", "sin"}
IGNORAR = ("vale la pena", "valio la pena", "por ejemplo", "de nada")
NO_NIEGAN = (("no paro de", "sigo"), ("no dejo de", "sigo"), ("no puedo dejar de", "sigo"), ("no puedo parar de", "sigo"))

RIESGO = re.compile(
    r"\bsuicid|quitarme la vida|quitarse la vida|\bquiero morir|me quiero morir|quiero morirme|"
    r"no quiero vivir|ya no quiero vivir|ya no quiero estar aqui|hacerme dano|lastimarme|matarme|"
    r"acabar con todo|acabar con mi vida|mejor muert|no vale la pena vivir|quiero desaparecer|quisiera desaparecer"
)

AYUDA = {
    "mensaje": "No tienes que cargar esto solo/a. Hay personas listas para escucharte ahora.",
    "lineas": [
        {"nombre": "Línea de la Vida", "telefono": "800 911 2000", "detalle": "Gratuita, 24 horas, todos los días"},
        {"nombre": "Emergencias", "telefono": "911", "detalle": "Si estás en peligro inmediato"},
    ],
}
RESPUESTA_RIESGO = (
    "Gracias por escribirlo: lo que sientes importa y no tienes que cargarlo solo/a. "
    "Busca ahora a alguien de confianza que esté cerca, o llama a la Línea de la Vida: 800 911 2000 "
    "(gratuita, 24 horas). Si estás en peligro inmediato, llama al 911."
)

RESP_NEGA_POSITIVO = ("Gracias por ser sincero/a con cómo te sientes. No tiene que ser un buen día "
                      "para que valga la pena registrarlo; si quieres, cuéntame un poco más.")
RESP_NEGA_NEGATIVO = "Qué bueno que no te sientas así. Gracias por registrar tu día."
RESP_YA_NO = "Qué alivio leer que eso ya no pesa como antes. Reconoce tu avance."


def norm(texto):
    t = unicodedata.normalize("NFD", str(texto).lower())
    t = "".join(c for c in t if unicodedata.category(c) != "Mn")
    return " ".join(re.findall(r"[a-z0-9]+", t))


def detectar_riesgo(texto):
    return bool(RIESGO.search(norm(texto)))


def _buscar(tokens, frase):
    n = len(frase)
    for i in range(len(tokens) - n + 1):
        if tokens[i:i + n] == frase:
            return i
    return -1


def analizar(texto, catalogo):
    """catalogo: lista de (emocion, puntuacion, palabras_clave, respuesta); la última es Neutral."""
    t = norm(texto)
    for frase in IGNORAR:
        t = t.replace(frase, " ")
    for viejo, nuevo in NO_NIEGAN:
        t = t.replace(viejo, nuevo)
    tokens = t.split()
    for emocion, punt, kws, resp in catalogo:
        for kw in kws:
            frase = norm(kw).split()
            if not frase:
                continue
            i = _buscar(tokens, frase)
            if i < 0:
                continue
            ventana = tokens[max(0, i - 3):i]
            if any(x in NEGADORES for x in ventana):
                if punt >= 0.3:
                    return "Tristeza", -0.5, RESP_NEGA_POSITIVO
                if punt <= -0.3:
                    if "ya" in ventana:
                        return "Alivio", 0.5, RESP_YA_NO
                    return "Neutral", 0.1, RESP_NEGA_NEGATIVO
            return emocion, punt, resp
    return catalogo[-1][0], catalogo[-1][1], catalogo[-1][3]
