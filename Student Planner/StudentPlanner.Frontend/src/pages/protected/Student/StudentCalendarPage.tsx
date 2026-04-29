import EventPanel from "../../../components/calendar/EventPanel";
import Calendar from "../../../components/calendar/Calendar";
import useEventPreviews from "../../../global-hooks/eventPreviewHooks";
import { useContext, useState } from "react";
import { ModalContext } from "../../../store/ModalContext";
import type { eventPreviewResponse } from "../../../types/eventPreviewResponse";
import { getNEvents } from "../../../api/helpers";
import { EventPreview } from "../../../features/events/components/EventPreview";

export default function StudentCalendarPage(){
    const {open} = useContext(ModalContext);
    const [range, setRange] = useState<{ from?: Date; days?: number;}>({});
        
    const {eventPreviews} = useEventPreviews({
        from: range.from,
        days: range.days,
    });
    const top10:eventPreviewResponse[] = getNEvents(eventPreviews,10);
    console.log(eventPreviews);
    return <>
        <Calendar events={eventPreviews} onDateClick={(start: string) => open({type: "createPersonal", startTime: start})}
            onRangeChange={(from, days) =>setRange({ from, days })}/>
        <EventPanel label="Upcoming Events">
            {top10.length==0?<p>No upcoming events...</p>:
            <ul className="events-list">
            {top10.map(e => (
                <li key={e.id}>
                    <EventPreview event={e} />
                </li>
                ))}
            </ul>}
        </EventPanel>
    </>
}