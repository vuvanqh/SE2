import '@testing-library/jest-dom';
import { describe, it, expect, vi, beforeAll } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';

import ViewEventModal from '../features/events/components/ViewEventModal';
import { ModalContext } from '../store/ModalContext'; //need this because ViewEventModal uses context!

// Mock the hook that fetches the single event and deletes it
const mockDeleteEvent = vi.fn();

vi.mock('../features/events/hooks/personalEventHooks', () => {
    return {
        useGetPersonalEvent: (eventId: string) => {
            // Give it a fake event
            if (eventId === '999') {
                return {
                    event: {
                        id: '999',
                        title: 'Project Meeting',
                        location: 'Library',
                        startTime: '2026-06-01T14:00',
                        endTime: '2026-06-01T16:00',
                        description: 'Discussing tests'
                    },
                    isLoading: false,
                    deleteEvent: mockDeleteEvent
                };
            }
            return { isLoading: true, event: null };
        }
    };
});

describe('View Event Component Test', () => {
    
    beforeAll(() => {
        const modalRoot = document.createElement('div');
        modalRoot.setAttribute('id', 'modal');
        document.body.appendChild(modalRoot);

        HTMLDialogElement.prototype.showModal = vi.fn();
        HTMLDialogElement.prototype.close = vi.fn();
    });

    
    it('Should display the event details correctly', () => {
        const mockOpen = vi.fn();
        
        render(
            
            <ModalContext.Provider value={{ open: mockOpen, close: vi.fn(), state: { stack: [] } }}>
                <ViewEventModal eventId="999" onClose={vi.fn()} />
            </ModalContext.Provider>
        );

        expect(screen.getByText('Project Meeting')).toBeInTheDocument();  // Check Title is rendered properly ??
        expect(screen.getByText('Discussing tests')).toBeInTheDocument(); // Check  Description is rendered properly ??
    });

    it('Should trigger the deletion function when the Delete button is clicked', async () => {
        const mockOnClose = vi.fn(); 
        const mockOpen = vi.fn();
        
        render(
            <ModalContext.Provider value={{ open: mockOpen, close: vi.fn(), state: { stack: [] } }}>
                <ViewEventModal eventId="999" onClose={mockOnClose} />
            </ModalContext.Provider>
        );

        const deleteButton = screen.getByRole('button', { name: 'Delete', hidden: true });
        await fireEvent.click(deleteButton);

        expect(mockDeleteEvent).toHaveBeenCalled();
        expect(mockOnClose).toHaveBeenCalled();
    });
});