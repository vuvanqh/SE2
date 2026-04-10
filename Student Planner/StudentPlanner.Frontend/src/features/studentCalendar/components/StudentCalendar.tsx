import Calendar from "../../../components/calendar/Calendar";
import type { personalEventResponse } from "../../../types/personalEventTypes";
import { useContext } from "react";
import { ModalContext } from "../../../store/ModalContext";

export default function StudentCalendar({events}:{events: personalEventResponse[]}){
    const {open} = useContext(ModalContext);

    return <>
        <Calendar events={events}
            onDateClick={(start: string) => open({type: "createPersonal", startTime: start})}/>
    </>
}