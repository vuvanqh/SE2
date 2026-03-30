const request = require('supertest');
const app = require('../../app');

jest.mock('../../data/usersData', () => ({
  getStudentById: jest.fn(),
}));

jest.mock('../../data/authData', () => ({
  saveToken: jest.fn(),
}));

const { getStudentById } = require('../../data/usersData');
const { saveToken } = require('../../data/authData');

describe('login routes', () => {
  test('returns 401 for invalid student_id', async () => {
    getStudentById.mockReturnValue(null);

    const res = await request(app)
      .post('/services/login')
      .send({ student_id: '999' });

    expect(res.status).toBe(401);
    expect(res.body).toEqual({ error: 'Invalid student' });
  });

  test('returns token for valid student_id', async () => {
    getStudentById.mockReturnValue({ id: '1', first_name: 'Jan' });

    const res = await request(app)
      .post('/services/login')
      .send({ student_id: '1' });

    expect(res.status).toBe(200);
    expect(res.body.token).toBeDefined();
    expect(saveToken).toHaveBeenCalled();
  });
});