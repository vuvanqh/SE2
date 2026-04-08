import '@testing-library/jest-dom';
import { describe, it, expect, vi, beforeAll, beforeEach } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';

import LoginPage from '../pages/public/LoginPage';

const mockNavigate = vi.fn();

vi.mock('react-router-dom', async () => {
    const actual = await vi.importActual<typeof import('react-router-dom')>('react-router-dom');
    return {
        ...actual,
        useNavigate: () => mockNavigate
    };
});

vi.mock('../global-hooks/authHooks', () => {
  return {
    useAuth: () => {
      return { login: vi.fn(), isLoginPending: false };
    }
  };
});

describe('Login Component Test', () => {
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

    it('Should display the welcome text and the login button', () => {
        render(
            <MemoryRouter initialEntries={['/login']}>
                <LoginPage />
            </MemoryRouter>
        );

        const welcomeText = screen.getByText('Welcome back to Student Planner');
        expect(welcomeText).toBeInTheDocument();

        const loginButton = screen.getByRole('button', { name: 'Log In', hidden: true });
        expect(loginButton).toBeInTheDocument();
    });

    // The Negative Test 
    it('Should show an error if the email does not end in @pw.edu.pl', () => {
        render(
            <MemoryRouter initialEntries={['/login']}>
                <LoginPage />
            </MemoryRouter>
        );

        const emailInput = document.querySelector('input[name="email"]') as HTMLInputElement;

    
        fireEvent.change(emailInput, { target: { value: 'student@gmail.com' } });   //  a wrong email address


        expect(emailInput.validationMessage).toBe('Use @pw.edu.pl email');    // Check if the system correctly gives the warning message
    });

    //  The Positive Test  
    it('Should accept a valid university email without errors', () => {
        render(
            <MemoryRouter initialEntries={['/login']}>
                <LoginPage />
            </MemoryRouter>
        );

        const emailInput = document.querySelector('input[name="email"]') as HTMLInputElement;

        // Pretend correct university email address
        fireEvent.change(emailInput, { target: { value: 'student@pw.edu.pl' } });

        // Check if the system accepts it
        expect(emailInput.validationMessage).toBe('');
    });

    it('Should show close X button and navigate home when clicked', () => {
        render(
            <MemoryRouter initialEntries={['/login']}>
                <LoginPage />
            </MemoryRouter>
        );

        const closeButton = screen.getByRole('button', { name: 'Close login modal', hidden: true });
        expect(closeButton).toBeInTheDocument();

        fireEvent.click(closeButton);
        expect(mockNavigate).toHaveBeenCalledWith('/');
    });
});