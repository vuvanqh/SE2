import { useState } from "react"
import AcademicEventCard from "../../../features/events/components/AcademicEventCard"
import { useAccessibleAcademicEvents } from "../../../features/events/hooks/academicEventHook"

export default function AcademicEventPage(){
    const {events} = useAccessibleAcademicEvents()
    const [search, setSearch] = useState("");
    const [includeSubscribed, setIncludeSubscribed] = useState(true);
    const [facultyOnly, setFacultyOnly] = useState(false);

    const filteredEvents = events.filter(event => {
        const matchesSearch = event.title.toLowerCase().includes(search.toLowerCase());
        const matchesSubscription = includeSubscribed || !event.isSubscribed;
        const matchesFaculty = !facultyOnly || event.facultyId !== null;
        
        return matchesSearch && matchesSubscription && matchesFaculty;
    });

    return <>
        <h1>Academic Events</h1>
        <div className="events-grid">
            {filteredEvents.length > 0 ? (
                filteredEvents.map(event => <AcademicEventCard key={event.id} event={event}/>)
            ) : (
                <p>No events found matching your criteria.</p>
            )}
        </div>
         <aside className="user-panel">
            <input 
                className="search-input" 
                placeholder="Search by event..."
                value={search}
                onChange={(e) => setSearch(e.target.value)}
            />

            <div className="filter-group">
                <p className="filter-title">Filters</p>
                <label className="filter-option">
                    <input 
                        type="checkbox" 
                        checked={includeSubscribed}
                        onChange={(e) => setIncludeSubscribed(e.target.checked)}
                    />
                    Include Subscribed
                </label>
                <label className="filter-option">
                    <input 
                        type="checkbox"
                        checked={facultyOnly}
                        onChange={(e) => setFacultyOnly(e.target.checked)}
                    />
                    Faculty Only
                </label>
            </div>
        </aside>
    </>
}