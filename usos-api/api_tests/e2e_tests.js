// import assert from 'node:assert/strict';
const assert = require('node:assert/strict');

const BASE_URL = 'http://localhost:3000';

async function request(path, options = {}) {
  const res = await fetch(`${BASE_URL}${path}`, {
    headers: {
      'Content-Type': 'application/json',
      ...(options.headers || {})
    },
    ...options
  });

  const text = await res.text();
  let body;

  try {
    body = text ? JSON.parse(text) : null;
  } catch {
    body = text;
  }

  return { status: res.status, body };
}

function ok(condition, message) {
  assert.equal(condition, true, message);
}

async function run() {
  console.log('1. login: valid user');
  const loginOk = await request('/services/login', {
    method: 'POST',
    body: JSON.stringify({ student_id: '1' })
  });
  // console.log('Actual status:', loginOk.status);
  // console.log('Actual body:', loginOk.body);
  assert.equal(loginOk.status, 200, 'login should return 200');
  ok(typeof loginOk.body?.token === 'string' && loginOk.body.token.length > 0, 'login should return token');

  const token = loginOk.body.token;
  const auth = { Authorization: `Bearer ${token}` };

  console.log('2. login: invalid user');
  const loginBad = await request('/services/login', {
    method: 'POST',
    body: JSON.stringify({ student_id: '9999' })
  });
  assert.equal(loginBad.status, 401, 'invalid login should return 401');

  console.log('3. auth guard: missing token');
  const noAuth = await request('/services/users/1');
  assert.equal(noAuth.status, 401, 'protected endpoint without token should return 401');

  console.log('4. users/:student_id');
  const userById = await request('/services/users/1', { headers: auth });
  assert.equal(userById.status, 200, 'users/:student_id should return 200');
  assert.equal(userById.body.student_id, '1');

  console.log('5. users/faculty/:faculty_id');
  const usersByFaculty = await request('/services/users/faculty/MINI', { headers: auth });
  assert.equal(usersByFaculty.status, 200, 'users/faculty should return 200');
  ok(Array.isArray(usersByFaculty.body), 'users/faculty should return array');

  console.log('6. users/course/:course_id');
  const usersByCourse = await request('/services/users/course/100', { headers: auth });
  // console.log('Returned status:', usersByCourse.status);
  // console.log('Returned body:', usersByCourse.body);
  assert.equal(usersByCourse.status, 200, 'users/course should return 200');
  ok(Array.isArray(usersByCourse.body), 'users/course should return array');

  console.log('7. groups/user');
  const groupsUser = await request('/services/groups/user?user_id=1', { headers: auth });
  assert.equal(groupsUser.status, 200, 'groups/user should return 200');
  assert.equal(groupsUser.body.user_id, '1');

  console.log('8. groups/group');
  const group = await request('/services/groups/group?course_id=100&group_number=1&class_type=Lecture&term_id=2025Z', {
    headers: auth
  });
  assert.equal(group.status, 200, 'groups/group should return 200');
  assert.equal(group.body.course_id, '100');
  assert.equal(group.body.group_number, '1');
  assert.equal(group.body.class_type, 'Lecture');
  assert.equal(group.body.term_id, '2025Z');

  console.log('9. groups/is-participant');
  const isParticipant = await request('/services/groups/is-participant?user_id=1&course_id=100&group_number=1&class_type=Lecture&term_id=2025Z', {
    headers: auth
  });
  assert.equal(isParticipant.status, 200, 'groups/is-participant should return 200');
  assert.equal(typeof isParticipant.body, 'boolean');

  console.log('10. groups/participants');
  const participants = await request('/services/groups/participants?course_id=100&group_number=1&class_type=Lecture&term_id=2025Z', {
    headers: auth
  });
  assert.equal(participants.status, 200, 'groups/participants should return 200');
  ok(Array.isArray(participants.body), 'groups/participants should return array');

  console.log('11. tt/user');
  const ttUser = await request('/services/tt/user?user_id=1&start=2025-10-01&days=30', { headers: auth });
  assert.equal(ttUser.status, 200, 'tt/user should return 200');
  ok(Array.isArray(ttUser.body), 'tt/user should return array');

  console.log('12. tt/room');
  const ttRoom = await request('/services/tt/room?room_id=R1&start=2025-10-01&days=30', { headers: auth });
  assert.equal(ttRoom.status, 200, 'tt/room should return 200');
  ok(Array.isArray(ttRoom.body), 'tt/room should return array');

  console.log('13. tt/course');
  const ttCourse = await request('/services/tt/course?course_id=100&term_id=2025Z&start=2025-10-01&days=30', {
    headers: auth
  });
  assert.equal(ttCourse.status, 200, 'tt/course should return 200');
  ok(Array.isArray(ttCourse.body), 'tt/course should return array');

  console.log('14. faculties');
  const faculties = await request('/services/faculties', { headers: auth });
  assert.equal(faculties.status, 200, 'faculties should return 200');
  ok(Array.isArray(faculties.body), 'faculties should return array');

  console.log('\nAll endpoint tests passed.');
}

run().catch(err => {
  console.error('\nTest failed:');
  console.error(err);
  process.exit(1);
});
