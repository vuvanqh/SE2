import { describe, it, expect } from "vitest";
import {
    extractErrors,
    isSameDay,
    toLocalInput,
    validateData,
    formatDate,
    getNEvents,
} from "../../src/api/helpers";

describe("extractErrors", () => {
    it("returns 'Unknown error' for falsy", () => {
        expect(extractErrors(null)).toEqual(["Unknown error"]);
        expect(extractErrors(undefined)).toEqual(["Unknown error"]);
    });

    it("wraps a string into a single-element array", () => {
        expect(extractErrors("boom")).toEqual(["boom"]);
    });

    it("returns arrays as-is", () => {
        expect(extractErrors(["a", "b"])).toEqual(["a", "b"]);
    });

    it("flattens ASP.NET ValidationProblemDetails errors", () => {
        const err = {
            response: {
                data: { errors: { Email: ["required"], Password: ["too short", "no digit"] } },
            },
        };
        expect(extractErrors(err)).toEqual(["required", "too short", "no digit"]);
    });

    it("uses .message when present", () => {
        expect(extractErrors({ response: { data: { message: "nope" } } })).toEqual(["nope"]);
    });

    it("falls back to a generic message", () => {
        expect(extractErrors({ response: { data: { weird: 1 } } })).toEqual([
            "An unexpected error occurred",
        ]);
    });

    it("reads errors from info.errors when no response.data", () => {
        expect(extractErrors({ info: { errors: ["x"] } })).toEqual(["x"]);
    });
});

describe("isSameDay", () => {
    it("returns true for same calendar day regardless of time", () => {
        expect(isSameDay("2025-06-15T22:00:00", new Date(2025, 5, 15, 1, 0, 0))).toBe(true);
    });

    it("returns false for different days", () => {
        expect(isSameDay("2025-06-15T22:00:00", new Date(2025, 5, 16))).toBe(false);
    });
});

describe("toLocalInput", () => {
    it("returns YYYY-MM-DDTHH:mm without seconds", () => {
        const out = toLocalInput(new Date(2025, 5, 15, 13, 45));
        expect(out).toMatch(/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}$/);
        expect(out).toBe("2025-06-15T13:45");
    });
});

describe("validateData", () => {
    // Use a fixed far-future date string in local-time format (no Z) so the
    // test is not sensitive to the test runner's timezone.
    const future = (offsetMin: number) => {
        const d = new Date(Date.now() + offsetMin * 60_000);
        const pad = (n: number) => String(n).padStart(2, "0");
        return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
    };

    it("accepts a valid event in the future", () => {
        expect(
            validateData({
                title: "Study",
                startTime: future(60),
                endTime: future(120),
            } as any),
        ).toEqual([]);
    });

    it("rejects when end <= start", () => {
        const errors = validateData({
            title: "Study",
            startTime: future(120),
            endTime: future(60),
        } as any);
        expect(errors).toContain("End time must be after start time.");
    });

    it("rejects start in the past", () => {
        const errors = validateData({
            title: "Study",
            startTime: future(-60),
            endTime: future(60),
        } as any);
        expect(errors).toContain("Start time cannot be in the past.");
    });

    it("rejects empty title", () => {
        const errors = validateData({
            title: "   ",
            startTime: future(60),
            endTime: future(120),
        } as any);
        expect(errors).toContain("Title cannot be empty.");
    });

    it("rejects empty start/end", () => {
        const errors = validateData({
            title: "Study",
            startTime: "",
            endTime: "",
        } as any);
        expect(errors).toEqual(
            expect.arrayContaining([
                "Start time cannot be empty.",
                "End time cannot be empty.",
            ]),
        );
    });
});

describe("formatDate", () => {
    it("returns a non-empty 'DD MMM YYYY HH:mm' string", () => {
        const out = formatDate("2025-06-15T13:45:00");
        expect(out).toMatch(/15 Jun 2025/);
        expect(out.length).toBeGreaterThan(0);
    });
});

describe("getNEvents", () => {
    const previews = [
        { startTime: new Date(Date.now() + 1 * 60 * 60 * 1000).toISOString() }, // soon
        { startTime: new Date(Date.now() + 5 * 60 * 60 * 1000).toISOString() }, // later
        { startTime: new Date(Date.now() - 60 * 60 * 1000).toISOString() }, // past
    ] as any[];

    it("filters out events in the past", () => {
        const result = getNEvents(previews, 10);
        expect(result.every((e) => new Date(e.startTime).getTime() > Date.now())).toBe(true);
    });

    it("limits to n results", () => {
        const result = getNEvents(previews, 1);
        expect(result).toHaveLength(1);
    });

    // After fix: getNEvents returns the SOONEST-upcoming events first, so
    // requesting n=1 from a list with two future events returns the closer one.
    it("returns the soonest upcoming events first", () => {
        const result = getNEvents(previews, 1);
        const futureSorted = previews
            .filter((p) => new Date(p.startTime).getTime() > Date.now())
            .sort((a, b) => new Date(a.startTime).getTime() - new Date(b.startTime).getTime());
        expect(result[0]).toEqual(futureSorted[0]);
    });
});
