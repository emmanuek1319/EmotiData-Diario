const { ObjectId } = require('mongodb');
const bcrypt = require('bcryptjs');
const { getDb, reply, HEADERS } = require('./_lib/db');

const MAX_TEAMS = 40;
const EDITABLE = ['name', 'category', 'categories', 'payments', 'school', 'members', 'champion', 'phone', 'isStudent'];

function idFilter(id) {
    const s = String(id);
    const conds = [{ id: s }, { _id: s }];
    if (/^[0-9a-fA-F]{24}$/.test(s)) conds.push({ _id: new ObjectId(s) });
    return { $or: conds };
}

// Nunca se envían las contraseñas (hash) al navegador
function publicTeam(t) {
    const copy = { ...t };
    delete copy.passwordHash;
    delete copy.password;
    return copy;
}

function hasName(t) {
    const n = t.name ? t.name : t.nom_;
    return !!(n && String(n).trim() !== '');
}

exports.handler = async (event, context) => {
    context.callbackWaitsForEmptyEventLoop = false;

    if (event.httpMethod === 'OPTIONS') return { statusCode: 200, headers: HEADERS, body: '' };

    try {
        const db = await getDb();
        const teams = db.collection('teams');

        // 1. GET: todos los equipos (sin contraseñas)
        if (event.httpMethod === 'GET') {
            const list = await teams.find({}).toArray();
            return reply(200, list.map(publicTeam));
        }

        // 2. DELETE: borrar equipo por id
        if (event.httpMethod === 'DELETE') {
            let data = {};
            try { data = JSON.parse(event.body || '{}'); } catch (e) { }
            const teamId = data.id || data._id || (event.queryStringParameters && event.queryStringParameters.id);
            if (!teamId) return reply(400, { error: 'ID de equipo requerido' });
            const r = await teams.deleteMany(idFilter(teamId));
            return reply(200, { ok: true, deletedCount: r.deletedCount });
        }

        if (event.httpMethod === 'POST') {
            let data;
            try { data = JSON.parse(event.body || '{}'); } catch (e) { return reply(400, { error: 'JSON inválido' }); }
            const action = data.action;

            // LOGIN
            if (action === 'login') {
                const uname = (data.username || '').trim();
                if (!uname || !data.password) return reply(401, { error: 'Usuario o contraseña faltante' });
                const team = await teams.findOne({
                    $or: [{ ownerUsername: uname }, { username: uname }, { nombreusua_: uname }]
                });
                if (!team) return reply(401, { error: 'Usuario no encontrado' });
                const hash = team.passwordHash || team.password || '';
                let valid = false;
                try { valid = await bcrypt.compare(String(data.password), hash); } catch (e) { valid = false; }
                if (!valid) return reply(401, { error: 'Contraseña incorrecta' });
                return reply(200, {
                    ok: true,
                    team: {
                        id: String(team._id),
                        name: team.name || team.nom_,
                        category: team.category || team.categor_,
                        ownerUsername: team.ownerUsername || team.username || team.nombreusua_,
                        school: team.school || team.escue_,
                        members: team.members || team.miembr_,
                        champion: team.champion || false
                    }
                });
            }

            // GUARDAR CAMBIOS (campeón, nombre, etc.). Solo actualiza equipos que ya existen.
            if (action === 'save_all') {
                const items = Array.isArray(data.data) ? data.data : [];
                let updated = 0;
                for (const item of items) {
                    if (!item) continue;
                    const key = item.id || item._id;
                    if (!key) continue;
                    const set = {};
                    EDITABLE.forEach(f => { if (item[f] !== undefined) set[f] = item[f]; });
                    if (!Object.keys(set).length) continue;
                    const r = await teams.updateOne(idFilter(key), { $set: set });
                    if (r && r.matchedCount) updated += 1;
                }
                return reply(200, { ok: true, updated });
            }

            // BORRAR (alternativa por POST)
            if (action === 'delete') {
                const id = data.id || data._id;
                if (!id) return reply(400, { error: 'ID de equipo requerido' });
                const r = await teams.deleteMany(idFilter(id));
                return reply(200, { ok: true, deletedCount: r.deletedCount });
            }

            // LIMPIAR equipos "fantasma" (sin nombre) que se hayan creado por error
            if (action === 'cleanup_ghosts') {
                const all = await teams.find({}).toArray();
                const ghosts = all.filter(t => !hasName(t));
                for (const g of ghosts) await teams.deleteOne({ _id: g._id });
                return reply(200, { ok: true, removed: ghosts.length });
            }

            if (action && action !== 'register') return reply(400, { error: 'Acción no válida' });

            // REGISTRO de equipo nuevo
            const cleanUsername = (data.username || data.ownerUsername || '').trim();
            const cleanTeamName = (data.name || '').trim();
            if (!cleanTeamName || !cleanUsername || !data.password) {
                return reply(400, { error: 'Faltan el nombre del equipo, el usuario o la contraseña.' });
            }

            const all = await teams.find({}).toArray();
            if (all.filter(hasName).length >= MAX_TEAMS) {
                return reply(400, { error: 'Cupo máximo alcanzado (' + MAX_TEAMS + ' equipos).' });
            }

            const existing = await teams.findOne({
                $or: [
                    { ownerUsername: cleanUsername }, { username: cleanUsername }, { nombreusua_: cleanUsername },
                    { name: cleanTeamName }, { nom_: cleanTeamName }
                ]
            });
            if (existing) return reply(400, { error: 'El nombre de equipo o usuario ya está registrado.' });

            const salt = await bcrypt.genSalt(10);
            const passwordHash = await bcrypt.hash(String(data.password), salt);

            const newTeam = {
                name: cleanTeamName,
                ownerUsername: cleanUsername,
                username: cleanUsername,
                phone: data.phone || '',
                passwordHash: passwordHash,
                members: Array.isArray(data.members) ? data.members : (data.members ? String(data.members).split(',').map(m => m.trim()) : []),
                category: data.category || '',
                categories: data.category ? [data.category] : [],
                school: data.school || '',
                isStudent: Boolean(data.isStudent),
                champion: false,
                payments: (data.payments && typeof data.payments === 'object') ? data.payments : {},
                createdAt: Date.now()
            };
            const result = await teams.insertOne(newTeam);
            return reply(201, { ok: true, id: String(result.insertedId), username: cleanUsername });
        }

        return reply(405, { error: 'Method Not Allowed' });
    } catch (error) {
        return reply(500, { error: error.message });
    }
};
