const db = require('../db');
function getFaculties(){
    return db.prepare(`
    SELECT faculty_id, faculty_name, faculty_code
    FROM faculties
    ORDER BY faculty_id
  `).all();
}
module.exports = {getFaculties};