import { describe, it, expect } from "vitest";
import { render, screen } from "@testing-library/react";
import Input from "../../src/components/common/Input";

describe("<Input />", () => {
    it("renders the label and an input bound by id", () => {
        render(<Input id="email" label="University Email" type="email" />);
        const input = screen.getByLabelText("University Email") as HTMLInputElement;
        expect(input).toBeInTheDocument();
        expect(input.name).toBe("email");
        expect(input.required).toBe(true);
        expect(input.type).toBe("email");
    });

    it("forwards extra props (placeholder, defaultValue)", () => {
        render(
            <Input
                id="title"
                label="Title"
                placeholder="Enter title"
                defaultValue="hello"
            />,
        );
        const input = screen.getByLabelText("Title") as HTMLInputElement;
        expect(input.placeholder).toBe("Enter title");
        expect(input.defaultValue).toBe("hello");
    });

    it("renders the error message and input-error class when error is provided", () => {
        const { container } = render(
            <Input id="x" label="X" error="Bad value" />,
        );
        expect(screen.getByText("Bad value")).toBeInTheDocument();
        expect(container.querySelector(".input-error")).not.toBeNull();
    });

    it("does not render the error block when no error", () => {
        const { container } = render(<Input id="x" label="X" />);
        expect(container.querySelector(".error-text")).toBeNull();
        expect(container.querySelector(".input-error")).toBeNull();
    });

    it("merges a custom className", () => {
        const { container } = render(
            <Input id="x" label="X" className="my-custom" />,
        );
        expect(container.querySelector(".input-group.my-custom")).not.toBeNull();
    });
});
