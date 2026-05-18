import { useQuery } from "@tanstack/react-query";
import { getEventPreviews } from "../api/eventPreviewClient";
import type { eventPreviewResponse } from "../types/eventPreviewResponse";

export default function useEventPreviews({from,days,facultyIds}:{from?: Date, days?: number, facultyIds?: string[]}){
    const {data, isPending} = useQuery<eventPreviewResponse[]>({
        queryKey: ["eventPreviews", from, days, facultyIds],
        queryFn: () => getEventPreviews(from, days, facultyIds)
    })
    
    return {
        eventPreviews: data??[],
        isPending
    }
}