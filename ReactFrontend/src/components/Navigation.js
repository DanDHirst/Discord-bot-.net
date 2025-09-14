import React from 'react';
import { Link, useLocation } from 'react-router-dom';

const Navigation = ({ onLogout }) => {
  const location = useLocation();

  return (
    <nav className="navigation">
      <div className="nav-content">
        <div className="nav-brand">
          🤖 Discord Bot Admin
        </div>
        <ul className="nav-links">
          <li>
            <Link 
              to="/dashboard" 
              className={location.pathname === '/dashboard' ? 'active' : ''}
            >
              Dashboard
            </Link>
          </li>
          <li>
            <Link 
              to="/blocked-users" 
              className={location.pathname === '/blocked-users' ? 'active' : ''}
            >
              Blocked Users
            </Link>
          </li>
        </ul>
        <button className="logout-btn" onClick={onLogout}>
          Logout
        </button>
      </div>
    </nav>
  );
};

export default Navigation;
