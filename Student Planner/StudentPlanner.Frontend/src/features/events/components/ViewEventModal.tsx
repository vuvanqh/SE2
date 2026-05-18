import Modal from "../../../components/modals/Modal";
import ViewEventDetails from "../../../components/common/ViewEventDetails";
import { useGetPersonalEvent } from "../hooks/personalEventHooks";
import { useContext } from "react";
import { ModalContext } from "../../../store/ModalContext";

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
           <div className="view-header">
                <div>
                    <h2>{event.title}</h2>
                    <p className="view-sub">Personal Event</p>
                </div>
                <button
                    className="modal-close-btn"
                    onClick={onClose}
                    type="button"
                    aria-label="Close event details"
                >
                    x
                </button>
           </div>

           <ViewEventDetails location={event.location} startTime={event.startTime} endTime={event.endTime} description={event.description} />

           <div className="modal-actions">
                <button className="btn-secondary" onClick={handleDelete}>Delete</button>
                <button className="btn-primary" onClick={() => open({type: "edit", eventId: event.id})}>Edit</button>
           </div>
        </Modal>
    );
}
