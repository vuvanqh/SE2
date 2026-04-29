import { useFaculties } from "../../../global-hooks/facultyHooks";
import { useAdmin } from "../../../features/admin/hooks/adminHooks";
import { useMemo, useState, useContext } from "react";
import { ModalContext } from "../../../store/ModalContext";
import FilterOption from "../../../components/common/FilterOption";
import { UNIVERSITY_ID } from "../../../constants/university";

export default function UserManagementPage(){
    const {faculties} = useFaculties();
    const {users, isUsersLoading, deleteUser} = useAdmin();
    const {open} = useContext(ModalContext);

    const [search, setSearch] = useState("");
    const [selectedRoles, setSelectedRoles] = useState<string[]>([]);
    const [selectedFaculties, setSelectedFaculties] = useState<string[]>([]);

    function toggleValue(value: string, setter: React.Dispatch<React.SetStateAction<string[]>>) {
        setter(prev =>
            prev.includes(value)
            ? prev.filter(v => v !== value)
            : [...prev, value]
        );
    }
    const filteredUsers = useMemo(() => {
    return users.filter(user => {

        const matchesSearch =
        !search ||
        `${user.firstName} ${user.lastName}`
            .toLowerCase()
            .includes(search.toLowerCase()) ||
        user.email.toLowerCase().includes(search.toLowerCase());

        const matchesRole =
        selectedRoles.length === 0 ||
        selectedRoles.includes(user.userRole);

        const matchesFaculty =
        selectedFaculties.length === 0 ||
        (user.facultyId && selectedFaculties.includes(user.facultyId)) ||
        (!user.facultyId && selectedFaculties.includes(UNIVERSITY_ID));

        return (
        matchesSearch &&
        matchesRole &&
        matchesFaculty
        );
    });
    }, [users, search, selectedRoles, selectedFaculties]);

    return <div className="user-management-page">
        <section className="users-card">
            <div className="users-header">
                <h2>Users</h2>
            </div>

            <ul className="users-list">
                {isUsersLoading? <p>Loading...</p> : filteredUsers.map((u) => (
                    <li key={u.id}>
                        <button className="user-row" onClick={()=>open({type:"userView", user: u, deleteUser})}>
                            <p className="user-name">
                                {u.firstName} {u.lastName}
                                <span className="role-badge">{u.userRole}</span>
                            </p>

                            <p className="user-meta">{u.email} • {u.faculty}</p>
                        </button>
                    </li>
                ))}
            </ul>
        </section>
        <aside className="user-panel">
            <input className="search-input" placeholder="Search users..." 
                value={search} onChange={(e)=>setSearch(e.target.value)}/>

            <button className="primary-action" onClick={()=>open({type:"createManager"})}>+ Create Manager</button>

            <div className="filter-group">
                <p className="filter-title">Roles</p>
                <FilterOption label="Admin" value={selectedRoles.includes("Admin")} onChange={() => toggleValue("Admin", setSelectedRoles)}/>
                <FilterOption label="Manager" value={selectedRoles.includes("Manager")} onChange={() => toggleValue("Manager", setSelectedRoles)}/>
                <FilterOption label="Student" value={selectedRoles.includes("Student")} onChange={() => toggleValue("Student", setSelectedRoles)}/>
            </div>
            <div className="filter-group">
                <p className="filter-title">Faculties</p>
                <FilterOption label="University" value={selectedFaculties.includes(UNIVERSITY_ID)}
                    onChange={() => toggleValue(UNIVERSITY_ID, setSelectedFaculties)}/>
                {faculties.map(f => <FilterOption key={f.facultyId} label={f.facultyName}
                     value={selectedFaculties.includes(f.facultyId)} 
                     onChange={() => toggleValue(f.facultyId, setSelectedFaculties)}/>)}
            </div>
            <button className="ghost-btn" onClick={()=>{
                setSearch("");
                setSelectedRoles([]);
                setSelectedFaculties([]);
                }}>
                Clear filters
            </button>

        </aside>
    </div>
}

