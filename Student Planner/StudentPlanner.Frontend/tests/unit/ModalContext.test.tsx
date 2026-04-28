import { describe, it, expect } from "vitest";
import { renderHook, act } from "@testing-library/react";
import { useReducer } from "react";

// Re-import the reducer indirectly via the provider component to assert
// stack semantics through behaviour rather than relying on private internals.
import ModalContextProvider, { ModalContext } from "../../src/store/ModalContext";
import { useContext } from "react";

function useModal() {
    return useContext(ModalContext);
}

describe("ModalContext", () => {
    it("starts with an empty stack", () => {
        const { result } = renderHook(() => useModal(), {
            wrapper: ({ children }) => <ModalContextProvider>{children}</ModalContextProvider>,
        });
        expect(result.current.state.stack).toEqual([]);
    });

    it("pushes and pops modals via open/close", () => {
        const { result } = renderHook(() => useModal(), {
            wrapper: ({ children }) => <ModalContextProvider>{children}</ModalContextProvider>,
        });

        act(() => result.current.open({ type: "createPersonal", startTime: "2099-01-01T10:00" }));
        expect(result.current.state.stack).toHaveLength(1);

        act(() => result.current.open({ type: "createManager" }));
        expect(result.current.state.stack).toHaveLength(2);
        expect(result.current.state.stack.at(-1)).toEqual({ type: "createManager" });

        act(() => result.current.close());
        expect(result.current.state.stack).toHaveLength(1);
        expect(result.current.state.stack.at(-1)?.type).toBe("createPersonal");

        act(() => result.current.close());
        expect(result.current.state.stack).toHaveLength(0);
    });

    it("close on an empty stack is a no-op (does not crash)", () => {
        const { result } = renderHook(() => useModal(), {
            wrapper: ({ children }) => <ModalContextProvider>{children}</ModalContextProvider>,
        });
        act(() => result.current.close());
        expect(result.current.state.stack).toEqual([]);
    });
});

// Bonus: makes sure the imported useReducer works the way we depend on.
describe("sanity: useReducer", () => {
    it("updates state via dispatch", () => {
        const { result } = renderHook(() =>
            useReducer((s: number, a: number) => s + a, 0),
        );
        act(() => result.current[1](5));
        expect(result.current[0]).toBe(5);
    });
});
