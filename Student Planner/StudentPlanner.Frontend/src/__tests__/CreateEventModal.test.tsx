import '@testing-library/jest-dom';
import { describe, it, expect, vi, beforeAll } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';

import CreateEventModal from '../features/events/components/CreateEventModal';

//  Mock the hook that handles sending the event to the database
const mockCreateEvent = vi.fn();
vi.mock('../features/events/hooks/personalEventHooks', () => {
    return {
        useGetAllPersonalEvents: () => {
            return { createEvent: mockCreateEvent };
        }
    };
});

describe('Create Event Component Test', () => {
    
    beforeAll(() => {
        const modalRoot = document.createElement('div');
        modalRoot.setAttribute('id', 'modal');
        document.body.appendChild(modalRoot);

        HTMLDialogElement.prototype.showModal = vi.fn();
        HTMLDialogElement.prototype.close = vi.fn();
    });

    it('Should display the Create Event title and input fields', () => {
        render(<CreateEventModal onClose={vi.fn()} />);

        expect(screen.getByText('Create Event')).toBeInTheDocument();   // Check title popup exists ??
        
        const createButton = screen.getByRole('button', { name: 'Create', hidden: true });  // Check "Create" button exists ??
        expect(createButton).toBeInTheDocument();
    });


    it('Should allow a user to type the Title into the form', () => {
        render(<CreateEventModal onClose={vi.fn()} />);

        const titleInput = document.querySelector('input[name="title"]') as HTMLInputElement;
     
        fireEvent.change(titleInput, { target: { value: 'My Math Exam' } });

        expect(titleInput.value).toBe('My Math Exam');
    });
});