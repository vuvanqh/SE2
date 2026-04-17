import { NavLink } from "react-router-dom"
import DashboardLayout from "../common/Dashboard";

export default function StudentPage(){
    return <DashboardLayout navItems={
            <>
                <NavLink to="" end className={({ isActive }) => isActive ? "nav-link active" : "nav-link"}>Calendar</NavLink>
                <NavLink to="/events" onClick={(e)=>e.preventDefault()}  className={({ isActive }) => isActive ? "nav-link active" : "nav-link"}>Events</NavLink>
            </>
        }/>
} 