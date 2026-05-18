import { apiClient } from "../../../api/apiClient";

const url = "academic-events"

export const getAllAcademicEvents = async () => (await apiClient.get(url)).data;
export const getAcademicEventById = async (eventId: string) => (await apiClient.get(url + `/${eventId}`)).data;
export const getFacultyEvents = async () => (await apiClient.get(url + `/faculty`)).data;