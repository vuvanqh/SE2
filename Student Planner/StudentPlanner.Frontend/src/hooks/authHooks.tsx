import { useMutation } from "@tanstack/react-query";
import { login as loginApi, register } from "../api/authApi";
import type { loginRequest, loginResponse } from "../types/authTypes";
import { useNavigate } from "react-router-dom";
import { queryClient } from "../api/queryClient";
import { successMessage, errorMessage } from "../toast/toastNotifications";

const loginFn = async(data: loginRequest) => {
    const resp = (await loginApi(data)) as loginResponse;
    localStorage.setItem("token",resp.token);
    localStorage.setItem("role",resp.userRole);
    queryClient.setQueryData(["user"], resp);
    return resp;
}


export function useAuth(){
    const navigate = useNavigate();

    const {mutateAsync, isPending: isLoginPending} = useMutation({
        mutationFn: loginFn,
        onSuccess: () => {
            successMessage("Logged in successfully!");
            navigate(`/${localStorage.getItem("role")?.toLocaleLowerCase()}`)
        },
        onError: (error)=> {errorMessage(error.message)}
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
        registerUser,
        isAuthenticated: !!localStorage.getItem("token"),
        isLoginPending,
        isRegisterPending
    }
}