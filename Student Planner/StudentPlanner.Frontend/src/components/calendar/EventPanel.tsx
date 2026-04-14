import type { ReactNode } from "react";

type eventPanelProps = {
    label: string
    children: ReactNode
}

export default function EventPanel({children, label}:eventPanelProps){
    return  <div className="events-panel">
        <h3 className="events-header">{label}</h3>
        <hr />    
       {children}
    </div>
}