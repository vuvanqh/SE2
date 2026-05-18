const db = require('./db');

function initDb() {
  db.exec(`
    PRAGMA foreign_keys = OFF;
    DROP TABLE IF EXISTS enrollments;
    DROP TABLE IF EXISTS usos_schedule_events;
    DROP TABLE IF EXISTS usos_rooms;
    DROP TABLE IF EXISTS usos_buildings;
    DROP TABLE IF EXISTS usos_course_groups;
    DROP TABLE IF EXISTS courses;
    DROP TABLE IF EXISTS students;
    DROP TABLE IF EXISTS faculties;
    DROP TABLE IF EXISTS terms;
    DROP TABLE IF EXISTS sessions;
    PRAGMA foreign_keys = ON;
  `);

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
      password TEXT NOT NULL,
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

  seed();
  
}

function seed() {
  const insertMany = db.transaction(() => {
    const insertFaculty = db.prepare(`
      INSERT OR IGNORE INTO faculties (faculty_id, faculty_name, faculty_code)
      VALUES (?, ?, ?)
    `);

    const insertStudent = db.prepare(`
      INSERT OR IGNORE INTO students
      (student_id, first_name, last_name, faculty_id, university_email, password, status)
      VALUES (?, ?, ?, ?, ?, ?, ?)
    `);

    const insertCourse = db.prepare(`
      INSERT OR IGNORE INTO courses (course_id, title, faculty_id)
      VALUES (?, ?, ?)
    `);

    const insertTerm = db.prepare(`
      INSERT OR IGNORE INTO terms (term_id, start_date, end_date, is_active)
      VALUES (?, ?, ?, ?)
    `);

    const insertCourseGroup = db.prepare(`
      INSERT OR IGNORE INTO usos_course_groups (course_id, group_number, class_type, term_id)
      VALUES (?, ?, ?, ?)
    `);

    const insertEnrollment = db.prepare(`
      INSERT OR IGNORE INTO enrollments
      (student_id, course_id, group_number, class_type, term_id)
      VALUES (?, ?, ?, ?, ?)
    `);

    const insertBuilding = db.prepare(`
      INSERT OR IGNORE INTO usos_buildings (building_id, building_name)
      VALUES (?, ?)
    `);

    const insertRoom = db.prepare(`
      INSERT OR IGNORE INTO usos_rooms
      (room_id, building_id, room_number, room_type)
      VALUES (?, ?, ?, ?)
    `);

    const insertScheduleEvent = db.prepare(`
      INSERT OR IGNORE INTO usos_schedule_events
      (course_id, group_number, class_type, title, term_id, start_time, end_time, room_id, is_cancelled)
      VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)
    `);

    const faculties = [
      ['MINI', 'Mathematics and Information Science', 'MINI01'],
      ['WEiTI', 'Electronics and IT', 'WEiTI01']
    ];

    const students = [
      ['1', 'Jan', 'Kowalski', 'MINI', 'jan.kowalski@pw.edu.pl', 'Jan12345!', 'ACTIVE'],
      ['2', 'Anna', 'Nowak', 'MINI', 'anna.nowak@pw.edu.pl', 'Anna12345', 'ACTIVE'],
      ['3', 'Piotr', 'Wiśniewski', 'MINI', 'piotr.wisniewski@pw.edu.pl', 'Piotr12345!', 'ACTIVE'],
      ['4', 'Katarzyna', 'Wójcik', 'MINI', 'katarzyna.wojcik@pw.edu.pl', 'Kasia12345!', 'ACTIVE'],
      ['5', 'Michał', 'Kamiński', 'MINI', 'michal.kaminski@pw.edu.pl', 'Michal12345!', 'ACTIVE'],
      ['6', 'Zofia', 'Lewandowska', 'MINI', 'zofia.lewandowska@pw.edu.pl', 'Zofia12345!', 'ACTIVE'],
      ['7', 'Tomasz', 'Zieliński', 'MINI', 'tomasz.zielinski@pw.edu.pl', 'Tomasz12345!', 'ACTIVE'],
      ['8', 'Julia', 'Szymańska', 'MINI', 'julia.szymanska@pw.edu.pl', 'Julia12345!', 'ACTIVE'],
      ['9', 'Paweł', 'Woźniak', 'MINI', 'pawel.wozniak@pw.edu.pl', 'Pawel12345!', 'ACTIVE'],
      ['10', 'Maria', 'Dąbrowska', 'MINI', 'maria.dabrowska@pw.edu.pl', 'Maria12345!', 'ACTIVE'],

      ['11', 'Adam', 'Kozłowski', 'WEiTI', 'adam.kozlowski@pw.edu.pl', 'Adam12345', 'ACTIVE'],
      ['12', 'Natalia', 'Jankowska', 'WEiTI', 'natalia.jankowska@pw.edu.pl', 'Natalia12345!', 'ACTIVE'],
      ['13', 'Krzysztof', 'Mazur', 'WEiTI', 'krzysztof.mazur@pw.edu.pl', 'Krzysztof12345!', 'ACTIVE'],
      ['14', 'Aleksandra', 'Krawczyk', 'WEiTI', 'aleksandra.krawczyk@pw.edu.pl', 'Ala12345!', 'ACTIVE'],
      ['15', 'Mateusz', 'Piotrowski', 'WEiTI', 'mateusz.piotrowski@pw.edu.pl', 'Mateusz12345!', 'ACTIVE'],
      ['16', 'Wiktoria', 'Grabowska', 'WEiTI', 'wiktoria.grabowska@pw.edu.pl', 'Wiktoria12345!', 'ACTIVE'],
      ['17', 'Jakub', 'Pawłowski', 'WEiTI', 'jakub.pawlowski@pw.edu.pl', 'Jakub12345!', 'ACTIVE'],
      ['18', 'Martyna', 'Michalska', 'WEiTI', 'martyna.michalska@pw.edu.pl', 'Martyna12345!', 'ACTIVE'],
      ['19', 'Damian', 'Król', 'WEiTI', 'damian.krol@pw.edu.pl', 'Damian12345', 'ACTIVE'],
      ['20', 'Oliwia', 'Wieczorek', 'WEiTI', 'oliwia.wieczorek@pw.edu.pl', 'Oliwia12345!', 'ACTIVE']
    ];

    const courses = [
      ['100', 'Computer Graphics', 'MINI'],
      ['101', 'Algorithms', 'MINI'],
      ['102', 'Discrete Mathematics', 'MINI'],
      ['103', 'Databases', 'MINI'],

      ['200', 'Digital Circuits', 'WEiTI'],
      ['201', 'Computer Networks', 'WEiTI'],
      ['202', 'Embedded Systems', 'WEiTI'],
      ['203', 'Signal Processing', 'WEiTI']
    ];

    const terms = [
      ['2026Z', '2026-10-01', '2026-02-28', 0],
      ['2026L', '2026-03-01', '2026-09-30', 1]
    ];

    const courseGroups = [
      ['100', '1', 'Lecture', '2026Z'],
      ['100', '1', 'Laboratory', '2026Z'],
      ['101', '1', 'Lecture', '2026Z'],
      ['101', '1', 'Laboratory', '2026Z'],
      ['102', '1', 'Lecture', '2026Z'],
      ['103', '1', 'Lecture', '2026L'],
      ['103', '1', 'Laboratory', '2026L'],

      ['200', '1', 'Lecture', '2026Z'],
      ['200', '1', 'Laboratory', '2026Z'],
      ['201', '1', 'Lecture', '2026L'],
      ['201', '1', 'Laboratory', '2026L'],
      ['202', '1', 'Lecture', '2026L'],
      ['202', '1', 'Laboratory', '2026L'],
      ['203', '1', 'Lecture', '2026Z']
    ];

    const buildings = [
      ['B1', 'Main Building'],
      ['B2', 'Lab Building'],
      ['B3', 'Electronics Center']
    ];

    const rooms = [
      ['R1', 'B1', '105', 'ROOM'],
      ['R2', 'B2', '201', 'LAB'],
      ['R3', 'B1', '210', 'ROOM'],
      ['R4', 'B3', '301', 'LAB']
    ];

    const scheduleEvents = [
      ['100', '1', 'Lecture', 'Computer Graphics - Lecture 1', '2026Z', '2026-10-02 10:00:00', '2026-10-02 12:00:00', 'R1', 0],
      ['101', '1', 'Laboratory', 'Algorithms - Lab 1', '2026Z', '2026-10-03 12:15:00', '2026-10-03 14:00:00', 'R2', 0],
      ['103', '1', 'Lecture', 'Databases - Lecture 1', '2026L', '2026-03-04 08:15:00', '2026-03-04 10:00:00', 'R3', 0],
      ['100', '1', 'Lecture', 'Computer Graphics - Lecture 5', '2026L', '2026-05-02 10:00:00', '2026-05-02 12:00:00', 'R1', 0],
      ['101', '1', 'Laboratory', 'Algorithms - Lab 5', '2026L', '2026-05-03 12:15:00', '2026-05-03 14:00:00', 'R2', 0],
      ['200', '1', 'Lecture', 'Digital Circuits - Lecture 1', '2026Z', '2026-10-06 09:00:00', '2026-10-06 11:00:00', 'R1', 0],
      ['201', '1', 'Laboratory', 'Computer Networks - Lab 1', '2026L', '2026-03-10 14:15:00', '2026-03-10 16:00:00', 'R4', 0],
      ['202', '1', 'Laboratory', 'Embedded Systems - Lab 1', '2026L', '2026-03-12 10:15:00', '2026-03-12 12:00:00', 'R4', 0]
    ];

    faculties.forEach(f => insertFaculty.run(...f));
    students.forEach(s => insertStudent.run(...s));
    courses.forEach(c => insertCourse.run(...c));
    terms.forEach(t => insertTerm.run(...t));
    courseGroups.forEach(g => insertCourseGroup.run(...g));
    buildings.forEach(b => insertBuilding.run(...b));
    rooms.forEach(r => insertRoom.run(...r));
    scheduleEvents.forEach(e => insertScheduleEvent.run(...e));

    const miniEnrollments = {
      '1': [
        ['100', '1', 'Lecture', '2026Z'],
        ['101', '1', 'Lecture', '2026Z'],
        ['101', '1', 'Laboratory', '2026Z'],
        ['100', '1', 'Lecture', '2026L'],
        ['101', '1', 'Laboratory', '2026L']
      ],
      '2': [
        ['100', '1', 'Lecture', '2026Z'],
        ['102', '1', 'Lecture', '2026Z'],
        ['103', '1', 'Lecture', '2026L']
      ],
      '3': [
        ['101', '1', 'Lecture', '2026Z'],
        ['101', '1', 'Laboratory', '2026Z'],
        ['103', '1', 'Laboratory', '2026L']
      ],
      '4': [
        ['100', '1', 'Lecture', '2026Z'],
        ['102', '1', 'Lecture', '2026Z']
      ],
      '5': [
        ['100', '1', 'Laboratory', '2026Z'],
        ['103', '1', 'Lecture', '2026L'],
        ['103', '1', 'Laboratory', '2026L']
      ],
      '6': [
        ['101', '1', 'Lecture', '2026Z'],
        ['102', '1', 'Lecture', '2026Z']
      ],
      '7': [
        ['100', '1', 'Lecture', '2026Z'],
        ['101', '1', 'Laboratory', '2026Z'],
        ['103', '1', 'Lecture', '2026L']
      ],
      '8': [
        ['102', '1', 'Lecture', '2026Z'],
        ['103', '1', 'Laboratory', '2026L']
      ],
      '9': [
        ['100', '1', 'Lecture', '2026Z'],
        ['101', '1', 'Lecture', '2026Z'],
        ['103', '1', 'Lecture', '2026L']
      ],
      '10': [
        ['100', '1', 'Laboratory', '2026Z'],
        ['102', '1', 'Lecture', '2026Z']
      ]
    };

    const weitiEnrollments = {
      '11': [
        ['200', '1', 'Lecture', '2026Z'],
        ['200', '1', 'Laboratory', '2026Z'],
        ['201', '1', 'Lecture', '2026L']
      ],
      '12': [
        ['200', '1', 'Lecture', '2026Z'],
        ['203', '1', 'Lecture', '2026Z']
      ],
      '13': [
        ['201', '1', 'Lecture', '2026L'],
        ['201', '1', 'Laboratory', '2026L'],
        ['202', '1', 'Lecture', '2026L']
      ],
      '14': [
        ['200', '1', 'Laboratory', '2026Z'],
        ['202', '1', 'Laboratory', '2026L']
      ],
      '15': [
        ['203', '1', 'Lecture', '2026Z'],
        ['201', '1', 'Lecture', '2026L']
      ],
      '16': [
        ['200', '1', 'Lecture', '2026Z'],
        ['202', '1', 'Lecture', '2026L'],
        ['202', '1', 'Laboratory', '2026L']
      ],
      '17': [
        ['201', '1', 'Laboratory', '2026L'],
        ['202', '1', 'Lecture', '2026L']
      ],
      '18': [
        ['200', '1', 'Lecture', '2026Z'],
        ['203', '1', 'Lecture', '2026Z'],
        ['201', '1', 'Lecture', '2026L']
      ],
      '19': [
        ['202', '1', 'Laboratory', '2026L'],
        ['201', '1', 'Lecture', '2026L']
      ],
      '20': [
        ['200', '1', 'Lecture', '2026Z'],
        ['201', '1', 'Laboratory', '2026L'],
        ['203', '1', 'Lecture', '2026Z']
      ]
    };

    [miniEnrollments, weitiEnrollments].forEach(group => {
      Object.entries(group).forEach(([studentId, list]) => {
        list.forEach(([courseId, groupNumber, classType, termId]) => {
          insertEnrollment.run(studentId, courseId, groupNumber, classType, termId);
        });
      });
    });
  });

  insertMany();
}

module.exports = { initDb };