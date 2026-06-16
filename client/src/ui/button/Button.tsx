import './Button.css';

interface ButtonProps {
    label: string;
    isPrimary?: boolean;
    onClick: () => void;
    [key: string]: any; // for additional props like style, className etc.
}

const Button = ({ label, isPrimary, onClick, ...props }: ButtonProps) => {
    return (
        <button 
            className={`ui-button ${isPrimary ? "primary-button": "secondary-button"}`}
            onClick={onClick}
            {...props}
        >
            {label}
        </button>
    );
};

export default Button;