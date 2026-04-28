import { useUser } from "../../../global-hooks/authHooks"
import type { academicEventResponse } from "../../../types/academic-event.types"
import { formatDate } from "../../../api/helpers";
import { useSubscribeToAcademicEvent, useUnsubscribeFromAcademicEvent } from "../hooks/academicEventHook";

type academicEventCardProps = {
    event: academicEventResponse
}

export default function AcademicEventCard({event}: academicEventCardProps){
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

    return <article>
        <div>
            <h2>{event.title}</h2>
            <p>{event.facultyName ?? "University Event"}</p>
        </div>
        <div>
            <p>{event.location}</p>
            <p>{formatDate(event.startTime)} - {formatDate(event.endTime)}</p>
        </div>
        <p>{event.description}</p>

        {user?.userRole==="Student" && 
        <div>
            <button onClick={handleSubscription} disabled={isPending}>
                {event.isSubscribed ? "Unsubscribe" : "Subscribe"}
            </button>
        </div>}
    </article>
}
