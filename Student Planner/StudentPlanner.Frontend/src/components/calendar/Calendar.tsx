import FullCalendar from '@fullcalendar/react'
import dayGridPlugin from '@fullcalendar/daygrid' // a plugin!
import interactionPlugin from "@fullcalendar/interaction"  // needed for dayClick
import type { EventContentArg } from '@fullcalendar/core'
import type { DateClickArg } from '@fullcalendar/interaction'
import type { EventClickArg } from '@fullcalendar/core'
import timeGridPlugin from "@fullcalendar/timegrid";
import { useRef, useContext } from "react";
import { ModalContext } from '../../store/ModalContext'
import { isSameDay, toLocalInput } from '../../api/helpers'
import type { eventPreviewResponse } from '../../types/eventPreviewResponse'

type calendarProps = {
  events: eventPreviewResponse[],
  onRangeChange?: (
    from: Date,
    to: Date
  ) => void;
  onDateClick?: (date:string) => void
}
export default function Calendar({events, onDateClick, onRangeChange}: calendarProps) {
  const { open } = useContext(ModalContext);
  const handleDatesSet = (arg: any) => {
    const from = arg.start;

    const to = new Date(arg.end);
    to.setDate(to.getDate() - 1);

    onRangeChange?.(from, to);
  };


  const handleEventClick = (arg: EventClickArg) => {
    open({ type: "view", eventPreview: arg.event.extendedProps as eventPreviewResponse });
  };

  const handleDateClick = (arg: DateClickArg) => {
    if (arg.view.type !== "timeGridWeek") {
      const filtered = events.filter(e =>
        isSameDay(e.startTime, arg.date)
      );

      open({type: "eventList", events: filtered});
      return;
    }

    const start = new Date(arg.date);
    onDateClick?.(toLocalInput(start));
  };


  const mappedEvents = events.map(e => ({
    id: e.id,
    title: e.title,
    start: new Date(e.startTime),
    end: new Date(e.endTime),
    extendedProps: {
      id: e.id,
      title: e.title,
      startTime: e.startTime,
      endTime: e.endTime,
      description: e.description,
      location: e.location,
      eventType: e.eventType,
    }
  }));


  const calendarRef = useRef<any>(null);

  return (
    <div className="calendar-wrapper">
      <FullCalendar
        ref={calendarRef}
        headerToolbar={{
          left: "title",
          center: "",
          right: "dayGridMonth,timeGridWeek,today prev,next"
        }}
        plugins={[
          dayGridPlugin,
          timeGridPlugin,
          interactionPlugin
        ]}
        initialView="dayGridMonth"
        weekends={true}
        firstDay={1}
        eventContent={renderEventContent}
        events={mappedEvents}
        eventClick={handleEventClick}
        dateClick={handleDateClick}
        datesSet={handleDatesSet}
        eventTimeFormat={{
          hour: "numeric",
          minute: "2-digit",
          meridiem: "short"
        }}
      />
    </div>
  );
}


function renderEventContent(eventInfo: EventContentArg) {
  return(
    <div className="event-content">
      <div className="event-title">{eventInfo.event.title}</div>
      <div className="event-time">{eventInfo.timeText}</div>
    </div>
  );
}