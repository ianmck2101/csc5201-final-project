import React from 'react';
import ReactDOM from 'react-dom/client'; // This is the new import path
import App from './App';

const root = ReactDOM.createRoot(document.getElementById('root')); // Create a root for your app
root.render(
  <React.StrictMode>
    <App />
  </React.StrictMode>
);
