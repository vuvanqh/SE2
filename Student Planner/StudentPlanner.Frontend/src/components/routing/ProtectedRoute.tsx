import { Navigate, Outlet } from "react-router-dom";
//import { useAuth } from "../../hooks/authHooks";

export default function ProtectedRoute(){
    //const {isAuthenticated} = useAuth();
    const isAuthenticated = true;
    if(!isAuthenticated)
        return <Navigate to="/login" replace/>
    return <Outlet/>
}