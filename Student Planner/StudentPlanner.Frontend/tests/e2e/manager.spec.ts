import { test, expect } from "@playwright/test";

test.describe("Manager E2E", () => {
    test.beforeEach(async ({ page }) => {
        await page.addInitScript(() => localStorage.clear());
    });

    test("manager can log in and lands on /manager", async ({ page }) => {
        await page.goto("/login");

        await page.getByLabel(/University Email/i).fill("manager@pw.edu.pl");
        await page.getByLabel(/Password/i).fill("Password1!");
        await page.getByRole("button", { name: /log in/i }).click();

        await expect(page).toHaveURL(/\/manager/);
    });

    test("manager visiting /admin is bounced (role guard)", async ({ page }) => {
        await page.addInitScript(() => {
            localStorage.setItem("token", "mgr-token");
            localStorage.setItem("role", "Manager");
            localStorage.setItem("facultyId", "fac-1");
        });
        await page.goto("/admin");
        await expect(page).toHaveURL(/\/(unauthorized|login)/);
    });
});
