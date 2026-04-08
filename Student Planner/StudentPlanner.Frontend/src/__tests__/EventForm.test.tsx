import '@testing-library/jest-dom';
import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';

import EventForm from '../components/common/EventForm';

describe('EventForm Component Test', () => {

    const initialValues = {
        title: '',
        location: '',
        startTime: '',
        endTime: '',
        description: '',
        errors: []
    };

    //  Does the form render all the correct input names?
    it('Should render the form inputs with empty values', () => {
        render(
            <EventForm 
                initialValues={initialValues} 
                submitLabel="Create Now" 
                onClose={vi.fn()} 
                onSubmit={vi.fn()} 
            />
        );

        // Check if labels appear
        expect(screen.getByText('Title')).toBeInTheDocument();
        expect(screen.getByText('Location')).toBeInTheDocument();
        expect(screen.getByText('Start Time')).toBeInTheDocument();
        expect(screen.getByText('End Time')).toBeInTheDocument();

        // Check if the custom Submit Label works
        expect(screen.getByRole('button', { name: 'Create Now' })).toBeInTheDocument();
    });


    it('Should trigger the onClose function when the Cancel button is clicked', () => {
        const mockOnClose = vi.fn();

        render(
            <EventForm 
                initialValues={initialValues} 
                submitLabel="Create Now" 
                onClose={mockOnClose} 
                onSubmit={vi.fn()} 
            />
        );

        const cancelButton = screen.getByRole('button', { name: 'Cancel' });
        fireEvent.click(cancelButton);

        expect(mockOnClose).toHaveBeenCalled();
    });

    it('Should display errors if the errors list is not empty', () => {
        const errorValues = {
            ...initialValues,
            errors: ['Error 1: Title is too short', 'Error 2: Invalid Date']
        };

        render(
            <EventForm 
                initialValues={errorValues} 
                submitLabel="Create Now" 
                onClose={vi.fn()} 
                onSubmit={vi.fn()} 
            />
        );

        expect(screen.getByText('Error 1: Title is too short')).toBeInTheDocument();
        expect(screen.getByText('Error 2: Invalid Date')).toBeInTheDocument();
    });
});