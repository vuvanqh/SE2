const db = require('../db');

function getUserGroups(user_id) {
 const rows = db.prepare(`
    SELECT
      g.term_id,
      c.course_id,
      c.title AS course_name,
      g.group_number,
      g.class_type,
      c.faculty_id AS course_fac_id
    FROM enrollments e
    JOIN usos_course_groups g
      ON g.course_id = e.course_id
     AND g.group_number = e.group_number
     AND g.class_type = e.class_type
     AND g.term_id = e.term_id
    JOIN courses c
      ON c.course_id = g.course_id
    WHERE e.student_id = ?
    ORDER BY g.term_id, c.course_id
  `).all(user_id);

  const grouped = {};

  for (const row of rows) {
    if (!grouped[row.term_id]) grouped[row.term_id] = [];

    grouped[row.term_id].push({
      course_id: row.course_id,
      course_name: row.course_name,
      group_number: row.group_number,
      class_type: row.class_type,
      course_fac_id: row.course_fac_id,
      term_id: row.term_id,
      relationship_type: 'participant'
    });
  }
  return grouped;
}

function getGroupById(course_id, group_number, class_type, term_id) {
   return db.prepare(`
    SELECT
      c.course_id AS course_unit_id,
      g.group_number,
      g.class_type,
      c.course_id,
      c.title,
      c.faculty_id,
      g.term_id
    FROM usos_course_groups g
    JOIN courses c ON c.course_id = g.course_id
    WHERE g.course_id = ?
      AND g.group_number = ?
      AND g.class_type = ?
      AND g.term_id = ?
  `).get(course_id, group_number, class_type, term_id) || null;
}

function getGroupParticipants(course_id, group_number, class_type, term_id) {
   return db.prepare(`
    SELECT s.student_id, s.first_name, s.last_name
    FROM enrollments e
    JOIN students s ON s.student_id = e.student_id
    WHERE e.course_id = ?
      AND e.group_number = ?
      AND e.class_type = ?
      AND e.term_id = ?
    ORDER BY s.last_name, s.first_name
  `).all(course_id, group_number, class_type, term_id);
}

module.exports = { 
    getUserGroups, 
    getGroupById, 
    getGroupParticipants };