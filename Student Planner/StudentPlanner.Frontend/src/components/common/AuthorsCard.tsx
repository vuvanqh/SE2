type AuthorsCardProps = {
    fullName: string;
    imageSrc: string;
    imgPos?: string;
};

export default function AuthorsCard({ fullName, imageSrc, imgPos = "" }: AuthorsCardProps) {
    return (
        <div className="author-card">
            <div className="author-image">
                <img src={imageSrc} alt={fullName} className={imgPos} />
            </div>

            <div className="author-info">
                <hr />
                <p>{fullName}</p>
            </div>
        </div>
    );
}