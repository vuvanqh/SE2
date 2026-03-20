import { Link } from 'react-router-dom'

function IntroPage() {
  return (
    <main className="intro-page">
      <section className="intro-card">
        <h1>Student Planner</h1>
        <p>Plan your study and events in one place.</p>

        <div className="intro-actions">
          <Link to="/login" className="action-link action-link-login">
            Go to Login
          </Link>
          <Link to="/register" className="action-link action-link-register">
            Go to Register
          </Link>
        </div>
      </section>
    </main>
  )
}

export default IntroPage