import Modal from "../../../components/modals/Modal";
import { useEventRequest } from "../hooks/eventRequestHooks";
import { useContext } from "react";
import { ModalContext } from "../../../store/ModalContext";
import { formatDate } from "../../../api/helpers";
import { useUser } from "../../../global-hooks/authHooks"

type createEventProps = {
    requiresRole?: ("Student" | "Manager" | "Admin") [],
    requestId: string,
    onClose: () => void
}

//NOTE: TRY TO CLEAN THE CONDITIONAL LOGIC BLOCK
export default function ViewEventRequestModal({ requestId, onClose }: createEventProps) {
    const { eventRequest, isPending, deleteRequest} = useEventRequest(requestId);
    const {open} = useContext(ModalContext);
    const {user} = useUser();

    if (isPending || !eventRequest) return <Modal open>Loading...</Modal>;

    const handleDelete = async () => {
        await deleteRequest();
        onClose();
    }
    
    const eventDetails = eventRequest.eventDetails;

    return (
        <Modal open onClose={onClose}>
            <div className="view-header">
                <div>
                    <h2>{eventDetails.title}</h2>
                    <p className="view-sub">{eventRequest.requestType}</p>
                </div>
                <span className={`event-badge ${eventRequest.status.toLowerCase()}`}>
                    {eventRequest.status}
                </span>
            </div>

            <div className="view-section">
                <p className="view-label">Details</p>
                <div className="view-content">
                    <p><strong>Location:</strong> {eventDetails.location}</p>
                    <p>{formatDate(eventDetails.startTime)} - {formatDate(eventDetails.endTime)}</p>
                </div>
            </div>

            <div className="view-section">
                <p className="view-label">Description</p>
                <p className="view-text">{eventDetails.description}</p>
            </div>

            {user && user.userRole=="Manager" &&
            <div className="modal-actions">
                    <button className="btn-secondary" onClick={handleDelete}>Delete</button>
                    <button className="btn-primary" onClick={() =>open({type: "editRequest", requestId})}>Edit</button>
            </div>}

            {user && user.userRole=="Admin" && eventRequest.status=="Pending" &&
            <div>
                <button>Approve</button>
                <button>Reject</button>
            </div>}
        </Modal>
    );
}