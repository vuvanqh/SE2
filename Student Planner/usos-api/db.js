const Database = require('better-sqlite3');
require('dotenv').config();

const db = new Database(process.env.DB_FILE || './usos.sqlite');

db.pragma('foreign_keys = ON');
db.pragma('journal_mode = WAL');

module.exports = db;