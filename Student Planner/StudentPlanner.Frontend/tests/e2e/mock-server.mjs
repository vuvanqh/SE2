// Minimal Node mock backend used by Playwright E2E tests. Implements just
// enough endpoints to drive the auth + admin flows. No external deps.
import { createServer } from "node:http";

const PORT = 5049;

function send(res, status, body, extraHeaders = {}) {
    res.writeHead(status, {
        "content-type": "application/json",
        "access-control-allow-origin": "*",
        "access-control-allow-credentials": "true",
        "access-control-allow-headers": "content-type, authorization",
        "access-control-allow-methods": "GET,POST,PUT,DELETE,OPTIONS",
        ...extraHeaders,
    });
    res.end(JSON.stringify(body));
}

function readJson(req) {
    return new Promise((resolve) => {
        let buf = "";
        req.on("data", (c) => (buf += c));
        req.on("end", () => {
            try { resolve(JSON.parse(buf || "{}")); } catch { resolve({}); }
        });
    });
}

const users = [
    { id: "u1", email: "student@pw.edu.pl", firstName: "Stu", lastName: "Dent", role: "Student", facultyId: "fac-1" },
    { id: "u2", email: "manager@pw.edu.pl", firstName: "Man", lastName: "Ager", role: "Manager", facultyId: "fac-1" },
];

const server = createServer(async (req, res) => {
    const url = req.url || "/";
    const method = req.method || "GET";
    if (method === "OPTIONS") return send(res, 204, {});

    if (url === "/api/health") return send(res, 200, { ok: true });

    // ---- AUTH
    if (url === "/api/auth/login" && method === "POST") {
        const body = await readJson(req);
        if (body.email === "student@pw.edu.pl" && body.password === "Password1!") {
            return send(res, 200, {
                token: "stu-token", userRole: "Student", facultyId: "fac-1",
                email: body.email, firstName: "Stu", lastName: "Dent",
            });
        }
        if (body.email === "manager@pw.edu.pl" && body.password === "Password1!") {
            return send(res, 200, {
                token: "mgr-token", userRole: "Manager", facultyId: "fac-1",
                email: body.email, firstName: "Man", lastName: "Ager",
            });
        }
        if (body.email === "admin@pw.edu.pl" && body.password === "Password1!") {
            return send(res, 200, {
                token: "adm-token", userRole: "Admin", facultyId: "fac-1",
                email: body.email, firstName: "Ad", lastName: "Min",
            });
        }
        return send(res, 401, { errors: ["Invalid credentials"] });
    }
    if (url === "/api/auth/register" && method === "POST") {
        const body = await readJson(req);
        if (!String(body.email || "").endsWith("@pw.edu.pl"))
            return send(res, 400, { errors: ["Email must use @pw.edu.pl domain"] });
        return send(res, 200, { ok: true });
    }
    if (url === "/api/auth/reset-password" && method === "POST") return send(res, 200, { ok: true });
    if (url === "/api/auth/verify-reset" && method === "POST") {
        const body = await readJson(req);
        if (body.token === "valid-token") return send(res, 200, { ok: true });
        return send(res, 400, { errors: ["Invalid or expired token"] });
    }
    if (url === "/api/auth/logout" && method === "POST") return send(res, 200, { ok: true });
    if (url === "/api/auth/refreshToken" && method === "POST") return send(res, 200, "refreshed-token");

    // ---- ADMIN
    if (url === "/api/admin/users" && method === "GET") return send(res, 200, users);
    if (url === "/api/admin/managers" && method === "POST") {
        const body = await readJson(req);
        const u = { id: `u-${users.length + 1}`, role: "Manager", facultyId: "fac-1", ...body };
        users.push(u);
        return send(res, 200, u);
    }
    if (url.startsWith("/api/admin/users/") && method === "DELETE") {
        const id = url.split("/").pop();
        const idx = users.findIndex((u) => u.id === id);
        if (idx >= 0) users.splice(idx, 1);
        return send(res, 200, { ok: true });
    }

    // ---- FACULTY
    if (url === "/api/Faculty" && method === "GET") {
        return send(res, 200, [
            { id: "fac-1", code: "MEiL", name: "Mech & Aero" },
            { id: "fac-2", code: "EiTI", name: "Electronics" },
        ]);
    }

    // ---- Catch-all empty arrays / 200 to avoid blocking the UI
    if (method === "GET") return send(res, 200, []);
    if (method === "POST" || method === "PUT" || method === "DELETE")
        return send(res, 200, { ok: true });

    send(res, 404, { error: "not found" });
});

server.listen(PORT, () => {
    // eslint-disable-next-line no-console
    console.log(`[mock-server] listening on http://localhost:${PORT}`);
});
