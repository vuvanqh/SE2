import { useActionState } from 'react'
import { useNavigate, useLocation, NavLink } from 'react-router-dom'
import Modal from '../../components/modals/Modal'
import { useAuth } from '../../global-hooks/authHooks';
import { extractErrors } from '../../api/helpers';
import type { ForgotPasswordState } from '../../features/auth/types/forgotPasswordTypes';
import {
  ForgotPasswordStep,
  INITIAL_FORGOT_PASSWORD_STATE
} from '../../features/auth/types/forgotPasswordTypes';
import RequestTokenStep from '../../features/auth/components/RequestTokenStep';
import ResetPasswordStep from '../../features/auth/components/ResetPasswordStep';

export default function ForgotPasswordPage() {
  const location = useLocation();
  const isForgotOpen = location.pathname === "/forgot-password";
  const navigate = useNavigate();

  const { sendResetToken, resetPassword, isResetPending } = useAuth();

  const [state, formAction, isPending] = useActionState(handleAction, INITIAL_FORGOT_PASSWORD_STATE);

  async function handleAction(prevState: ForgotPasswordState, formData: FormData): Promise<ForgotPasswordState> {
    if (prevState.step === ForgotPasswordStep.REQUEST_TOKEN) {
      return handleRequestToken(prevState, formData);
    }

    if (prevState.step === ForgotPasswordStep.RESET_PASSWORD) {
      return handleResetPassword(prevState, formData);
    }

    return prevState;
  }

  async function handleRequestToken(prevState: ForgotPasswordState, formData: FormData): Promise<ForgotPasswordState> {
    const email = formData.get('email') as string;

    try {
      await sendResetToken({ email });
      return {
        ...prevState,
        email,
        step: ForgotPasswordStep.RESET_PASSWORD,
        errors: []
      };
    } catch (err: any) {
      const errors = extractErrors(err);
      return {
        ...prevState,
        email,
        errors: errors.length > 0 ? errors : ["Failed to send reset email. Please try again later."]
      };
    }
  }

  async function handleResetPassword(prevState: ForgotPasswordState, formData: FormData): Promise<ForgotPasswordState> {
    const data = {
      email: prevState.email,
      token: formData.get('token') as string,
      newPassword: formData.get('newPassword') as string,
      confirmNewPassword: formData.get('confirmNewPassword') as string
    };

    if (data.newPassword !== data.confirmNewPassword) {
      return { ...prevState, ...data, errors: ["Passwords don't match"] };
    }

    try {
      await resetPassword(data);
      return { ...prevState, ...data, errors: [] };
    } catch (err: any) {
      const errors = extractErrors(err);
      const finalErrors = errors.length > 0 ? errors : ["Invalid or expired token"];
      return { ...prevState, ...data, errors: finalErrors };
    }
  }

  return (
    <Modal open={isForgotOpen} className="register-page" onClose={() => navigate("/")}>
      <p>{state.step === ForgotPasswordStep.REQUEST_TOKEN ? 'Reset Password' : 'Enter Reset Token'}</p>

      <form action={formAction} className='auth-form'>
        {state.step === ForgotPasswordStep.REQUEST_TOKEN ? (
          <RequestTokenStep email={state.email} />
        ) : (
          <ResetPasswordStep state={state} />
        )}

        {state.errors?.map(error => <small className="error-text" key={error}>{error}</small>)}

        <button disabled={isPending || isResetPending}>
          {(isPending || isResetPending) ? 'Processing...' : (state.step === ForgotPasswordStep.REQUEST_TOKEN ? 'Send Token' : 'Reset Password')}
        </button>

        <NavLink to="/login">Cancel</NavLink>
      </form>
    </Modal>
  );
}
