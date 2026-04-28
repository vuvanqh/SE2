import type { ReactNode } from "react";
import Modal from "../modals/Modal";

type FormModalProps = {
    title: string;
    open: boolean;
    onClose: () => void;
    children: ReactNode;
};

export default function FormModal({ title, open, onClose, children }: FormModalProps) {
    return (
        <Modal open={open} onClose={onClose}>
            <div className="modal-header">
                <h2>{title}</h2>
            </div>
            <hr className="modal-divider" />
            {children}
        </Modal>
    );
}
