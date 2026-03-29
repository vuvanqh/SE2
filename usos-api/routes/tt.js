const express = require('express');
const router = express.Router();
const { getUserTimetable, getCourseTimetable, getRoomTimetable } = require('../data/ttData');

// GET /services/tt/user
router.get('/user', (req, res) => {
  const { user_id, start, days } = req.query;

  if (!user_id) {
    return res.status(400).json({ error: 'user_id required' });
  }

  const timetable = getUserTimetable(user_id, start, days);
  res.json(timetable);
});

// GET /services/tt/room
router.get('/room', (req, res) => {
    const { room_id, start, days } = req.query;
    if (!room_id) {
        return res.status(400).json({ error: 'room_id required' });
    }

    const timetable = getRoomTimetable(room_id, start, days);
    res.json(timetable);
});

// GET /services/tt/course
router.get('/course', (req, res) => {
  const { course_id, term_id, start, days } = req.query;
  if (!course_id || !term_id) return res.status(400).json({ error: 'course_id and term_id required' });

  const timetable = getCourseTimetable(course_id, term_id, start, days);
  res.json(timetable);
});

module.exports = router;
