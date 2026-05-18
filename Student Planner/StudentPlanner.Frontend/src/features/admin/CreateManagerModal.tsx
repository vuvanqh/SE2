import Input from "../../components/common/Input"
import Modal from "../../components/modals/Modal"
import { useFaculties } from "../../global-hooks/facultyHooks";
import Select from "react-select";
import { useState } from "react";
import { useActionState } from "react";
import { useAdmin } from "./hooks/adminHooks";
import { emailValidator } from "../../api/helpers";
import { UNIVERSITY_ID } from "../../constants/university";

type stateType = {
    firstName: string,
    lastName: string,
    email: string,
    facultyId: string | undefined,
    errors: string[]
}

const initialValues: stateType = {
    firstName: "",
    lastName: "",
    email: "",
    facultyId: "",
    errors: []
}

export default function CreateManagerModal({onClose}: {onClose: () => void}){
    const {faculties, isPending} = useFaculties();
    const {createManager} = useAdmin();
    const [faculty, setFaculty] = useState<{
        value: string;
        label: string;
    } | null>(null);

    const [state, formAction] = useActionState(async (_: stateType, formData: FormData) => {
        const data = {
            firstName: formData.get("firstName") as string,
            lastName: formData.get("lastName") as string,
            email: formData.get("email") as string,
            facultyId: (formData.get("facultyId") as string) || undefined,
        };

        if (data.facultyId === UNIVERSITY_ID) {
            data.facultyId = undefined;
        }

        const { facultyId, ...otherFields } = data;
        if (!Object.values(otherFields).every(Boolean)){
            return { ...data, errors: ["First name, last name and email are required"] };
        }
        try{
            const result = await createManager(data);
            onClose();
            return { ...data, errors: result ?? [] };
        }
        catch(err: any){
            return { ...data, errors: [err.message || "An error occurred"] };
        }
    }, initialValues);

    const selectedFaculty = state.facultyId === UNIVERSITY_ID
    ? { value: UNIVERSITY_ID, label: "University" }
    : state.facultyId && faculties.find(f => f.facultyId === state.facultyId)
    ? {
        value: state.facultyId,
        label: faculties.find(f => f.facultyId === state.facultyId)!.facultyName
      }
    : null;

    return <Modal open onClose={onClose}>
            <h2 className="modal-title">Create Manager Account</h2>
            {isPending? <p>Loading...</p> : 
            <form action={formAction} className="modal-form">
                <div className="input-row">
                    <Input label="First Name" id="firstName" name="firstName" type="text" defaultValue={state.firstName}/>
                    <Input label="Last Name" id="lastName" name="lastName" type="text" defaultValue={state.lastName}/>
                </div>
                <Input label="Email" id="email"name="email" type="email" defaultValue={state.email} 
                pattern="^[^@]+@pw\.edu\.pl$" onChange={emailValidator}/>
                
                <Select placeholder="Select faculty..." value={faculty?? selectedFaculty} onChange={setFaculty}
                    options={[
                        { value: UNIVERSITY_ID, label: "University" },
                        ...faculties.map((f) => ({
                            value: f.facultyId,
                            label: f.facultyName,
                        }))
                    ]}
                    classNamePrefix="react-select"
                />

                {/* submitted with form */}
                <input type="hidden" name="facultyId" value={faculty?.value ?? ""} />
                <div className="modal-actions">
                    <button type="button" className="btn-secondary" onClick={onClose}>Cancel</button>
                    <button type="submit" className="btn-primary">Create</button>
                </div>
                {state.errors.length > 0 && (
                <ul className="error-list">
                    {state.errors.map(err => (
                        <li key={err}>{err}</li>
                    ))}
                </ul>)}
            </form>}
    </Modal>
}