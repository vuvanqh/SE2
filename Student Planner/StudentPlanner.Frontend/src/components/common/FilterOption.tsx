export default function FilterOption({label, value, onChange, ...props}: {label: string, value: boolean, onChange: (newValue: boolean) => void}){
    return <label className="filter-option" {...props}>
        <input type="checkbox" checked={value} onChange={(e) => onChange(e.target.checked)}/>
        {label}
    </label>
}