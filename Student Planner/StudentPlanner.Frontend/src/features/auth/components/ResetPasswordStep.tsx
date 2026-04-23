import Input from '../../../components/common/Input';
import type { ForgotPasswordState } from '../types/forgotPasswordTypes';

interface ResetPasswordStepProps {
  state: ForgotPasswordState;
}

export default function ResetPasswordStep({ state }: ResetPasswordStepProps) {
  return (
    <>
      <small>If the email exists, a token was sent.</small>
      <Input 
        type="text" 
        id="token" 
        label="Reset Token" 
        defaultValue={state.token} 
        required 
      />
      <Input 
        type="password" 
        id="newPassword" 
        label="New Password" 
        defaultValue={state.newPassword} 
        required 
      />
      <Input 
        type="password" 
        id="confirmNewPassword" 
        label="Confirm Password" 
        defaultValue={state.confirmNewPassword} 
        required 
      />
    </>
  );
}
