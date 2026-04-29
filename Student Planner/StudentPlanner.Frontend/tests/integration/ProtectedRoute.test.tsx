import { describe, it, expect, afterEach } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { QueryClientProvider } from "@tanstack/react-query";
import { makeQueryClient } from "../test-utils";
import { queryClient as globalQueryClient } from "../../src/api/queryClient";
import ProtectedRoute from "../../src/components/routing/ProtectedRoute";

afterEach(() => {
    // useAuth/useUser hit the global queryClient singleton; reset it.
    globalQueryClient.clear();
});

function renderTree(initialEntry: string, primeUserCache = false) {
    const qc = makeQueryClient();
    if (primeUserCache) {
        globalQueryClient.setQueryData(["user"], { token: "tok", userRole: "Student" });
    }
    return render(
        <QueryClientProvider client={qc}>
            <MemoryRouter initialEntries={[initialEntry]}>
                <Routes>
                    <Route element={<ProtectedRoute />}>
                        <Route path="/secret" element={<div>secret-content</div>} />
                    </Route>
                    <Route path="/login" element={<div>login-page</div>} />
                </Routes>
            </MemoryRouter>
        </QueryClientProvider>,
    );
}

describe("<ProtectedRoute /> (integration)", () => {
    it("redirects to /login when the user is not authenticated", async () => {
        renderTree("/secret", /* primeUserCache */ false);

        await waitFor(() => {
            expect(screen.getByText("login-page")).toBeInTheDocument();
        });
    });

    it("renders the nested route when the user query cache contains a user", async () => {
        renderTree("/secret", /* primeUserCache */ true);
        await waitFor(() => {
            expect(screen.getByText("secret-content")).toBeInTheDocument();
        });
    });
});
