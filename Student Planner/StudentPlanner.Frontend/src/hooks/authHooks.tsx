import { useMutation } from "@tanstack/react-query";
import { login as loginApi, register } from "../api/authApi";
import type { loginRequest, loginResponse } from "../types/authTypes";
import { useNavigate } from "react-router-dom";
import { queryClient } from "../api/queryClient";

const loginFn = async(data: loginRequest) => {
    const resp = (await loginApi(data)) as loginResponse;
    localStorage.setItem("token",resp.token);
    localStorage.setItem("role",resp.role);
    queryClient.setQueryData(["user"], resp);
    return resp;
}


export function useAuth(){
    const navigate = useNavigate();

    const {mutateAsync, isPending: isLoginPending} = useMutation({
        mutationFn: loginFn,
        onSuccess: () => {
            navigate("/main")
        },
        onError: ()=> {console.log("error")}
    })

    const {mutate: registerUser} = useMutation({
        mutationFn: register,
        onSuccess: () => {
            navigate("/")
        }
    })
    return {
        login: mutateAsync, 
        registerUser,
        isAuthenticated: !!localStorage.getItem("token"),
        isLoginPending
    }
}