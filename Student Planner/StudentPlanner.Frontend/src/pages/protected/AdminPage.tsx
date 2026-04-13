import { NavLink, Outlet, useNavigate } from "react-router-dom"
import Navbar from "../../components/layout/Navbar"
import { useEffect } from "react";
import { useUser } from "../../global-hooks/authHooks";

export default function AdminPage(){
    const navigate = useNavigate();
    const {user} = useUser();

    useEffect(()=>{
        if(user===undefined)
            navigate("/");
        
    },[user])

    if(user==undefined)
        return null;

    return <>
        <Navbar>
                <div>
                    <div>
                        <h1>Welcome {user.firstName} {user.lastName}</h1>
                    </div>
                </div>
                <div>
                    <NavLink to="" className={({ isActive }) => isActive ? "nav-link active" : "nav-link"}>Events</NavLink>
                    <NavLink to="/events" onClick={(e)=>e.preventDefault()} className={({ isActive }) => isActive ? "nav-link active" : "nav-link"}>Event Requests</NavLink>
                    <NavLink to="/events" onClick={(e)=>e.preventDefault()} className={({ isActive }) => isActive ? "nav-link active" : "nav-link"}>Users</NavLink>
                    <button onClick={()=>{navigate("/"); localStorage.clear()}}>Log out</button>
                </div>
        </Navbar>
        <main className="main-content">
            
        </main>
        <Outlet/>
    </>
} 