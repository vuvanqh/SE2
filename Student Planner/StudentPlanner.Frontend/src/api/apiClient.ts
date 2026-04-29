import axios from "axios";

const apiUrl = import.meta.env.VITE_API_BASE_URL + "/api";

export let refreshPromise: Promise<string> | null = null;

export const apiClient = axios.create({
    baseURL: apiUrl,
    withCredentials: true
});

const refreshClient = axios.create({
    baseURL: apiUrl,
    withCredentials: true
});

apiClient.interceptors.request.use(config => {
    const token = localStorage.getItem("token");
    if(token)
        config.headers.Authorization = `Bearer ${token}`;

    return config;
})

apiClient.interceptors.response.use(response => response,
    async error => {
        const request = error.config;
        
        console.log("INTERCEPTOR ERROR:", error.response);
        console.log("STATUS:", error.response?.status);

        if(error.response?.status != 401 || request._retry || request.url === "/auth/login" || request.url === "/auth/register")
        {
            if (error.response?.status === 403 && error.response.data?.message === "USOS_AUTH_REQUIRED") {
                window.location.href = "/usos-login";
            }
            return Promise.reject(error);
        }

        request._retry = true;

        try {
            if(!refreshPromise){
                refreshPromise = refreshClient.post("/auth/refreshToken")
                .then(res => {
                    const newToken = res.data;
                    localStorage.setItem("token", newToken);
                    return newToken;
                }).finally(() => refreshPromise=null);
            }
            const newToken = await refreshPromise;

            request.headers.Authorization = `Bearer ${newToken}`;
            return apiClient(request);
        }
        catch(err){
            localStorage.clear();
            window.location.href = "/";
            return Promise.reject(err);
        }
    }
)