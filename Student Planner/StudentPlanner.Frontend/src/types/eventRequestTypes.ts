

export type eventRequestResponse = {
    id: string,
    status: "Pending" | "Approved" | "Rejected" ,
    facultyId?: string,
    managerId: string,
    reviewedBy?: string,
    createdAt: string,
    reviewedAt: string
    eventDetails: eventDetails,
    requestType: "Create" | "Update" | "Delete"
}


export type createEventRequestResponse = {
    requestType: "Create"
}

export type updateEventRequestResponse = {
    eventId: string,
    requestType: "Update"
}

export type deleteEventRequestResponse = {
    eventId: string,
    requestType: "Delete"
}

export type eventRequest = createEventRequestResponse | updateEventRequestResponse | deleteEventRequestResponse;


export type createEventRequest = {
    facultyId?: string,
    eventId?: string,
    requestType: "Create" | "Update" | "Delete",
    eventDetails: eventDetails
}


export type eventDetails = {
    title: string,
    startTime: string,
    endTime: string,
    location?: string,
    description?: string
}
