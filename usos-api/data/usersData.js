const db = require('../db');

function getStudentById(student_id) {
  return db.prepare(`
    SELECT student_id, first_name, last_name, faculty_id, university_email, status
    FROM students
    WHERE student_id = ?
    `).get(student_id) || null;
}

function getStudentsByFaculty(faculty_id) {
  return db.prepare(`
    SELECT student_id, first_name, last_name, faculty_id, university_email, status
    FROM students
    WHERE faculty_id = ?
    ORDER BY last_name, first_name
  `).all(faculty_id);
}

function getStudentsByCourse(course_id) {
   return db.prepare(`
    SELECT s.student_id, s.first_name, s.last_name, s.faculty_id, s.university_email, s.status
    FROM students s
    JOIN enrollments e ON e.student_id = s.student_id
    WHERE e.course_id = ?
    ORDER BY s.last_name, s.first_name
  `).all(course_id);
}

module.exports = {
  getStudentById,
  getStudentsByFaculty,
  getStudentsByCourse
};