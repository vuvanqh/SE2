import type { InputHTMLAttributes } from "react";

type FilterOptionProps = {
    label: string;
    value: boolean;
    onChange: (newValue: boolean) => void;
} & Omit<InputHTMLAttributes<HTMLInputElement>, 'value' | 'onChange' | 'type'>;

export default function FilterOption({label, value, onChange, ...props}: FilterOptionProps){
    return <label className="filter-option">
        <input type="checkbox" checked={value} onChange={(e) => onChange(e.target.checked)} {...props} />
        {label}
    </label>
}