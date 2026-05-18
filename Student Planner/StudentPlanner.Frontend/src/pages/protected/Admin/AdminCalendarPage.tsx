import { useState, type ReactNode} from "react";
import EventPanel from "../../../components/calendar/EventPanel";
import Calendar from "../../../components/calendar/Calendar";
import { useAllEventRequests } from "../../../features/eventRequests/hooks/eventRequestHooks";
import { EventPreview } from "../../../features/events/components/EventPreview";
import AdminRequestPreview from "../../../features/eventRequests/components/AdminRequestPreview";
import type { eventPreviewResponse } from "../../../types/eventPreviewResponse";
import useEventPreviews from "../../../global-hooks/eventPreviewHooks";
import FilterOption from "../../../components/common/FilterOption";
import { useFaculties } from "../../../global-hooks/facultyHooks";
import { getNEvents } from "../../../api/helpers";
import { UNIVERSITY_ID } from "../../../constants/university";

export default function AdminCalendarPage(){
    const [viewRequests, setVievRequests] = useState(false);
    const {eventRequests} = useAllEventRequests();
    const {faculties} = useFaculties();
    const [selectedFaculties, setSelectedFaculties] = useState<string[]>([]);
    const [appliedFaculties, setAppliedFaculties] = useState<string[]>([]);
    const [range, setRange] = useState<{ from?: Date; days?: number;}>({});

    const {eventPreviews} = useEventPreviews({
        facultyIds: appliedFaculties,
        from: range.from,
        days: range.days,
    });

    function toggleValue(value: string, setter: React.Dispatch<React.SetStateAction<string[]>>) {
        setter(prev =>
            prev.includes(value)
            ? prev.filter(v => v !== value)
            : [...prev, value]
        );
    }

    const top10:eventPreviewResponse[] = getNEvents(eventPreviews,10);
    let content: ReactNode;


    if (viewRequests) {
    content =
        eventRequests.length === 0 ? (
        <p>No recent requests...</p>
        ) : (
        <ul className="events-list">
            {eventRequests.map(e => (
            <li key={e.id}>
                <AdminRequestPreview eventRequest={e} />
            </li>
            ))}
        </ul>
        );
    } else {
    content =
        top10.length === 0 ? (
        <p>No upcoming events...</p>
        ) : (
        <ul className="events-list">
            {top10.map(e => (
            <li key={e.id}>
                <EventPreview event={e} />
            </li>
            ))}
        </ul>
        );
    }

    const filtersChanged = selectedFaculties.slice().sort().join(",") !== appliedFaculties.slice().sort().join(",");

    
    return <>
        <aside className="user-panel">
            <h3 className="filter-title">
                Faculty Filters
            </h3>
            <div className="filter-group">
                <p className="filter-title">Faculties</p>
                <FilterOption label="University" value={selectedFaculties.includes(UNIVERSITY_ID)}
                        onChange={() => toggleValue(UNIVERSITY_ID, setSelectedFaculties)}/>
                {faculties.map(f => <FilterOption key={f.facultyId} label={f.facultyName}
                        value={selectedFaculties.includes(f.facultyId)} 
                        onChange={() => toggleValue(f.facultyId, setSelectedFaculties)}/>)}
            </div>
            <button onClick={()=>setAppliedFaculties([...selectedFaculties])} disabled={!filtersChanged}>
                Apply
            </button>
            <button onClick={() => {setSelectedFaculties([]); setAppliedFaculties([]);}}>
                Clear
            </button>
        </aside>
        <Calendar events={eventPreviews} onRangeChange={(from, days) =>setRange({ from, days })}/>
        <EventPanel label={viewRequests?"Recent Requests":"Upcoming events"}> 
            <div className="events-controls">
                <div className="toggle-group">
                    <button className={`toggle-btn ${!viewRequests ? "active" : ""}`} onClick={()=>setVievRequests(false)}>View Events</button>
                    <button className={`toggle-btn ${viewRequests ? "active" : ""}`} onClick={()=>setVievRequests(true)}>View Requests</button>
                </div>
            </div>
            
            {content}
        </EventPanel>
    </>
}