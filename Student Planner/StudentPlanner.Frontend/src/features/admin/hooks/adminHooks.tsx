import { useQuery, useMutation } from "@tanstack/react-query";
import { createManager, getUsers, deleteUser } from "../../../api/admin.api";
import type { createManagerRequest, userResponse } from "../../../types/admin.types";
import { queryClient } from "../../../api/queryClient";
import { successMessage } from "../../../toast/toastNotifications";

export function useAdmin() {
    const { data, isLoading } = useQuery<userResponse[]>({
        queryKey: ["admin", "users"],
        queryFn: getUsers
    });

    const {mutate} = useMutation({
        mutationFn: (payload: createManagerRequest) => createManager(payload),
        onSuccess: () => {
            queryClient.invalidateQueries({queryKey: ["admin", "users"]});
            successMessage("Manager created successfully");
        }
    });

    const {mutate: del} = useMutation({
        mutationFn: (userId: string) => deleteUser(userId),
        onSuccess: () => {
            queryClient.invalidateQueries({queryKey: ["admin", "users"]});
            successMessage("User deleted successfully");
        }
    });

    return {
        users: data??[],
        isUsersLoading: isLoading,
        createManager: mutate,
        deleteUser: del
    };
}