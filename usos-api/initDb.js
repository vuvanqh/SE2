const db = require('./db');

function initDb() {
  db.exec(`
    CREATE TABLE IF NOT EXISTS faculties (
      faculty_id TEXT PRIMARY KEY,
      faculty_name TEXT NOT NULL,
      faculty_code TEXT NOT NULL
    );

    CREATE TABLE IF NOT EXISTS students (
      student_id TEXT PRIMARY KEY,
      first_name TEXT NOT NULL,
      last_name TEXT NOT NULL,
      faculty_id TEXT NOT NULL,
      university_email TEXT NOT NULL UNIQUE,
      status TEXT NOT NULL,
      FOREIGN KEY (faculty_id) REFERENCES faculties(faculty_id)
    );

    CREATE TABLE IF NOT EXISTS courses (
      course_id TEXT PRIMARY KEY,
      title TEXT NOT NULL,
      faculty_id TEXT NOT NULL,
      FOREIGN KEY (faculty_id) REFERENCES faculties(faculty_id)
    );

    CREATE TABLE IF NOT EXISTS terms (
      term_id TEXT PRIMARY KEY,
      start_date TEXT NOT NULL,
      end_date TEXT NOT NULL,
      is_active INTEGER NOT NULL CHECK (is_active IN (0, 1))
    );

    CREATE TABLE IF NOT EXISTS usos_course_groups (
      group_id INTEGER PRIMARY KEY AUTOINCREMENT,
      course_id TEXT NOT NULL,
      group_number TEXT NOT NULL,
      class_type TEXT NOT NULL,
      term_id TEXT NOT NULL,
      UNIQUE(course_id, group_number, class_type, term_id),
      FOREIGN KEY (course_id) REFERENCES courses(course_id),
      FOREIGN KEY (term_id) REFERENCES terms(term_id)
    );

    CREATE TABLE IF NOT EXISTS enrollments (
      id INTEGER PRIMARY KEY AUTOINCREMENT,
      student_id TEXT NOT NULL,
      course_id TEXT NOT NULL,
      group_number TEXT NOT NULL,
      class_type TEXT NOT NULL,
      term_id TEXT NOT NULL,
      UNIQUE(student_id, course_id, group_number, class_type, term_id),
      FOREIGN KEY (student_id) REFERENCES students(student_id),
      FOREIGN KEY (course_id) REFERENCES courses(course_id),
      FOREIGN KEY (term_id) REFERENCES terms(term_id)
    );

    CREATE TABLE IF NOT EXISTS usos_buildings (
      building_id TEXT PRIMARY KEY,
      building_name TEXT NOT NULL
    );

    CREATE TABLE IF NOT EXISTS usos_rooms (
      room_id TEXT PRIMARY KEY,
      building_id TEXT NOT NULL,
      room_number TEXT NOT NULL,
      room_type TEXT NOT NULL,
      FOREIGN KEY (building_id) REFERENCES usos_buildings(building_id)
    );

    CREATE TABLE IF NOT EXISTS usos_schedule_events (
      usos_event_id INTEGER PRIMARY KEY AUTOINCREMENT,
      course_id TEXT NOT NULL,
      group_number TEXT NOT NULL,
      class_type TEXT NOT NULL,
      term_id TEXT NOT NULL,
      title TEXT NOT NULL,
      start_time TEXT NOT NULL,
      end_time TEXT NOT NULL,
      room_id TEXT,
      is_cancelled INTEGER NOT NULL DEFAULT 0 CHECK (is_cancelled IN (0, 1)),
      FOREIGN KEY (course_id) REFERENCES courses(course_id),
      FOREIGN KEY (room_id) REFERENCES usos_rooms(room_id),
      FOREIGN KEY (term_id) REFERENCES terms(term_id)
    );

    CREATE TABLE IF NOT EXISTS sessions (
      id INTEGER PRIMARY KEY AUTOINCREMENT,
      student_id TEXT NOT NULL,
      token TEXT NOT NULL UNIQUE,
      created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
      FOREIGN KEY (student_id) REFERENCES students(student_id)
    );
  `);

  const { count } = db.prepare('SELECT COUNT(*) AS count FROM faculties').get();
  if (count === 0) {
    seed();
  }
}

function seed() {
  const insertMany = db.transaction(() => {
    db.prepare(`
      INSERT OR IGNORE INTO faculties (faculty_id, faculty_name, faculty_code)
      VALUES (?, ?, ?)
    `).run('MINI', 'Mathematics and Information Science', 'MINI01');

    db.prepare(`
      INSERT OR IGNORE INTO faculties (faculty_id, faculty_name, faculty_code)
      VALUES (?, ?, ?)
    `).run('WEiTI', 'Electronics and IT', 'WEiTI01');

    db.prepare(`
      INSERT OR IGNORE INTO students
      (student_id, first_name, last_name, faculty_id, university_email, status)
      VALUES (?, ?, ?, ?, ?, ?)
    `).run('1', 'Jan', 'Kowalski', 'MINI', 'jan.kowalski@pw.edu.pl', 'ACTIVE');

    db.prepare(`
      INSERT OR IGNORE INTO students
      (student_id, first_name, last_name, faculty_id, university_email, status)
      VALUES (?, ?, ?, ?, ?, ?)
    `).run('2', 'Anna', 'Nowak', 'MINI', 'anna.nowak@pw.edu.pl', 'ACTIVE');

    db.prepare(`
      INSERT OR IGNORE INTO courses (course_id, title, faculty_id)
      VALUES (?, ?, ?)
    `).run('100', 'Computer Graphics', 'MINI');

    db.prepare(`
      INSERT OR IGNORE INTO courses (course_id, title, faculty_id)
      VALUES (?, ?, ?)
    `).run('101', 'Algorithms', 'MINI');

    db.prepare(`
      INSERT OR IGNORE INTO terms (term_id, start_date, end_date, is_active)
      VALUES (?, ?, ?, ?)
    `).run('2025Z', '2025-10-01', '2026-02-28', 0);

    db.prepare(`
      INSERT OR IGNORE INTO terms (term_id, start_date, end_date, is_active)
      VALUES (?, ?, ?, ?)
    `).run('2025L', '2026-03-01', '2026-09-30', 1);

    db.prepare(`
      INSERT OR IGNORE INTO usos_course_groups (group_number, course_id, class_type, term_id)
      VALUES (?, ?, ?, ?)
    `).run('1', '100', 'Lecture', '2025Z');

    db.prepare(`
      INSERT OR IGNORE INTO usos_course_groups (group_number, course_id, class_type, term_id)
      VALUES (?, ?, ?, ?)
    `).run('1', '101', 'Laboratory', '2025L');

    db.prepare(`
      INSERT OR IGNORE INTO enrollments
      (student_id, course_id, group_number, class_type, term_id)
      VALUES (?, ?, ?, ?, ?)
    `).run('1', '100', '1', 'Lecture', '2025Z');

    db.prepare(`
      INSERT OR IGNORE INTO enrollments
      (student_id, course_id, group_number, class_type, term_id)
      VALUES (?, ?, ?, ?, ?)
    `).run('2', '100', '1', 'Lecture', '2025Z');

    db.prepare(`
      INSERT OR IGNORE INTO enrollments
      (student_id, course_id, group_number, class_type, term_id)
      VALUES (?, ?, ?, ?, ?)
    `).run('2', '101', '1', 'Laboratory', '2025L');

    db.prepare(`
      INSERT OR IGNORE INTO usos_buildings
      (building_id, building_name)
      VALUES (?, ?)
    `).run('B1', 'Main Building');

    db.prepare(`
      INSERT OR IGNORE INTO usos_buildings
      (building_id, building_name)
      VALUES (?, ?)
    `).run('B2','Lab Building');

    db.prepare(`
      INSERT OR IGNORE INTO usos_rooms
      (room_id, building_id, room_number, room_type)
      VALUES (?, ?, ?, ?)
    `).run('R1', 'B1', '105', 'ROOM');

    db.prepare(`
      INSERT OR IGNORE INTO usos_rooms
      (room_id, building_id, room_number, room_type)
      VALUES (?, ?, ?, ?)
    `).run('R2', 'B2', '201', 'LAB');

    db.prepare(`
      INSERT OR IGNORE INTO usos_schedule_events
      (course_id, group_number, class_type, title, term_id, start_time, end_time, room_id, is_cancelled)
      VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)
    `).run(
      '100',
      '1',
      'Lecture',
      'Computer Graphics 1',
      '2025Z',
      '2025-10-01 10:00:00',
      '2025-10-01 12:00:00',
      'R1',
      0
    );

    db.prepare(`
      INSERT OR IGNORE INTO usos_schedule_events
      (course_id, group_number, class_type, title, term_id, start_time, end_time, room_id, is_cancelled)
      VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)
    `).run(
      '101',
      '1',
      'Laboratory',
      'Algorithms Lab 1',
      '2025L',
      '2026-03-22 14:00:00',
      '2026-03-22 16:00:00',
      'R2',
      0
    );
  });

  insertMany();
}

module.exports = { initDb };