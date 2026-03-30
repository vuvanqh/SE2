const db = require('../db');

function saveToken(student_id, token) {
  db.prepare(`
    INSERT INTO sessions(student_id, token)
    VALUES (?,?)
    `).run(student_id, token);
}

function getSessionByToken(token) {
  return db.prepare(`
    SELECT student_id, token, created_at
    FROM sessions
    WHERE token = ?
    `).get(token) || null;
}

module.exports = {
  saveToken,
  getSessionByToken
};