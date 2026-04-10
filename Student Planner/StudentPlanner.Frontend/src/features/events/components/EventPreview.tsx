import { formatDate } from "../../../api/helpers";
import type { personalEventResponse } from "../../../types/personalEventTypes";

export function EventPreview({ event }: {event: personalEventResponse}) {
  return (
    <button className="event-item">
      <div className="event-title">
        <span>{event.title}</span>
      </div>

      <div className="event-time">
        {formatDate(event.startTime)} - {formatDate(event.endTime)}
      </div>
    </button>
  );
}