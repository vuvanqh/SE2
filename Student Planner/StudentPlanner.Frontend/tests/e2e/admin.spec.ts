import { test, expect } from "@playwright/test";

test.describe("Admin user management E2E", () => {
    test.beforeEach(async ({ page }) => {
        await page.addInitScript(() => {
            localStorage.setItem("token", "adm-token");
            localStorage.setItem("role", "Admin");
            localStorage.setItem("facultyId", "fac-1");
        });
    });

    test("admin can view the user list", async ({ page }) => {
        await page.goto("/admin/users");

        // List should contain seeded users from the mock server
        await expect(page.getByText("student@pw.edu.pl")).toBeVisible();
        await expect(page.getByText("manager@pw.edu.pl")).toBeVisible();
    });
});
