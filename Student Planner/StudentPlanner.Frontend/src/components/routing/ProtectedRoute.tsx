import { useNavigate, Outlet } from "react-router-dom";
import { useAuth } from "../../global-hooks/authHooks";
import ModalContextProvider from "../../store/ModalContext";
import ModalRoot from "../modals/ModalRoot";
import { useEffect } from "react";
import { registerEventRequestListeners } from "../../signalR/listeners/eventRequestListener";
import { stopConnections } from "../../signalR/connections/eventRequestConnection";

export default function ProtectedRoute(){
    const {isAuthenticated} = useAuth();
    const navigate = useNavigate();
    useEffect(()=>{
        
        if(!isAuthenticated)
        {
            navigate("/login");
            stopConnections();
            //toast message
        }
    }, [isAuthenticated]);

     useEffect(() => {
        if (!isAuthenticated) return;
        registerEventRequestListeners();

        return () => {
            stopConnections();
        };
    }, [isAuthenticated]);

    return <ModalContextProvider>
        <Outlet/>
        <ModalRoot/>    
    </ModalContextProvider>
}