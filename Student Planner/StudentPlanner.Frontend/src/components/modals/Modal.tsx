import type { ReactNode } from "react";
import { createPortal } from "react-dom";
import { useRef, useEffect } from "react";

export type ModalProps = {
    open: boolean;
    onClose?: () => void;
    children: ReactNode;
    className?: string;
};
export default function Modal({open, children, onClose, className=""}: ModalProps){
    const dialog = useRef<HTMLDialogElement | null>(null);

    useEffect(()=>{
        const curr = dialog.current;

        if(open) 
            curr?.showModal();
        else if (!open && curr?.open)
            curr.close();
        
    }, [open])

    return createPortal(<dialog ref={dialog} onClose={onClose} className={`modal ${className}`}>
        {children}
    </dialog>, document.getElementById("modal")!);
}