import Calendar from "../../../components/calendar/Calendar";
import type { personalEventResponse } from "../../../types/personalEventTypes";
import { useContext } from "react";
import { ModalContext } from "../../../store/ModalContext";

type managerCalendarProps = {
    events: personalEventResponse[],
}

export default function ManagerCalendar({events}: managerCalendarProps){
    const {open} = useContext(ModalContext);

    return <>
        <Calendar events={events??[]} onDateClick={(start: string) => open({type: "createRequest", startTime: start})}/>
    </>
}