export type personalEventResponse = {
    id: string,
    title: string,
    description?: string,
    startTime: string,
    endTime: string,
    location?: string
}

export type createPersonalEventRequest = {
    title: string,
    description?: string,
    startTime: string,
    endTime: string,
    location?: string
}

export type updatePersonalEventRequest = {
    title: string,
    description?: string,
    startTime: string,
    endTime: string,
    location?: string
}