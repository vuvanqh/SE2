import Calendar from "../../../components/calendar/Calendar";
import { useContext } from "react";
import { ModalContext } from "../../../store/ModalContext";
import type { eventPreviewResponse } from "../../../types/eventPreviewResponse";

type managerCalendarProps = {
    events: eventPreviewResponse[],
}

export default function ManagerCalendar({events}: managerCalendarProps){
    const {open} = useContext(ModalContext);

    return <>
        <Calendar events={events??[]} onDateClick={(start: string) => open({type: "createRequest", startTime: start})}/>
    </>
}