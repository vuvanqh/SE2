import FormModal from "../../../components/common/FormModal";
import EventForm from "../../../components/common/EventForm";
import { useGetAllPersonalEvents } from "../hooks/personalEventHooks";
import { extractErrors } from "../../../api/helpers";
import type { createPersonalEventRequest } from "../../../types/personalEventTypes";

type createEventProps = {
    requiresRole?: ("Student" | "Manager" | "Admin") [],
    startTime?: string,
    onClose: () => void
}

export default function CreateEventModal({ startTime, onClose }: createEventProps) {
  const { createEvent } = useGetAllPersonalEvents();

    const initial = {
        title: "",
        location: "",
        startTime: startTime ?? "",
        endTime: startTime ?? "",
        description: "",
        errors: []
    };
    const handleSubmit = async (data:createPersonalEventRequest) => {
        try {
            await createEvent(data);
            onClose();
            return null;
        } catch (e) {
            return extractErrors(e);
        }
    }
  return (
    <FormModal title="Create Personal Event" open onClose={onClose}>
        <EventForm initialValues={initial} submitLabel="Create" onClose={onClose} onSubmit={handleSubmit}/>
    </FormModal>
  );
}