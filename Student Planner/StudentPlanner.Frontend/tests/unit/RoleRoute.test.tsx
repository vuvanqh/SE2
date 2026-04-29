import { describe, it, expect } from "vitest";
import { render, screen } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import RoleRoute from "../../src/components/routing/RoleRoute";

function renderAt(role: string | null, path: string, allowed: string[]) {
    if (role === null) localStorage.removeItem("role");
    else localStorage.setItem("role", role);

    return render(
        <MemoryRouter initialEntries={[path]}>
            <Routes>
                <Route
                    path="/student"
                    element={
                        <RoleRoute allowed={allowed}>
                            <div>student-content</div>
                        </RoleRoute>
                    }
                />
                <Route path="/unauthorized" element={<div>unauthorized-page</div>} />
            </Routes>
        </MemoryRouter>,
    );
}

describe("<RoleRoute />", () => {
    it("renders children when the user role is in the allowed list", () => {
        renderAt("Student", "/student", ["Student"]);
        expect(screen.getByText("student-content")).toBeInTheDocument();
    });

    it("redirects to /unauthorized when role is not allowed", () => {
        renderAt("Manager", "/student", ["Student"]);
        expect(screen.getByText("unauthorized-page")).toBeInTheDocument();
    });

    it("redirects to /unauthorized when no role stored", () => {
        renderAt(null, "/student", ["Student"]);
        expect(screen.getByText("unauthorized-page")).toBeInTheDocument();
    });
});
