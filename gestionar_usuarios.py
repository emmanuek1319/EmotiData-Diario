"""Administración de cuentas de EmotiData (se ejecuta en TU computadora, nunca en el servidor).

Uso (PowerShell, con la URI de Atlas en la variable de entorno MONGO_URI):
  python gestionar_usuarios.py revisar                 # duplicados y cuentas sin contraseña
  python gestionar_usuarios.py migrar                  # marca a los alumnos existentes como "canalizados" y normaliza correos
  python gestionar_usuarios.py psicologa --correo dulce@tudominio.mx --nombre "Lic. Dulce Contreras"
  python gestionar_usuarios.py password --correo alumno@utvm.edu.mx
  python gestionar_usuarios.py lista
"""
import os
import sys
import argparse
import getpass
import secrets
from collections import Counter

from dotenv import load_dotenv
from pymongo import MongoClient
from werkzeug.security import generate_password_hash

load_dotenv()
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from emoti_auth import validar_password, RE_CORREO  # noqa: E402

URI = os.environ.get("MONGO_URI") or os.environ.get("ATLAS_URI")
if not URI:
    sys.exit("Define MONGO_URI con la dirección de Atlas antes de ejecutar este script.")
db = MongoClient(URI)["diario_inteligente"]


def pedir_password():
    while True:
        p = getpass.getpass("Contraseña (mínimo 8, con letras y números): ")
        err = validar_password(p)
        if err:
            print("  ✗", err)
        elif p != getpass.getpass("Repite la contraseña: "):
            print("  ✗ No coinciden.")
        else:
            return p


def revisar():
    correos = Counter(str(u.get("correo", "")).strip().lower() for u in db.usuarios.find({"correo": {"$type": "string"}}))
    dup = [c for c, n in correos.items() if n > 1]
    print("Correos duplicados:", dup or "ninguno")
    metas = Counter((m["usuario_id"], m.get("mes"), str(m.get("titulo", "")).strip()) for m in db.metas_habitos.find({"estado": "activo"}))
    print("Metas activas repetidas:", [k for k, n in metas.items() if n > 1] or "ninguna")
    sin = [str(u["_id"]) for u in db.usuarios.find({"password_hash": {"$exists": False}})]
    print("Cuentas sin contraseña (no podrán entrar hasta que les asignes una):", sin or "ninguna")
    print("Entradas sin usuario válido:", db.diario_logs.count_documents({"usuario_id": {"$nin": [u["_id"] for u in db.usuarios.find({}, {"_id": 1})]}}))


def migrar():
    n = 0
    for u in db.usuarios.find({"rol": "estudiante", "canalizado": {"$exists": False}, "creado_por": {"$exists": False}, "consentimientos": {"$exists": False}}):
        db.usuarios.update_one({"_id": u["_id"]}, {"$set": {"canalizado": True}})
        n += 1
    for u in db.usuarios.find({"correo": {"$type": "string"}}):
        c = u["correo"].strip().lower()
        if c != u["correo"]:
            db.usuarios.update_one({"_id": u["_id"]}, {"$set": {"correo": c}})
    print(f"✅ {n} alumnos existentes marcados como canalizados (la psicóloga los seguirá viendo).")


def psicologa(a):
    correo = a.correo.strip().lower()
    if not RE_CORREO.match(correo):
        sys.exit("Correo inválido.")
    doc = {"nombre_completo": a.nombre, "correo": correo, "rol": "psicologa", "activo": True, "verificado": True, "password_hash": generate_password_hash(pedir_password())}
    u = db.usuarios.find_one({"correo": correo})
    if u:
        db.usuarios.update_one({"_id": u["_id"]}, {"$set": doc})
        print("✅ Cuenta actualizada:", u["_id"])
    else:
        doc["_id"] = a.id or "USR_PSICOLOGA_" + secrets.token_hex(2).upper()
        db.usuarios.insert_one(doc)
        print("✅ Cuenta creada:", doc["_id"])


def password(a):
    u = db.usuarios.find_one({"correo": a.correo.strip().lower()})
    if not u:
        sys.exit("No existe esa cuenta.")
    db.usuarios.update_one({"_id": u["_id"]}, {"$set": {"password_hash": generate_password_hash(pedir_password()), "verificado": True}})
    print("✅ Contraseña actualizada para", u["_id"])


def lista():
    for u in db.usuarios.find({}, {"correo": 1, "rol": 1, "password_hash": 1, "verificado": 1, "canalizado": 1}):
        c = str(u.get("correo", "—"))
        print(f"{u['_id']:<22} {u.get('rol', '?'):<10} {c[:2]}***{c[c.find('@'):] if '@' in c else ''}  contraseña={'sí' if u.get('password_hash') else 'NO'}  verificado={u.get('verificado', '—')}  canalizado={u.get('canalizado', '—')}")


if __name__ == "__main__":
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    sp = ap.add_subparsers(dest="cmd", required=True)
    sp.add_parser("revisar")
    sp.add_parser("migrar")
    sp.add_parser("lista")
    p1 = sp.add_parser("psicologa")
    p1.add_argument("--correo", required=True)
    p1.add_argument("--nombre", required=True)
    p1.add_argument("--id")
    p2 = sp.add_parser("password")
    p2.add_argument("--correo", required=True)
    a = ap.parse_args()
    {"revisar": revisar, "migrar": migrar, "lista": lista}.get(a.cmd, lambda: {"psicologa": psicologa, "password": password}[a.cmd](a))()
