import React, { useState } from 'react'
import { Link } from 'react-router-dom'

function LoginPage() {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [emailError, setEmailError] = useState('')
  const [passwordError, setPasswordError] = useState('')

  function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault()

    let isValid = true

    if (email.trim() === '') {
      setEmailError('Email is required.')
      isValid = false
    } else {
      setEmailError('')
    }

    if (password.trim() === '') {
      setPasswordError('Password is required.')
      isValid = false
    } else {
      setPasswordError('')
    }

    if (isValid) {
      alert('Login form is valid (UI only for now).')
    }
  }

  return (
    <main className="login-page">
      <section className="login-card">
        <h1>Login</h1>
        <p>Welcome back to Student Planner</p>

        <form onSubmit={handleSubmit} noValidate>
          <label htmlFor="email">Email</label>
          <input
            id="email"
            type="email"
            placeholder="Enter your email"
            value={email}
            onChange={(event) => setEmail(event.target.value)}
          />
          {emailError && <small className="error-text">{emailError}</small>}

          <label htmlFor="password">Password</label>
          <input
            id="password"
            type="password"
            placeholder="Enter your password"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
          />
          {passwordError && <small className="error-text">{passwordError}</small>}

          <button type="submit">Login</button>
        </form>

        <div className="auth-link-row">
          <Link to="/register" className="text-link">
            Don’t have an account? Register
          </Link>
        </div>

        <div className="auth-link-row">
          <Link to="/" className="text-link">
            Back to Intro
          </Link>
        </div>
      </section>
    </main>
  )
}

export default LoginPage