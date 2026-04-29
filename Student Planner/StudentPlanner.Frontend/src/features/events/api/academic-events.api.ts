import { apiClient } from "../../../api/apiClient";

const url = "/academic-events";

export const getAcademicEvents = async (facultyIds?: string[]) => (await apiClient.get(url, {params: facultyIds?.length?
    { facultyIds } : undefined,
    paramsSerializer: { indexes: null } })).data;
export const getAcademicEvent = async (eventId: string) => (await apiClient.get(url + `/${eventId}`)).data;
export const subscribeToAcademicEvent = async (eventId: string) => (await apiClient.put(url + `/${eventId}/subscribe`)).data;
export const unsubscribeFromAcademicEvent = async (eventId: string) => (await apiClient.put(url + `/${eventId}/unsubscribe`)).data;
