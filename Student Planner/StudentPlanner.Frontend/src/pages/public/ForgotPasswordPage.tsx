import { useActionState, useEffect } from 'react'
import { useNavigate, useLocation, NavLink } from 'react-router-dom'
import Modal from '../../components/modals/Modal'
import Input from '../../components/common/Input';
import { useAuth } from '../../global-hooks/authHooks';

const ForgotPasswordStep = {
  REQUEST_TOKEN: 'REQUEST_TOKEN',
  RESET_PASSWORD: 'RESET_PASSWORD'
} as const;

type ForgotPasswordStep = typeof ForgotPasswordStep[keyof typeof ForgotPasswordStep];

type StateType = {
  step: ForgotPasswordStep,
  email: string,
  token: string,
  newPassword: string,
  confirmNewPassword: string,
  errors: string[],
  success: boolean
}

const INITIAL_STATE: StateType = {
  step: ForgotPasswordStep.REQUEST_TOKEN,
  email: "",
  token: "",
  newPassword: "",
  confirmNewPassword: "",
  errors: [],
  success: false
}

export default function ForgotPasswordPage() {
  const location = useLocation();
  const isForgotOpen = location.pathname === "/forgot-password";
  const navigate = useNavigate();
  const { sendResetToken, resetPassword, isResetPending } = useAuth();

  const [state, formAction] = useActionState(handleAction, INITIAL_STATE);


  useEffect(() => {
    if (state.success) {
      navigate("/login");
    }
  }, [state.success, navigate]);

  async function handleAction(prevState: StateType, formData: FormData): Promise<StateType> {
    if (prevState.step === ForgotPasswordStep.REQUEST_TOKEN) {
      return handleRequestToken(prevState, formData);
    }

    if (prevState.step === ForgotPasswordStep.RESET_PASSWORD) {
      return handleResetPassword(prevState, formData);
    }

    return prevState;
  }

  async function handleRequestToken(prevState: StateType, formData: FormData): Promise<StateType> {
    const email = formData.get('email') as string;

    try {
      await sendResetToken({ email });
    }
    catch (err) {
      // catch and ignore errors here to prevent account enumeration.

    }

    return {
      ...prevState,
      email,
      step: ForgotPasswordStep.RESET_PASSWORD,
      errors: []
    };
  }

  async function handleResetPassword(prevState: StateType, formData: FormData): Promise<StateType> {
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
      const raw = err.response?.data;
      let errors = [];

      if (typeof raw === "string") {
        errors.push(raw);
      } else if (Array.isArray(raw)) {
        errors.push(...raw);
      } else if (raw?.errors && typeof raw.errors === "object") {
        // Handle ASP.NET Core ValidationProblemDetails
        errors.push(...Object.values(raw.errors).flat() as string[]);
      } else {
        errors.push("Invalid or expired token");
      }

      return { ...prevState, ...data, errors };
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

        <button disabled={isResetPending}>
          {isResetPending ? 'Processing...' : (state.step === ForgotPasswordStep.REQUEST_TOKEN ? 'Send Token' : 'Reset Password')}
        </button>

        <NavLink to="/login">Cancel</NavLink>
      </form>
    </Modal>
  );
}

function RequestTokenStep({ email }: { email: string }) {
  return (
    <Input type="email" id="email" label="University Email" defaultValue={email} required />
  );
}

function ResetPasswordStep({ state }: { state: StateType }) {
  return (
    <>
      <small>If the email exists, a token was sent.</small>
      <Input type="text" id="token" label="Reset Token" defaultValue={state.token} required />
      <Input type="password" id="newPassword" label="New Password" defaultValue={state.newPassword} required />
      <Input type="password" id="confirmNewPassword" label="Confirm Password" defaultValue={state.confirmNewPassword} required />
    </>
  );
}
