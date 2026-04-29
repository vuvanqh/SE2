
import { erConnection as connection, startConnections } from "../connections/eventRequestConnection";
import { queryClient } from "../../api/queryClient";
import { infoMessage } from "../../toast/toastNotifications";

const handler = () => {
    console.log("signalR")
    infoMessage("Event request updated");
    queryClient.invalidateQueries({ queryKey: ["eventRequests", "all"] });
};

const academicEventHanlder = (message: string, facultyId?: string) => {
    infoMessage(message);
    queryClient.invalidateQueries({ queryKey: ["academic-events", facultyId] });
}
    

export const registerEventRequestListeners = async () => {
    if (connection.state === "Disconnected") {
        await startConnections();
    }
    connection.off("refreshEventRequests", handler);
    connection.off("academicEvent", academicEventHanlder);
    connection.on("refreshEventRequests", handler);
    connection.on("academicEvent", academicEventHanlder);
};