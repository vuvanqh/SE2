import { useGetAllPersonalEvents } from "../../../features/events/hooks/personalEventHooks";
import EventPanel from "../../../components/calendar/EventPanel";
import { formatDate } from "../../../api/helpers";
import StudentCalendar from "../../../features/studentCalendar/components/StudentCalendar";

export default function StudentCalendarPage(){
    const {events} = useGetAllPersonalEvents();
     const top10 = [...events].sort((a, b) => {
        const dateA = new Date(a.startTime);
        const dateB = new Date(b.startTime);
        return dateB.getTime() - dateA.getTime();
    }).filter(d => new Date(d.startTime).getTime() > Date.now()).slice(0, 10);

    return <>
        <StudentCalendar events={events}/>
        <EventPanel label="Upcoming Events">
            {top10.length==0?<p>No upcoming events...</p>:
            <ul className="events-list">
            {top10.map(e => (
                <li key={e.id} className="event-item">
                    <button className="event-button">{e.title}</button>
                    <p>{formatDate(e.startTime)} - {formatDate(e.endTime)}</p>
                </li>
                ))}
            </ul>}
        </EventPanel>
    </>
}