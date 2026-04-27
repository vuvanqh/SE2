import { useQuery } from "@tanstack/react-query";
import type { academicEventResponse } from "../../../types/academic-event.types";
import { getAcademicEvent, getAcademicEvents } from "../../../api/events/academic-events.api";

export function useAccessibleAcademicEvents(facultyIds?: string[]){
    facultyIds?.slice().sort().join(",")
    const {data, isLoading} = useQuery<academicEventResponse[]>({
        queryKey: ["academic-events", facultyIds],
        queryFn: () =>getAcademicEvents(facultyIds)
    })
    return {
        events: data??[],
        isLoading
    }
}

export function useGetAcademicEvent(eventId:string){
    const {data, isLoading} = useQuery<academicEventResponse>({
        queryKey: ["academic-events", "event",eventId],
        queryFn: () => getAcademicEvent(eventId),
        enabled: !!eventId
    })

    return {
        event: data as academicEventResponse,
        isLoading,
    }
}
