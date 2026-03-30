
type authorsCardProps = {
    fullName: string,
    image_src: string,
    img_pos?: string
}

export default function AuthorsCard({fullName, image_src, img_pos=""}: authorsCardProps){
    return (
    <div className="author-card">
        <div className="author-image">
            <img src={image_src} alt={fullName} className={img_pos}/>
        </div>

        <div className="author-info">
            <hr />
            <p>{fullName}</p>
        </div>
    </div>)
}