import { NavLink, useNavigate } from "react-router-dom"
import { useEffect, useState } from "react";
import { useUser } from "../../../global-hooks/authHooks";
import DashboardLayout from "../common/Dashboard";

export default function ManagerPage(){
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
        <NavLink to="events" className={({ isActive }) => isActive ? "nav-link active" : "nav-link"}>Events</NavLink>
        <NavLink to="requests" className={({ isActive }) => isActive ? "nav-link active" : "nav-link"}>My Requests</NavLink>
    </>}/>
} 