import Modal from "../../../components/modals/Modal";
import { useGetAcademicEvent } from "../hooks/academicEventHook";
import { useUser } from "../../../global-hooks/authHooks";

type createEventProps = {
    requiresRole?: ("Student" | "Manager" | "Admin") [],
    eventId: string,
    onClose: () => void
}


export default function ViewAcademicEventModal({ eventId, onClose }: createEventProps) {
    const { event, isLoading} = useGetAcademicEvent(eventId);
    const {user} = useUser();

    if (isLoading || !event) return <Modal open>Loading...</Modal>;
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

           {user?.userRole=="Student" && <div className="modal-actions">
                <button className="btn-secondary">Unsubscribe</button>
           </div>}
        </Modal>
    );
}