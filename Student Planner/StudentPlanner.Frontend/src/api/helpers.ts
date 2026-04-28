import type { eventPreviewResponse } from "../types/eventPreviewResponse";
import type { createPersonalEventRequest } from "../types/personalEventTypes";

export function extractErrors(err: any): string[] {
    const raw = err?.response?.data || err?.info?.errors || err;

    if (!raw) return ["Unknown error"];

    if (typeof raw === "string") return [raw];
    if (Array.isArray(raw)) return raw;
    
    if (raw?.errors && typeof raw.errors === "object") {
        // Handle ASP.NET Core ValidationProblemDetails
        return Object.values(raw.errors).flat() as string[];
    }

    if (raw?.message) return [raw.message];

    return ["An unexpected error occurred"];
}


export function isSameDay(a: string, b: Date) {
  const d = new Date(a);

  return (
    d.getFullYear() === b.getFullYear() &&
    d.getMonth() === b.getMonth() &&
    d.getDate() === b.getDate()
  );
}

export function toLocalInput(date: Date) {
  const offset = date.getTimezoneOffset() * 60000;
  return new Date(date.getTime() - offset).toISOString().slice(0, 16);
}


export function validateData(data: createPersonalEventRequest){
    const errors: string[] = []

    const start = new Date(data.startTime);
    const end = new Date(data.endTime);
    const now = new Date();

    if (end <= start) 
        errors.push("End time must be after start time.");        

    if (start < now) 
        errors.push("Start time cannot be in the past.");    
            
    if(data.title.trim().length==0)
        errors.push("Title cannot be empty.")

    if(data.startTime.trim().length==0)
        errors.push("Start time cannot be empty.")

    if(data.endTime.trim().length==0)
        errors.push("End time cannot be empty.")

    return errors;
}

export function formatDate(input: string) {
  const date = new Date(input);

  const datePart = date.toLocaleDateString("en-GB", {
    day: "2-digit",
    month: "short",
    year: "numeric",
  });

  const startTime = date.toLocaleTimeString([], {
    hour: "2-digit",
    minute: "2-digit",
  });


  return `${datePart} ${startTime}`;
}

export function emailValidator(e: React.ChangeEvent<HTMLInputElement>){
  const input = e.currentTarget;
  const valid = /^[^@]+@pw\.edu\.pl$/.test(input.value);

  input.setCustomValidity(valid ? "" : "Use @pw.edu.pl email");
  input.reportValidity();
}

export function getNEvents(eventPreviews: eventPreviewResponse[], n: number){
  // Return the next N upcoming events, sorted soonest-first.
  return [...eventPreviews]
      .filter(d => new Date(d.startTime).getTime() > Date.now())
      .sort((a, b) => new Date(a.startTime).getTime() - new Date(b.startTime).getTime())
      .slice(0, n);
}