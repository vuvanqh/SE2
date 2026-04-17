import FullCalendar from '@fullcalendar/react'
import dayGridPlugin from '@fullcalendar/daygrid' // a plugin!
import interactionPlugin from "@fullcalendar/interaction"  // needed for dayClick
import type { EventContentArg } from '@fullcalendar/core'
import type { DateClickArg } from '@fullcalendar/interaction'
import type { EventClickArg } from '@fullcalendar/core'
import type { personalEventResponse } from '../../types/personalEventTypes'
import timeGridPlugin from "@fullcalendar/timegrid";
import { useRef, useContext, useState } from "react";
import { ModalContext } from '../../store/ModalContext'
import { isSameDay, toLocalInput } from '../../api/helpers'
import useEventPreviews from '../../global-hooks/eventPreviewHooks';
//import useUsosEvents from '../../global-hooks/usosHooks'

type calendarProps = {
  events: personalEventResponse[],
  onDateClick: (date:string) => void
}

export default function Calendar({events, onDateClick}:calendarProps) {
  const {open} = useContext(ModalContext);
  const [range, setRange] = useState<{from?: string, to?: string}>({});
  
  let from = range.from? new Date(range.from): new Date(Date.now());
  var date = new Date(from);
  let to = range.to? new Date(range.to): new Date(date.setMonth(date.getMonth() + 1));
  const { eventPreviews } = useEventPreviews(from, to);
  //const {usosEvents} = useUsosEvents(from, to);
  const handleDatesSet = (arg: any) => {
    const from = toLocalInput(arg.start);

    const end = new Date(arg.end);
    end.setDate(end.getDate() - 1);
    const to = toLocalInput(end);

    setRange(prev => {
      if (prev.from === from && prev.to === to) return prev;
      return { from, to };
    });
  };

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
    onDateClick(toLocalInput(start));
  }


  const mappedEvents = eventPreviews.map(e => ({
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

  return <>
  <div className="calendar-wrapper">
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
      datesSet={handleDatesSet}
    />
  </div>
  </>
}



function renderEventContent(eventInfo: EventContentArg) {
  return(
    <div className="event-content">
      <div className="event-title">{eventInfo.event.title}</div>
      <div className="event-time">{eventInfo.timeText}</div>
    </div>
  );
}