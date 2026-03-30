import { useActionState, useEffect } from 'react'
import { useNavigate, useLocation } from 'react-router-dom'
import Modal from '../../components/modals/Modal'
import Input from '../../components/common/Input';

type stateType = {
  email: string,
  password: string,
  success: boolean,
  errors?: string | null
}

const initial_state = {
  email: "",
  password: "",
  success: false,
  errors: null
}


function handleAction(_: stateType, formData: FormData): Promise<stateType>{
    const data = {
        email: formData.get('email') as string,
        password: formData.get('password') as string
    }

    //submit data

    return new Promise(resolve => setTimeout(()=>{
        const isSuccess = Math.random()%2==0
        resolve({
            email: data.email,
            password: '',
            success: isSuccess,
            errors: isSuccess ? null : "invalid credentials"
        });
    }, 1000))
}

export default function RegisterPage() {
  const location = useLocation();
  const isRegisterOpen = location.pathname == "/register";
  const navigate = useNavigate();

  const [state,formAction] = useActionState(handleAction, initial_state);

  useEffect(()=>{
    if(state.success && state.errors==null)
        navigate("/");
  }, [state, navigate])
  
  function emailValidator(e: React.ChangeEvent<HTMLInputElement>){
        const input = e.currentTarget;
        const valid = /^[^@]+@pw\.edu\.pl$/.test(input.value);

        input.setCustomValidity(valid ? "" : "Use @pw.edu.pl email");
        input.reportValidity();
  }
  //TO-DO: DISABLE CREATE BUTTON WHILE IS PENDING
  return (
    <Modal open={isRegisterOpen} className="register-page" onClose={()=>navigate("/")}>    
        <p>Join Student Planner today</p>

        <form key={state.email + state.password} action={formAction}>
            <Input type="email" id="email" label="University Email" defaultValue={state.email}
                pattern="^[^@]+@pw\.edu\.pl$" onChange={emailValidator}/>

            <Input type="password" id="password" label="Password" defaultValue={state.password}
                error={state.errors}/>
          
          <button type="submit">Create Account</button> 
        </form>
    </Modal>
  )
}
