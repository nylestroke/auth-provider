﻿import axios from 'axios';

const instance = axios.create({
    baseURL: process.env.API_URL,
});

// instance.interceptors.request.use(config => {
//     config.headers.Authorization =
//         'Bearer ' + localStorage.getItem('accessToken');
//
//     return config;
// });

export default instance;