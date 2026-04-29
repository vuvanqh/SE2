import { useActionState } from 'react'
import { useNavigate, useLocation } from 'react-router-dom'
import Modal from '../../components/modals/Modal'
import Input from '../../components/common/Input';
import { useAuth } from '../../global-hooks/authHooks';

type stateType = {
  email: string,
  password: string,
  errors?: string[] | null
}

const initial_state = {
  email: "",
  password: "",
  errors: null
}

export default function UsosLoginPage() {
  const location = useLocation();
  const isUsosLoginOpen = location.pathname === "/usos-login";
  const navigate = useNavigate();
  const { usosLogin, isUsosLoginPending } = useAuth();

  const [state, formAction] = useActionState(handleAction, initial_state);

  async function handleAction(_: stateType, formData: FormData) {
    const data = {
      email: formData.get('email') as string,
      password: formData.get('password') as string
    }

    if (data.email.trim().length === 0 || data.password.length === 0) {
      return {
        email: data.email,
        password: '',
        errors: ['Invalid form data']
      }
    }

    try {
      if (usosLogin) {
        await usosLogin(data);
      }
      return {
        email: data.email,
        password: '',
        errors: null
      }
    }
    catch (err: any) {
      const raw = err.info?.errors;
      let errors = [];
      if (Array.isArray(raw))
        errors.push(...raw);

      if (typeof raw === "string")
        errors.push(raw);

      return {
        email: data.email,
        password: '',
        errors: ["Invalid USOS credentials", ...errors]
      }
    }
  }

  return (
    <Modal open={isUsosLoginOpen} className="register-page" onClose={() => navigate("/")}>
      <p>USOS Authentication Required</p>
      <small style={{ display: 'block', marginBottom: '1rem', color: 'rgba(255,255,255,0.7)' }}>
        Your USOS session has expired or your credentials have changed. 
        Please log in to your USOS account to sync your schedule.
      </small>

      <form action={formAction} className='auth-form'>
        <Input 
          type="email" 
          id="email" 
          name="email"
          label="USOS Email" 
          defaultValue={state.email}
          required
        />

        <Input 
          type="password" 
          id="password" 
          name="password"
          label="USOS Password" 
          defaultValue={state.password}
          required
        />

        {state.errors?.map(error => <small className="error-text" key={error}>{error}</small>)}

        <button disabled={isUsosLoginPending}>Log In to USOS</button>
      </form>
    </Modal>
  )
}
