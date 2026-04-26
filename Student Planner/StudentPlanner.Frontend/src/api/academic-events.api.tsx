import { apiClient } from "./apiClient";

const url = "/academic-events";

export const getAcademicEvents = async () => (await apiClient.get(url)).data;
export const getAcademicEvent = async (eventId: string) => (await apiClient.get(url + `/${eventId}`)).data;
export const getAcademicEventByFaculty = async () => (await apiClient.get(url + `/faculty`)).data; 