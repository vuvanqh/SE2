import { useUser } from "../../../global-hooks/authHooks"
import type { academicEventResponse } from "../../../types/academic-event.types"

type academicEventCardProps = {
    event: academicEventResponse
}

export default function AcademicEventCard({event}: academicEventCardProps){
    const subscribedToEvents: academicEventResponse[] = []
    const {user} = useUser();

    return <article>
        <div>
            <h2>{event.title}</h2>
            <p>{event.facultyId}</p>
        </div>
        <div>
            <p>{event.location}</p>
            <p>{event.startTime} - {event.endTime}</p>
        </div>
        <p>{event.description}</p>

        {user?.userRole==="Student" && 
        <div>
            <button>
                {subscribedToEvents.includes(event) ? "Unsubscribe" : "Subscribe"}
            </button>
        </div>}
    </article>
}