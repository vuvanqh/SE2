import { describe, it, expect } from "vitest";
import { renderHook, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { MemoryRouter } from "react-router-dom";
import { useAdmin } from "../../src/features/admin/hooks/adminHooks";
import { toast } from "react-toastify";

function makeWrapper(qc = new QueryClient({ defaultOptions: { queries: { retry: false } } })) {
    return function Wrapper({ children }: { children: React.ReactNode }) {
        return (
            <QueryClientProvider client={qc}>
                <MemoryRouter>{children}</MemoryRouter>
            </QueryClientProvider>
        );
    };
}

describe("useAdmin (integration)", () => {
    it("loads users from the API", async () => {
        const { result } = renderHook(() => useAdmin(), { wrapper: makeWrapper() });
        await waitFor(() => {
            expect(result.current.isUsersLoading).toBe(false);
        });
        expect(result.current.users).toHaveLength(2);
        expect(result.current.users[0].email).toBe("student@pw.edu.pl");
    });

    it("creates a manager and shows a success toast", async () => {
        const { result } = renderHook(() => useAdmin(), { wrapper: makeWrapper() });
        await waitFor(() => {
            expect(result.current.isUsersLoading).toBe(false);
        });

        result.current.createManager({
            email: "boss@pw.edu.pl",
            firstName: "B",
            lastName: "Oss",
            facultyId: "fac-1",
        } as any);

        await waitFor(() => {
            expect(toast.success).toHaveBeenCalledWith(
                "Manager created successfully",
                expect.anything(),
            );
        });
    });

    it("deletes a user and shows a success toast", async () => {
        const { result } = renderHook(() => useAdmin(), { wrapper: makeWrapper() });
        await waitFor(() => {
            expect(result.current.isUsersLoading).toBe(false);
        });

        result.current.deleteUser("u1");

        await waitFor(() => {
            expect(toast.success).toHaveBeenCalledWith(
                "User deleted successfully",
                expect.anything(),
            );
        });
    });
});
