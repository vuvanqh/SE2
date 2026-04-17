import { useFaculty } from "../../../global-hooks/facultyHooks"
import type { eventRequestResponse } from "../../../types/eventRequestTypes"
import { useUser } from "../../../global-hooks/authHooks"

type eventRequestPreviewCardProps = {
    eventRequest: eventRequestResponse
}

export default function EventRequestPreveiwCard({eventRequest}: eventRequestPreviewCardProps){
    const {faculty, isPending} = useFaculty({facultyId: eventRequest.facultyId})
    const {user} = useUser();

    if(isPending)
        return <p>Loading...</p>;
    
    return <article className="event-request-card">
        <div>
            <div>
                <p>{eventRequest.eventDetails.title}</p>
                <p>{faculty?.facultyCode}</p>
            </div>

            <div>
                <p>{eventRequest.requestType}</p>
                <p>{eventRequest.status}</p>
            </div>
        </div>

        <div>
            <p>{eventRequest.eventDetails.location}</p>
            <p>{eventRequest.eventDetails.startTime} - {eventRequest.eventDetails.endTime}</p>
        </div>

        {user?.userRole==="Admin" &&
        <div>
            <button>Approve</button>
            <button>Reject</button>
        </div>}
    </article>
}