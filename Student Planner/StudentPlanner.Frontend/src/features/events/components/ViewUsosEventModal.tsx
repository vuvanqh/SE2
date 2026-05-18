import Modal from "../../../components/modals/Modal";
import ViewEventDetails from "../../../components/common/ViewEventDetails";
import { useGetUsosEvent } from "../hooks/usosEventHooks";

type createEventProps = {
    requiresRole?: ("Student" | "Manager" | "Admin") [],
    eventId: string,
    onClose: () => void
}


export default function ViewUsosEventModal({ eventId, onClose }: createEventProps) {
    const { event, isLoading} = useGetUsosEvent(eventId);

    if (isLoading || !event) return <Modal open>Loading...</Modal>;
    return (
        <Modal open onClose={onClose}>
           <div className="view-header">
                <div>
                    <h2>{event.title}</h2>
                    <p className="view-sub">USOS Event</p>
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

           <ViewEventDetails location={`${event.buildingName} ${event.roomNumber}}`} startTime={event.startTime} endTime={event.endTime} />
        </Modal>
    );
}
