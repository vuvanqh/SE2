import { describe, it, expect, beforeEach } from "vitest";
import { apiClient, refreshPromise } from "../../src/api/apiClient";
import { server } from "../mocks/server";
import { http, HttpResponse } from "msw";

const BASE = "http://localhost:5049/api";

describe("apiClient interceptors", () => {
    beforeEach(() => {
        localStorage.clear();
    });

    it("attaches the bearer token from localStorage", async () => {
        let received: string | null = null;
        server.use(
            http.get(`${BASE}/ping`, ({ request }) => {
                received = request.headers.get("Authorization");
                return HttpResponse.json({ ok: true });
            }),
        );
        localStorage.setItem("token", "abc123");
        await apiClient.get("/ping");
        expect(received).toBe("Bearer abc123");
    });

    it("does not send Authorization header when no token", async () => {
        let received: string | null = "missing";
        server.use(
            http.get(`${BASE}/ping`, ({ request }) => {
                received = request.headers.get("Authorization");
                return HttpResponse.json({ ok: true });
            }),
        );
        await apiClient.get("/ping");
        expect(received).toBeNull();
    });

    it("auto-refreshes the token on 401 and retries the original request", async () => {
        let attempts = 0;
        server.use(
            http.get(`${BASE}/protected`, () => {
                attempts++;
                if (attempts === 1) return new HttpResponse(null, { status: 401 });
                return HttpResponse.json({ ok: true });
            }),
            http.post(`${BASE}/auth/refreshToken`, () =>
                HttpResponse.json("new-token"),
            ),
        );

        localStorage.setItem("token", "old");
        const res = await apiClient.get("/protected");
        expect(res.data).toEqual({ ok: true });
        expect(localStorage.getItem("token")).toBe("new-token");
        expect(attempts).toBe(2);
    });

    it("does not refresh on /login 401 (lets caller handle credentials error)", async () => {
        server.use(
            http.post(`${BASE}/auth/login`, () => new HttpResponse(null, { status: 401 })),
        );
        await expect(apiClient.post("/auth/login", {})).rejects.toMatchObject({
            response: { status: 401 },
        });
        expect(refreshPromise).toBeNull();
    });
});
