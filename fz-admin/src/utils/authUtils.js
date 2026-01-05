export const isAuthenticated = () => {
    const token = localStorage.getItem("token")
    return !!token
}

/**
 * Lấy thông tin user từ localStorage
 */
export const getUser = () => {
    const userStr = localStorage.getItem("user")
    if (!userStr) return null
    
    try {
        return JSON.parse(userStr)
    } catch (error) {
        console.error("Error parsing user data:", error)
        return null
    }
}

/**
 * Lấy token từ localStorage
 */
export const getToken = () => {
    return localStorage.getItem("token")
}

/**
 * Lấy refresh token từ localStorage
 */
export const getRefreshToken = () => {
    return localStorage.getItem("refreshToken")
}

/**
 * Lưu thông tin đăng nhập
 */
export const setAuthData = (userData) => {
    localStorage.setItem("token", userData.token)
    localStorage.setItem("refreshToken", userData.refreshToken)
    localStorage.setItem("sessionId", userData.sessionId)
    localStorage.setItem("deviceId", userData.deviceId)
    localStorage.setItem("user", JSON.stringify({
        userID: userData.userID,
        userName: userData.userName,
        email: userData.email,
        isEmailVerified: userData.isEmailVerified,
        tokenExpiration: userData.tokenExpiration,
        refreshTokenExpiration: userData.refreshTokenExpiration
    }))
}

/**
 * Xóa toàn bộ thông tin đăng nhập
 */
export const clearAuthData = () => {
    localStorage.removeItem("token")
    localStorage.removeItem("refreshToken")
    localStorage.removeItem("sessionId")
    localStorage.removeItem("deviceId")
    localStorage.removeItem("user")
}

/**
 * Kiểm tra xem token đã hết hạn chưa
 */
export const isTokenExpired = () => {
    const user = getUser()
    if (!user || !user.tokenExpiration) return true
    
    const expirationDate = new Date(user.tokenExpiration)
    const now = new Date()
    
    return expirationDate <= now
}

/**
 * Đăng xuất user
 */
export const logout = () => {
    clearAuthData()
    window.location.href = "/login"
}

/**
 * Lấy header authorization cho API calls
 */
export const getAuthHeader = () => {
    const token = getToken()
    return token ? { Authorization: `Bearer ${token}` } : {}
}