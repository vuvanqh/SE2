import type { eventPreviewResponse } from "../../../types/eventPreviewResponse";
import ViewAcademicEventModal from "./ViewAcademicEventModal";
import ViewEventModal from "./ViewEventModal";
import ViewUsosEventModal from "./ViewUsosEventModal";

interface ViewEventRootProps {
    eventPreview: eventPreviewResponse;
    onClose: () => void;
}

export default function ViewEventRoot({eventPreview, onClose}: ViewEventRootProps){
    switch(eventPreview.eventType){
        case "AcademicEvent": return <ViewAcademicEventModal eventId={eventPreview.id} onClose={onClose}/>;
        case "PersonalEvent": return <ViewEventModal eventId={eventPreview.id} onClose={onClose}/>;
        case "UsosEvent": return <ViewUsosEventModal eventId={eventPreview.id} onClose={onClose}/>;
        default:
            return <div>Unknown Event Type</div>
    }
}