# ?? FRONTEND INTEGRATION GUIDE - Broadcast Notification

## ?? **OVERVIEW**

Panduan implementasi fitur broadcast notification di frontend (React/Next.js/Vue).

---

## ?? **API INTEGRATION**

### **1. API Service Setup**

```javascript
// services/notificationService.js
import axios from 'axios';

const API_BASE_URL = 'https://localhost:7195/api';

export const notificationService = {
  // Broadcast notification
  async broadcast(data, token) {
    const response = await axios.post(
      `${API_BASE_URL}/notifications/broadcast`,
      data,
      {
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        }
      }
    );
    return response.data;
  },

  // Get user notifications
  async getUserNotifications(token, page = 1, limit = 10, isRead = null) {
    const params = new URLSearchParams({ page, limit });
    if (isRead !== null) params.append('is_read', isRead);
    
    const response = await axios.get(
      `${API_BASE_URL}/notifications?${params}`,
      {
        headers: { 'Authorization': `Bearer ${token}` }
      }
    );
    return response.data;
  },

  // Get unread count
  async getUnreadCount(token) {
    const response = await axios.get(
      `${API_BASE_URL}/notifications/unread-count`,
      {
        headers: { 'Authorization': `Bearer ${token}` }
      }
    );
    return response.data;
  },

  // Mark as read
  async markAsRead(notificationId, token) {
    const response = await axios.put(
      `${API_BASE_URL}/notifications/${notificationId}/read`,
      {},
      {
        headers: { 'Authorization': `Bearer ${token}` }
      }
    );
    return response.data;
  },

  // Delete notification
  async deleteNotification(notificationId, token) {
    const response = await axios.delete(
      `${API_BASE_URL}/notifications/${notificationId}`,
      {
        headers: { 'Authorization': `Bearer ${token}` }
      }
    );
    return response.data;
  }
};
```

---

## ?? **UI COMPONENTS**

### **2. Broadcast Form Component (React)**

```jsx
// components/BroadcastNotificationForm.jsx
import React, { useState } from 'react';
import { notificationService } from '../services/notificationService';

const BroadcastNotificationForm = ({ token, onSuccess }) => {
  const [formData, setFormData] = useState({
    title: '',
    message: '',
    type: 'info',
    priority: 'medium',
    linkUrl: '',
    targetRole: 'all'
  });
  
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError(null);

    try {
      const result = await notificationService.broadcast(formData, token);
      
      if (result.success) {
        alert(`? Notifikasi berhasil dikirim ke ${result.data.notificationsSent} pengguna`);
        
        // Reset form
        setFormData({
          title: '',
          message: '',
          type: 'info',
          priority: 'medium',
          linkUrl: '',
          targetRole: 'all'
        });
        
        if (onSuccess) onSuccess(result);
      }
    } catch (err) {
      setError(err.response?.data?.message || 'Gagal mengirim notifikasi');
    } finally {
      setLoading(false);
    }
  };

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  return (
    <div className="broadcast-notification-form">
      <h2>?? Broadcast Notifikasi</h2>
      
      {error && (
        <div className="alert alert-danger">
          {error}
        </div>
      )}

      <form onSubmit={handleSubmit}>
        {/* Title */}
        <div className="form-group">
          <label>Judul *</label>
          <input
            type="text"
            name="title"
            value={formData.title}
            onChange={handleChange}
            placeholder="Contoh: Pengumuman Penting"
            maxLength={200}
            required
            className="form-control"
          />
          <small className="text-muted">Maksimal 200 karakter</small>
        </div>

        {/* Message */}
        <div className="form-group">
          <label>Pesan *</label>
          <textarea
            name="message"
            value={formData.message}
            onChange={handleChange}
            placeholder="Tulis pesan notifikasi di sini..."
            maxLength={1000}
            rows={4}
            required
            className="form-control"
          />
          <small className="text-muted">Maksimal 1000 karakter</small>
        </div>

        {/* Type */}
        <div className="form-group">
          <label>Tipe *</label>
          <select
            name="type"
            value={formData.type}
            onChange={handleChange}
            required
            className="form-control"
          >
            <option value="info">?? Info</option>
            <option value="warning">?? Warning</option>
            <option value="error">? Error</option>
            <option value="success">? Success</option>
            <option value="reminder">?? Reminder</option>
            <option value="system">?? System</option>
            <option value="message">?? Message</option>
          </select>
        </div>

        {/* Priority */}
        <div className="form-group">
          <label>Prioritas *</label>
          <select
            name="priority"
            value={formData.priority}
            onChange={handleChange}
            required
            className="form-control"
          >
            <option value="low">?? Low</option>
            <option value="medium">?? Medium</option>
            <option value="high">?? High</option>
            <option value="urgent">?? Urgent</option>
          </select>
        </div>

        {/* Link URL (Optional) */}
        <div className="form-group">
          <label>Link URL (Opsional)</label>
          <input
            type="text"
            name="linkUrl"
            value={formData.linkUrl}
            onChange={handleChange}
            placeholder="/laporan atau https://example.com"
            className="form-control"
          />
          <small className="text-muted">Link ke halaman detail (opsional)</small>
        </div>

        {/* Target Role */}
        <div className="form-group">
          <label>Target Pengguna *</label>
          <select
            name="targetRole"
            value={formData.targetRole}
            onChange={handleChange}
            required
            className="form-control"
          >
            <option value="all">?? Semua Pengguna</option>
            <option value="Petugas">????? Petugas</option>
            <option value="Operator">????? Operator</option>
            <option value="Pemilik">?? Pemilik</option>
          </select>
        </div>

        {/* Submit Button */}
        <button
          type="submit"
          disabled={loading}
          className="btn btn-primary btn-block"
        >
          {loading ? '? Mengirim...' : '?? Kirim Broadcast'}
        </button>
      </form>
    </div>
  );
};

export default BroadcastNotificationForm;
```

---

### **3. Notification List Component**

```jsx
// components/NotificationList.jsx
import React, { useState, useEffect } from 'react';
import { notificationService } from '../services/notificationService';

const NotificationList = ({ token }) => {
  const [notifications, setNotifications] = useState([]);
  const [loading, setLoading] = useState(true);
  const [unreadCount, setUnreadCount] = useState(0);

  useEffect(() => {
    fetchNotifications();
    fetchUnreadCount();
  }, []);

  const fetchNotifications = async () => {
    try {
      const result = await notificationService.getUserNotifications(token);
      if (result.success) {
        setNotifications(result.data);
      }
    } catch (error) {
      console.error('Error fetching notifications:', error);
    } finally {
      setLoading(false);
    }
  };

  const fetchUnreadCount = async () => {
    try {
      const result = await notificationService.getUnreadCount(token);
      if (result.success) {
        setUnreadCount(result.data.count);
      }
    } catch (error) {
      console.error('Error fetching unread count:', error);
    }
  };

  const handleMarkAsRead = async (notificationId) => {
    try {
      await notificationService.markAsRead(notificationId, token);
      
      // Update local state
      setNotifications(prev => 
        prev.map(notif => 
          notif.id === notificationId 
            ? { ...notif, isRead: true, readAt: new Date() }
            : notif
        )
      );
      
      setUnreadCount(prev => Math.max(0, prev - 1));
    } catch (error) {
      console.error('Error marking as read:', error);
    }
  };

  const handleDelete = async (notificationId) => {
    if (!confirm('Hapus notifikasi ini?')) return;

    try {
      await notificationService.deleteNotification(notificationId, token);
      
      // Remove from local state
      setNotifications(prev => prev.filter(n => n.id !== notificationId));
      
      // Update unread count if deleted notification was unread
      const deletedNotif = notifications.find(n => n.id === notificationId);
      if (deletedNotif && !deletedNotif.isRead) {
        setUnreadCount(prev => Math.max(0, prev - 1));
      }
    } catch (error) {
      console.error('Error deleting notification:', error);
    }
  };

  const getNotificationIcon = (type) => {
    const icons = {
      info: '??',
      warning: '??',
      error: '?',
      success: '?',
      reminder: '??',
      system: '??',
      message: '??'
    };
    return icons[type] || '??';
  };

  const getPriorityBadge = (priority) => {
    const badges = {
      low: { text: 'Low', color: 'badge-secondary' },
      medium: { text: 'Medium', color: 'badge-primary' },
      high: { text: 'High', color: 'badge-warning' },
      urgent: { text: 'Urgent', color: 'badge-danger' }
    };
    return badges[priority] || badges.medium;
  };

  if (loading) {
    return <div>? Loading notifications...</div>;
  }

  return (
    <div className="notification-list">
      <div className="notification-header">
        <h3>?? Notifikasi</h3>
        {unreadCount > 0 && (
          <span className="badge badge-danger">{unreadCount} Belum Dibaca</span>
        )}
      </div>

      {notifications.length === 0 ? (
        <div className="text-center text-muted py-4">
          Tidak ada notifikasi
        </div>
      ) : (
        <div className="notification-items">
          {notifications.map(notif => (
            <div
              key={notif.id}
              className={`notification-item ${!notif.isRead ? 'unread' : ''}`}
            >
              <div className="notification-content">
                <div className="notification-header">
                  <span className="notification-icon">
                    {getNotificationIcon(notif.type)}
                  </span>
                  <strong className="notification-title">{notif.title}</strong>
                  <span className={`badge ${getPriorityBadge(notif.priority).color}`}>
                    {getPriorityBadge(notif.priority).text}
                  </span>
                </div>
                
                <p className="notification-message">{notif.message}</p>
                
                {notif.linkUrl && (
                  <a href={notif.linkUrl} className="notification-link">
                    View Details ?
                  </a>
                )}
                
                <div className="notification-footer">
                  <small className="text-muted">
                    {new Date(notif.createdAt).toLocaleString('id-ID')}
                  </small>
                  
                  <div className="notification-actions">
                    {!notif.isRead && (
                      <button
                        onClick={() => handleMarkAsRead(notif.id)}
                        className="btn btn-sm btn-outline-primary"
                      >
                        ? Tandai Dibaca
                      </button>
                    )}
                    
                    <button
                      onClick={() => handleDelete(notif.id)}
                      className="btn btn-sm btn-outline-danger"
                    >
                      ??? Hapus
                    </button>
                  </div>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default NotificationList;
```

---

### **4. Notification Badge (Navbar)**

```jsx
// components/NotificationBadge.jsx
import React, { useState, useEffect } from 'react';
import { notificationService } from '../services/notificationService';

const NotificationBadge = ({ token, onClick }) => {
  const [unreadCount, setUnreadCount] = useState(0);

  useEffect(() => {
    fetchUnreadCount();
    
    // Poll every 30 seconds
    const interval = setInterval(fetchUnreadCount, 30000);
    
    return () => clearInterval(interval);
  }, []);

  const fetchUnreadCount = async () => {
    try {
      const result = await notificationService.getUnreadCount(token);
      if (result.success) {
        setUnreadCount(result.data.count);
      }
    } catch (error) {
      console.error('Error fetching unread count:', error);
    }
  };

  return (
    <button
      className="notification-badge-btn"
      onClick={onClick}
      title="Notifikasi"
    >
      ??
      {unreadCount > 0 && (
        <span className="notification-badge-count">
          {unreadCount > 99 ? '99+' : unreadCount}
        </span>
      )}
    </button>
  );
};

export default NotificationBadge;
```

---

## ?? **CSS STYLING**

```css
/* styles/notification.css */

.broadcast-notification-form {
  max-width: 600px;
  margin: 0 auto;
  padding: 20px;
  background: #fff;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
}

.broadcast-notification-form h2 {
  margin-bottom: 20px;
  color: #333;
}

.form-group {
  margin-bottom: 15px;
}

.form-group label {
  display: block;
  margin-bottom: 5px;
  font-weight: 500;
  color: #555;
}

.form-control {
  width: 100%;
  padding: 8px 12px;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 14px;
}

.form-control:focus {
  outline: none;
  border-color: #007bff;
  box-shadow: 0 0 0 3px rgba(0,123,255,0.1);
}

.btn-primary {
  background-color: #007bff;
  color: white;
  padding: 10px 20px;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 16px;
  width: 100%;
}

.btn-primary:hover {
  background-color: #0056b3;
}

.btn-primary:disabled {
  background-color: #6c757d;
  cursor: not-allowed;
}

/* Notification List */
.notification-list {
  max-width: 800px;
  margin: 0 auto;
  padding: 20px;
}

.notification-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.notification-item {
  background: #fff;
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  padding: 15px;
  margin-bottom: 10px;
  transition: box-shadow 0.2s;
}

.notification-item:hover {
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
}

.notification-item.unread {
  border-left: 4px solid #007bff;
  background: #f8f9ff;
}

.notification-content {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.notification-header {
  display: flex;
  align-items: center;
  gap: 10px;
}

.notification-icon {
  font-size: 20px;
}

.notification-title {
  flex: 1;
  font-size: 16px;
  color: #333;
}

.notification-message {
  color: #666;
  margin: 0;
}

.notification-link {
  color: #007bff;
  text-decoration: none;
  font-size: 14px;
}

.notification-link:hover {
  text-decoration: underline;
}

.notification-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-top: 10px;
  border-top: 1px solid #f0f0f0;
}

.notification-actions {
  display: flex;
  gap: 5px;
}

/* Badge Colors */
.badge {
  padding: 3px 8px;
  border-radius: 12px;
  font-size: 11px;
  font-weight: 600;
}

.badge-secondary { background: #6c757d; color: white; }
.badge-primary { background: #007bff; color: white; }
.badge-warning { background: #ffc107; color: #000; }
.badge-danger { background: #dc3545; color: white; }

/* Notification Badge (Navbar) */
.notification-badge-btn {
  position: relative;
  background: none;
  border: none;
  font-size: 24px;
  cursor: pointer;
  padding: 5px 10px;
}

.notification-badge-count {
  position: absolute;
  top: 0;
  right: 0;
  background: #dc3545;
  color: white;
  border-radius: 10px;
  padding: 2px 6px;
  font-size: 11px;
  font-weight: bold;
  min-width: 18px;
  text-align: center;
}
```

---

## ?? **REAL-TIME NOTIFICATIONS (Optional)**

### **Using WebSocket/SignalR**

```javascript
// services/notificationHub.js
import * as signalR from '@microsoft/signalr';

export class NotificationHub {
  constructor(token) {
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl('https://localhost:7195/notificationHub', {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .build();
  }

  async start() {
    try {
      await this.connection.start();
      console.log('? SignalR Connected');
    } catch (err) {
      console.error('? SignalR Connection Error:', err);
    }
  }

  onNotificationReceived(callback) {
    this.connection.on('ReceiveNotification', callback);
  }

  async stop() {
    await this.connection.stop();
  }
}

// Usage in React component
useEffect(() => {
  const hub = new NotificationHub(token);
  
  hub.start();
  
  hub.onNotificationReceived((notification) => {
    // Show toast notification
    toast.info(`?? ${notification.title}: ${notification.message}`);
    
    // Refresh notification list
    fetchNotifications();
    fetchUnreadCount();
  });
  
  return () => {
    hub.stop();
  };
}, [token]);
```

---

## ?? **MOBILE-FRIENDLY CONSIDERATIONS**

```jsx
// Responsive Broadcast Form
<div className="broadcast-notification-form mobile-friendly">
  {/* Use touch-friendly input sizes */}
  <input
    type="text"
    style={{ minHeight: '44px', fontSize: '16px' }}
  />
  
  {/* Stack form elements on mobile */}
  <style jsx>{`
    @media (max-width: 768px) {
      .broadcast-notification-form {
        padding: 15px;
      }
      
      .notification-item {
        padding: 10px;
      }
      
      .notification-actions {
        flex-direction: column;
        width: 100%;
      }
      
      .notification-actions button {
        width: 100%;
      }
    }
  `}</style>
</div>
```

---

## ? **IMPLEMENTATION CHECKLIST**

- [ ] Install axios: `npm install axios`
- [ ] Create `services/notificationService.js`
- [ ] Create `BroadcastNotificationForm` component
- [ ] Create `NotificationList` component
- [ ] Create `NotificationBadge` component
- [ ] Add CSS styling
- [ ] Test broadcast functionality
- [ ] Test notification display
- [ ] Test mark as read
- [ ] Test delete notification
- [ ] (Optional) Implement real-time updates with SignalR
- [ ] Test on mobile devices

---

## ?? **NEXT STEPS**

1. Integrate components into your admin dashboard
2. Add role-based rendering (show broadcast form only for Pemilik/Operator)
3. Implement toast notifications for better UX
4. Add pagination for notification list
5. Add filter options (by type, priority, read status)
6. Implement sound notifications for urgent alerts

---

**Created**: 2024-12-14  
**Framework**: React (adaptable to Vue/Angular)  
**Status**: ? Ready for Integration
