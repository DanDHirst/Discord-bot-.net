import React, { useState, useEffect } from 'react';
import axios from 'axios';

const BlockedUsers = () => {
  const [blockedUsers, setBlockedUsers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [newUser, setNewUser] = useState({
    userId: '',
    username: '',
    reason: ''
  });
  const [adding, setAdding] = useState(false);

  useEffect(() => {
    fetchBlockedUsers();
  }, []);

  const fetchBlockedUsers = async () => {
    try {
      setLoading(true);
      const response = await axios.get('/api/blockedusers');
      setBlockedUsers(response.data.blockedUsers);
    } catch (error) {
      console.error('Error fetching blocked users:', error);
      alert('Failed to fetch blocked users');
    } finally {
      setLoading(false);
    }
  };

  const handleAddUser = async (e) => {
    e.preventDefault();
    
    if (!newUser.userId || !newUser.username) {
      alert('Please fill in both User ID and Username');
      return;
    }

    try {
      setAdding(true);
      const response = await axios.post('/api/blockedusers', {
        userId: newUser.userId,
        username: newUser.username,
        reason: newUser.reason || null,
        blockedBy: 'admin' // In a real app, this would be the current user
      });

      // Add the new user to the list
      setBlockedUsers([response.data, ...blockedUsers]);
      
      // Clear the form
      setNewUser({
        userId: '',
        username: '',
        reason: ''
      });

      alert('User blocked successfully!');
    } catch (error) {
      console.error('Error blocking user:', error);
      if (error.response?.status === 409) {
        alert('User is already blocked');
      } else {
        alert('Failed to block user');
      }
    } finally {
      setAdding(false);
    }
  };

  const handleUnblockUser = async (userId, username) => {
    if (!window.confirm(`Are you sure you want to unblock ${username}?`)) {
      return;
    }

    try {
      const userToUnblock = blockedUsers.find(user => user.userId === userId);
      await axios.delete(`/api/blockedusers/${userToUnblock.id}`);
      
      // Remove the user from the list
      setBlockedUsers(blockedUsers.filter(user => user.userId !== userId));
      alert('User unblocked successfully!');
    } catch (error) {
      console.error('Error unblocking user:', error);
      alert('Failed to unblock user');
    }
  };

  const handleInputChange = (e) => {
    setNewUser({
      ...newUser,
      [e.target.name]: e.target.value
    });
  };

  const formatDate = (dateString) => {
    return new Date(dateString).toLocaleString();
  };

  if (loading) {
    return <div className="loading">Loading blocked users...</div>;
  }

  return (
    <div className="blocked-users">
      <h1>Blocked Users</h1>
      
      <div className="add-user-form">
        <h3>Add User to Block List</h3>
        <form onSubmit={handleAddUser}>
          <div className="form-row">
            <div className="form-group">
              <label htmlFor="userId">User ID</label>
              <input
                type="text"
                id="userId"
                name="userId"
                value={newUser.userId}
                onChange={handleInputChange}
                placeholder="e.g., 141942384211656705"
                required
              />
            </div>
            <div className="form-group">
              <label htmlFor="username">Username</label>
              <input
                type="text"
                id="username"
                name="username"
                value={newUser.username}
                onChange={handleInputChange}
                placeholder="e.g., user#1234"
                required
              />
            </div>
            <div className="form-group">
              <label htmlFor="reason">Reason (Optional)</label>
              <input
                type="text"
                id="reason"
                name="reason"
                value={newUser.reason}
                onChange={handleInputChange}
                placeholder="Reason for blocking..."
              />
            </div>
            <button type="submit" className="add-btn" disabled={adding}>
              {adding ? 'Adding...' : 'Block User'}
            </button>
          </div>
        </form>
      </div>

      <div className="users-table">
        {blockedUsers.length === 0 ? (
          <div className="empty-state">
            <h3>No Blocked Users</h3>
            <p>Users added to the block list will appear here.</p>
          </div>
        ) : (
          <table>
            <thead>
              <tr>
                <th>Username</th>
                <th>User ID</th>
                <th>Blocked Date</th>
                <th>Reason</th>
                <th>Blocked By</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {blockedUsers.map((user) => (
                <tr key={user.id}>
                  <td>{user.username}</td>
                  <td>
                    <code>{user.userId}</code>
                  </td>
                  <td>{formatDate(user.blockedAt)}</td>
                  <td>{user.reason || '-'}</td>
                  <td>{user.blockedBy}</td>
                  <td>
                    <button
                      className="unblock-btn"
                      onClick={() => handleUnblockUser(user.userId, user.username)}
                    >
                      Unblock
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
};

export default BlockedUsers;
