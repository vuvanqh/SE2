import FullCalendar from '@fullcalendar/react'
import dayGridPlugin from '@fullcalendar/daygrid' // a plugin!
import interactionPlugin from "@fullcalendar/interaction"  // needed for dayClick
import type { EventContentArg } from '@fullcalendar/core'
import type { DateClickArg } from '@fullcalendar/interaction'
import type { EventClickArg } from '@fullcalendar/core'
import type { personalEventResponse } from '../../types/personalEventTypes'
import timeGridPlugin from "@fullcalendar/timegrid";
import { useRef, useContext } from "react";
import { ModalContext } from '../../store/ModalContext'
import { isSameDay, toLocalInput } from '../../api/helpers'

export default function Calendar({events}:{events: personalEventResponse[]}) {
  const {open} = useContext(ModalContext);
  const handleEventClick = (arg: EventClickArg) => {
    open({type: "view", eventId: arg.event.id});
  }
  const handleDateClick = (arg: DateClickArg) => {
    if (arg.view.type !== "timeGridWeek")
    {
      const filtered = events.filter(e => isSameDay(e.startTime, arg.date));
      open({type: "eventList", events: filtered})
      return;
    }

    const start = new Date(arg.date);
    open({type: "createPersonal", startTime: toLocalInput(start)});
  }


  const mappedEvents = events.map(e => ({
    id: e.id,
    title: e.title,
    start: new Date(e.startTime),
    end: new Date(e.endTime),
    extendedProps: {
      description: e.description,
      location: e.location
    }
  }));

  const calendarRef = useRef<any>(null);

  return <div className="calendar-wrapper">
    <FullCalendar
     ref={calendarRef}
     headerToolbar={{
        left: "title",
        center: "",
        right: "dayGridMonth,timeGridWeek,today prev,next"
      }}
      plugins={[dayGridPlugin, timeGridPlugin, interactionPlugin]}
      initialView="dayGridMonth"
      weekends={true}
      firstDay={1}
      eventContent={renderEventContent}   
      events={[
          { title: 'event 1', start: '2026-03-28' },
          { title: 'event 2', start: '2019-04-02' },
          ...mappedEvents
      ]}
      eventClick={handleEventClick}
      dateClick={handleDateClick}
      eventTimeFormat={{
        hour: 'numeric',
        minute: '2-digit',
        meridiem: 'short'
      }}
    />
  </div>
}



function renderEventContent(eventInfo: EventContentArg) {
  return(
    <div className="event-content">
      <div className="event-time">{eventInfo.timeText}</div>
      <div className="event-title">{eventInfo.event.title}</div>
    </div>
  );
}