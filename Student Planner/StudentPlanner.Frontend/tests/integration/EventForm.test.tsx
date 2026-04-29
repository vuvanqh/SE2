import { describe, it, expect, vi } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import EventForm from "../../src/components/common/EventForm";

const validInitial = {
    title: "",
    location: "",
    startTime: "",
    endTime: "",
    description: "",
    errors: [] as string[],
};

function fillFutureDates(form: HTMLElement) {
    const start = form.querySelector<HTMLInputElement>("#startTime")!;
    const end = form.querySelector<HTMLInputElement>("#endTime")!;
    const pad = (n: number) => String(n).padStart(2, "0");
    const fmt = (d: Date) =>
        `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;

    const startD = new Date(Date.now() + 60 * 60_000);
    const endD = new Date(Date.now() + 120 * 60_000);
    return {
        start,
        end,
        startValue: fmt(startD),
        endValue: fmt(endD),
    };
}

describe("<EventForm /> (integration)", () => {
    it("submits valid data and calls onSubmit", async () => {
        const onSubmit = vi.fn().mockResolvedValue(null);
        const onClose = vi.fn();

        const { container } = render(
            <EventForm
                initialValues={validInitial}
                onClose={onClose}
                onSubmit={onSubmit}
                submitLabel="Save"
            />,
        );

        const user = userEvent.setup();
        await user.type(screen.getByLabelText(/Title/i), "Study session");
        await user.type(screen.getByLabelText(/Location/i), "Library");
        await user.type(screen.getByLabelText(/Description/i), "notes");

        const { start, end, startValue, endValue } = fillFutureDates(container);
        // Use fireEvent for datetime-local since user.type doesn't always work.
        // Instead set the values directly and dispatch input event.
        start.value = startValue;
        start.dispatchEvent(new Event("input", { bubbles: true }));
        end.value = endValue;
        end.dispatchEvent(new Event("input", { bubbles: true }));

        await user.click(screen.getByRole("button", { name: "Save" }));

        await waitFor(() => {
            expect(onSubmit).toHaveBeenCalledTimes(1);
        });
        expect(onSubmit).toHaveBeenCalledWith(
            expect.objectContaining({
                title: "Study session",
                location: "Library",
                startTime: startValue,
                endTime: endValue,
            }),
        );
    });

    it("shows validation errors and does NOT call onSubmit when end <= start", async () => {
        const onSubmit = vi.fn();
        render(
            <EventForm
                initialValues={validInitial}
                onClose={vi.fn()}
                onSubmit={onSubmit}
                submitLabel="Save"
            />,
        );
        const user = userEvent.setup();
        await user.type(screen.getByLabelText(/Title/i), "Bad");
        await user.type(screen.getByLabelText(/Location/i), "L");
        await user.type(screen.getByLabelText(/Description/i), "d");

        const start = document.querySelector<HTMLInputElement>("#startTime")!;
        const end = document.querySelector<HTMLInputElement>("#endTime")!;
        start.value = "2099-01-01T12:00";
        start.dispatchEvent(new Event("input", { bubbles: true }));
        end.value = "2099-01-01T11:00";
        end.dispatchEvent(new Event("input", { bubbles: true }));

        await user.click(screen.getByRole("button", { name: "Save" }));

        await waitFor(() => {
            expect(
                screen.getByText("End time must be after start time."),
            ).toBeInTheDocument();
        });
        expect(onSubmit).not.toHaveBeenCalled();
    });

    it("invokes onClose when Cancel is clicked", async () => {
        const onClose = vi.fn();
        const onSubmit = vi.fn();
        render(
            <EventForm
                initialValues={validInitial}
                onClose={onClose}
                onSubmit={onSubmit}
                submitLabel="Save"
            />,
        );
        const user = userEvent.setup();
        await user.click(screen.getByRole("button", { name: /cancel/i }));
        expect(onClose).toHaveBeenCalled();
        // onSubmit must NOT have been triggered: the Cancel button must not
        // implicitly submit the form (regression test for type=button bug).
        expect(onSubmit).not.toHaveBeenCalled();
    });

    it("displays errors returned by onSubmit (server-side errors)", async () => {
        const onSubmit = vi.fn().mockResolvedValue(["Server error: conflict"]);
        const { container } = render(
            <EventForm
                initialValues={validInitial}
                onClose={vi.fn()}
                onSubmit={onSubmit}
                submitLabel="Save"
            />,
        );
        const user = userEvent.setup();
        await user.type(screen.getByLabelText(/Title/i), "Study");
        await user.type(screen.getByLabelText(/Location/i), "L");
        await user.type(screen.getByLabelText(/Description/i), "d");
        const { start, end, startValue, endValue } = fillFutureDates(container);
        start.value = startValue;
        start.dispatchEvent(new Event("input", { bubbles: true }));
        end.value = endValue;
        end.dispatchEvent(new Event("input", { bubbles: true }));

        await user.click(screen.getByRole("button", { name: "Save" }));

        await waitFor(() => {
            expect(screen.getByText("Server error: conflict")).toBeInTheDocument();
        });
    });
});
