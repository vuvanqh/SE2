import { formatDate } from "../../../api/helpers";
import type { eventRequestResponse } from "../../../types/eventRequestTypes";
import { useContext } from "react";
import { ModalContext } from "../../../store/ModalContext";

export function EventRequestPreview({ eventRequest }: { eventRequest: eventRequestResponse }) {
    const details = eventRequest.eventDetails;
    const {open} = useContext(ModalContext);
    return (
        <button className="event-item" onClick={()=>open({type:"viewRequest", requestId:eventRequest.id})}>
            <div className="event-title">
                <span>{details.title}</span>
                <span className={`event-badge ${eventRequest.status.toLowerCase()}`}>
                {eventRequest.status}
                </span>
            </div>

            <div className="event-time">
                {formatDate(details.startTime)} - {formatDate(details.endTime)}
            </div>

            <div className="event-time">
                {eventRequest.requestType}
            </div>
        </button>
    );
}