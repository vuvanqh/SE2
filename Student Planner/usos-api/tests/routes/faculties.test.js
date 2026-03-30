const request = require('supertest');
const app = require('../../app');

jest.mock('../../data/authData', () => ({
  getSessionByToken: jest.fn(),
}));

jest.mock('../../data/usersData', () => ({
  getStudentById: jest.fn(),
}));

jest.mock('../../data/facultiesData', () => ({
  getFaculties: jest.fn(),
}));

const { getSessionByToken } = require('../../data/authData');
const { getStudentById } = require('../../data/usersData');
const { getFaculties } = require('../../data/facultiesData');

describe('faculties routes', () => {
  test('requires authentication', async () => {
    const res = await request(app).get('/services/faculties');

    expect(res.status).toBe(401);
  });

  test('returns faculties list', async () => {
    getSessionByToken.mockReturnValue({ student_id: '1' });
    getStudentById.mockReturnValue({ id: '1', first_name: 'Jan' });
    getFaculties.mockReturnValue([{ faculty_id: 'W1', name: 'Math' }]);

    const res = await request(app)
      .get('/services/faculties')
      .set('Authorization', 'Bearer validtoken');

    expect(res.status).toBe(200);
    expect(res.body).toEqual([{ faculty_id: 'W1', name: 'Math' }]);
  });
});