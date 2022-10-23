import React from 'react';
import { Route, Routes } from 'react-router-dom';
import LoginPage from './pages/Login/Login';
import RegisterPage from './pages/Register/Register';
import NotFoundPage from './pages/NotFound/NotFound';

const App = () => {
  return (
    <div className={'container'}>
      <Routes>
        <Route path='/authorize' element={<LoginPage />} />
        <Route path='/authorize/register' element={<RegisterPage />} />
        <Route path='*' element={<NotFoundPage />} />
      </Routes>
    </div>
  );
};

export default App;
