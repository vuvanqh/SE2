import { http, HttpResponse } from "msw";

// Must match VITE_API_BASE_URL + "/api" in apiClient.ts. We hard-code the
// base since import.meta.env is resolved at build time.
const BASE = "http://localhost:5049/api";

export const handlers = [
    // ---------------------------------------------------------------- Auth
    http.post(`${BASE}/auth/login`, async ({ request }) => {
        const body = (await request.json()) as { email: string; password: string };
        if (body.email === "student@pw.edu.pl" && body.password === "Password1!") {
            return HttpResponse.json({
                token: "student-token",
                userRole: "Student",
                facultyId: "fac-1",
                email: body.email,
                firstName: "Stu",
                lastName: "Dent",
            });
        }
        if (body.email === "manager@pw.edu.pl" && body.password === "Password1!") {
            return HttpResponse.json({
                token: "manager-token",
                userRole: "Manager",
                facultyId: "fac-1",
                email: body.email,
                firstName: "Man",
                lastName: "Ager",
            });
        }
        if (body.email === "admin@pw.edu.pl" && body.password === "Password1!") {
            return HttpResponse.json({
                token: "admin-token",
                userRole: "Admin",
                facultyId: "fac-1",
                email: body.email,
                firstName: "Ad",
                lastName: "Min",
            });
        }
        return HttpResponse.json(
            { errors: ["Invalid credentials"] },
            { status: 401 },
        );
    }),

    http.post(`${BASE}/auth/register`, async ({ request }) => {
        const body = (await request.json()) as { email: string; password: string };
        if (!body.email?.endsWith("@pw.edu.pl")) {
            return HttpResponse.json(
                { errors: ["Email must use @pw.edu.pl domain"] },
                { status: 400 },
            );
        }
        return HttpResponse.json({ ok: true });
    }),

    http.post(`${BASE}/auth/reset-password`, async () => HttpResponse.json({ ok: true })),

    http.post(`${BASE}/auth/verify-reset`, async ({ request }) => {
        const body = (await request.json()) as { token: string };
        if (body.token === "valid-token") return HttpResponse.json({ ok: true });
        return HttpResponse.json(
            { errors: ["Invalid or expired token"] },
            { status: 400 },
        );
    }),

    http.post(`${BASE}/auth/logout`, () => HttpResponse.json({ ok: true })),

    http.post(`${BASE}/auth/refreshToken`, () => HttpResponse.json("refreshed-token")),

    // -------------------------------------------------------------- Admin
    http.get(`${BASE}/admin/users`, () =>
        HttpResponse.json([
            {
                id: "u1",
                email: "student@pw.edu.pl",
                firstName: "Stu",
                lastName: "Dent",
                role: "Student",
                facultyId: "fac-1",
            },
            {
                id: "u2",
                email: "manager@pw.edu.pl",
                firstName: "Man",
                lastName: "Ager",
                role: "Manager",
                facultyId: "fac-1",
            },
        ]),
    ),
    http.post(`${BASE}/admin/managers`, () =>
        HttpResponse.json({ id: "u-new", role: "Manager" }),
    ),
    http.delete(`${BASE}/admin/users/:id`, () => HttpResponse.json({ ok: true })),

    // ------------------------------------------------------------ Faculty
    http.get(`${BASE}/Faculty`, () =>
        HttpResponse.json([
            { id: "fac-1", code: "MEiL", name: "Mechanical & Aerospace" },
            { id: "fac-2", code: "EiTI", name: "Electronics & Information Tech" },
        ]),
    ),

    // -------------------------------------------------- Personal events
    http.get(`${BASE}/personal-event`, () => HttpResponse.json([])),
    http.post(`${BASE}/personal-event/create`, async ({ request }) => {
        const body = (await request.json()) as Record<string, unknown>;
        return HttpResponse.json({ id: "ev-1", ...body });
    }),
    http.put(`${BASE}/personal-event/update/:id`, () => HttpResponse.json({ ok: true })),
    http.delete(`${BASE}/personal-event/delete/:id`, () => HttpResponse.json({ ok: true })),
    http.get(`${BASE}/personal-event/:id`, () =>
        HttpResponse.json({
            id: "ev-1",
            title: "Personal",
            location: "Library",
            startTime: "2099-01-01T10:00:00",
            endTime: "2099-01-01T11:00:00",
            description: "demo",
        }),
    ),

    // --------------------------------------------------- Event requests
    // GET /event-requests   -> manager's own requests (used by useMyEventRequests)
    http.get(`${BASE}/event-requests`, () => HttpResponse.json([])),
    http.get(`${BASE}/event-requests/all`, () => HttpResponse.json([])),
    http.post(`${BASE}/event-requests/create`, async ({ request }) => {
        const body = (await request.json()) as Record<string, unknown>;
        return HttpResponse.json({
            id: "req-1",
            status: "Pending",
            createdAt: new Date().toISOString(),
            ...body,
        });
    }),
    http.delete(`${BASE}/event-requests/delete/:id`, () => HttpResponse.json({ ok: true })),
    http.patch(`${BASE}/event-requests/approve/:id`, () => HttpResponse.json({ ok: true })),
    http.patch(`${BASE}/event-requests/reject/:id`, () => HttpResponse.json({ ok: true })),
    http.get(`${BASE}/event-requests/:id`, () =>
        HttpResponse.json({
            id: "req-1",
            status: "Pending",
            facultyId: "fac-1",
            managerId: "u2",
            createdAt: new Date().toISOString(),
            reviewedAt: "",
            requestType: "Create",
            eventDetails: {
                title: "Existing event",
                location: "Room A",
                startTime: "2099-02-01T10:00:00",
                endTime: "2099-02-01T11:00:00",
                description: "some desc",
            },
        }),
    ),

    // ------------------------------------------------ Academic events
    http.get(`${BASE}/academic-events`, () => HttpResponse.json([])),

    // ----------------------------------------------- Event previews
    http.get(`${BASE}/event-previews`, () => HttpResponse.json([])),
];
