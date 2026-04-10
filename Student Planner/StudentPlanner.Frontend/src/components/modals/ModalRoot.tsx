import { useContext } from "react";
import { ModalContext } from "../../store/ModalContext";
import CreateEventModal from "../../features/events/components/CreateEventModal";
import EventListModal from "../../features/events/components/EventListModal";
import EditEventModal from "../../features/events/components/EditEventModal";
import ViewEventModal from "../../features/events/components/ViewEventModal";
import CreateEventRequestModal from "../../features/eventRequests/components/CreateEventRequestModal";

export default function ModalRoot(){
    const {state, close} = useContext(ModalContext);
    if(state.stack.length==0)
        return null;

    const modal = state.stack[state.stack.length - 1];
    switch(modal?.type)
    {
        case "createPersonal": return <CreateEventModal requiresRole={["Student"]} startTime={modal.startTime} key={modal.startTime} onClose={close}/>;
        case "eventList": return <EventListModal events={modal.events}/>;
        case "view": return <ViewEventModal eventId={modal.eventId} onClose={close}/>
        case "edit": return <EditEventModal eventId={modal.eventId} onClose={close}/>
        case "createRequest": return <CreateEventRequestModal requiresRole={["Manager"]} startTime={modal.startTime} onClose={close}/>
        default: return null;
    }
}