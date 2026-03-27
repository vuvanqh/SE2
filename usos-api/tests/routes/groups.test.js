const request = require('supertest');

jest.mock('../../data/authData', () => ({
  getSessionByToken: jest.fn(),
}));

jest.mock('../../data/usersData', () => ({
  getStudentById: jest.fn(),
}));

jest.mock('../../data/groupsData', () => ({
  getUserGroups: jest.fn(),
  getGroupById: jest.fn(),
  getGroupParticipants: jest.fn(),
}));

const { getSessionByToken } = require('../../data/authData');
const { getStudentById } = require('../../data/usersData');
const {
  getUserGroups,
  getGroupById,
  getGroupParticipants,
} = require('../../data/groupsData');

const app = require('../../app');

describe('groups routes', () => {
  beforeEach(() => {
    jest.clearAllMocks();

    getSessionByToken.mockReturnValue({ student_id: '1' });
    getStudentById.mockReturnValue({
      student_id: '1',
      first_name: 'Jan',
      last_name: 'Kowalski',
    });
  });

  test('GET /services/groups/user returns 400 when user_id is missing', async () => {
    const res = await request(app)
      .get('/services/groups/user')
      .set('Authorization', 'Bearer validtoken');

    expect(res.status).toBe(400);
    expect(res.body).toEqual({ error: 'user_id is required' });
  });

  test('GET /services/groups/user returns grouped user groups with terms', async () => {
    const mockedGroups = {
      '2025Z': [
        { course_id: '100', group_number: '1', class_type: 'Lecture' },
      ],
      '2025L': [
        { course_id: '101', group_number: '2', class_type: 'Laboratory' },
      ],
    };

    getUserGroups.mockReturnValue(mockedGroups);

    const res = await request(app)
      .get('/services/groups/user?user_id=1')
      .set('Authorization', 'Bearer validtoken');

    expect(res.status).toBe(200);
    expect(res.body).toEqual({
      user_id: '1',
      groups: mockedGroups,
      terms: ['2025Z', '2025L'],
    });
    expect(getUserGroups).toHaveBeenCalledWith('1');
  });

  test('GET /services/groups/group returns 400 when params are missing', async () => {
    const res = await request(app)
      .get('/services/groups/group?course_id=100')
      .set('Authorization', 'Bearer validtoken');

    expect(res.status).toBe(400);
    expect(res.body).toEqual({ error: 'missing params' });
  });

  test('GET /services/groups/group returns 404 when group is not found', async () => {
    getGroupById.mockReturnValue(null);

    const res = await request(app)
      .get('/services/groups/group')
      .query({
        course_id: '100',
        group_number: '1',
        class_type: 'Lecture',
        term_id: '2025Z',
      })
      .set('Authorization', 'Bearer validtoken');

    expect(res.status).toBe(404);
    expect(res.body).toEqual({ error: 'Group not found' });
    expect(getGroupById).toHaveBeenCalledWith('100', '1', 'Lecture', '2025Z');
  });

  test('GET /services/groups/participants returns 400 when params are missing', async () => {
    const res = await request(app)
      .get('/services/groups/participants?course_id=100')
      .set('Authorization', 'Bearer validtoken');

    expect(res.status).toBe(400);
    expect(res.body).toEqual({ error: 'missing params' });
  });

  test('GET /services/groups/participants returns participants list', async () => {
    const participants = [
      { student_id: '1', first_name: 'Jan' },
      { student_id: '2', first_name: 'Anna' },
    ];

    getGroupParticipants.mockReturnValue(participants);

    const res = await request(app)
      .get('/services/groups/participants')
      .query({
        course_id: '100',
        group_number: '1',
        class_type: 'Lecture',
        term_id: '2025Z',
      })
      .set('Authorization', 'Bearer validtoken');

    expect(res.status).toBe(200);
    expect(res.body).toEqual(participants);
    expect(getGroupParticipants).toHaveBeenCalledWith('100', '1', 'Lecture', '2025Z');
  });

  test('GET /services/groups/is-participant returns true when user belongs to group', async () => {
    getGroupParticipants.mockReturnValue([
      { student_id: '1' },
      { student_id: '2' },
    ]);

    const res = await request(app)
      .get('/services/groups/is-participant')
      .query({
        user_id: '1',
        course_id: '100',
        group_number: '1',
        class_type: 'Lecture',
        term_id: '2025Z',
      })
      .set('Authorization', 'Bearer validtoken');

    expect(res.status).toBe(200);
    expect(res.body).toBe(true);
  });

  test('GET /services/groups/is-participant returns false when user does not belong to group', async () => {
    getGroupParticipants.mockReturnValue([
      { student_id: '2' },
      { student_id: '3' },
    ]);

    const res = await request(app)
      .get('/services/groups/is-participant')
      .query({
        user_id: '1',
        course_id: '100',
        group_number: '1',
        class_type: 'Lecture',
        term_id: '2025Z',
      })
      .set('Authorization', 'Bearer validtoken');

    expect(res.status).toBe(200);
    expect(res.body).toBe(false);
  });
});