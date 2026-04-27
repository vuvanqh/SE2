export type academicEventResponse = {
    id: string,
    facultyId: string,
    title: string,
    description?: string,
    startTime: string,
    endTime: string,
    location?: string,
    isSubscribed: boolean
}
