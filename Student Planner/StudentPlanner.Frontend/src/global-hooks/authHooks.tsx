import { useMutation, useQuery } from "@tanstack/react-query";
import { login as loginApi, register, requestResetToken, verifyAndResetPassword } from "../api/authApi";
import type { loginRequest, loginResponse } from "../types/authTypes";
import { useNavigate } from "react-router-dom";
import { queryClient } from "../api/queryClient";
import { successMessage, errorMessage } from "../toast/toastNotifications";




export function useAuth(){
    const navigate = useNavigate();

    const {mutateAsync, isPending: isLoginPending} = useMutation({
        mutationFn: async(data: loginRequest) => {
            const resp = await loginApi(data);
            console.log("LOGIN RESP:", resp);
            return resp;
        },
        onSuccess: (data) => {
            localStorage.setItem("token",data.token);
            localStorage.setItem("role",data.userRole);
            localStorage.setItem("facultyId", data.facultyId ?? "");
            queryClient.setQueryData(["user"], data);

            successMessage("Logged in successfully!");
            navigate(`/${data.userRole.toLowerCase()}`);
        },
        onError: (error)=> {errorMessage(error.message)}
    })

    const {mutateAsync: sendResetToken, isPending: isRequestPending} = useMutation({
        mutationFn: requestResetToken,
        onError: (error)=> {errorMessage(error.message)}
    })

    const {mutateAsync: resetPassword, isPending: isResetPending} = useMutation({
        mutationFn: verifyAndResetPassword,
        onSuccess: () => {
            successMessage("Password reset successfully! You can now log in.");
            navigate("/login");
        },
        onError: (error)=> {errorMessage(error.message)}
    })

    const {mutateAsync: logout} = useMutation({
        mutationFn: async() => {
            await localStorage.clear();
            queryClient.setQueryData(["user"], undefined);
            navigate("/")
        }
    })

    const {mutate: registerUser, isPending: isRegisterPending} = useMutation({
        mutationFn: register,
        onSuccess: () => {
            successMessage("Registered in successfully! Feel free to log in now.");
            navigate("/")
        },
        onError: (error)=> {errorMessage(error.message)}
    })

    return {
        login: mutateAsync, 
        logout,
        registerUser,
        sendResetToken,
        resetPassword,
        isAuthenticated: !!queryClient.getQueryData(["user"]),
        isLoginPending,
        isRegisterPending,
        isResetPending: isRequestPending || isResetPending
    }
}

export function useUser(){
    const {data} = useQuery({
        queryKey: ["user"],
        queryFn: getStoredUser,
        initialData: () => queryClient.getQueryData(["user"]),
        staleTime: Infinity,
    })
    return {
        user: data
    }
}

function getStoredUser(): loginResponse | undefined {
    const token = localStorage.getItem("token");
    const role = localStorage.getItem("role");

    if (!token || !role) return undefined;

    return {
        token,
        userRole: role,
    } as loginResponse;
}