import { useMutation, useQuery } from "@tanstack/react-query";
import { getNotificationPreferences, updateNotificationPreferences } from "../../../api/notification-preferences.api";
import { queryClient } from "../../../api/queryClient";
import { errorMessage, infoMessage } from "../../../toast/toastNotifications";
import type { notificationPreferenceResponse } from "../../../types/notification-preferences.types";

export function useNotificationPreferences(enabled: boolean = true){
    const {data, isLoading} = useQuery<notificationPreferenceResponse>({
        queryKey: ["notification-preferences"],
        queryFn: getNotificationPreferences,
        enabled
    });

    const {mutateAsync: updatePreferences, isPending} = useMutation({
        mutationFn: updateNotificationPreferences,
        onSuccess: () => {
            queryClient.invalidateQueries({queryKey: ["notification-preferences"]});
            infoMessage("Notification preferences updated");
        },
        onError: () => errorMessage("Failed to update notification preferences")
    });

    return {
        preferences: data,
        isLoading,
        isPending,
        updatePreferences
    };
}
