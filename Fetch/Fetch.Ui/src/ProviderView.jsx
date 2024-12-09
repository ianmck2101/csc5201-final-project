import React, { useState, useEffect } from 'react';
import axios from 'axios';
import './ProviderView.css';

const ProviderView = () => {
  const [requests, setRequests] = useState([]);
  const [error, setError] = useState(null);
  const [token] = useState(localStorage.getItem('token') || null);

  // Fetch all requests associated with the provider
  useEffect(() => {
    fetchProviderRequests();
  }, []);

  const fetchProviderRequests = async () => {
    try {
      const response = await axios.get('http://localhost:8080/Request/providerView', {
        headers: { Authorization: `Bearer ${token}` },
      });
      setRequests(response.data);
    } catch (err) {
      console.error('Error fetching requests:', err);
      setError('Error fetching requests');
    }
  };

  // Handle accepting a request
  const handleAcceptRequest = async (requestId, providerId) => {
    try {
      await axios.post(
        `http://localhost:8080/Request/${requestId}/commands/accept/${providerId}`,
        {},
        {
          headers: { Authorization: `Bearer ${token}` },
        }
      );
      alert('Request accepted!');
      fetchProviderRequests(); // Refresh the list of requests
    } catch (err) {
      console.error('Error accepting request:', err);
      alert('Failed to accept request');
    }
  };

  const mapStatusIdToString = (statusId) => {
    switch(statusId) {
        case 0: 
            return "Open"
        case 1: 
            return "Closed"
        case 2: 
            return "Accepted"
        default: 
            return "Unknown"
    }
  }

  return (
    <div className="provider-container">
      <h1>Provider Dashboard</h1>
      {error && <p className="error-message">{error}</p>}

      <h2>Associated Requests</h2>
      <div className="request-list">
        {requests.length > 0 ? (
          <ul>
            {requests.map((request) => (
              <li key={request.id}>
                <h3>{request.title}</h3>
                <p>{request.description}</p>
                <p>
                  <strong>Status:</strong> {mapStatusIdToString(request.status)}
                </p>
                {request.status !== 2 && (
                  <button onClick={() => handleAcceptRequest(request.id, request.providerId)}>
                    Accept Request
                  </button>
                )}
                {request.accepted && <p>Request Accepted</p>}
              </li>
            ))}
          </ul>
        ) : (
          <p>No requests available.</p>
        )}
      </div>
    </div>
  );
};

export default ProviderView;
