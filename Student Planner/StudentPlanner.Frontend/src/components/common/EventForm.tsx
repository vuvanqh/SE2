import Input from "../common/Input";
import { useActionState } from "react";
import { validateData } from "../../api/helpers";

type stateType = {
    title: string,
    location?: string,
    startTime: string,
    endTime: string,
    description?: string,
    errors: string[]
}

type EventFormProps = {
  initialValues: stateType;
  onClose: () => void;
  onSubmit: (data: any) => Promise<string[] | null>;
  submitLabel: string;
};

export default function EventForm({ initialValues, onClose, onSubmit, submitLabel }: EventFormProps) {
    const [state, formAction] = useActionState(async (_: stateType, formData: FormData) => {
        const data = {
            title: formData.get("title") as string,
            location: formData.get("location") as string || undefined,
            startTime: formData.get("startTime") as string,
            endTime: formData.get("endTime") as string,
            description: formData.get("description") as string || undefined,
        };

        const errors = validateData(data);
        if (errors.length) return { ...data, errors };

        const result = await onSubmit(data);
        return { ...data, errors: result ?? [] };
    }, initialValues);

  return (
    <form action={formAction} className="modal-form">
      <div className="modal-body">
            <Input id="title" label="Title" type="text" defaultValue={state.title} placeholder="Title..."/>
            <Input id="location" label="Location" type="text" defaultValue={state.location} placeholder="Location..."/>
            <div className="form-row">
                <Input id="startTime" label="Start Time" type="datetime-local" defaultValue={state.startTime} placeholder="Start Time..."/>
                <Input id="endTime" label="End Time" type="datetime-local" defaultValue={state.endTime} placeholder="End Time..."/>
            </div>
            <Input id="description" label="Description" type="text" defaultValue={state.description} placeholder="Description..."/>

            {state.errors.length > 0 && (
                <div className="form-errors">
                    {state.errors.map((err, i) => (
                    <p key={i}>{err}</p>
                    ))}
                </div>
            )}
        </div>

        <div className="modal-actions">
            <button type="button" className="btn-secondary" onClick={onClose}>
                Cancel
            </button>

            <button type="submit" className="btn-primary">
               {submitLabel}
            </button>
        </div>
    </form>
  );
}