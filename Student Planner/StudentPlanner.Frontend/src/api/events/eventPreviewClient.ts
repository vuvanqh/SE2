import { apiClient } from "../apiClient";

const url = "event-preview"

export const getEventPreviews = async (from?:Date, days?:number, facultyIds?: string[]) => (await apiClient.get(url, {
    params: {
        from,
        days,
        facultyIds
    },
    paramsSerializer: { indexes: null }   
})).data;
