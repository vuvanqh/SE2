import { describe, it, expect, afterEach } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { QueryClientProvider } from "@tanstack/react-query";
import { makeQueryClient } from "../test-utils";
import { queryClient as globalQueryClient } from "../../src/api/queryClient";
import RoleRoute from "../../src/components/routing/RoleRoute";
import ProtectedRoute from "../../src/components/routing/ProtectedRoute";

afterEach(() => {
    globalQueryClient.clear();
});

/**
 * End-to-end-ish (in-process) routing checks: combining ProtectedRoute and
 * RoleRoute to verify the layered access control.
 */
function renderApp(opts: { token?: string; role?: string; entry?: string } = {}) {
    if (opts.token) localStorage.setItem("token", opts.token);
    if (opts.role) localStorage.setItem("role", opts.role);

    const qc = makeQueryClient();
    if (opts.token && opts.role) {
        globalQueryClient.setQueryData(["user"], { token: opts.token, userRole: opts.role });
    }

    return render(
        <QueryClientProvider client={qc}>
            <MemoryRouter initialEntries={[opts.entry ?? "/student"]}>
                <Routes>
                    <Route element={<ProtectedRoute />}>
                        <Route
                            path="/student"
                            element={
                                <RoleRoute allowed={["Student"]}>
                                    <div>student-area</div>
                                </RoleRoute>
                            }
                        />
                        <Route
                            path="/admin"
                            element={
                                <RoleRoute allowed={["Admin"]}>
                                    <div>admin-area</div>
                                </RoleRoute>
                            }
                        />
                    </Route>
                    <Route path="/login" element={<div>login-page</div>} />
                    <Route path="/unauthorized" element={<div>unauthorized-page</div>} />
                </Routes>
            </MemoryRouter>
        </QueryClientProvider>,
    );
}

describe("Routing access control", () => {
    it("unauthenticated → /login", async () => {
        renderApp({ entry: "/student" });
        await waitFor(() => {
            expect(screen.getByText("login-page")).toBeInTheDocument();
        });
    });

    it("Student visiting /student → student-area", async () => {
        renderApp({ token: "t", role: "Student", entry: "/student" });
        await waitFor(() => {
            expect(screen.getByText("student-area")).toBeInTheDocument();
        });
    });

    it("Student visiting /admin → /unauthorized", async () => {
        renderApp({ token: "t", role: "Student", entry: "/admin" });
        await waitFor(() => {
            expect(screen.getByText("unauthorized-page")).toBeInTheDocument();
        });
    });

    it("Admin visiting /admin → admin-area", async () => {
        renderApp({ token: "t", role: "Admin", entry: "/admin" });
        await waitFor(() => {
            expect(screen.getByText("admin-area")).toBeInTheDocument();
        });
    });
});
