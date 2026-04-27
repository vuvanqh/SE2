import { apiClient } from "./apiClient";
import type { notificationPreferenceResponse, updateNotificationPreferenceRequest } from "../types/notification-preferences.types";

const url = "/notification-preferences";

export const getNotificationPreferences = async () =>
    (await apiClient.get<notificationPreferenceResponse>(url)).data;

export const updateNotificationPreferences = async (payload: updateNotificationPreferenceRequest) =>
    (await apiClient.put(url, payload)).data;
