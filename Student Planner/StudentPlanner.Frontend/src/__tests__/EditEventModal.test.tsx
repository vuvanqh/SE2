import '@testing-library/jest-dom';
import { describe, it, expect, vi, beforeAll } from 'vitest';
import { render, screen } from '@testing-library/react';

import EditEventModal from '../features/events/components/EditEventModal';

// Mock the hook that fetches the single event and updates it
const mockUpdateEvent = vi.fn();

vi.mock('../features/events/hooks/personalEventHooks', () => {
    return {
        useGetPersonalEvent: (eventId: string) => {
             // give it a fake event 
            if (eventId === '123') {
                return {
                    event: {
                        id: '123',
                        title: 'Existing Math Exam',
                        location: 'Room 205',
                        startTime: '2026-05-10T10:00',
                        endTime: '2026-05-10T12:00',
                        description: 'Final exam for Calculus'
                    },
                    isLoading: false,
                    updateEvent: mockUpdateEvent
                };
            }
            
            return { isLoading: true, event: null };  // else, pretend it's loading
        }
    };
});

describe('Edit Event Component Test', () => {
    
    beforeAll(() => {
        const modalRoot = document.createElement('div');
        modalRoot.setAttribute('id', 'modal');
        document.body.appendChild(modalRoot);

        HTMLDialogElement.prototype.showModal = vi.fn();
        HTMLDialogElement.prototype.close = vi.fn();
    });

    //  check if ttitle and save button are correct
    it('Should display the Edit Event title and the Save button', () => {
        render(<EditEventModal eventId="123" onClose={vi.fn()} />);

        expect(screen.getByText('Edit Event')).toBeInTheDocument();   // Check "Edit Event" title is shown ??
        
        // Check "Save" button exists (instead of "Create") ??
        const saveButton = screen.getByRole('button', { name: 'Save', hidden: true });
        expect(saveButton).toBeInTheDocument();
    });

   
    it('Should pre-fill the form with the existing event details', () => {
        render(<EditEventModal eventId="123" onClose={vi.fn()} />);

        const titleInput = document.querySelector('input[name="title"]') as HTMLInputElement;        
        const locationInput = document.querySelector('input[name="location"]') as HTMLInputElement;

        expect(titleInput.value).toBe('Existing Math Exam');
        expect(locationInput.value).toBe('Room 205');
    });
});