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
           <h2>{event.title}</h2>

           <ViewEventDetails location={`${event.buildingName} ${event.roomNumber}}`} startTime={event.startTime} endTime={event.endTime} />
        </Modal>
    );
}
