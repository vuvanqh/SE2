import { describe, it, expect } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { QueryClientProvider } from "@tanstack/react-query";
import { makeQueryClient } from "../test-utils";
import RegisterPage from "../../src/pages/public/RegisterPage";
import { toast } from "react-toastify";

function renderRegister() {
    return render(
        <QueryClientProvider client={makeQueryClient()}>
            <MemoryRouter initialEntries={["/register"]}>
                <Routes>
                    <Route path="/register" element={<RegisterPage />} />
                    <Route path="/" element={<div>home</div>} />
                </Routes>
            </MemoryRouter>
        </QueryClientProvider>,
    );
}

describe("RegisterPage (integration)", () => {
    it("registers a new account and navigates home with success toast", async () => {
        const user = userEvent.setup();
        renderRegister();

        await user.type(screen.getByLabelText(/University Email/i), "newbie@pw.edu.pl");
        await user.type(screen.getByLabelText(/Password/i), "Password1!");
        await user.click(screen.getByRole("button", { name: /create account/i }));

        await waitFor(() => {
            expect(screen.getByText("home")).toBeInTheDocument();
        });
        expect(toast.success).toHaveBeenCalledWith(
            expect.stringContaining("Registered"),
            expect.anything(),
        );
    });
});
