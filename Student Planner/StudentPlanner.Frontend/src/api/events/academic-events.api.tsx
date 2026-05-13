import { apiClient } from "../apiClient";
import type { pagedResult } from "../../types/pagination.types";
import type { academicEventResponse } from "../../types/academic-event.types";

const url = "/academic-events";

export const getAcademicEvents = async (facultyIds?: string[], page: number = 1, pageSize: number = 10) =>
    (await apiClient.get<pagedResult<academicEventResponse>>(url, {
        params: {
            ...(facultyIds?.length ? { facultyIds } : {}),
            page,
            pageSize
        },
        paramsSerializer: { indexes: null }
    })).data;
export const getAcademicEvent = async (eventId: string) => (await apiClient.get(url + `/${eventId}`)).data;
export const subscribeToAcademicEvent = async (eventId: string) => (await apiClient.put(url + `/${eventId}/subscribe`)).data;
export const unsubscribeFromAcademicEvent = async (eventId: string) => (await apiClient.put(url + `/${eventId}/unsubscribe`)).data;
