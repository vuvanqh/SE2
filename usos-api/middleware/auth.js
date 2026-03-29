const { getSessionByToken } = require('../data/authData');
const { getStudentById } = require('../data/usersData');

function authMiddleware(req, res, next) {
  const authHeader = req.headers.authorization;

  if (!authHeader) {
    return res.status(401).json({ error: "No authorization header provided" });
  }

  const token = authHeader.split(' ')[1];

  const session = getSessionByToken(token);

  if (!session) {
    return res.status(401).json({ error: "Invalid token" });
  }

  req.student_id = session.student_id;

  const student = getStudentById(session.student_id);

  if (!student) {
    return res.status(401).json({ error: "Student not found" });
  }

  req.student = student;
  req.student_id = student.student_id;

  next();
}

module.exports = authMiddleware;