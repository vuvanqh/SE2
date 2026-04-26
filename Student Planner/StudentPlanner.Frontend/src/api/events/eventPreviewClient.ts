import { apiClient } from "../apiClient";

const url = "event-preview"

export const getEventPreviews = async (from?:Date, to?:Date) => (await apiClient.get(url, {
    params: {
        from,
        to
    }
})).data;
