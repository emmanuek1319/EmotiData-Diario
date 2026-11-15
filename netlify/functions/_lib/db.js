const { MongoClient } = require('mongodb');

let cachedClient = null;

const HEADERS = {
    'Access-Control-Allow-Origin': '*',
    'Access-Control-Allow-Headers': 'Content-Type, Authorization',
    'Access-Control-Allow-Methods': 'GET, POST, DELETE, OPTIONS',
    'Content-Type': 'application/json',
    'Cache-Control': 'no-store'
};

// La cadena de conexión SOLO se lee de la variable de entorno de Netlify (nunca va en el código).
async function getDb() {
    const uri = process.env.MONGODB_URI;
    if (!uri) throw new Error('Falta configurar la variable de entorno MONGODB_URI en Netlify.');
    if (!cachedClient) {
        cachedClient = new MongoClient(uri);
        await cachedClient.connect();
    }
    return cachedClient.db(process.env.MONGODB_DB || 'furia_uat');
}

function reply(statusCode, obj) {
    return { statusCode, headers: HEADERS, body: JSON.stringify(obj) };
}

module.exports = { getDb, reply, HEADERS };
