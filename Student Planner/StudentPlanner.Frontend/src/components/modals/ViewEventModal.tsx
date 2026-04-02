import Modal from "./Modal";
import { useGetPersonalEvent } from "../../hooks/personalEventHooks";
import { useContext } from "react";
import { ModalContext } from "../../store/ModalContext";

type createEventProps = {
    requiresRole?: ("Student" | "Manager" | "Admin") [],
    eventId: string,
    onClose: () => void
}


export default function ViewEventModal({ eventId, onClose }: createEventProps) {
    const { event, isLoading, deleteEvent} = useGetPersonalEvent(eventId);
    const {open} = useContext(ModalContext);

    if (isLoading || !event) return <Modal open>Loading...</Modal>;

    const handleDelete = async () => {
        await deleteEvent();
        onClose();
    }

    return (
        <Modal open onClose={onClose}>
           <h2>{event.title}</h2>

            <div className="view-section">
                <p className="view-label">Details</p>
                <div className="view-content">
                    <p><strong>Location:</strong> {event.location}</p>
                    <p>{event.startTime} - {event.endTime}</p>
                </div>
            </div>

            <div className="view-section">
                <p className="view-label">Description</p>
                <p className="view-text">{event.description}</p>
            </div>

           <div className="modal-actions">
                <button className="btn-secondary" onClick={handleDelete}>Delete</button>
                <button className="btn-primary" onClick={() => open({type: "edit", eventId: event.id})}>Edit</button>
           </div>
        </Modal>
    );
}