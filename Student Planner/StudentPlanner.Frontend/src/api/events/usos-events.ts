import { apiClient } from "../apiClient";

const url = "usos-events"

export const getUsosEvent = async (eventId: string) => (await apiClient.get(url + `/${eventId}`)).data;
