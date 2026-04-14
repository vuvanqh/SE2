import { useNavigate } from "react-router-dom";
import { useUser } from "../../global-hooks/authHooks";

export default function Sidebar({open}: {open: boolean}){
    const navigate = useNavigate();
    const {user} = useUser();

    return <aside className={`sidebar ${open ? "open" : ""}`} onClick={(e) => e.stopPropagation()}>
        <div className="user-details">
            <img/>
            <div>
                <p>{user?.firstName} {user?.lastName}</p>
                <p>{user?.userRole} {user?.facultyCode?` - ${user.facultyCode}`:""}</p>
            </div>
        </div>

        <div className="sidebar-actions">
            <button>Change Passwd</button>
            <button className="danger">Delete Account</button>
            <button className="danger" onClick={()=>{navigate("/"); localStorage.clear()}}>Log out</button>
        </div>
    </aside>
}

