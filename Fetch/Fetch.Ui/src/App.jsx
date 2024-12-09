import React, { useState, useEffect } from 'react';
import axios from 'axios';
import Login from './Login';
import RequestorView from './RequestorView';
import ProviderView from './ProviderView';

const App = () => {
    const [userRole, setUserRole] = useState(null);
    const [token, setToken] = useState(localStorage.getItem('token') || null);

    const handleLogin = async (newToken) => {
        setToken(newToken);
        try {
            const response = await axios.get('http://localhost:8080/verify', {
                headers: { Authorization: `Bearer ${newToken}` }
            });
            setUserRole(response.data.role);
        } catch (err) {
            console.error('Error verifying token', err);
            setToken(null);
        }
    };

    useEffect(() => {
        if(!!token) {
            handleLogin(token);
        }
    }, []);

    if (!token) {
        return <Login onLogin={handleLogin} />;
    }

    if (userRole == 'Requestor') {
        return <RequestorView />;
    } else if (userRole == 'Provider') {
        return <ProviderView />;
    }

    return <div>Loading...</div>;
};

export default App;
