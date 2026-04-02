import { useQuery, useMutation } from "@tanstack/react-query";
import { createPersonalEvent, getPersonalEventById, getPersonalEvents, deletePersonalEvent, updatePersonalEvent } from "../api/personalEventClient";
import type { personalEventResponse, updatePersonalEventRequest } from "../types/personalEventTypes";
import { queryClient } from "../api/queryClient";

export function useGetAllPersonalEvents(){
    const {data, isLoading} = useQuery<personalEventResponse[]>({
        queryKey: ["personal-events"],
        queryFn: getPersonalEvents
    })

    const {mutateAsync: createEvent} = useMutation({
        mutationFn: createPersonalEvent,
        onSuccess: () => {
            invalidatePE();
        }
    })
    return {
        events: data??[],
        isLoading,
        createEvent
    }
}

export function useGetPersonalEvent(eventId:string){
    console.log("HOOK CALLED WITH:", eventId);
    const {data, isLoading} = useQuery<personalEventResponse>({
        queryKey: ["personal-event", eventId],
        queryFn: () => getPersonalEventById(eventId),
    })

    const {mutateAsync: updateEvent} = useMutation({
        mutationFn: (request: updatePersonalEventRequest) => updatePersonalEvent(eventId, request),
        onSuccess: () => {
            invalidatePE()
        }
    })

    const {mutateAsync: deleteEvent} = useMutation({
        mutationFn: () => deletePersonalEvent(eventId),
        onSuccess: () => {
            invalidatePE()
        }
    })

    return {
        event: data as personalEventResponse,
        isLoading,
        updateEvent,
        deleteEvent
    }
}


const invalidatePE = () => queryClient.invalidateQueries({queryKey: ["personal-events"]})