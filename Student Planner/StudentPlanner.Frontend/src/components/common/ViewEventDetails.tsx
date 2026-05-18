import { formatDate } from "../../api/helpers";

type ViewEventDetailsProps = {
    location?: string;
    startTime: string;
    endTime: string;
    description?: string;
};

export default function ViewEventDetails({ location, startTime, endTime, description }: ViewEventDetailsProps) {
    return (
        <>
            <div className="view-section">
                <p className="view-label">Details</p>
                <div className="view-content">
                    {location && <p><strong>Location:</strong> {location}</p>}
                    <p>{formatDate(startTime)} - {formatDate(endTime)}</p>
                </div>
            </div>

            <div className="view-section">
                <p className="view-label">Description</p>
                <p className="view-text">{description}</p>
            </div>
        </>
    );
}
