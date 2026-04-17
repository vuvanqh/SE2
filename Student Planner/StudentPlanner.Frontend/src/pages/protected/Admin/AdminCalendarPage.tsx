import { useState, type ReactNode} from "react";
import EventPanel from "../../../components/calendar/EventPanel";
import ManagerCalendar from "../../../features/managerCalendar/components/ManagerCalendar";
import type { personalEventResponse } from "../../../types/personalEventTypes";
import { useAllEventRequests } from "../../../features/eventRequests/hooks/eventRequestHooks";
import { EventPreview } from "../../../features/events/components/EventPreview";
import AdminRequestPreview from "../../../features/eventRequests/components/AdminRequestPreview";

export default function AdminCalendarPage(){
    const [viewRequests, setVievRequests] = useState(false);
    const {eventRequests} = useAllEventRequests();
    const top10:personalEventResponse[] = [];
    // [...events].sort((a, b) => {
    //     const dateA = new Date(a.startTime);
    //     const dateB = new Date(b.startTime);
    //     return dateB.getTime() - dateA.getTime();
    // }).filter(d => new Date(d.startTime).getTime() > Date.now()).slice(0, 10);

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
    
    
    return <>
        <ManagerCalendar events={[]}/>
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