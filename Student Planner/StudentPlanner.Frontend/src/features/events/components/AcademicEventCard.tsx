import { useUser } from "../../../global-hooks/authHooks"
import type { academicEventResponse } from "../../../types/academic-event.types"
import { formatDate } from "../../../api/helpers";
import { useSubscribeToAcademicEvent, useUnsubscribeFromAcademicEvent } from "../hooks/academicEventHook";

type academicEventCardProps = {
    event: academicEventResponse
    onViewDetails: () => void
}

export default function AcademicEventCard({event, onViewDetails}: academicEventCardProps){
    const {user} = useUser();
    const {subscribeToEvent, isPending: isSubscribePending} = useSubscribeToAcademicEvent(event.id);
    const {unsubscribeFromEvent, isPending: isUnsubscribePending} = useUnsubscribeFromAcademicEvent(event.id);
    const isPending = isSubscribePending || isUnsubscribePending;

    async function handleSubscription(){
        if (event.isSubscribed) {
            await unsubscribeFromEvent();
            return;
        }

        await subscribeToEvent();
    }

    return <article className="academic-event-card">
        <div className="academic-event-card-header">
            <div>
                <h2>{event.title}</h2>
                <p className="academic-event-card-faculty">{event.facultyName ?? "University Event"}</p>
            </div>
            <span className="role-badge">{event.isSubscribed ? "Subscribed" : "Open"}</span>
        </div>

        <div className="academic-event-card-meta">
            {event.location && <p>{event.location}</p>}
            <p>{formatDate(event.startTime)} - {formatDate(event.endTime)}</p>
        </div>

        <p className="academic-event-card-description">{event.description}</p>

        <div className="academic-event-card-actions">
            <button className="ghost-btn" onClick={onViewDetails} type="button">
                Details
            </button>
            {user?.userRole==="Student" && 
            <button className="ghost-btn" onClick={handleSubscription} disabled={isPending} type="button">
                {event.isSubscribed ? "Unsubscribe" : "Subscribe"}
            </button>}
        </div>
    </article>
}
