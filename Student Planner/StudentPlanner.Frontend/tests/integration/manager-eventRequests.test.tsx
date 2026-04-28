import { describe, it, expect, afterEach, vi } from "vitest";
import { renderHook, waitFor, act } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { MemoryRouter } from "react-router-dom";
import {
    useCreateRequest,
    useEventRequest,
    useMyEventRequests,
} from "../../src/features/eventRequests/hooks/eventRequestHooks";
import { server } from "../mocks/server";
import { http, HttpResponse } from "msw";
import { toast } from "react-toastify";

const BASE = "http://localhost:5049/api";

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

afterEach(() => {
    localStorage.clear();
});

describe("Manager: useCreateRequest (create academic-event request)", () => {
    it("submits a Create request with the manager's facultyId and event details", async () => {
        // Manager sets facultyId at login.
        localStorage.setItem("facultyId", "fac-1");

        let captured: any = null;
        server.use(
            http.post(`${BASE}/event-requests/create`, async ({ request }) => {
                captured = await request.json();
                return HttpResponse.json({
                    id: "req-new",
                    status: "Pending",
                    ...(captured as object),
                });
            }),
        );

        const { result } = renderHook(() => useCreateRequest(), { wrapper });

        await act(async () => {
            result.current.createRequest({
                facultyId: "fac-1",
                requestType: "Create",
                eventDetails: {
                    title: "Algorithms midterm",
                    location: "Room 314",
                    startTime: "2099-05-01T10:00",
                    endTime: "2099-05-01T12:00",
                    description: "Closed-book midterm",
                },
            });
        });

        await waitFor(() => {
            expect(captured).not.toBeNull();
        });
        expect(captured.facultyId).toBe("fac-1");
        expect(captured.requestType).toBe("Create");
        expect(captured.eventDetails.title).toBe("Algorithms midterm");
        await waitFor(() => {
            expect(toast).toHaveBeenCalledWith(
                expect.anything(),
                expect.objectContaining({
                    className: expect.stringContaining("app-toast-info"),
                }),
            );
        });
    });

    it("submits a Delete request with the eventId of the event to remove", async () => {
        localStorage.setItem("facultyId", "fac-1");

        let captured: any = null;
        server.use(
            http.post(`${BASE}/event-requests/create`, async ({ request }) => {
                captured = await request.json();
                return HttpResponse.json({ id: "req-del", status: "Pending" });
            }),
        );

        const { result } = renderHook(() => useCreateRequest(), { wrapper });

        await act(async () => {
            result.current.createRequest({
                facultyId: "fac-1",
                requestType: "Delete",
                eventId: "ev-old",
                eventDetails: {
                    title: "irrelevant",
                    startTime: "2099-05-01T10:00",
                    endTime: "2099-05-01T12:00",
                },
            });
        });

        await waitFor(() => {
            expect(captured).not.toBeNull();
        });
        expect(captured.requestType).toBe("Delete");
        expect(captured.eventId).toBe("ev-old");
    });

    it("does not toast success when the API rejects the create request", async () => {
        localStorage.setItem("facultyId", "fac-1");

        server.use(
            http.post(`${BASE}/event-requests/create`, () =>
                HttpResponse.json({ errors: ["server boom"] }, { status: 500 }),
            ),
        );

        const { result } = renderHook(() => useCreateRequest(), { wrapper });
        const infoCallsBefore = (toast as any).mock.calls.length;

        await act(async () => {
            result.current.createRequest({
                facultyId: "fac-1",
                requestType: "Create",
                eventDetails: {
                    title: "X",
                    startTime: "2099-05-01T10:00",
                    endTime: "2099-05-01T12:00",
                },
            });
        });

        // Wait a tick and assert no info-toast appeared.
        await new Promise((r) => setTimeout(r, 50));
        const infoToastsAfter = (toast as any).mock.calls
            .slice(infoCallsBefore)
            .filter(([, opts]: any[]) =>
                String(opts?.className ?? "").includes("app-toast-info"),
            );
        expect(infoToastsAfter).toHaveLength(0);
    });
});

describe("Manager: useEventRequest (delete own request)", () => {
    it("loads the request by id and lets the manager delete it", async () => {
        let deletedId: string | undefined;
        server.use(
            http.delete(`${BASE}/event-requests/delete/:id`, ({ params }) => {
                deletedId = params.id as string;
                return HttpResponse.json({ ok: true });
            }),
        );

        const { result } = renderHook(() => useEventRequest("req-1"), { wrapper });

        await waitFor(() => {
            expect(result.current.eventRequest?.id).toBe("req-1");
        });

        await act(async () => {
            result.current.deleteRequest();
        });

        await waitFor(() => {
            expect(deletedId).toBe("req-1");
        });
    });

    it("approve and reject hit the correct PATCH endpoints", async () => {
        let approveHit = false;
        let rejectHit = false;
        server.use(
            http.patch(`${BASE}/event-requests/approve/:id`, () => {
                approveHit = true;
                return HttpResponse.json({ ok: true });
            }),
            http.patch(`${BASE}/event-requests/reject/:id`, () => {
                rejectHit = true;
                return HttpResponse.json({ ok: true });
            }),
        );

        const { result } = renderHook(() => useEventRequest("req-1"), { wrapper });

        await waitFor(() => {
            expect(result.current.eventRequest?.id).toBe("req-1");
        });

        await act(async () => {
            result.current.approveRequest();
        });
        await waitFor(() => expect(approveHit).toBe(true));

        await act(async () => {
            result.current.rejectRequest();
        });
        await waitFor(() => expect(rejectHit).toBe(true));
    });
});

describe("Manager: useMyEventRequests", () => {
    it("calls GET /event-requests (manager's own requests)", async () => {
        let hit = false;
        server.use(
            http.get(`${BASE}/event-requests`, () => {
                hit = true;
                return HttpResponse.json([
                    {
                        id: "r1",
                        status: "Pending",
                        facultyId: "fac-1",
                        managerId: "u2",
                        createdAt: new Date().toISOString(),
                        reviewedAt: "",
                        requestType: "Create",
                        eventDetails: {
                            title: "Mine",
                            startTime: "2099-05-01T10:00",
                            endTime: "2099-05-01T11:00",
                        },
                    },
                ]);
            }),
        );

        const { result } = renderHook(() => useMyEventRequests(), { wrapper });
        await waitFor(() => {
            expect(result.current.isPending).toBe(false);
        });
        expect(hit).toBe(true);
        expect(result.current.eventRequests).toHaveLength(1);
        expect(result.current.eventRequests[0].id).toBe("r1");
    });
});
