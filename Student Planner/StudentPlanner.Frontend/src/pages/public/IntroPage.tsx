import { Outlet, useNavigate } from 'react-router-dom'
import Navbar from '../../components/layout/Navbar'
import IntroSection from '../../components/sections/IntroSection';
import AuthorsCard from '../../components/common/AuthorsCard';
import {images} from "../../assets/index";


const authors = [
  { name: "Przech Katarzyna", imageSrc: images.authors.kasia, imgPos:"img-kasia"},
  { name: "Rubayet Rafsan", imageSrc: images.authors.rafsan,  imgPos:"img-rafsan"},
  { name: "Shpylovyi Sviatoslav", imageSrc: images.authors.sviat, imgPos:"img-sviat"},
  { name: "Staszewska Barbara", imageSrc: images.authors.basia, imgPos:"img-basia"},
  { name: "Vu Van Quoc Hoang", imageSrc: images.authors.hoang, imgPos:"img-hoang"},
]

function IntroPage() {
  const navigate = useNavigate();

  return <>
    <Navbar>
            <div>
                <div>
                    <h1>Student Planner</h1>
                </div>
            </div>
            <div>
                <button onClick={()=>navigate("/register")}>Register</button>
                <button onClick={()=>navigate("/login")}>Login</button>
            </div>
    </Navbar>
    <header className="hero">
          <div className="hero-content">
              <h1>Student Planner</h1>
              <p>Plan your study and events in one place.</p>
              <a href="#learn-more">Learn More</a>
          </div>
    </header>

    <IntroSection left={true} title="Designed for Better Organization" image_src={images.hero.us} id="learn-more" className='img-us'>         
        This planner helps students structure their time effectively by combining task management, scheduling, and goal tracking into a single, intuitive system. It reduces cognitive load and supports consistent productivity habits.
    </IntroSection>

    <IntroSection left={false} title="Why Use This Planner?" image_src={images.sections.one} className='img-section1'>
        Using a structured planning system improves time management, reduces stress, and enhances academic performance. This tool is designed to support focus, consistency, and long-term succes
    </IntroSection>

    <IntroSection left={true} title="Start Planning Today" image_src={images.sections.two} className='img-section2'>
        Take control of your schedule and build better study habits with a planner designed to support your goals.
    </IntroSection>

    <div className="authors-section">
      <p className="authors-title">Our Team: </p>

      <div className='authors-container'>
        {authors.map(author => (
          <AuthorsCard key={author.name} imageSrc={author.imageSrc} fullName={author.name} imgPos={author.imgPos}/>
        ))}
      </div>
    </div>

    <Outlet/>
</>
  
}

export default IntroPage