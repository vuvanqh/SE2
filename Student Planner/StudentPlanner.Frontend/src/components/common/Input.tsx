type InputProps = {
    label: string,
    id: string,
    className?: string,
    error?: string | null
} & React.InputHTMLAttributes<HTMLInputElement>; 

export default function Input({label, id, className = '', error = '', ...props}:InputProps){
    return <div className={`input-group ${error ? 'input-error' : ''} ${className}`}>
        <label htmlFor={id} className="input-label">
            {label}
        </label>

        <input id={id} name={id} className="input-field" {...props} required/>
        {error && <small className="error-text">{error}</small>}
    </div>
}