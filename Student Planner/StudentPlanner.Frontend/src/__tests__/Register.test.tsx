import '@testing-library/jest-dom';
import { describe, it, expect, vi, beforeAll, beforeEach } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';

import RegisterPage from '../pages/public/RegisterPage';

const mockNavigate = vi.fn();

vi.mock('react-router-dom', async () => {
    const actual = await vi.importActual<typeof import('react-router-dom')>('react-router-dom');
    return {
        ...actual,
        useNavigate: () => mockNavigate
    };
});

//  pretend we are signing up
vi.mock('../global-hooks/authHooks', () => {
    return {
        useAuth: () => {
            return { registerUser: vi.fn(), isRegisterPending: false };
        }
    };
});

describe('Register Component Test', () => {
    
    beforeAll(() => {
        const modalRoot = document.createElement('div');
        modalRoot.setAttribute('id', 'modal');
        document.body.appendChild(modalRoot);

        HTMLDialogElement.prototype.showModal = vi.fn();
        HTMLDialogElement.prototype.close = vi.fn();
    });

    beforeEach(() => {
        mockNavigate.mockClear();
    });

       // negative test
    it('Should display the registration welcome text and the create button', () => {
        render(
            <MemoryRouter initialEntries={['/register']}>
                <RegisterPage />
            </MemoryRouter>
        );

        const welcomeText = screen.getByText('Join Student Planner today');
        expect(welcomeText).toBeInTheDocument();

        const submitButton = screen.getByRole('button', { name: 'Create Account', hidden: true });
        expect(submitButton).toBeInTheDocument();
    });

    
    it('Should show an error if the email does not end in @pw.edu.pl during registration', () => {
        render(
            <MemoryRouter initialEntries={['/register']}>
                <RegisterPage />
            </MemoryRouter>
        );

       
        const emailInput = document.querySelector('input[name="email"]') as HTMLInputElement;        
        fireEvent.change(emailInput, { target: { value: 'student@gmail.com' } });  // Pretend user enters wrong email address
    
        expect(emailInput.validationMessage).toBe('Use @pw.edu.pl email');   // Check the system correctly gives the warning message
    });

       // positive test 
    it('Should accept a valid university email for registration without errors', () => {
        render(
            <MemoryRouter initialEntries={['/register']}>
                <RegisterPage />
            </MemoryRouter>
        );

        const emailInput = document.querySelector('input[name="email"]') as HTMLInputElement;

        fireEvent.change(emailInput, { target: { value: 'student@pw.edu.pl' } });   // correct university email address

        expect(emailInput.validationMessage).toBe('');  
    });

    it('Should show close X button and navigate home when clicked', () => {
        render(
            <MemoryRouter initialEntries={['/register']}>
                <RegisterPage />
            </MemoryRouter>
        );

        const closeButton = screen.getByRole('button', { name: 'Close registration modal', hidden: true });
        expect(closeButton).toBeInTheDocument();

        fireEvent.click(closeButton);
        expect(mockNavigate).toHaveBeenCalledWith('/');
    });
});