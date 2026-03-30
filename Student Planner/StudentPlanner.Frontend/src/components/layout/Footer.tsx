

export default function Footer(){
    return <>
        <footer className="footer">
            <div className="footer-container">
                <div className="footer-left">
                <h3>Student Planner</h3>
                <p>Organize your studies, track your goals, and stay consistent.</p>
                </div>

                <div className="footer-right">
                <a href="#">About</a>
                <a href="#">Features</a>
                <a href="#">Contact</a>
                </div>
            </div>

            <div className="footer-bottom">
                © {new Date().getFullYear()} Student Planner. All rights reserved.
            </div>
        </footer>
    </>
}