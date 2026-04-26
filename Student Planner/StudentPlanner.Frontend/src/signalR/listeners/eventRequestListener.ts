
import { erConnection as connection, startConnections } from "../connections/eventRequestConnection";
import { queryClient } from "../../api/queryClient";
import { successMessage } from "../../toast/toastNotifications";
import { infoMessage } from "../../toast/toastNotifications";

const handler = () => {
    console.log("signalR")
    infoMessage("Event request updated");
    queryClient.invalidateQueries({ queryKey: ["eventRequests", "all"] });
    successMessage("Event requests refreshed");
};
    

export const registerEventRequestListeners = async () => {
    if (connection.state === "Disconnected") {
        await startConnections();
    }
    connection.off("refreshEventRequests", handler);
    connection.on("refreshEventRequests", handler);
};