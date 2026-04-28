import "@testing-library/jest-dom/vitest";
import { afterAll, afterEach, beforeAll, beforeEach, vi } from "vitest";
import { cleanup } from "@testing-library/react";
import { server } from "./mocks/server";

// HTMLDialogElement.showModal/close are not implemented in jsdom.
if (typeof HTMLDialogElement !== "undefined") {
    if (!HTMLDialogElement.prototype.showModal) {
        HTMLDialogElement.prototype.showModal = function (this: HTMLDialogElement) {
            this.setAttribute("open", "");
        };
    }
    if (!HTMLDialogElement.prototype.close) {
        HTMLDialogElement.prototype.close = function (this: HTMLDialogElement) {
            this.removeAttribute("open");
            this.dispatchEvent(new Event("close"));
        };
    }
}

// react-toastify spams the DOM and uses portals. Stub it so we can assert
// success/error notifications without rendering its tree.
vi.mock("react-toastify", async () => {
    const actual = await vi.importActual<typeof import("react-toastify")>("react-toastify");
    const toast = Object.assign(vi.fn(), {
        success: vi.fn(),
        error: vi.fn(),
        info: vi.fn(),
        warn: vi.fn(),
    });
    return {
        ...actual,
        toast,
        ToastContainer: () => null,
        Slide: actual.Slide,
    };
});

// SignalR makes real connections – stub it everywhere.
vi.mock("@microsoft/signalr", () => {
    class HubConnectionBuilder {
        withUrl() { return this; }
        withAutomaticReconnect() { return this; }
        configureLogging() { return this; }
        build() {
            return {
                start: vi.fn().mockResolvedValue(undefined),
                stop: vi.fn().mockResolvedValue(undefined),
                on: vi.fn(),
                off: vi.fn(),
                state: "Disconnected",
            };
        }
    }
    return {
        HubConnectionBuilder,
        LogLevel: { Information: 1, Warning: 2, Error: 3 },
        HttpTransportType: { WebSockets: 1, ServerSentEvents: 2, LongPolling: 4 },
        HubConnectionState: {
            Disconnected: "Disconnected",
            Connecting: "Connecting",
            Connected: "Connected",
            Disconnecting: "Disconnecting",
            Reconnecting: "Reconnecting",
        },
    };
});

// Ensure the modal portal target exists for components that use createPortal.
beforeEach(() => {
    if (!document.getElementById("modal")) {
        const el = document.createElement("div");
        el.id = "modal";
        document.body.appendChild(el);
    }
});

beforeAll(() => server.listen({ onUnhandledRequest: "error" }));
afterEach(() => {
    server.resetHandlers();
    localStorage.clear();
    sessionStorage.clear();
    cleanup();
    vi.clearAllMocks();
    document.getElementById("modal")?.remove();
});
afterAll(() => server.close());
