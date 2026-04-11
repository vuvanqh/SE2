import { useQuery, useMutation } from "@tanstack/react-query";
import { approveEventRequest, createEventRequest, deleteEventRequest, getEventRequestById, getMyRequests, rejectEventRequest } from "../../../api/eventRequestApi";
import type { createEventRequest as createRequestType, eventRequestResponse } from "../../../types/eventRequestTypes";
import { queryClient } from "../../../api/queryClient";

export function useAllEventRequests(){
    const {data, isPending} = useQuery<eventRequestResponse[]>({
        queryKey: ["eventRequests", "all"],
        queryFn: getMyRequests
    })

    return {
        eventRequests: data??[],
        isPending
    }
}

export function useCreateRequest(){
    const {mutate: createRequest} = useMutation({
        mutationFn: (payload: createRequestType) => createEventRequest(payload),
        onSuccess: () => {
            queryClient.invalidateQueries({queryKey: ["eventRequests", "all"]})
        }
    })

    return {
        createRequest
    }
}

export function useEventRequest(requestId: string){
    const {data, isPending} = useQuery<eventRequestResponse>({
        queryKey: ["eventRequests", requestId],
        queryFn: ()=> getEventRequestById(requestId)
    })

    const {mutate: deleteRequest} = useMutation({
        mutationFn: () => deleteEventRequest(requestId),
        onSuccess: () => {
            queryClient.invalidateQueries({queryKey: ["eventRequests", "all"]})
        }
    })

    const {mutate: approveRequest} = useMutation({
        mutationFn: () => approveEventRequest(requestId),
        onSuccess: () => {
            queryClient.invalidateQueries({queryKey: ["eventRequests", "all"]})
        }
    })
    const {mutate: rejectRequest} = useMutation({
        mutationFn: () => rejectEventRequest(requestId),
        onSuccess: () => {
            queryClient.invalidateQueries({queryKey: ["eventRequests", "all"]})
        }
    })

    return {
        eventRequest: data,
        isPending,
        deleteRequest,
        approveRequest,
        rejectRequest
    }
}