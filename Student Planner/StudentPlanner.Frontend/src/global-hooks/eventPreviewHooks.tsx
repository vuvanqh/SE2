import { useQuery } from "@tanstack/react-query";
import { getEventPreviews } from "../api/eventPreviewClient";
import type { eventPreviewResponse } from "../types/eventPreviewResponse";

export default function useEventPreviews(from?: Date, to?: Date){
    const {data, isPending} = useQuery<eventPreviewResponse[]>({
        queryKey: ["eventPreviews", from, to],
        queryFn: () => getEventPreviews(from, to)
    })

    return {
        eventPreviews: data??[],
        isPending
    }
}