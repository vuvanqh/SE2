const express = require('express');
const router = express.Router();

const authMiddleware = require('../middleware/auth');

const {
  getStudentById,
  getStudentsByFaculty,
  getStudentsByCourse
} = require('../data/usersData');

// GET /services/users/faculty/:faculty_id
router.get('/faculty/:faculty_id', authMiddleware, (req, res) => {
  const students = getStudentsByFaculty(req.params.faculty_id);
  res.json(students);
});

// GET /services/users/me
router.get('/me', authMiddleware, (req, res) => {
  res.json(req.student);
});

// GET /services/users/course/:course_id
router.get('/course/:course_id',authMiddleware, (req, res) => {
  const students = getStudentsByCourse(req.params.course_id);
  res.json(students);
});

// GET /services/users/:student_id
router.get('/:student_id',authMiddleware, (req, res) => {
  const student = getStudentById(req.params.student_id);
  if (!student) return res.status(404).json({ error: "Student not found" });
  res.json(student);
});

module.exports = router;
