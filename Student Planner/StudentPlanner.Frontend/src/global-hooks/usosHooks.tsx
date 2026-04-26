import { useQuery } from "@tanstack/react-query";
import type { eventPreviewResponse } from "../types/eventPreviewResponse";
import { getUsosEvents } from "../api/events/usos-events";

export default function useUsosEvents(from?: Date, to?: Date){
    const {data, isPending} = useQuery<eventPreviewResponse[]>({
        queryKey: ["usosEvents", from, to],
        queryFn: () => getUsosEvents(from, to)
    })

    return {
        usosEvents: data??[],
        isPending
    }
}