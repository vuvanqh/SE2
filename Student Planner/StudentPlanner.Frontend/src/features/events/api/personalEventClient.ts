import type { createPersonalEventRequest, updatePersonalEventRequest } from "../types/personalEventTypes";
import { apiClient } from "../../../api/apiClient";

const personalEventUrl = "/personal-event";

export const getPersonalEvents = async () => (await apiClient.get(personalEventUrl)).data;
export const getPersonalEventById = async (eventId: string) => (await apiClient.get(personalEventUrl + `/${eventId}`)).data;
export const createPersonalEvent = async (payload: createPersonalEventRequest) => (await apiClient.post(personalEventUrl + `/create`, payload)).data;
export const updatePersonalEvent = async (eventId: string, payload: updatePersonalEventRequest) => (await apiClient.put(personalEventUrl + `/update/${eventId}`, payload)).data;
export const deletePersonalEvent = async (eventId: string) => (await apiClient.delete(personalEventUrl + `/delete/${eventId}`)).data



