import axios from "axios"
import { getToken, getRefreshToken, clearAuthData, setAuthData } from "../utils/authUtils"

// Tạo axios instance
const api = axios.create({
  baseURL: process.env.REACT_APP_API_URL || "http://localhost:8080/api",
  headers: {
    "Content-Type": "application/json",
  },
})

// Request interceptor - Thêm token vào mỗi request
api.interceptors.request.use(
  (config) => {
    const token = getToken()
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

// Response interceptor - Xử lý token hết hạn
api.interceptors.response.use(
  (response) => {
    return response
  },
  async (error) => {
    const originalRequest = error.config

    // Nếu lỗi 401 và chưa retry
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true

      try {
        const refreshToken = getRefreshToken()
        
        if (!refreshToken) {
          // Không có refresh token, logout
          clearAuthData()
          window.location.href = "/login"
          return Promise.reject(error)
        }

        // Gọi API refresh token
        const response = await axios.post(
          `${process.env.REACT_APP_API_URL || "http://localhost:8080/api"}/auth/refresh`,
          {
            refreshToken: refreshToken,
          }
        )

        if (response.data.errorCode === 200) {
          const newUserData = response.data.data
          
          // Lưu token mới
          setAuthData(newUserData)
          
          // Retry request ban đầu với token mới
          originalRequest.headers.Authorization = `Bearer ${newUserData.token}`
          return api(originalRequest)
        } else {
          // Refresh token thất bại, logout
          clearAuthData()
          window.location.href = "/login"
          return Promise.reject(error)
        }
      } catch (refreshError) {
        // Lỗi khi refresh token, logout
        clearAuthData()
        window.location.href = "/login"
        return Promise.reject(refreshError)
      }
    }

    // Các lỗi khác
    return Promise.reject(error)
  }
)

export default api