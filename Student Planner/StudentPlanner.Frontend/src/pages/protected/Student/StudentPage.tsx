import { NavLink } from "react-router-dom"
import DashboardLayout from "../common/Dashboard";

export default function StudentPage(){
    return <DashboardLayout navItems={
            <>
                <NavLink to="/student" end className={({ isActive }) => isActive ? "nav-link active" : "nav-link"}>Calendar</NavLink>
                <NavLink to="/student/events" className={({ isActive }) => isActive ? "nav-link active" : "nav-link"}>Events</NavLink>
            </>
        }/>
} 