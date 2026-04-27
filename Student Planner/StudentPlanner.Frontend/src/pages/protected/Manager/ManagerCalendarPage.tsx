import { useState,  useContext , type ReactNode} from "react";
import EventPanel from "../../../components/calendar/EventPanel";
import Calendar from "../../../components/calendar/Calendar";
import { useMyEventRequests } from "../../../features/eventRequests/hooks/eventRequestHooks";
import { EventPreview } from "../../../features/events/components/EventPreview";
import { EventRequestPreview } from "../../../features/eventRequests/components/EventRequestPreview";
import { ModalContext } from "../../../store/ModalContext";
import useEventPreviews from "../../../global-hooks/eventPreviewHooks";
import type { eventPreviewResponse } from "../../../types/eventPreviewResponse";
import { getNEvents } from "../../../api/helpers";

export default function ManagerCalendarPage(){
    const [viewRequests, setVievRequests] = useState(false);
    const {eventRequests} = useMyEventRequests();
    const {open} = useContext(ModalContext);
    const [range, setRange] = useState<{ from?: Date; to?: Date;}>({});
    
    const {eventPreviews} = useEventPreviews({
        from: range.from,
        to: range.to,
    });
    
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
                <EventRequestPreview eventRequest={e} />
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
        <Calendar events={top10??[]} onDateClick={(start: string) => open({type: "createRequest", startTime: start})}
            onRangeChange={(from, to) =>setRange({ from, to })}/>
        <EventPanel label={viewRequests?"Recent Requests":"Upcoming events"}>
            <div className="events-controls">
                <button className="primary-action" onClick={()=>open({type:"createRequest"})}>+ Create Request</button>

                <div className="toggle-group">
                    <button className={`toggle-btn ${!viewRequests ? "active" : ""}`} onClick={()=>setVievRequests(false)}>View Events</button>
                    <button className={`toggle-btn ${viewRequests ? "active" : ""}`} onClick={()=>setVievRequests(true)}>View Requests</button>
                </div>
            </div>
            
            {content}
        </EventPanel>
    </>
}