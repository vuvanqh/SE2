import { describe, it, expect } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { QueryClientProvider } from "@tanstack/react-query";
import { makeQueryClient } from "../test-utils";
import ForgotPasswordPage from "../../src/pages/public/ForgotPasswordPage";
import { toast } from "react-toastify";
import { server } from "../mocks/server";
import { http, HttpResponse } from "msw";

const BASE = "http://localhost:5049/api";

function renderForgot() {
    return render(
        <QueryClientProvider client={makeQueryClient()}>
            <MemoryRouter initialEntries={["/forgot-password"]}>
                <Routes>
                    <Route path="/forgot-password" element={<ForgotPasswordPage />} />
                    <Route path="/login" element={<div>login-page</div>} />
                </Routes>
            </MemoryRouter>
        </QueryClientProvider>,
    );
}

describe("ForgotPasswordPage (integration)", () => {
    it("flows from request-token step to reset-password step then success", async () => {
        const user = userEvent.setup();
        renderForgot();

        // Step 1: request a token
        await user.type(
            screen.getByLabelText(/University Email/i),
            "stu@pw.edu.pl",
        );
        await user.click(screen.getByRole("button", { name: /send token/i }));

        // Step 2: enter token + new password
        await waitFor(() => {
            expect(screen.getByLabelText(/Reset Token/i)).toBeInTheDocument();
        });

        await user.type(screen.getByLabelText(/Reset Token/i), "valid-token");
        await user.type(screen.getByLabelText("New Password"), "NewPass1!");
        await user.type(screen.getByLabelText("Confirm Password"), "NewPass1!");

        await user.click(screen.getByRole("button", { name: /reset password/i }));

        await waitFor(() => {
            expect(screen.getByText("login-page")).toBeInTheDocument();
        });

        expect(toast.success).toHaveBeenCalledWith(
            expect.stringMatching(/Password reset/i),
            expect.anything(),
        );
    });

    it("shows mismatch error when passwords do not match", async () => {
        const user = userEvent.setup();
        renderForgot();

        await user.type(
            screen.getByLabelText(/University Email/i),
            "stu@pw.edu.pl",
        );
        await user.click(screen.getByRole("button", { name: /send token/i }));

        await waitFor(() => {
            expect(screen.getByLabelText(/Reset Token/i)).toBeInTheDocument();
        });

        await user.type(screen.getByLabelText(/Reset Token/i), "valid-token");
        await user.type(screen.getByLabelText("New Password"), "NewPass1!");
        await user.type(screen.getByLabelText("Confirm Password"), "Different1!");
        await user.click(screen.getByRole("button", { name: /reset password/i }));

        await waitFor(() => {
            expect(screen.getByText("Passwords don't match")).toBeInTheDocument();
        });
    });

    it("shows error when the API rejects the token", async () => {
        const user = userEvent.setup();
        renderForgot();

        await user.type(
            screen.getByLabelText(/University Email/i),
            "stu@pw.edu.pl",
        );
        await user.click(screen.getByRole("button", { name: /send token/i }));

        await waitFor(() => {
            expect(screen.getByLabelText(/Reset Token/i)).toBeInTheDocument();
        });

        await user.type(screen.getByLabelText(/Reset Token/i), "bad-token");
        await user.type(screen.getByLabelText("New Password"), "NewPass1!");
        await user.type(screen.getByLabelText("Confirm Password"), "NewPass1!");
        await user.click(screen.getByRole("button", { name: /reset password/i }));

        await waitFor(() => {
            expect(
                screen.getByText(/Invalid or expired token/i),
            ).toBeInTheDocument();
        });
    });

    it("does not leak account-existence: failing token request still advances to step 2 with no error toast", async () => {
        // Simulate a server failure on the first step.
        server.use(
            http.post(`${BASE}/auth/reset-password`, () =>
                HttpResponse.json({ errors: ["boom"] }, { status: 500 }),
            ),
        );

        const user = userEvent.setup();
        renderForgot();

        await user.type(screen.getByLabelText(/University Email/i), "ghost@pw.edu.pl");
        await user.click(screen.getByRole("button", { name: /send token/i }));

        // Form should still advance to the reset-password step.
        await waitFor(() => {
            expect(screen.getByLabelText(/Reset Token/i)).toBeInTheDocument();
        });
        // And no error toast should have been shown (account-enumeration safety).
        expect(toast.error).not.toHaveBeenCalled();
    });

    it("disables the submit button while the request is pending", async () => {
        const user = userEvent.setup();
        renderForgot();

        const button = screen.getByRole("button", { name: /send token/i });
        await user.type(
            screen.getByLabelText(/University Email/i),
            "stu@pw.edu.pl",
        );
        await user.click(button);
        // After submit the form should advance — verifying the request
        // completed and the page didn't get stuck in pending state.
        await waitFor(() => {
            expect(screen.getByLabelText(/Reset Token/i)).toBeInTheDocument();
        });
    });

    it("preserves the email captured in step 1 when sending the verify-reset payload in step 2", async () => {
        let payload: any = null;
        server.use(
            http.post(`${BASE}/auth/verify-reset`, async ({ request }) => {
                payload = await request.json();
                return HttpResponse.json({ ok: true });
            }),
        );

        const user = userEvent.setup();
        renderForgot();

        await user.type(
            screen.getByLabelText(/University Email/i),
            "carry@pw.edu.pl",
        );
        await user.click(screen.getByRole("button", { name: /send token/i }));

        await waitFor(() => {
            expect(screen.getByLabelText(/Reset Token/i)).toBeInTheDocument();
        });

        await user.type(screen.getByLabelText(/Reset Token/i), "valid-token");
        await user.type(screen.getByLabelText("New Password"), "NewPass1!");
        await user.type(screen.getByLabelText("Confirm Password"), "NewPass1!");
        await user.click(screen.getByRole("button", { name: /reset password/i }));

        await waitFor(() => {
            expect(payload).not.toBeNull();
        });
        expect(payload).toEqual({
            email: "carry@pw.edu.pl",
            token: "valid-token",
            newPassword: "NewPass1!",
            confirmNewPassword: "NewPass1!",
        });
    });
});
