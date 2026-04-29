
export type eventPreviewResponse = {
    id: string,
    title: string,
    location?: string,
    startTime: string,
    endTime: string,
    description: string
    eventType: "PersonalEvent" | "AcademicEvent" | "UsosEvent"
}