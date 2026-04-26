import { useActionState } from 'react'
import { useNavigate, useLocation, NavLink } from 'react-router-dom'
import Modal from '../../components/modals/Modal'
import Input from '../../components/common/Input';
import { useAuth } from '../../global-hooks/authHooks';
import { emailValidator } from '../../api/helpers';

type stateType = {
  email: string,
  password: string,
  errors?: string[] | null
}

const initial_state = {
  email: "",
  password: "",
  success: false,
  errors: null
}

export default function LoginPage() {
  const location = useLocation();
  const isLoginOpen = location.pathname == "/login";
  const navigate = useNavigate();
  const {login, isLoginPending} = useAuth();


  const [state,formAction] = useActionState(handleAction, initial_state);

  async function handleAction(_: stateType, formData: FormData){
    const data = {
        email: formData.get('email') as string,
        password: formData.get('password') as string
    }

    if (data.email.trim().length === 0|| data.password.length===0) {
      return {
        email: '',
        password: '',
        errors: ['Invalid form data']
      }
    }

    try {
      await login(data);
      return {
        email: data.email,
        password: '',
        errors: null
      }
    }
    catch(err: any)
    {
      const raw = err.info?.errors;
      let errors = [];
      if (Array.isArray(raw)) 
          errors.push(...raw);
      
      if (typeof raw === "string") 
          errors.push(raw);
      
      return {
        email: data.email,
        password: '',
        errors: ["invalid credentials", ...errors]
      }
    }
  }

  return (
    <Modal open={isLoginOpen} className="register-page" onClose={()=>navigate("/")}>    
        <p>Welcome back to Student Planner</p>

        <form action={formAction} className='auth-form'>
            <Input type="email" id="email" label="University Email" defaultValue={state.email}
                pattern="^[^@]+@pw\.edu\.pl$" onChange={emailValidator}/>

            <Input type="password" id="password" label="Password" defaultValue={state.password}/>


            <NavLink to="/forgot-password">Forgot Password?</NavLink>

            {state.errors?.map(error => <small className="error-text" key={error}>{error}</small>)}

          <button disabled={isLoginPending}>Log In</button> 
        </form>
    </Modal>
  )
}
