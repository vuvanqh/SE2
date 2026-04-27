import AcademicEventCard from "../../../features/events/components/AcademicEventCard"
import { useAccessibleAcademicEvents } from "../../../features/events/hooks/academicEventHook"

export default function AcademicEventPage(){
    const {events} = useAccessibleAcademicEvents()
    return <>
        <h1>Academic Events</h1>
        {events.map(event => <AcademicEventCard key={event.id} event={event}/>)}
         <aside className="user-panel">
            <input className="search-input" placeholder="Search by event..."/>

            <div className="filter-group">
                <p className="filter-title">Filters</p>
                <label className="filter-option"><input type="checkbox"/>Include Subscribed</label>
                <label className="filter-option"><input type="checkbox"/>Faculty Only</label>
            </div>
        </aside>
    </>
}