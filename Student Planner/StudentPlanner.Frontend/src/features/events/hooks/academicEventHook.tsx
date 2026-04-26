import { useQuery } from "@tanstack/react-query";
import type { academicEventResponse } from "../../../types/academic-event.types";
import { getAcademicEvent, getAcademicEventByFaculty, getAcademicEvents } from "../../../api/academic-events.api";
import { useUser } from "../../../global-hooks/authHooks";

export function useGetAllAcademicEvents(){
    const {data, isLoading} = useQuery<academicEventResponse[]>({
        queryKey: ["academic-events"],
        queryFn: getAcademicEvents
    })
    return {
        events: data??[],
        isLoading
    }
}

export function useGetAcademicEvent(eventId:string){
    const {data, isLoading} = useQuery<academicEventResponse>({
        queryKey: ["academic-event", eventId],
        queryFn: () => getAcademicEvent(eventId),
    })

    return {
        event: data as academicEventResponse,
        isLoading,
    }
}

export function useGetAcademicEventByFaculty(){
    const {user} = useUser();
    const {data, isLoading} = useQuery<academicEventResponse[]>({
        queryKey: ["academic-event", user? user.facultyId: "faculty"],
        queryFn: getAcademicEventByFaculty
    })

    return {
        events: data?? [],
        isLoading,
    }
}