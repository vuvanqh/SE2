const db = require('../db');

function parseDate(value) {
  return new Date(value.replace(' ', 'T'));
}
function filterByDateRange(rows, start, days) {
  const startDate = start ? new Date(start) : new Date();
  const endDate = new Date(startDate);
  endDate.setDate(startDate.getDate() + (days ? Number(days) : 7));

  return rows.filter(row => {
    const rowDate = parseDate(row.start_time);
    return rowDate >= startDate && rowDate < endDate;
  });
}
function mapTimetableRow(row) {
  return {
    title: row.title,
    start_time: row.start_time,
    end_time: row.end_time,
    course_id: row.course_id,
    class_type: row.class_type,
    group_number: row.group_number,
    building_id: row.building_id || null,
    building_name: row.building_name || '',
    room_number: row.room_number || '',
    room_id: row.room_id || null
  };
}
function getUserTimetable(user_id, start, days) {
  const rows = db.prepare(`
    SELECT
      se.title,
      se.start_time,
      se.end_time,
      se.course_id,
      se.class_type,
      se.group_number,
      r.room_id,
      r.room_number,
      b.building_id,
      b.building_name
    FROM enrollments e
    JOIN usos_schedule_events se
      ON se.course_id = e.course_id
     AND se.group_number = e.group_number
     AND se.class_type = e.class_type
     AND se.term_id = e.term_id
    LEFT JOIN usos_rooms r ON r.room_id = se.room_id
    LEFT JOIN usos_buildings b ON b.building_id = r.building_id
    WHERE e.student_id = ?
    ORDER BY se.start_time
  `).all(user_id);

  return filterByDateRange(rows, start, days).map(mapTimetableRow);
}

function getRoomTimetable(room_id, start, days) {
 const rows = db.prepare(`
    SELECT
      se.title,
      se.start_time,
      se.end_time,
      se.course_id,
      se.class_type,
      se.group_number,
      r.room_id,
      r.room_number,
      b.building_id,
      b.building_name
    FROM usos_schedule_events se
    LEFT JOIN usos_rooms r ON r.room_id = se.room_id
    LEFT JOIN usos_buildings b ON b.building_id = r.building_id
    WHERE se.room_id = ?
    ORDER BY se.start_time
  `).all(room_id);

  return filterByDateRange(rows, start, days).map(mapTimetableRow);
}

function getCourseTimetable(course_id, term_id, start, days) {
   const rows = db.prepare(`
    SELECT
      se.title,
      se.start_time,
      se.end_time,
      se.course_id,
      se.class_type,
      se.group_number,
      r.room_id,
      r.room_number,
      b.building_id,
      b.building_name
    FROM usos_schedule_events se
    LEFT JOIN usos_rooms r ON r.room_id = se.room_id
    LEFT JOIN usos_buildings b ON b.building_id = r.building_id
    WHERE se.course_id = ?
      AND se.term_id = ?
    ORDER BY se.start_time
  `).all(course_id, term_id);

  return filterByDateRange(rows, start, days).map(mapTimetableRow);
}

module.exports = {
  getUserTimetable,
  getRoomTimetable,
  getCourseTimetable
};