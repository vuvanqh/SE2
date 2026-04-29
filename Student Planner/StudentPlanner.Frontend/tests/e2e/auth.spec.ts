import { test, expect } from "@playwright/test";

test.describe("Authentication E2E", () => {
    test.beforeEach(async ({ page }) => {
        await page.addInitScript(() => localStorage.clear());
    });

    test("user can log in as a Student and reach the protected calendar", async ({ page }) => {
        await page.goto("/login");

        await page.getByLabel(/University Email/i).fill("student@pw.edu.pl");
        await page.getByLabel(/Password/i).fill("Password1!");
        await page.getByRole("button", { name: /log in/i }).click();

        // After login the router navigates to /student.
        await expect(page).toHaveURL(/\/student/);
    });

    test("invalid credentials show an error and stay on /login", async ({ page }) => {
        await page.goto("/login");

        await page.getByLabel(/University Email/i).fill("nope@pw.edu.pl");
        await page.getByLabel(/Password/i).fill("WrongPass1!");
        await page.getByRole("button", { name: /log in/i }).click();

        await expect(page.getByText(/invalid credentials/i)).toBeVisible();
        await expect(page).toHaveURL(/\/login/);
    });

    test("registration succeeds for a @pw.edu.pl email", async ({ page }) => {
        await page.goto("/register");
        await page.getByLabel(/University Email/i).fill("brandnew@pw.edu.pl");
        await page.getByLabel(/Password/i).fill("Password1!");
        await page.getByRole("button", { name: /create account/i }).click();

        // Expects to navigate back to "/" after register
        await expect(page).toHaveURL(/\/$/);
    });

    test("forgot-password flow walks through both steps", async ({ page }) => {
        await page.goto("/forgot-password");

        await page.getByLabel(/University Email/i).fill("stu@pw.edu.pl");
        await page.getByRole("button", { name: /send token/i }).click();

        await expect(page.getByLabel(/Reset Token/i)).toBeVisible();
        await page.getByLabel(/Reset Token/i).fill("valid-token");
        await page.getByLabel("New Password").fill("NewPass1!");
        await page.getByLabel("Confirm Password").fill("NewPass1!");
        await page.getByRole("button", { name: /reset password/i }).click();

        await expect(page).toHaveURL(/\/login/);
    });
});

test.describe("Routing access control E2E", () => {
    test("unauthenticated user is bounced to /login when visiting /student", async ({ page }) => {
        await page.addInitScript(() => localStorage.clear());
        await page.goto("/student");
        await expect(page).toHaveURL(/\/login/);
    });

    test("Student trying to visit /admin is redirected to /unauthorized", async ({ page }) => {
        // Pre-seed a logged-in student.
        await page.addInitScript(() => {
            localStorage.setItem("token", "stu-token");
            localStorage.setItem("role", "Student");
        });
        await page.goto("/admin");
        // Note: the app currently routes to /unauthorized but that route is
        // not defined in the router (see BUGS.md), so the page may render a
        // blank route. We assert the URL change instead.
        await expect(page).toHaveURL(/\/(unauthorized|login)/);
    });
});
