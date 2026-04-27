import { useQuery } from "@tanstack/react-query";
import { getEventPreviews } from "../api/events/eventPreviewClient";
import type { eventPreviewResponse } from "../types/eventPreviewResponse";

export default function useEventPreviews({from,to,facultyIds}:{from?: Date, to?: Date, facultyIds?: string[]}){
    const {data, isPending} = useQuery<eventPreviewResponse[]>({
        queryKey: ["eventPreviews", from, to, facultyIds],
        queryFn: () => getEventPreviews(from, to, facultyIds)
    })

    return {
        eventPreviews: data??[],
        isPending
    }
}