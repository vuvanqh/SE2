import { useQuery } from "@tanstack/react-query";
import { getUsosEvent } from "../../../api/events/usos-events";

type usosEventResponse = {
    id: string,
    title: string,
    startTime: string,
    endTime: string,
    classType: string,
    groupNumber?: string,
    buildingName: string,
    roomNumber: string
}

export function useGetUsosEvent(eventId:string){
    const {data, isLoading} = useQuery<usosEventResponse>({
        queryKey: ["usos-events", "event",eventId],
        queryFn: () => getUsosEvent(eventId),
        enabled: !!eventId
    })

    return {
        event: data as usosEventResponse,
        isLoading,
    }
}
