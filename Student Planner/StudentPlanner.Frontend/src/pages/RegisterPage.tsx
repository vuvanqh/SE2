import React, { useState } from 'react'
import { Link } from 'react-router-dom'



function RegisterPage() {
  const [firstName, setFirstName] = useState('')
  const [lastName, setLastName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')

  const [firstNameError, setFirstNameError] = useState('')
  const [lastNameError, setLastNameError] = useState('')
  const [emailError, setEmailError] = useState('')
  const [passwordError, setPasswordError] = useState('')
  const [confirmPasswordError, setConfirmPasswordError] = useState('')

  function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault()

    let isValid = true

    if (firstName.trim() === '') {
      setFirstNameError('First name is required.')
      isValid = false
    } else {
      setFirstNameError('')
    }

    if (lastName.trim() === '') {
      setLastNameError('Last name is required.')
      isValid = false
    } else {
      setLastNameError('')
    }

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

    if (confirmPassword.trim() === '') {
      setConfirmPasswordError('Confirm password is required.')
      isValid = false
    } else if (password !== confirmPassword) {
      setConfirmPasswordError('Passwords do not match.')
      isValid = false
    } else {
      setConfirmPasswordError('')
    }

    if (isValid) {
      alert('Register form is valid (UI only for now).')
    }
  }

  return (
    <main className="register-page">
      <section className="register-card">
        <h1>Create Account</h1>
        <p>Join Student Planner today</p>

        <form onSubmit={handleSubmit} noValidate>
          <label htmlFor="firstName">First Name</label>
          <input
            id="firstName"
            type="text"
            placeholder="Enter your first name"
            value={firstName}
            onChange={(event) => setFirstName(event.target.value)}
          />
          {firstNameError && <small className="error-text">{firstNameError}</small>}

          <label htmlFor="lastName">Last Name</label>
          <input
            id="lastName"
            type="text"
            placeholder="Enter your last name"
            value={lastName}
            onChange={(event) => setLastName(event.target.value)}
          />
          {lastNameError && <small className="error-text">{lastNameError}</small>}

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

          <label htmlFor="confirmPassword">Confirm Password</label>
          <input
            id="confirmPassword"
            type="password"
            placeholder="Confirm your password"
            value={confirmPassword}
            onChange={(event) => setConfirmPassword(event.target.value)}
          />
          {confirmPasswordError && <small className="error-text">{confirmPasswordError}</small>}

          <button type="submit">Create Account</button>
        </form>

        <div className="auth-link-row">
          <Link to="/login" className="text-link">
            Already have an account? Login
          </Link>
        </div>
      </section>
    </main>
  )
}

export default RegisterPage