
type introSectionProps = {
    left: boolean,
    title: string,
    children?: React.ReactNode,
    image_src: string,
    id?: string,
    className?: string
}

export default function IntroSection({left, title, children, image_src, id="", className=""}:introSectionProps){
    const imageClass = `col col-lg-6 ${left ? "order-lg-1" : "order-lg-2"}`;
    const textClass  = `col col-lg-6 ${left ? "order-lg-2" : "order-lg-1"}`;
    
    return <section className={left ? "intro-left" : "intro-right"} id={id}>
        <div className="container">
          <div className="row">

            <div className={imageClass}>
              <div className="p-5">
                <img className={`img-fluid rounded-circle ${className}`} src={image_src} alt="" />
              </div>
            </div>


          <div className={textClass}>
              <div className="p-5">
                <h2 className="display-4">{title}</h2>
                <p>{children}</p>
              </div>
            </div>

          </div>
        </div>
      </section>
}