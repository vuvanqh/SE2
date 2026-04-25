export const ForgotPasswordStep = {
  REQUEST_TOKEN: 'REQUEST_TOKEN',
  RESET_PASSWORD: 'RESET_PASSWORD'
} as const;

export type ForgotPasswordStep = typeof ForgotPasswordStep[keyof typeof ForgotPasswordStep];

export type ForgotPasswordState = {
  step: ForgotPasswordStep;
  email: string;
  token: string;
  newPassword: string;
  confirmNewPassword: string;
  errors: string[];
}

export const INITIAL_FORGOT_PASSWORD_STATE: ForgotPasswordState = {
  step: ForgotPasswordStep.REQUEST_TOKEN,
  email: "",
  token: "",
  newPassword: "",
  confirmNewPassword: "",
  errors: []
}
