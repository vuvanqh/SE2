import type { createManagerRequest } from "../types/admin.types";
import { apiClient } from "./apiClient";

const url = "/admin";

export const getUsers = async () => (await apiClient.get(url + "/users")).data;
export const getManagers = async () => (await apiClient.get(url + "/managers")).data;
export const deleteUser = async (userId: string) => (await apiClient.delete(url + `/users/${userId}`)).data;
export const createManager = async (payload: createManagerRequest) => (await apiClient.post(url + "/managers", payload)).data;
export const syncUsers = async () => (await apiClient.post(url + "/users/sync")).data;
