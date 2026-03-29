const request = require('supertest');

jest.mock('../../data/authData', () => ({
  getSessionByToken: jest.fn(),
}));

jest.mock('../../data/usersData', () => ({
  getStudentById: jest.fn(),
  getStudentsByFaculty: jest.fn(),
  getStudentsByCourse: jest.fn(),
}));

const { getSessionByToken } = require('../../data/authData');
const {
  getStudentById,
  getStudentsByFaculty,
  getStudentsByCourse,
} = require('../../data/usersData');

const app = require('../../app');

describe('users routes', () => {
  beforeEach(() => {
    jest.clearAllMocks();

    getSessionByToken.mockReturnValue({ student_id: '1' });
    getStudentById.mockImplementation((id) => {
      if (id === '1') {
        return {
          student_id: '1',
          first_name: 'Jan',
          last_name: 'Kowalski',
        };
      }
      return null;
    });
  });

  test('GET /services/users/faculty/:faculty_id returns students for faculty', async () => {
    const students = [
      { student_id: '1', first_name: 'Jan' },
      { student_id: '2', first_name: 'Anna' },
    ];

    getStudentsByFaculty.mockReturnValue(students);

    const res = await request(app)
      .get('/services/users/faculty/WI')
      .set('Authorization', 'Bearer validtoken');

    expect(res.status).toBe(200);
    expect(res.body).toEqual(students);
    expect(getStudentsByFaculty).toHaveBeenCalledWith('WI');
  });

  test('GET /services/users/course/:course_id returns students for course', async () => {
    const students = [
      { student_id: '1', first_name: 'Jan' },
      { student_id: '3', first_name: 'Piotr' },
    ];

    getStudentsByCourse.mockReturnValue(students);

    const res = await request(app)
      .get('/services/users/course/100')
      .set('Authorization', 'Bearer validtoken');

    expect(res.status).toBe(200);
    expect(res.body).toEqual(students);
    expect(getStudentsByCourse).toHaveBeenCalledWith('100');
  });

  test('GET /services/users/:student_id returns student when found', async () => {
    const student = {
      student_id: '2',
      first_name: 'Anna',
      last_name: 'Nowak',
    };

    getStudentById.mockImplementation((id) => {
      if (id === '1') {
        return {
          student_id: '1',
          first_name: 'Jan',
          last_name: 'Kowalski',
        };
      }
      if (id === '2') {
        return student;
      }
      return null;
    });

    const res = await request(app)
      .get('/services/users/2')
      .set('Authorization', 'Bearer validtoken');

    expect(res.status).toBe(200);
    expect(res.body).toEqual(student);
    expect(getStudentById).toHaveBeenCalledWith('2');
  });

  test('GET /services/users/:student_id returns 404 when student is missing', async () => {
    getStudentById.mockImplementation((id) => {
      if (id === '1') {
        return {
          student_id: '1',
          first_name: 'Jan',
          last_name: 'Kowalski',
        };
      }
      return null;
    });

    const res = await request(app)
      .get('/services/users/999')
      .set('Authorization', 'Bearer validtoken');

    expect(res.status).toBe(404);
    expect(res.body).toEqual({ error: 'Student not found' });
  });
});