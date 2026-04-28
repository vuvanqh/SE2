import { describe, it, expect, vi, beforeEach } from "vitest";

// Re-mock react-toastify here too because vi.mock from setup.ts is hoisted
// per file – but the global vi.mock in setup.ts already covers this. We
// just re-import to grab the spies.
import { toast } from "react-toastify";
import {
    successMessage,
    errorMessage,
    infoMessage,
} from "../../src/toast/toastNotifications";

describe("toastNotifications", () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it("successMessage delegates to toast.success with the right class", () => {
        successMessage("done");
        expect(toast.success).toHaveBeenCalledTimes(1);
        const [msg, opts] = (toast.success as any).mock.calls[0];
        expect(msg).toBe("done");
        expect(opts.className).toContain("app-toast-success");
    });

    it("errorMessage delegates to toast.error", () => {
        errorMessage("boom");
        expect(toast.error).toHaveBeenCalledWith(
            "boom",
            expect.objectContaining({
                className: expect.stringContaining("app-toast-error"),
            }),
        );
    });

    it("infoMessage calls the default toast() with system-toast content", () => {
        infoMessage("hi");
        expect(toast).toHaveBeenCalledTimes(1);
        const [content, opts] = (toast as any).mock.calls[0];
        expect(content).toBeTruthy(); // JSX node
        expect(opts.className).toContain("app-toast-info");
    });
});
