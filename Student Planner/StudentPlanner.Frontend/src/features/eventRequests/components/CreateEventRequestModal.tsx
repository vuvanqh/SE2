import Modal from "../../../components/modals/Modal";
import EventForm from "../../../components/common/EventForm";
import { useAllEventRequests } from "../hooks/eventRequestHooks";
import { extractErrors } from "../../../api/helpers";
import type { createEventRequest, eventDetails } from "../../../types/eventRequestTypes";
import { useUser } from "../../../global-hooks/authHooks";

type createEventRequestProps = {
    requiresRole?: ("Student" | "Manager" | "Admin") [],
    startTime?: string,
    onClose: () => void
}

export default function CreateEventRequestModal({ startTime, onClose }: createEventRequestProps) {
    const { createRequest } = useAllEventRequests();
    const {user} = useUser();
    const initial = {
        title: "",
        location: "",
        startTime: startTime ?? "",
        endTime: startTime ?? "",
        description: "",
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
            requestType: "Create",
            eventDetails: data
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
                <h2>Create Event Request</h2>
            </div>
            <hr className="modal-divider" />

            <EventForm initialValues={initial} submitLabel="Create" onClose={onClose} onSubmit={handleSubmit}/>
        </Modal>
    );
}