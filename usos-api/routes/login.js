const express = require('express');
const router = express.Router();
const { getStudentById } = require('../data/usersData');
const { saveToken } = require('../data/authData');
const crypto = require('crypto');

router.post('/', (req, res) => {
  const { student_id } = req.body;

  const student = getStudentById(student_id);

  if (!student) {
    return res.status(401).json({ error: "Invalid student" });
  }

  const token = crypto.randomBytes(32).toString('hex');

  saveToken(student_id, token);

  res.json({ token });
});

module.exports = router;

