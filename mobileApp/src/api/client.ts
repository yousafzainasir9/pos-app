import axios from 'axios';
import AsyncStorage from '@react-native-async-storage/async-storage';

// Backend API Configuration
// Your backend runs on HTTPS: https://localhost:7021 and HTTP: http://localhost:5021
// For Android Emulator: http://10.0.2.2:5021/api (when running with 'https' profile)
// For Android Emulator: http://10.0.2.2:5000/api (when running with 'http' profile)

// Change this based on which profile your backend is using
const API_BASE_URL = 'http://10.0.2.2:5021/api';  // For 'https' profile
// const API_BASE_URL = 'http://10.0.2.2:5000/api';  // For 'http' profile

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor to add auth token
apiClient.interceptors.request.use(
  async (config) => {
    const token = await AsyncStorage.getItem('authToken');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response interceptor for error handling
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // Handle unauthorized - clear token
      AsyncStorage.removeItem('authToken');
      AsyncStorage.removeItem('refreshToken');
      AsyncStorage.removeItem('user');
    }
    return Promise.reject(error);
  }
);

export default apiClient;
