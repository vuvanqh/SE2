import * as signalR from "@microsoft/signalr";
import { refreshPromise } from "../../api/apiClient";
import { HubConnectionState } from "@microsoft/signalr";

const erConnectionUrl = import.meta.env.VITE_API_BASE_URL + "/hubs/eventRequest";

export const erConnection = new signalR.HubConnectionBuilder()
    .withUrl(erConnectionUrl,
         { accessTokenFactory: () => localStorage.getItem("token")!,
           withCredentials: true })
    .withAutomaticReconnect({
                nextRetryDelayInMilliseconds: () => {
                    if (!refreshPromise) return null;
                    return 2000;
                }
            })
    .build();


export async function startConnections(){
    if(erConnection.state==HubConnectionState.Disconnected) 
        await erConnection.start().catch(err => console.error("SignalR error notification: ",err));
}

export async function stopConnections(){
    if(erConnection.state==HubConnectionState.Disconnected)
        await erConnection.stop();
}