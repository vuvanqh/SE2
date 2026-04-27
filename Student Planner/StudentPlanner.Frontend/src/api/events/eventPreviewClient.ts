import { apiClient } from "../apiClient";

const url = "event-preview"

export const getEventPreviews = async (from?:Date, to?:Date, facultyIds?: string[]) => (await apiClient.get(url, {
    params: {
        from,
        to,
        facultyIds
    },
    paramsSerializer: { indexes: null }   
})).data;
