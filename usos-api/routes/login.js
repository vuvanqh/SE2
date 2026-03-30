const express = require('express');
const router = express.Router();
const { getStudentById,getStudentByEmail } = require('../data/usersData');
const { saveToken } = require('../data/authData');
const crypto = require('crypto');

router.post('/', (req, res) => {
  const { student_email, password} = req.body;

  
  if (!student_email || !password) {
    return res.status(400).json({ error: 'student_email and password are required' });
  }
  
  const student = getStudentByEmail(student_email);
  if (!student) {
    return res.status(401).json({ error: "Invalid student" });
  }
  if(student.password !==password){
    return res.status(401).json({ error: 'Invalid student_id or password' });
  }
  const token = crypto.randomBytes(32).toString('hex');

  saveToken(student.student_id, token);

  res.json({ token });
});

module.exports = router;

