import { formatDate } from "../../../api/helpers";
import type { personalEventResponse } from "../../../types/personalEventTypes";
import type { academicEventResponse } from "../types/academic-event.types"
import type { usosEventResponse } from "../hooks/usosEventHooks";

export function EventPreview({ event }: {event: personalEventResponse | academicEventResponse | usosEventResponse}) {
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