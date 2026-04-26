import { NavLink, useNavigate } from "react-router-dom"
import { useEffect, useState } from "react";
import { useUser } from "../../../global-hooks/authHooks";
import DashboardLayout from "../common/Dashboard";

export default function AdminPage(){
    const navigate = useNavigate();
    const {user} = useUser();
    const [sidebarOpen, setSidebarOpen] = useState(false);

    useEffect(()=>{
        if(user===undefined)
            navigate("/");
        
    },[user])

    useEffect(() => {
        const handleClick = () => setSidebarOpen(false);

        const handleKey = (e: KeyboardEvent) => {
            if (e.key === "Escape") {
                setSidebarOpen(false);
            }
        };

        if (sidebarOpen) {
            document.addEventListener("click", handleClick);
            document.addEventListener("keydown", handleKey);
        }
        return () => {
            document.removeEventListener("click", handleClick);
            document.removeEventListener("keydown", handleKey);
        }
    }, [sidebarOpen]);

    if(user==undefined)
        return null;
    return <DashboardLayout navItems={<>
        <NavLink to="" className={({ isActive }) => isActive ? "nav-link active" : "nav-link"}>Calendar</NavLink>
        <NavLink to="requests" onClick={(e)=>e.preventDefault()} className={({ isActive }) => isActive ? "nav-link active" : "nav-link"}>Events</NavLink>
        <NavLink to="events" onClick={(e)=>e.preventDefault()} className={({ isActive }) => isActive ? "nav-link active" : "nav-link"}>Event Requests</NavLink>
        <NavLink to="users" className={({ isActive }) => isActive ? "nav-link active" : "nav-link"}>Users</NavLink>
    </>}/>
} 