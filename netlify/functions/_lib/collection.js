const { ObjectId } = require('mongodb');
const { getDb, reply, HEADERS } = require('./db');

// Filtro para encontrar un documento por su id (texto, o ObjectId de Mongo)
function idFilter(id) {
    const s = String(id);
    const conds = [{ id: s }, { _id: s }];
    if (/^[0-9a-fA-F]{24}$/.test(s)) conds.push({ _id: new ObjectId(s) });
    return { $or: conds };
}

// Crea el manejador de una colección simple (patrocinadores, galería, combates, config...)
//  GET                        -> devuelve todos los documentos
//  POST {action:'save_all'}   -> guarda/actualiza cada elemento (NUNCA borra los que no vengan)
//  POST {action:'delete'}  o  DELETE {id}  -> borra un elemento
function makeCollectionHandler(name) {
    return async (event, context) => {
        context.callbackWaitsForEmptyEventLoop = false;

        if (event.httpMethod === 'OPTIONS') return { statusCode: 200, headers: HEADERS, body: '' };

        try {
            const db = await getDb();
            const col = db.collection(name);

            if (event.httpMethod === 'GET') {
                const docs = await col.find({}).toArray();
                return reply(200, docs.map(d => ({ ...d, id: d.id ? d.id : String(d._id) })));
            }

            let data = {};
            try { data = JSON.parse(event.body || '{}'); } catch (e) { return reply(400, { error: 'JSON inválido' }); }

            if (event.httpMethod === 'DELETE' || (event.httpMethod === 'POST' && data.action === 'delete')) {
                const id = data.id || data._id || (event.queryStringParameters && event.queryStringParameters.id);
                if (!id) return reply(400, { error: 'ID requerido' });
                const r = await col.deleteMany(idFilter(id));
                return reply(200, { ok: true, deletedCount: r.deletedCount });
            }

            if (event.httpMethod === 'POST' && data.action === 'save_all') {
                const items = Array.isArray(data.data) ? data.data : [];
                const ops = [];
                items.forEach(item => {
                    if (!item || typeof item !== 'object') return;
                    const key = item.id || item._id;
                    if (!key) return;
                    const rest = { ...item };
                    delete rest._id;
                    ops.push({
                        updateOne: {
                            filter: { _id: String(key) },
                            update: { $set: { ...rest, id: String(key) } },
                            upsert: true
                        }
                    });
                });
                if (ops.length) await col.bulkWrite(ops);
                return reply(200, { ok: true, saved: ops.length });
            }

            return reply(400, { error: 'Acción no válida' });
        } catch (error) {
            return reply(500, { error: error.message });
        }
    };
}

module.exports = { makeCollectionHandler, idFilter };
