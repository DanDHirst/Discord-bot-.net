import React, { useState, useEffect } from 'react';
import apiService from '../services/apiService';

const Dashboard = () => {
  const [stats, setStats] = useState({
    activeTimers: 0,
    completedTimers: 0,
    blockedUsers: 0,
    totalTimers: 0
  });
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchStats();
  }, []);

  const fetchStats = async () => {
    try {
      setLoading(true);
      
      // Fetch blocked users count
      const blockedUsersData = await apiService.getBlockedUsers();
      const blockedUsersCount = blockedUsersData.blockedUsers.length;

      // Fetch timer statistics (this would need an endpoint)
      // For now, we'll use placeholder data
      const timerStats = {
        activeTimers: 5,
        completedTimers: 23,
        totalTimers: 28
      };

      setStats({
        ...timerStats,
        blockedUsers: blockedUsersCount
      });
    } catch (error) {
      console.error('Error fetching stats:', error);
      // Keep placeholder data on error
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return <div className="loading">Loading dashboard...</div>;
  }

  return (
    <div className="dashboard">
      <h1>Dashboard</h1>
      <div className="stats-grid">
        <div className="stat-card">
          <h3>Active Timers</h3>
          <div className="stat-number">{stats.activeTimers}</div>
        </div>
        <div className="stat-card">
          <h3>Completed Timers</h3>
          <div className="stat-number">{stats.completedTimers}</div>
        </div>
        <div className="stat-card">
          <h3>Blocked Users</h3>
          <div className="stat-number">{stats.blockedUsers}</div>
        </div>
        <div className="stat-card">
          <h3>Total Timers</h3>
          <div className="stat-number">{stats.totalTimers}</div>
        </div>
      </div>
      
      <div className="dashboard-info">
        <div className="stat-card">
          <h3>Bot Status</h3>
          <p>The Discord bot is running and monitoring for expired timers every 30 seconds.</p>
          <p><strong>Features:</strong></p>
          <ul style={{ marginTop: '0.5rem', paddingLeft: '1.5rem' }}>
            <li>PING/PONG responses</li>
            <li>Timer commands (1-1440 minutes)</li>
            <li>Custom reminder messages</li>
            <li>User blocking system</li>
            <li>Automatic notifications</li>
          </ul>
        </div>
      </div>
    </div>
  );
};

export default Dashboard;
