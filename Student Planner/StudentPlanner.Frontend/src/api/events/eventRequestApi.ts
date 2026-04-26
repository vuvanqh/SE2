import type { createEventRequest as creteType } from "../../types/eventRequestTypes";
import { apiClient } from "../apiClient";

const requestUrl = "/event-requests";

export const getMyRequests = async () => (await apiClient.get(requestUrl)).data;
export const getAllEventRequests = async () => (await apiClient.get(requestUrl + "/all")).data;
export const getEventRequestById = async (requestId: string) => (await apiClient.get(requestUrl + `/${requestId}`)).data;
export const createEventRequest = async (payload: creteType) => (await apiClient.post(requestUrl + `/create`, payload)).data;
export const deleteEventRequest = async (requestId: string) => (await apiClient.delete(requestUrl + `/delete/${requestId}`)).data;
export const approveEventRequest = async (requestId: string) => (await apiClient.patch(requestUrl + `/approve/${requestId}`)).data;
export const rejectEventRequest = async (requestId: string) => (await apiClient.patch(requestUrl + `/reject/${requestId}`)).data;