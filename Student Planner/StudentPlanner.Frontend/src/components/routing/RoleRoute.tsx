import { Navigate } from "react-router-dom";

export default function RoleRoute({allowed, children}:{allowed:string[], children: React.ReactNode  }){
    const role = localStorage.getItem("role");
    if(!role || !allowed.includes(role))
        return <Navigate to="/unauthorized" replace />;
    return children

}