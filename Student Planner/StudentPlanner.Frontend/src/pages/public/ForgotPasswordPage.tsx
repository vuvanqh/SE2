import { useEffect } from 'react'
import { useNavigate, useLocation, NavLink } from 'react-router-dom'
import Modal from '../../components/modals/Modal'
import { useForgotPassword } from '../../features/auth/hooks/useForgotPassword';
import { ForgotPasswordStep } from '../../features/auth/types/forgotPasswordTypes';
import RequestTokenStep from '../../features/auth/components/RequestTokenStep';
import ResetPasswordStep from '../../features/auth/components/ResetPasswordStep';

export default function ForgotPasswordPage() {
  const location = useLocation();
  const isForgotOpen = location.pathname === "/forgot-password";
  const navigate = useNavigate();
  
  const { state, formAction, isPending } = useForgotPassword();

  useEffect(() => {
    if (state.success) {
      navigate("/login");
    }
  }, [state.success, navigate]);

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

        <button disabled={isPending}>
          {isPending ? 'Processing...' : (state.step === ForgotPasswordStep.REQUEST_TOKEN ? 'Send Token' : 'Reset Password')}
        </button>

        <NavLink to="/login">Cancel</NavLink>
      </form>
    </Modal>
  );
}
