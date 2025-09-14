import React, { useState } from 'react';

const Login = ({ onLogin }) => {
  const [credentials, setCredentials] = useState({
    username: '',
    password: ''
  });
  const [error, setError] = useState('');

  const handleSubmit = (e) => {
    e.preventDefault();
    setError('');

    // Simple hardcoded authentication for demo
    // In a real app, this would call an API endpoint
    if (credentials.username === 'admin' && credentials.password === 'discord123') {
      onLogin(true);
    } else {
      setError('Invalid username or password');
    }
  };

  const handleChange = (e) => {
    setCredentials({
      ...credentials,
      [e.target.name]: e.target.value
    });
  };

  return (
    <div className="login-container">
      <form className="login-form" onSubmit={handleSubmit}>
        <h2>Discord Bot Admin</h2>
        <div className="form-group">
          <label htmlFor="username">Username</label>
          <input
            type="text"
            id="username"
            name="username"
            value={credentials.username}
            onChange={handleChange}
            required
          />
        </div>
        <div className="form-group">
          <label htmlFor="password">Password</label>
          <input
            type="password"
            id="password"
            name="password"
            value={credentials.password}
            onChange={handleChange}
            required
          />
        </div>
        <button type="submit" className="login-btn">
          Login
        </button>
        {error && <div className="error-message">{error}</div>}
        <div style={{ marginTop: '1rem', textAlign: 'center', fontSize: '0.9rem', color: '#72767d' }}>
          Demo credentials: admin / discord123
        </div>
      </form>
    </div>
  );
};

export default Login;
