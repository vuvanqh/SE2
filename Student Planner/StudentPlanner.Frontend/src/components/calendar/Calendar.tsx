import FullCalendar from '@fullcalendar/react'
import dayGridPlugin from '@fullcalendar/daygrid' // a plugin!
import interactionPlugin from "@fullcalendar/interaction"  // needed for dayClick
import type { EventContentArg } from '@fullcalendar/core'
import type { DateClickArg } from '@fullcalendar/interaction'
import type { EventClickArg } from '@fullcalendar/core'


const handleEventClick = (arg: EventClickArg) => {
  alert(`Event: ${arg.event.title}`)
}

const handleDateClick = (arg: DateClickArg) => {
    alert(arg.dateStr)
}

export default function Calendar() {
  return <div className="calendar-wrapper">
    <FullCalendar
    plugins={[ dayGridPlugin, interactionPlugin ]}
    initialView="dayGridMonth"
    weekends={true}
    firstDay={1}
    eventContent={renderEventContent}   
    events={[
        { title: 'event 1', date: '2026-03-28' },
        { title: 'event 2', date: '2019-04-02' }
    ]}
    eventClick={handleEventClick}
    dateClick={handleDateClick}
    />
  </div>
}



function renderEventContent(eventInfo: EventContentArg) {
  return(
    <>
      <b>{eventInfo.timeText}</b>
      <i>{eventInfo.event.title}</i>
    </>
  )
}