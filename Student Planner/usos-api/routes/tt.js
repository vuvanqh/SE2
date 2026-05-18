const express = require('express');
const router = express.Router();
const { getUserTimetable, getCourseTimetable, getRoomTimetable, getEventById } = require('../data/ttData');

// GET /services/tt/user
router.get('/user', (req, res) => {
  const { start, days } = req.query;
  const user_id = req.student_id;
  if (!user_id) {
    return res.status(400).json({ error: 'Unauthorized' });
  }

  console.log(`[USOS-API] GET /services/tt/user: student_id=${user_id}, start=${start}, days=${days}`);
  const timetable = getUserTimetable(user_id, start, days);
  console.log(`[USOS-API] Found ${timetable.length} events for student ${user_id}`);
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

router.get('/event/:id', (req, res) => {

  const user_id = req.student_id;

  if (!user_id) {
    return res.status(401).json({
      error: 'Unauthorized'
    });
  }

  const eventId = Number(req.params.id);

  if (Number.isNaN(eventId)) {
    return res.status(400).json({
      error: 'Invalid event id'
    });
  }

  const event = getEventById(
      eventId,
      user_id
  );

  if (!event) {
    return res.status(404).json({
      error: 'Event not found'
    });
  }

  res.json(event);
});

module.exports = router;


