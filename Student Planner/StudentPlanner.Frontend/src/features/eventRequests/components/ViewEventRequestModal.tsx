import Modal from "../../../components/modals/Modal";
import ViewEventDetails from "../../../components/common/ViewEventDetails";
import { useEventRequest } from "../hooks/eventRequestHooks";
import { useUser } from "../../../global-hooks/authHooks"

type createEventProps = {
    requiresRole?: ("Student" | "Manager" | "Admin")[],
    requestId: string,
    onClose: () => void
}

export default function ViewEventRequestModal({ requestId, onClose }: createEventProps) {
    const { eventRequest, isPending, deleteRequest } = useEventRequest(requestId);
    const { user } = useUser();

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

            <ViewEventDetails location={eventDetails.location} startTime={eventDetails.startTime} endTime={eventDetails.endTime} description={eventDetails.description} />

            {user && user.userRole == "Manager" && eventRequest.status == "Pending" &&
                <div className="modal-actions">
                    <button className="btn-secondary" onClick={handleDelete}>Delete</button>
                </div>}

            {user && user.userRole == "Admin" && eventRequest.status == "Pending" &&
                <div>
                    <button>Approve</button>
                    <button>Reject</button>
                </div>}
        </Modal>
    );
}