import { useMutation, useQuery } from "@tanstack/react-query";
import { queryClient } from "../../../api/queryClient";
import type { academicEventResponse } from "../../../types/academic-event.types";
import type { pagedResult } from "../../../types/pagination.types";
import { getAcademicEvent, getAcademicEvents, subscribeToAcademicEvent, unsubscribeFromAcademicEvent } from "../../../api/events/academic-events.api";
import { errorMessage, infoMessage } from "../../../toast/toastNotifications";

export function useAccessibleAcademicEvents(facultyIds?: string[], page: number = 1, pageSize: number = 10){
    facultyIds?.slice().sort().join(",")
    const {data, isLoading} = useQuery<pagedResult<academicEventResponse>>({
        queryKey: ["academic-events", facultyIds, page, pageSize],
        queryFn: () =>getAcademicEvents(facultyIds, page, pageSize)
    })
    return {
        events: data?.items ?? [],
        page: data?.page ?? page,
        pageSize: data?.pageSize ?? pageSize,
        totalCount: data?.totalCount ?? 0,
        totalPages: data?.totalPages ?? 0,
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

export function useSubscribeToAcademicEvent(eventId: string){
    const {mutateAsync, isPending} = useMutation({
        mutationFn: () => subscribeToAcademicEvent(eventId),
        onSuccess: () => {
            invalidateAcademicEvents(eventId);
            infoMessage("Subscribed to event successfully");
        },
        onError: () => errorMessage("Failed to subscribe to event")
    });

    return {
        subscribeToEvent: mutateAsync,
        isPending
    };
}

export function useUnsubscribeFromAcademicEvent(eventId: string){
    const {mutateAsync, isPending} = useMutation({
        mutationFn: () => unsubscribeFromAcademicEvent(eventId),
        onSuccess: () => {
            invalidateAcademicEvents(eventId);
            infoMessage("Unsubscribed from event successfully");
        },
        onError: () => errorMessage("Failed to unsubscribe from event")
    });

    return {
        unsubscribeFromEvent: mutateAsync,
        isPending
    };
}

function invalidateAcademicEvents(eventId?: string){
    queryClient.invalidateQueries({queryKey: ["academic-events"]});
    queryClient.invalidateQueries({queryKey: ["eventPreviews"]});

    if (eventId) {
        queryClient.invalidateQueries({queryKey: ["academic-events", "event", eventId]});
    }
}
