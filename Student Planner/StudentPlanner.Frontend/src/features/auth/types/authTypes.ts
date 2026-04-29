export type loginRequest = {
    email: string,
    password: string
}

export type loginResponse = {
  token: string,
  firstName: string,
  lastName: string,
  email: string,
  userRole: "Student" | "Manager" | "Admin",
  facultyId?: string,
  facultyCode?: string
}

export type usosLoginRequest = {
    email: string,
    password: string
}

export type registerRequest = {
    // firstName: string,
    // lastName: string,
    email: string,
    password: string
}

export type forgotPasswordRequest = {
    email: string
}

export type resetPasswordRequest = {
    email: string,
    token: string,
    newPassword: string,
    confirmNewPassword: string
}