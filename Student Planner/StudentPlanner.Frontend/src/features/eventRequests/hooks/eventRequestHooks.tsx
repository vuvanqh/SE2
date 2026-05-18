import { useQuery, useMutation } from "@tanstack/react-query";
import { approveEventRequest, createEventRequest, deleteEventRequest, getAllEventRequests, getEventRequestById, getMyRequests, rejectEventRequest } from "../../../api/events/eventRequestApi";
import type { createEventRequest as createRequestType, eventRequestResponse } from "../../../types/eventRequestTypes";
import { queryClient } from "../../../api/queryClient";
import { infoMessage } from "../../../toast/toastNotifications";
import { useUser } from "../../../global-hooks/authHooks";

export function useMyEventRequests(){
    const {user} = useUser();
    const {data, isPending} = useQuery<eventRequestResponse[]>({
        queryKey: ["eventRequests", "all"],
        queryFn: getMyRequests,
        enabled: !!user && user.userRole=="Manager"
    })

    return {
        eventRequests: data??[],
        isPending
    }
}

export function useAllEventRequests(){
    const {user} = useUser();
    const {data, isPending} = useQuery<eventRequestResponse[]>({
        queryKey: ["eventRequests", "all"],
        queryFn: getAllEventRequests,
        enabled: !!user && user.userRole=="Admin"
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
            infoMessage("Event request created successfully");
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
            infoMessage("Event request deleted successfully");
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