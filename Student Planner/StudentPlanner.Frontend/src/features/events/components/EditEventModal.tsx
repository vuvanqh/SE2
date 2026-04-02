import Modal from "../../../components/modals/Modal";
import { useGetPersonalEvent } from "../hooks/personalEventHooks";
import { extractErrors } from "../../../api/helpers";
import EventForm from "../../../components/common/EventForm";
import type { createPersonalEventRequest } from "../../../types/personalEventTypes";

type createEventProps = {
    requiresRole?: ("Student" | "Manager" | "Admin") [],
    eventId: string,
    onClose: () => void
}


export default function EditEventModal({ eventId, onClose }: createEventProps) {
    const { event, isLoading, updateEvent } = useGetPersonalEvent(eventId);

    if (isLoading || !event) return <Modal open>Loading...</Modal>;

    const initial = {
        title: event.title,
        location: event.location,
        startTime: event.startTime,
        endTime: event.endTime,
        description: event.description,
        errors: []
    };

    const handleSubmit = async (data:createPersonalEventRequest) => {
        try {
                await updateEvent(data);
                onClose();
                return null;
            } catch (e) {
                return extractErrors(e);
            }
        }

    return (
        <Modal open onClose={onClose}>
            <div className="modal-header">
                <h2>Edit Event</h2>
            </div>
            <hr className="modal-divider" />

            <EventForm initialValues={initial} onClose={onClose} submitLabel="Save" onSubmit={handleSubmit}/>
        </Modal>
    );
}