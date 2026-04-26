import Calendar from "../../../components/calendar/Calendar";
import { useContext } from "react";
import { ModalContext } from "../../../store/ModalContext";
import type { eventPreviewResponse } from "../../../types/eventPreviewResponse";

export default function StudentCalendar({events}:{events: eventPreviewResponse[]}){
    const {open} = useContext(ModalContext);

    return <>
        <Calendar events={events}
            onDateClick={(start: string) => open({type: "createPersonal", startTime: start})}/>
    </>
}