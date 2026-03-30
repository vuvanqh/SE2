export type loginRequest = {
    email: string,
    password: string
}

export type loginResponse = {
  token: string,
  firstName: string,
  lastName: string,
  email: string,
  role: string
}

export type registerRequest = {
    firstName: string,
    lastName: string,
    email: string,
    password: string
    confirmPassword: string
}