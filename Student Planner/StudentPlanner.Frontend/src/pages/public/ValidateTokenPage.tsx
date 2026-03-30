import { useActionState } from 'react'
import { useNavigate, useLocation } from 'react-router-dom'
import Modal from '../../components/modals/Modal'
import Input from '../../components/common/Input';

type stateType = {
  token: string,
  password: string,
  confirmPassword: string,
  errors?: stateErrors | null
}

type stateErrors = {
  tokenError?: string,
  passwordError?: string,
  confirmPasswordError?: string,
}

const initial_state = {
  token: "",
  password: "",
  confirmPassword: "",
  errors: null
}

function handleAction(prev: stateType, _: FormData): Promise<stateType>{
  return new Promise((_, _reject)=>prev)
}

export default function ValidateTokenPage() {
  const location = useLocation();
  const isRegisterOpen = location.pathname == "/register";
  const navigate = useNavigate();


  const [state,formAction] = useActionState(handleAction, initial_state);
  return (
    <Modal open={isRegisterOpen} className="register-page" onClose={()=>navigate("/")}>    
        <p>We've sent a verification token to your email.</p>

        <form action={formAction} noValidate>
          <Input type="text" id="token" label="Token" value={state.token}
            error={state.errors?.tokenError}/>

          <div className='form-row'>
            <Input type="password" id="password" label="Password" value={state.password}
              error={state.errors?.passwordError}/>

            <Input type="password" id="confirmPassword" label="Confirm Password" value={state.confirmPassword}
              error={state.errors?.confirmPasswordError}/>
          </div>
          
          <button type="submit">Create Account</button>
        </form>
    </Modal>
  )
}
