import { useActionState } from 'react'
import { useNavigate, useLocation } from 'react-router-dom'
import Modal from '../../components/modals/Modal'
import Input from '../../components/common/Input';
import { useAuth } from '../../global-hooks/authHooks';

type stateType = {
  email: string,
  password: string,
  confirmPassword: string,
  success: boolean,
  errors: string[]
}

const initial_state: stateType = {
  email: "",
  password: "",
  confirmPassword: "",
  success: false,
  errors: []
}

export default function RegisterPage() {
  const location = useLocation();
  const isRegisterOpen = location.pathname == "/register";
  const navigate = useNavigate();
  const {registerUser, isRegisterPending} = useAuth();
  const [state,formAction] = useActionState(handleAction, initial_state);


  async function handleAction(_: stateType, formData: FormData){
      const data = {
          email: formData.get('email') as string,
          password: formData.get('password') as string,
          confirmPassword: formData.get('confirmPassword') as string
      }

      try{
        await registerUser(data);
        return {
          ...data,
          errors: [],
          success: true
        }
      }
      catch(err: any){
        const raw = err.info?.errors;
        let errors = [];
        if (Array.isArray(raw)) 
            errors.push(...raw);
        
        if (typeof raw === "string") 
            errors.push(raw);
        
        return {
          ...data,
          success: false,
          errors: ["invalid credentials", ...errors]
        }
      }
  }

  
  function emailValidator(e: React.ChangeEvent<HTMLInputElement>){
        const input = e.currentTarget;
        const valid = /^[^@]+@pw\.edu\.pl$/.test(input.value);

        input.setCustomValidity(valid ? "" : "Use @pw.edu.pl email");
        input.reportValidity();
  }

  //TO-DO: DISABLE CREATE BUTTON WHILE IS PENDING
  return (
    <Modal open={isRegisterOpen} className="register-page" onClose={()=>navigate("/")}>    
        <button
          type="button"
          className="auth-modal-close"
          aria-label="Close registration modal"
          onClick={() => navigate("/")}
        >
          X
        </button>
        <p>Join Student Planner today</p>

        <form action={formAction} className="auth-form">
            <Input type="email" id="email" label="University Email" defaultValue={state.email}
                pattern="^[^@]+@pw\.edu\.pl$" onChange={emailValidator}/>

            <Input type="password" id="password" label="Password" defaultValue={state.password}/>
            

            {state.errors?.map(error => <small className="error-text" key={error}>{error}</small>)}
          <button type="submit" disabled={isRegisterPending}>Create Account</button> 
        </form>
    </Modal>
  )
}
