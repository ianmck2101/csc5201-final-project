import React, { useState, useEffect } from 'react';
import axios from 'axios';
import './App.css';

const App = () => {
  const [requests, setRequests] = useState([]);
  const [formData, setFormData] = useState({
    title: '',
    description: '',
    price: '',
    dueDate: '',
    category: 0,
    id: 0
  });

  // Fetch all requests on component mount
  useEffect(() => {
    fetchRequests();
  }, []);

  const fetchRequests = async () => {
    try {
      const response = await axios.get('http://localhost:8080/Request');
      setRequests(response.data.requests);
    } catch (error) {
      console.error('Error fetching requests:', error);
    }
  };

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      const payload = {
        request: {
            ...formData,
            price: parseFloat(formData.price),
            dueDate: new Date(formData.dueDate).toISOString(),
        }
      };
      const response = await axios.post('http://localhost:8080/Request/New', payload);
      alert('Request submitted successfully!');
      setFormData({
        title: '',
        description: '',
        price: '',
        dueDate: '',
        category: 0,
        id: 0
      });
      fetchRequests(); // Refresh the list of requests
    } catch (error) {
      console.error('Error submitting request:', error);
      alert('Failed to submit request.');
    }
  };

  const mapCategoryIdToString = (id) => {
    switch(id) {
        case 0:
            return 'Dog Walking'
        case 1: 
            return 'House Sitting'
        default: 
            break;
    }
  }

  return (
    <div className="container">
      <h1>Service Request UI</h1>

      <form onSubmit={handleSubmit} className="request-form">
        <div>
          <label htmlFor="title">Title:</label>
          <input
            type="text"
            id="title"
            name="title"
            value={formData.title}
            onChange={handleChange}
            required
          />
        </div>

        <div>
          <label htmlFor="description">Description:</label>
          <textarea
            id="description"
            name="description"
            value={formData.description}
            onChange={handleChange}
            required
          ></textarea>
        </div>

        <div>
          <label htmlFor="price">Price:</label>
          <input
            type="number"
            id="price"
            name="price"
            value={formData.price}
            onChange={handleChange}
            required
            min="0"
            step="0.01"
          />
        </div>

        <div>
          <label htmlFor="dueDate">Due Date:</label>
          <input
            type="datetime-local"
            id="dueDate"
            name="dueDate"
            value={formData.dueDate}
            onChange={handleChange}
            required
          />
        </div>

        <div>
          <label htmlFor="category">Category:</label>
          <select
            id="category"
            name="category"
            value={formData.category}
            onChange={handleChange}
            required
          >
            <option value={0}>Dog Walking</option>
            <option value={1}>House Sitting</option>
          </select>
        </div>

        <button type="submit">Submit Request</button>
      </form>

      <h2>Existing Requests</h2>
      <div className="request-list">
        {requests.length > 0 ? (
          <ul>
            {requests.map((req) => (
              <li key={req.id}>
                <h3>{req.title}</h3>
                <p>{req.description}</p>
                <p>
                  <strong>Price:</strong> ${req.price}
                </p>
                <p>
                  <strong>Due Date:</strong> {new Date(req.dueDate).toLocaleString()}
                </p>
                <p>
                  <strong>Category:</strong> {mapCategoryIdToString(req.category)}
                </p>
              </li>
            ))}
          </ul>
        ) : (
          <p>No requests found.</p>
        )}
      </div>
    </div>
  );
};

export default App;
