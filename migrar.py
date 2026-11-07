import os
from pymongo import MongoClient

print("🔄 Conectando a tu MongoDB local...")
origen = MongoClient("mongodb://localhost:27017/")

# Mostrar las bases de datos locales
nombres_bds = origen.list_database_names()
print(f"📂 Bases de datos locales encontradas: {nombres_bds}")

# Conectar a la base que estamos buscando
db_local = origen["diario_inteligente"]
colecciones = db_local.list_collection_names()
print(f"📑 Colecciones encontradas en 'diario_inteligente': {colecciones}")

if not colecciones:
    print("❌ No hay colecciones. Revisa la lista de arriba y cambia el nombre de la base de datos en el script si es necesario.")
else:
    print("🚀 Conectando a Atlas y migrando...")
    destino = MongoClient(os.environ["ATLAS_URI"])["diario_inteligente"]
    for nombre in colecciones:
        docs = list(db_local[nombre].find())
        if not docs:
            print(f"⏭️  {nombre}: vacía, se omite")
            continue
        if destino[nombre].count_documents({}) > 0:
            print(f"⚠️  {nombre}: ya tiene datos en Atlas, se omite")
            continue
        destino[nombre].insert_many(docs)
        print(f"✅ {nombre}: {len(docs)} docs migrados a Atlas")