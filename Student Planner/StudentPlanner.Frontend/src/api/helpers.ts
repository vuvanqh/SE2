import type { createPersonalEventRequest } from "../types/personalEventTypes";

export function extractErrors(err: any): string[] {
    const raw = err?.info?.errors;

    if (!raw) return ["Unknown error"];

    if (Array.isArray(raw)) return raw;
    if (typeof raw === "string") return [raw];

    return ["Unexpected error format"];
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