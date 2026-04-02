import { Outlet, useNavigate } from "react-router-dom"
import Navbar from "../../components/layout/Navbar"
import Calendar from "../../components/calendar/Calendar";
import {useGetAllPersonalEvents} from "../../hooks/personalEventHooks"
import { useEffect } from "react";
import { useUser } from "../../hooks/authHooks";

export default function StudentPage(){
    const navigate = useNavigate();
    const {user} = useUser();
    const {events} = useGetAllPersonalEvents();

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
                    <button onClick={()=>{navigate("/"); localStorage.clear()}}>Log out</button>
                </div>
        </Navbar>
        <main className="main-content">
            <Calendar events={events}/>
        </main>
        <Outlet/>
    </>
} 