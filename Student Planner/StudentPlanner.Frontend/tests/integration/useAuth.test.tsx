import { describe, it, expect } from "vitest";
import { renderHook, waitFor, act } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { MemoryRouter } from "react-router-dom";
import { useAuth } from "../../src/global-hooks/authHooks";

function wrapper({ children }: { children: React.ReactNode }) {
    const qc = new QueryClient({
        defaultOptions: {
            queries: { retry: false },
            mutations: { retry: false },
        },
    });
    return (
        <QueryClientProvider client={qc}>
            <MemoryRouter>{children}</MemoryRouter>
        </QueryClientProvider>
    );
}

describe("useAuth (integration)", () => {
    it("login() resolves and writes credentials to localStorage", async () => {
        const { result } = renderHook(() => useAuth(), { wrapper });

        await act(async () => {
            await result.current.login({
                email: "manager@pw.edu.pl",
                password: "Password1!",
            });
        });

        await waitFor(() => {
            expect(localStorage.getItem("token")).toBe("manager-token");
        });
        expect(localStorage.getItem("role")).toBe("Manager");
    });

    it("login() rejects with an error on invalid credentials", async () => {
        const { result } = renderHook(() => useAuth(), { wrapper });

        await expect(
            result.current.login({
                email: "wrong@pw.edu.pl",
                password: "Wrong",
            }),
        ).rejects.toBeDefined();
        expect(localStorage.getItem("token")).toBeNull();
    });

    it("logout() clears localStorage", async () => {
        const { result } = renderHook(() => useAuth(), { wrapper });

        // Pre-populate
        localStorage.setItem("token", "x");
        localStorage.setItem("role", "Student");

        await act(async () => {
            await result.current.logout(undefined as any);
        });

        expect(localStorage.getItem("token")).toBeNull();
        expect(localStorage.getItem("role")).toBeNull();
    });
});
