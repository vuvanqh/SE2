import { apiClient } from "./apiClient";
import type {loginRequest, registerRequest} from '../types/authTypes';

const authUrl = "/auth";

export const login = async (payload: loginRequest) => (await apiClient.post(authUrl + "/login", payload)).data;
export const register = async (payload: registerRequest) => (await apiClient.post(authUrl + "/register", payload)).data;
