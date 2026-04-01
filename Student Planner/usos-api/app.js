const express = require('express');
const cors = require('cors');

const groupsRoutes = require('./routes/groups');
const ttRoutes = require('./routes/tt');
const usersRoutes = require('./routes/users');
const facultiesRoutes = require('./routes/faculties');
const loginRoutes = require('./routes/login');
const authMiddleware = require('./middleware/auth');
const db = require('./db');
const { initDb } = require('./initDb');
// const {
//   getUserTimetable,
//   getRoomTimetable,
//   getCourseTimetable
// } = require('./data/ttData'); // adjust path
// const {
//   getUserGroups,
//   getGroupById,
//   getGroupParticipants
// } = require('./data/groupsData'); 

initDb();
console.log("STUDENTS:", db.prepare("SELECT * FROM students").all());
// console.log('--- USER 1 ---');
// console.dir(getUserTimetable('1', '2025-10-01', 30), { depth: null });

// console.log('--- USER 2 ---');
// console.dir(getUserTimetable('2', '2026-03-21', 10), { depth: null });

// console.log('--- ROOM R1 ---');
// console.dir(getRoomTimetable('R1', '2025-10-01', 30), { depth: null });

// console.log('--- ROOM R2 ---');
// console.dir(getRoomTimetable('R2', '2026-03-21', 10), { depth: null });

// console.log('--- COURSE 100 / 2025Z ---');
// console.dir(getCourseTimetable('100', '2025Z', '2025-10-01', 30), { depth: null });

// console.log('--- COURSE 101 / 2025L ---');
// console.dir(getCourseTimetable('101', '2025L', '2026-03-21', 10), { depth: null });

// console.log('--- getUserGroups(1) ---');
// const user1Groups = getUserGroups('1');
// console.dir(user1Groups, { depth: null });

// console.log('--- getUserGroups(2) ---');
// const user2Groups = getUserGroups('2');
// console.dir(user2Groups, { depth: null });

// console.log('--- getGroupById(100, 1, Lecture, 2025Z) ---');
// const group1 = getGroupById('100', '1', 'Lecture', '2025Z');
// console.dir(group1, { depth: null });

// console.log('--- getGroupParticipants(100, 1, Lecture, 2025Z) ---');
// const participants1 = getGroupParticipants('100', '1', 'Lecture', '2025Z');
// console.dir(participants1, { depth: null });

// console.log('--- getGroupById(101, 1, Laboratory, 2025Z) ---');
// const group2 = getGroupById('101', '1', 'Laboratory', '2025Z');
// console.dir(group2, { depth: null });

// console.log('--- getGroupParticipants(101, 1, Laboratory, 2025Z) ---');
// const participants2 = getGroupParticipants('101', '1', 'Laboratory', '2025Z');
// console.dir(participants2, { depth: null });


const app = express();
app.use(cors());
app.use(express.json());

app.use('/services/login', loginRoutes);
app.use('/services/users', authMiddleware, usersRoutes);
app.use('/services/groups', authMiddleware, groupsRoutes);
app.use('/services/tt', authMiddleware, ttRoutes);
app.use('/services/faculties', facultiesRoutes);

const PORT = process.env.PORT || 3000;
if (require.main == module){
app.listen(PORT, () => {
    console.log(`API działa na http://localhost:${PORT}`);
});
}
module.exports = app;