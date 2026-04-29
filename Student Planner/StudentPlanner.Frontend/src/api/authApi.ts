import { apiClient } from "./apiClient";
import type {loginRequest, registerRequest, forgotPasswordRequest, resetPasswordRequest, usosLoginRequest} from '../types/authTypes';

const authUrl = "/auth";

export const login = async (payload: loginRequest) => (await apiClient.post(authUrl + "/login", payload)).data;
export const register = async (payload: registerRequest) => (await apiClient.post(authUrl + "/register", payload)).data;
export const requestResetToken = async (payload: forgotPasswordRequest) => (await apiClient.post(authUrl + "/reset-password", payload)).data;
export const verifyAndResetPassword = async (payload: resetPasswordRequest) => (await apiClient.post(authUrl + "/verify-reset", payload)).data;
export const logout = async () => (await apiClient.post(authUrl + "/logout")).data;
export const usosLogin = async (payload: usosLoginRequest) => (await apiClient.post(authUrl + "/usos-login", payload)).data;