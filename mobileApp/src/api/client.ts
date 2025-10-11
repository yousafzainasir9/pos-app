import axios from 'axios';
import AsyncStorage from '@react-native-async-storage/async-storage';

// ============================================
// IMPORTANT: UPDATE THIS TO MATCH YOUR BACKEND PORT!
// ============================================
// Backend is running on port 5021
const API_BASE_URL = 'http://10.0.2.2:5021/api';

console.log('🔧 API Base URL:', API_BASE_URL);

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  timeout: 30000, // Increased to 30 seconds
  headers: {
    'Content-Type': 'application/json',
    'Accept': 'application/json',
  },
});

// Request logging interceptor
apiClient.interceptors.request.use(
  async (config) => {
    console.log('🚀 API Request:', config.method?.toUpperCase(), config.url);
    console.log('📍 Full URL:', config.baseURL + config.url);
    console.log('📦 Data:', JSON.stringify(config.data));
    
    const token = await AsyncStorage.getItem('authToken');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
      console.log('🔐 Token attached');
    }
    return config;
  },
  (error) => {
    console.log('❌ Request Setup Error:', error);
    return Promise.reject(error);
  }
);

// Response logging interceptor
apiClient.interceptors.response.use(
  (response) => {
    console.log('✅ API Response:', response.status, response.config.url);
    console.log('📦 Response Data:', JSON.stringify(response.data));
    return response;
  },
  (error) => {
    console.log('❌ API Error:', error.message);
    console.log('❌ Error Config:', error.config?.url);
    
    if (error.response) {
      // Server responded with error
      console.log('📍 Status:', error.response.status);
      console.log('📍 Response:', JSON.stringify(error.response.data));
    } else if (error.request) {
      // Request made but no response
      console.log('📍 No response received from server');
      console.log('📍 Request was sent to:', error.config?.baseURL + error.config?.url);
      console.log('📍 This usually means:');
      console.log('   - Backend is not running (Check Visual Studio!)');
      console.log('   - Request timeout (increased to 30s)');
      console.log('   - Firewall blocking connection');
      console.log('📍 Request details:', error.request);
    } else {
      // Error in request setup
      console.log('📍 Error setting up request:', error.message);
    }
    
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
