import Modal from "../../../components/modals/Modal";
import ViewEventDetails from "../../../components/common/ViewEventDetails";
import { useUser } from "../../../global-hooks/authHooks";
import { useGetAcademicEvent, useSubscribeToAcademicEvent, useUnsubscribeFromAcademicEvent } from "../hooks/academicEventHook";

type createEventProps = {
    requiresRole?: ("Student" | "Manager" | "Admin") [],
    eventId: string,
    onClose: () => void
}


export default function ViewAcademicEventModal({ eventId, onClose }: createEventProps) {
    const { event, isLoading} = useGetAcademicEvent(eventId);
    const {user} = useUser();
    const {subscribeToEvent, isPending: isSubscribePending} = useSubscribeToAcademicEvent(eventId);
    const {unsubscribeFromEvent, isPending: isUnsubscribePending} = useUnsubscribeFromAcademicEvent(eventId);

    async function handleSubscription(){
        if (!event) return;

        if (event.isSubscribed) {
            await unsubscribeFromEvent();
            return;
        }

        await subscribeToEvent();
    }

    if (isLoading || !event) return <Modal open>Loading...</Modal>;
    return (
        <Modal open onClose={onClose}>
           <h2>{event.title}</h2>

           <ViewEventDetails location={event.location} startTime={event.startTime} endTime={event.endTime} description={event.description} />

           {user?.userRole=="Student" && <div className="modal-actions">
                <button className="btn-secondary" onClick={handleSubscription} disabled={isSubscribePending || isUnsubscribePending}>
                    {event.isSubscribed ? "Unsubscribe" : "Subscribe"}
                </button>
           </div>}
        </Modal>
    );
}
