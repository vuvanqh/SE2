import '@testing-library/jest-dom';
import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';

import Input from '../components/common/Input';

describe('Input Component Test', () => {

    it('Should render the input with its label', () => {
        render(<Input id="test-input" label="My Custom Label" />);
        
        // Check label is rendered ??
        const labelElement = screen.getByText('My Custom Label');
        expect(labelElement).toBeInTheDocument();

        const inputElement = document.querySelector('input[name="test-input"]') as HTMLInputElement;
        expect(inputElement).toBeInTheDocument();
    });

    it('Should display an error message and add an error class when an error is provided', () => {
        render(<Input id="test-error" label="Error Label" error="This field is very required!" />);
        
        const errorMessage = screen.getByText('This field is very required!');
        expect(errorMessage).toBeInTheDocument();
        
        expect(errorMessage).toHaveClass('error-text');
    });
});