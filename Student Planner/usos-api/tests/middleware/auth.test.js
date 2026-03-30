const authMiddleware = require('../../middleware/auth');

jest.mock('../../data/authData', () => ({
  getSessionByToken: jest.fn(),
}));

jest.mock('../../data/usersData', () => ({
  getStudentById: jest.fn(),
}));

const { getSessionByToken } = require('../../data/authData');
const { getStudentById } = require('../../data/usersData');

describe('auth middleware', () => {
  let req, res, next;

  beforeEach(() => {
    req = { headers: {} };
    res = {
      status: jest.fn().mockReturnThis(),
      json: jest.fn(),
    };
    next = jest.fn();
  });

  test('returns 401 when authorization header is missing', () => {
    authMiddleware(req, res, next);

    expect(res.status).toHaveBeenCalledWith(401);
    expect(res.json).toHaveBeenCalledWith({ error: 'No authorization header provided' });
    expect(next).not.toHaveBeenCalled();
  });

  test('returns 401 when token is invalid', () => {
    req.headers.authorization = 'Bearer badtoken';
    getSessionByToken.mockReturnValue(null);

    authMiddleware(req, res, next);

    expect(res.status).toHaveBeenCalledWith(401);
    expect(res.json).toHaveBeenCalledWith({ error: 'Invalid token' });
    expect(next).not.toHaveBeenCalled();
  });

  test('returns 401 when student is not found', () => {
    req.headers.authorization = 'Bearer validtoken';
    getSessionByToken.mockReturnValue({ student_id: '1' });
    getStudentById.mockReturnValue(null);

    authMiddleware(req, res, next);

    expect(res.status).toHaveBeenCalledWith(401);
    expect(res.json).toHaveBeenCalledWith({ error: 'Student not found' });
    expect(next).not.toHaveBeenCalled();
  });

  test('calls next and attaches student for valid token', () => {
    const student = { id: '1', first_name: 'Jan' };

    req.headers.authorization = 'Bearer validtoken';
    getSessionByToken.mockReturnValue({ student_id: '1' });
    getStudentById.mockReturnValue(student);

    authMiddleware(req, res, next);

    expect(req.student).toEqual(student);
    expect(next).toHaveBeenCalled();
  });
});