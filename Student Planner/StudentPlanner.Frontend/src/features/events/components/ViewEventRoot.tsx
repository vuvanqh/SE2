import type { eventPreviewResponse } from "../../../types/eventPreviewResponse";
import ViewAcademicEventModal from "./ViewAcademicEventModal";
import ViewEventModal from "./ViewEventModal";

export default function ViewEventRoot({eventPreveiw, onClose}: {eventPreveiw: eventPreviewResponse, onClose: () => void}){
    switch(eventPreveiw.eventType){
        case "AcademicEvent": return <ViewAcademicEventModal eventId={eventPreveiw.id} onClose={onClose}/>;
        case "PersonalEvent": return <ViewEventModal eventId={eventPreveiw.id} onClose={onClose}/>;
        default:
            return <div>Unknown Event Type</div>
    }
}