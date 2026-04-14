import Modal from "../../../components/modals/Modal";
import EventForm from "../../../components/common/EventForm";
import { useCreateRequest, useEventRequest } from "../hooks/eventRequestHooks";
import { extractErrors } from "../../../api/helpers";
import type { createEventRequest, eventDetails } from "../../../types/eventRequestTypes";
import { useUser } from "../../../global-hooks/authHooks";

type createEventRequestProps = {
    requiresRole?: ("Student" | "Manager" | "Admin") [],
    requestId: string,
    onClose: () => void
}

export default function EditEventRequestModal({ requestId, onClose }: createEventRequestProps) {
    const { eventRequest, isPending} = useEventRequest(requestId);
    const {createRequest} = useCreateRequest();
    const {user} = useUser();
    if(eventRequest==undefined || isPending)
        return null;

    const details = eventRequest?.eventDetails;

    const initial = {
        title: details.title,
        location: details.location,
        startTime: details.startTime,
        endTime: details.endTime,
        description: details.description,
        errors: []
    };
    console.log(localStorage.getItem("facultyId"), user);
    const handleSubmit = async (data:eventDetails) => {
        let errors = []
        if(data.description?.trim().length==0)
            errors.push("Description must be provided.");
        if(data.location?.trim().length==0)
            errors.push("Location must be specified.");
        
        const payload: createEventRequest = {
            facultyId: localStorage.getItem("facultyId")!,
            requestType: "Update",
            eventDetails: data,
            eventId: eventRequest.id
        }
            
        try {
            await createRequest(payload);
            onClose();
            return null;
        } catch (e) {
            return [...errors, ...extractErrors(e)];
        }
    }
    return (
        <Modal open onClose={onClose}>
            <div className="modal-header">
                <h2>Update Event Request</h2>
            </div>
            <hr className="modal-divider" />

            <EventForm initialValues={initial} submitLabel="Create" onClose={onClose} onSubmit={handleSubmit}/>
        </Modal>
    );
}