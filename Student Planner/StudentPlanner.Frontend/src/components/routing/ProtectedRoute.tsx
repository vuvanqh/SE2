import { useNavigate, Outlet } from "react-router-dom";
import { useAuth } from "../../global-hooks/authHooks";
import ModalContextProvider from "../../store/ModalContext";
import ModalRoot from "../modals/ModalRoot";
import { useEffect } from "react";

export default function ProtectedRoute(){
    const {isAuthenticated} = useAuth();
    const navigate = useNavigate();
    useEffect(()=>{
        if(!isAuthenticated)
        {
            navigate("/login");
            //toast message
        }
    }, [isAuthenticated]);
    return <ModalContextProvider>
        <Outlet/>
        <ModalRoot/>    
    </ModalContextProvider>
}