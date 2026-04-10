import { useQuery, useMutation } from "@tanstack/react-query";
import { approveEventRequest, createEventRequest, deleteEventRequest, getEventRequestById, getMyRequests, rejectEventRequest } from "../../../api/eventRequestApi";
import type { createEventRequest as createRequestType, eventRequestResponse } from "../../../types/eventRequestTypes";

export function useAllEventRequests(){
    const {data, isPending} = useQuery<eventRequestResponse[]>({
        queryKey: ["eventRequests", "all"],
        queryFn: getMyRequests
    })

     const {mutate: createRequest} = useMutation({
        mutationFn: (payload: createRequestType) => createEventRequest(payload)
    })

    return {
        eventRequests: data??[],
        createRequest,
        isPending
    }
}

export function useEventRequest(requestId: string){
    const {data, isPending} = useQuery({
        queryKey: ["eventRequests", requestId],
        queryFn: ()=> getEventRequestById(requestId)
    })

    const {mutate: deleteRequest} = useMutation({
        mutationFn: () => deleteEventRequest(requestId)
    })

    const {mutate: approveRequest} = useMutation({
        mutationFn: () => approveEventRequest(requestId)
    })
    const {mutate: rejectRequest} = useMutation({
        mutationFn: () => rejectEventRequest(requestId)
    })

    return {
        eventRequest: data,
        isPending,
        deleteRequest,
        approveRequest,
        rejectRequest
    }
}