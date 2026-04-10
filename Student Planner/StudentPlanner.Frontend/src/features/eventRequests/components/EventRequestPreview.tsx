import { formatDate } from "../../../api/helpers";
import type { eventRequestResponse } from "../../../types/eventRequestTypes";

export function EventRequestPreview({ eventRequest }: { eventRequest: eventRequestResponse }) {
    const details = eventRequest.eventDetails;
    return (
        <>
            <div className="event-title">
                <span>{details.title}</span>
                <span className={`event-badge ${status.toLowerCase()}`}>
                {eventRequest.status}
                </span>
            </div>

            <div className="event-time">
                {formatDate(details.startTime)} - {formatDate(details.endTime)}
            </div>

            <div className="event-time">
                {eventRequest.requestType}
            </div>
        </>
    );
}