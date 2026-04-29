import { useState } from "react"
import AcademicEventCard from "../../../features/events/components/AcademicEventCard"
import { useAccessibleAcademicEvents } from "../../../features/events/hooks/academicEventHook"
import { useUser } from "../../../global-hooks/authHooks";
import FilterOption from "../../../components/common/FilterOption";

export default function AcademicEventPage(){
    const {events} = useAccessibleAcademicEvents()
    const [search, setSearch] = useState("");
    const [includeSubscribed, setIncludeSubscribed] = useState(true);
    const [facultyOnly, setFacultyOnly] = useState(false);
    const {user} = useUser();

    const filteredEvents = events.filter(event => {
        const matchesSearch = event.title.toLowerCase().includes(search.toLowerCase());
        const matchesSubscription = includeSubscribed || !event.isSubscribed;
        const matchesFaculty = !facultyOnly || event.facultyId !== null;
        
        return matchesSearch && matchesSubscription && matchesFaculty;
    });

    return <div className="user-management-page">
        <section className="users-card">
            <div className="users-header">
                <h2>Academic Events</h2>
            </div>
            <ul className="users-list">
                {filteredEvents.length > 0 ? (
                    filteredEvents.map(event => (
                        <li key={event.id}>
                            <AcademicEventCard event={event}/>
                        </li>
                    ))
                ) : (
                    <p>No events found matching your criteria.</p>
                )}
            </ul>
        </section>
        <aside className="user-panel">
            <input 
                className="search-input" 
                placeholder="Search by event..."
                value={search}
                onChange={(e) => setSearch(e.target.value)}
            />

            <div className="filter-group">
                <p className="filter-title">Filters</p>
                {user?.userRole=="Student" && <FilterOption 
                    label="Include Subscribed"
                    value={includeSubscribed}
                    onChange={setIncludeSubscribed}
                />}
                <FilterOption 
                    label="Faculty Only"
                    value={facultyOnly}
                    onChange={setFacultyOnly}
                />
            </div>
            <button className="ghost-btn" onClick={()=>{
                setSearch("");
                setIncludeSubscribed(true);
                setFacultyOnly(false);
            }}>
                Clear filters
            </button>
        </aside>
    </div>
}