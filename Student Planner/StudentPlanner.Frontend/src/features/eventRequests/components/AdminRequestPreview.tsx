import type { eventRequestResponse } from "../../../types/eventRequestTypes";
import { useEventRequest } from "../hooks/eventRequestHooks";
import { EventRequestPreview } from "./EventRequestPreview";

type adminRequestPreviewProps = {
    eventRequest: eventRequestResponse
}

export default function AdminRequestPreview({eventRequest}: adminRequestPreviewProps){
    const {approveRequest, rejectRequest} = useEventRequest(eventRequest.id);
    return  <div className="admin-request-row">
        <EventRequestPreview eventRequest={eventRequest}/>

        {eventRequest.status==="Pending" && <div className="admin-actions">
            <button className="approve-btn" onClick={() => approveRequest()}>Approve</button>
            <button className="reject-btn" onClick={() => rejectRequest()}>Reject</button>
        </div>}
    </div>
}

