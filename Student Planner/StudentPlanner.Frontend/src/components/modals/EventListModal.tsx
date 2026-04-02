import Modal from "./Modal";
import { useContext } from "react";
import { ModalContext } from "../../store/ModalContext";
import type { personalEventResponse } from "../../types/personalEventTypes";

export default function EventListModal({events}:{events: personalEventResponse[]}){
    const {close, open} = useContext(ModalContext);

    return <Modal open onClose={close}>
        <h2>Event List</h2>
        <ul className="event-list">
            {events.map(e => (
                <li key={e.id}>
                    <button className="event-item" onClick={() => open({type: "view", eventId: e.id})}>
                        <span className="event-title">{e.title}</span>
                        <span className="event-time">
                        {e.startTime} - {e.endTime}
                        </span>
                    </button>
                </li>
            ))}
        </ul>
    </Modal>
}