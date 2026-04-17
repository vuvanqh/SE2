import { apiClient } from "./apiClient";

const url = "usos-event"

export const getUsosEvents = async (from?:Date, to?:Date) => (await apiClient.get(url, {
    params: {
        from,
        to
    }
})).data;
