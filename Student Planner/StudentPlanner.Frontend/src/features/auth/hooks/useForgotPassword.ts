import { useActionState } from 'react';
import { useAuth } from '../../../global-hooks/authHooks';
import { extractErrors } from '../../../api/helpers';
import type { ForgotPasswordState } from '../types/forgotPasswordTypes';
import { 
  ForgotPasswordStep, 
  INITIAL_FORGOT_PASSWORD_STATE 
} from '../types/forgotPasswordTypes';

export function useForgotPassword() {
  const { sendResetToken, resetPassword, isResetPending } = useAuth();

  const [state, formAction] = useActionState(handleAction, INITIAL_FORGOT_PASSWORD_STATE);

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
    } catch (err) {
      // Catch and ignore errors here to prevent account enumeration.
    }

    return {
      ...prevState,
      email,
      step: ForgotPasswordStep.RESET_PASSWORD,
      errors: []
    };
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
      return { ...prevState, ...data, success: true, errors: [] };
    } catch (err: any) {
      const errors = extractErrors(err);
      
      // Fallback message if extractErrors returns something generic or empty
      const finalErrors = errors.length > 0 ? errors : ["Invalid or expired token"];
      
      return { ...prevState, ...data, errors: finalErrors };
    }
  }

  return {
    state,
    formAction,
    isPending: isResetPending
  };
}
