const request = require('supertest');
const app = require('../../app');

jest.mock('../../data/usersData', () => ({
  getStudentByEmail: jest.fn(),
}));

jest.mock('../../data/authData', () => ({
  saveToken: jest.fn(),
}));

const { getStudentById, getStudentByEmail } = require('../../data/usersData');
const { saveToken } = require('../../data/authData');

describe('login routes', () => {
  test('returns 401 for invalid student_id', async () => {
    getStudentByEmail.mockReturnValue(null);

    const res = await request(app)
      .post('/services/login')
      .send({student_email: 'jan@example.com',
      password: 'secret'});

    expect(res.status).toBe(401);
    expect(res.body).toEqual({ error: 'Invalid student' });
  });

  test('returns token for valid student_id', async () => {
    getStudentByEmail.mockReturnValue({ id: '1', first_name: 'Jan', password: 'Jan12345'});
    const res = await request(app)
      .post('/services/login')
      .send({
        student_email: "jan.kowalski@pw.edu.pl",
        password: "Jan12345"
      });
    expect(res.status).toBe(200);
    expect(res.body.token).toBeDefined();
    expect(saveToken).toHaveBeenCalled();
  });
});