import { Outlet, useNavigate } from "react-router-dom"
import Navbar from "../../components/layout/Navbar"
import Calendar from "../../components/calendar/Calendar";
import { queryClient } from "../../api/queryClient";

export default function StudentPage(){
    const navigate = useNavigate();
    const user = queryClient.getQueryData(['user']);
    return <>
        <Navbar>
                <div>
                    <div>
                        <h1>Welcome {user.firstName} {user.lastName}</h1>
                    </div>
                </div>
                <div>
                    <button onClick={()=>navigate("/register")}>Register</button>
                    <button onClick={()=>navigate("/")}>Log out</button>
                </div>
        </Navbar>
        <main className="main-content">
            <Calendar/>
        </main>
        <Outlet/>
    </>
} 