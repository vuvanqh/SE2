type InputProps = {
    label: string;
    id: string;
    className?: string;
    error?: string | null;
    ref?: React.Ref<HTMLInputElement>;
} & React.InputHTMLAttributes<HTMLInputElement>; 

export default function Input({label, id, className = '', error = '', ref, ...props}: InputProps){
    return <div className={`input-group ${error ? 'input-error' : ''} ${className}`}>
        <label htmlFor={id} className="input-label">
            {label}
        </label>

        <input id={id} name={props.name || id} className="input-field" ref={ref} {...props} />
        {error && <small className="error-text">{error}</small>}
    </div>
}