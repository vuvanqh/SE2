import { apiClient } from "../apiClient";

const url = "usos-event"

export const getUsosEvent = async (eventId: string) => (await apiClient.get(url + `/${eventId}`)).data;
