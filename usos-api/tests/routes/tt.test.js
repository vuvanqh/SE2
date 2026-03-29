const request = require('supertest');
const app = require('../../app');

jest.mock('../../data/authData', () => ({
  getSessionByToken: jest.fn(),
}));

jest.mock('../../data/usersData', () => ({
  getStudentById: jest.fn(),
}));

jest.mock('../../data/ttData', () => ({
  getUserTimetable: jest.fn(),
  getRoomTimetable: jest.fn(),
  getCourseTimetable: jest.fn(),
}));

const { getSessionByToken } = require('../../data/authData');
const { getStudentById } = require('../../data/usersData');
const { getUserTimetable, getRoomTimetable, getCourseTimetable } = require('../../data/ttData');

beforeEach(() => {
  getSessionByToken.mockReturnValue({ student_id: '1' });
  getStudentById.mockReturnValue({ id: '1' });
});

describe('tt routes', () => {
  test('GET /user returns 400 when user_id is missing', async () => {
    const res = await request(app)
      .get('/services/tt/user')
      .set('Authorization', 'Bearer token');

    expect(res.status).toBe(400);
  });

  test('GET /user returns timetable', async () => {
    getUserTimetable.mockReturnValue([{ course_id: 'INF101' }]);

    const res = await request(app)
      .get('/services/tt/user?user_id=1')
      .set('Authorization', 'Bearer token');

    expect(res.status).toBe(200);
    expect(res.body).toEqual([{ course_id: 'INF101' }]);
  });

  test('GET /room returns 400 when room_id is missing', async () => {
    const res = await request(app)
      .get('/services/tt/room')
      .set('Authorization', 'Bearer token');

    expect(res.status).toBe(400);
  });

  test('GET /room returns room timetable', async () => {
    getRoomTimetable.mockReturnValue([{ room_id: 'A1' }]);

    const res = await request(app)
      .get('/services/tt/room?room_id=A1')
      .set('Authorization', 'Bearer token');

    expect(res.status).toBe(200);
    expect(res.body).toEqual([{ room_id: 'A1' }]);
  });

  test('GET /course returns 400 when params are missing', async () => {
    const res = await request(app)
      .get('/services/tt/course?course_id=INF101')
      .set('Authorization', 'Bearer token');

    expect(res.status).toBe(400);
  });

  test('GET /course returns course timetable', async () => {
    getCourseTimetable.mockReturnValue([{ course_id: 'INF101', term_id: '2025L' }]);

    const res = await request(app)
      .get('/services/tt/course?course_id=INF101&term_id=2025L')
      .set('Authorization', 'Bearer token');

    expect(res.status).toBe(200);
    expect(res.body).toEqual([{ course_id: 'INF101', term_id: '2025L' }]);
  });
});