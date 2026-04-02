import { useContext } from "react";
import { ModalContext } from "../../store/ModalContext";
import CreateEventModal from "./CreateEventModal";
import EventListModal from "./EventListModal";
import EditEventModal from "./EditEventModal";
import ViewEventModal from "./ViewEventModal";

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
        default: return null;
    }
}