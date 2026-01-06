// services/api.js
import axios from "axios"

const isAdmin = () => {
    return localStorage.getItem("isAdmin") === "true"
}

const API_BASE_URL = "https://filmzone-api.koyeb.app"

const api = axios.create({
    baseURL: API_BASE_URL,
    headers: {
        "Content-Type": "application/json",
        credentials: "include",
    },
})

// --- 1. Request Interceptor: Gắn token vào mỗi request ---
api.interceptors.request.use(
    config => {
        const token = localStorage.getItem("token")
        if (token) {
            config.headers.Authorization = `Bearer ${token}`
        }
        return config
    },
    error => {
        return Promise.reject(error)
    }
)

// --- 2. Response Interceptor: Xử lý khi Token hết hạn (Lỗi 401) ---
api.interceptors.response.use(
    response => {
        return response
    },
    async error => {
        const originalRequest = error.config

        if (error.response) {
            console.log("Headers Axios nhận được:", error.response.headers)
        }

        if (error.response) {
            const headers = error.response.headers
            const isUnauthorized = error.response.status === 401
            const isTokenExpired = isUnauthorized && !originalRequest._retry

            if (isTokenExpired) {
                originalRequest._retry = true

                try {
                    console.log("Phát hiện lỗi 401, đang gọi Refresh Token...")
                    const refreshToken = localStorage.getItem("refreshToken")

                    if (!refreshToken) {
                        console.log("Không có refresh token, logout.")
                        handleLogout()
                        return Promise.reject(error)
                    }

                    const response = await axios.post(
                        `${API_BASE_URL}/login/auth/refresh`,
                        {
                            refreshToken: refreshToken,
                        }
                    )

                    if (response.data && response.data.errorCode === 200) {
                        const {
                            accessToken,
                            accessTokenExpiresAt,
                            refreshToken,
                            refreshTokenExpiresAt,
                        } = response.data.data

                        localStorage.setItem("token", accessToken)
                        localStorage.setItem(
                            "tokenExpiration",
                            accessTokenExpiresAt
                        )
                        if (refreshToken) {
                            localStorage.setItem("refreshToken", refreshToken)
                            localStorage.setItem(
                                "refreshTokenExpiration",
                                refreshTokenExpiresAt
                            )
                        }

                        api.defaults.headers.common[
                            "Authorization"
                        ] = `Bearer ${accessToken}`
                        originalRequest.headers[
                            "Authorization"
                        ] = `Bearer ${accessToken}`

                        console.log("Refresh thành công, gọi lại request cũ...")
                        return api(originalRequest)
                    }
                } catch (refreshError) {
                    console.error("Lỗi khi Refresh Token:", refreshError)
                    handleLogout()
                    return Promise.reject(refreshError)
                }
            }
        }

        return Promise.reject(error)
    }
)

// Hàm Logout chung
const handleLogout = () => {
    localStorage.clear()
    window.location.href = "/login"
}

// Account APIs
export const startMfaTotp = data => {
    const token = localStorage.getItem("token")
    return api.post("/account/mfa/totp/start", data, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const confirmMfaTotp = data => {
    const token = localStorage.getItem("token")
    return api.post("/account/mfa/totp/confirm", data, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const disableMfaTotp = data => {
    const token = localStorage.getItem("token")
    return api.post("/account/mfa/totp/disable", data, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const startPasswordChangeEmail = data => {
    const token = localStorage.getItem("token")
    return api.post("/account/password/change/email/start", data, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const verifyPasswordChangeEmail = data => {
    const token = localStorage.getItem("token")
    return api.post("/account/password/change/email/verify", data, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const verifyPasswordChangeMfa = data => {
    const token = localStorage.getItem("token")
    return api.post("/account/password/change/mfa/verify", data, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const commitPasswordChange = data => {
    const token = localStorage.getItem("token")
    return api.post("/account/password/change/commit", data, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const startPasswordForgotEmail = data => {
    const token = localStorage.getItem("token")
    return api.post("/account/password/forgot/email/start", data, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const verifyPasswordForgotEmail = data => {
    const token = localStorage.getItem("token")
    return api.post("/account/password/forgot/email/verify", data, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const verifyPasswordForgotMfa = data => {
    const token = localStorage.getItem("token")
    return api.post("/account/password/forgot/mfa/verify", data, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const commitPasswordForgot = data => {
    const token = localStorage.getItem("token")
    return api.post("/account/password/forgot/commit", data, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

// ArchiveUpload APIs
export const uploadArchiveFile = data => {
    const token = localStorage.getItem("token")
    return api.post("/api/upload/archive/file", data, {
        headers: {
            Authorization: `Bearer ${token}`,
            "Content-Type": "multipart/form-data",
        },
    })
}

export const uploadArchiveLink = data => {
    const token = localStorage.getItem("token")
    return api.post("/api/upload/archive/link", data, {
        headers: {
            Authorization: `Bearer ${token}`,
            "Content-Type": "application/json",
        },
    })
}

// Comment APIs
export const createComment = data => {
    const token = localStorage.getItem("token")
    return api.post("/api/Comment/CreateComment", data, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const updateComment = data => {
    const token = localStorage.getItem("token")
    return api.put("/api/Comment/UpdateComment", data, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const deleteComment = id => {
    const token = localStorage.getItem("token")
    return api.delete(`/api/Comment/DeleteComment/${id}`, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const getCommentById = id => {
    const token = localStorage.getItem("token")
    return api.get(`/api/Comment/GetCommentByID/${id}`, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const getCommentsByUserId = userID => {
    const token = localStorage.getItem("token")
    return api.get(
        `/api/Comment/GetCommentsByUserID/${userID}?userID=${userID}`,
        {
            headers: { Authorization: `Bearer ${token}` },
        }
    )
}

export const getCommentsByMovieId = movieID => {
    const token = localStorage.getItem("token")
    return api.get(`/api/Comment/GetCommentsByMovieID/${movieID}`, {
        params: { movieID },
        headers: { Authorization: `Bearer ${token}` },
    })
}

// Episode APIs
export const createEpisode = data => {
    const token = localStorage.getItem("token")
    return api.post("/api/Episode/CreateEpisode", data, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const updateEpisode = data => {
    const token = localStorage.getItem("token")
    return api.put("/api/Episode/UpdateEpisode", data, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const deleteEpisode = id => {
    const token = localStorage.getItem("token")
    return api.delete(`/api/Episode/DeleteEpisode/${id}`, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const getEpisodeById = id => {
    const token = localStorage.getItem("token")
    return api.get(`/api/Episode/GetEpisodeById/${id}`, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const getAllEpisodes = () => {
    const token = localStorage.getItem("token")
    return api.get("/api/Episode/GetAllEpisodes/getAll", {
        headers: { Authorization: `Bearer ${token}` },
    })
}

// EpisodeSource APIs
export const createEpisodeSource = data => {
    const token = localStorage.getItem("token")
    return api.post("/movie/EpisodeSource/CreateEpisodeSource", data, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const updateEpisodeSource = data => {
    const token = localStorage.getItem("token")
    return api.put("/movie/EpisodeSource/UpdateEpisodeSource", data, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const deleteEpisodeSource = id => {
    const token = localStorage.getItem("token")
    return api.delete(`/movie/EpisodeSource/DeleteEpisodeSource/${id}`, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const getEpisodeSourceById = id => {
    const token = localStorage.getItem("token")
    return api.get(`/movie/EpisodeSource/GetEpisodeSourceById/${id}`, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const getEpisodeSourcesByEpisodeId = episodeId => {
    const token = localStorage.getItem("token")
    return api.get(
        `/movie/EpisodeSource/GetEpisodeSourcesByEpisodeId/${episodeId}`,
        {
            headers: { Authorization: `Bearer ${token}` },
        }
    )
}

// EpisodeWatchProgress APIs
export const createEpisodeWatchProgress = data => {
    const token = localStorage.getItem("token")
    return api.post(
        "/api/EpisodeWatchProgress/CreateEpisodeWatchProgress",
        data,
        {
            headers: { Authorization: `Bearer ${token}` },
        }
    )
}

export const updateEpisodeWatchProgress = data => {
    const token = localStorage.getItem("token")
    return api.put(
        "/api/EpisodeWatchProgress/UpdateEpisodeWatchProgress",
        data,
        {
            headers: { Authorization: `Bearer ${token}` },
        }
    )
}

export const deleteEpisodeWatchProgress = id => {
    const token = localStorage.getItem("token")
    return api.delete(
        `/api/EpisodeWatchProgress/DeleteEpisodeWatchProgress/${id}`,
        {
            headers: { Authorization: `Bearer ${token}` },
        }
    )
}

export const getEpisodeWatchProgressById = id => {
    const token = localStorage.getItem("token")
    return api.get(
        `/api/EpisodeWatchProgress/GetEpisodeWatchProgressByID/${id}`,
        {
            headers: { Authorization: `Bearer ${token}` },
        }
    )
}

export const getEpisodeWatchProgressByUserId = userId => {
    const token = localStorage.getItem("token")
    return api.get(
        `/api/EpisodeWatchProgress/GetEpisodeWatchProgressByUserID/user/${userId}`,
        {
            headers: { Authorization: `Bearer ${token}` },
        }
    )
}

export const getEpisodeWatchProgressByEpisodeId = episodeId => {
    const token = localStorage.getItem("token")
    return api.get(
        `/api/EpisodeWatchProgress/GetEpisodeWatchProgressByEpisodeID/episode/${episodeId}`,
        {
            headers: { Authorization: `Bearer ${token}` },
        }
    )
}

// FZ.WebAPI Health
export const getHealthz = () => {
    const token = localStorage.getItem("token")
    return api.get("/healthz", {
        headers: { Authorization: `Bearer ${token}` },
    })
}

// ImageSource APIs
export const createImageSource = data => {
    const token = localStorage.getItem("token")
    return api.post("/movie/ImageSource/CreateImageSource", data, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const updateImageSource = data => {
    const token = localStorage.getItem("token")
    return api.put("/movie/ImageSource/UpdateImageSource", data, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const deleteImageSource = id => {
    const token = localStorage.getItem("token")
    return api.delete(`/movie/ImageSource/DeleteImageSource/${id}`, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const getImageSourcesByType = Type => {
    const token = localStorage.getItem("token")
    return api.get(`/movie/ImageSource/GetImageSourcesByType/${Type}`, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

// Login APIs
export const userLogin = data => {
    const token = localStorage.getItem("token")
    return api.post("/login/StaffLogin", data, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const loginMobile = data => {
    const token = localStorage.getItem("token")
    return api.post("/login/login/mobile", data, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const googleLogin = () => {
    const token = localStorage.getItem("token")
    return api.get("/login/google-login", {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const logout = data => {
    const token = localStorage.getItem("token")
    return api.post("/login/logout", data, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const logoutSession = sessionId => {
    const token = localStorage.getItem("token")
    return api.post(
        `/login/logout/session/${sessionId}`,
        {},
        {
            headers: { Authorization: `Bearer ${token}` },
        }
    )
}

export const logoutAll = data => {
    const token = localStorage.getItem("token")
    return api.post("/login/logout/all", data, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const googleCallback = () => {
    const token = localStorage.getItem("token")
    return api.get("/login/google/callback", {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const signinGoogle = () => {
    const token = localStorage.getItem("token")
    return api.get("/login/signin-google", {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const verifyMfa = data => {
    const token = localStorage.getItem("token")
    return api.post("/login/mfa/verify", data, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const refreshAuth = data => {
    const token = localStorage.getItem("token")
    return api.post("/login/auth/refresh", data, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

// Movie APIs
export const createMovie = data => {
    const token = localStorage.getItem("token")
    return api.post("/api/Movie/CreateMovie", data, {
        headers: {
            Authorization: `Bearer ${token}`,
            "Content-Type": "multipart/form-data",
        },
    })
}

export const updateMovie = data => {
    const token = localStorage.getItem("token")
    return api.put("/api/Movie/UpdateMovie", data, {
        headers: {
            Authorization: `Bearer ${token}`,
            "Content-Type": "multipart/form-data",
        },
    })
}

export const deleteMovie = id => {
    const token = localStorage.getItem("token")
    return api.delete(`/api/Movie/DeleteMovie/${id}`, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const getMovieById = id => {
    const token = localStorage.getItem("token")
    return api.get(`/api/Movie/GetMovieById/${id}`, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const getAllMovies = () => {
    const token = localStorage.getItem("token")
    return api.get("/api/Movie/GetAllMovies/gellAll", {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const getAllMoviesMainScreen = () => {
    const token = localStorage.getItem("token")
    return api.get("/api/Movie/GetAllMoviesMainScreen/mainScreen", {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const getAllMoviesNewReleaseMainScreen = () => {
    const token = localStorage.getItem("token")
    return api.get(
        "/api/Movie/GetAllMoviesNewReleaseMainScreen/newReleaseMainScreen",
        {
            headers: { Authorization: `Bearer ${token}` },
        }
    )
}

export const getWatchNowMovieById = id => {
    const token = localStorage.getItem("token")
    return api.get(`/api/Movie/GetWatchNowMovieByID/watchNow/${id}`, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

// MoviePerson APIs
export const addPersonToMovie = data => {
    const token = localStorage.getItem("token")
    return api.post("/movie/MoviePerson/AddPersonToMovie", data, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const removePersonFromMovie = id => {
    const token = localStorage.getItem("token")
    return api.delete(`/movie/MoviePerson/RemovePersonFromMovie/${id}`, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const getMoviesByPerson = personID => {
    const token = localStorage.getItem("token")
    return api.get(`/movie/MoviePerson/GetMoviesByPerson/${personID}`, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const getPersonsByMovie = movieID => {
    const token = localStorage.getItem("token")
    return api.get(`/movie/MoviePerson/GetPersonsByMovie/${movieID}`, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

// MovieSource APIs
export const createMovieSource = data => {
    const token = localStorage.getItem("token")
    return api.post("/movie/MovieSource/CreateMovieSource", data, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const updateMovieSource = data => {
    const token = localStorage.getItem("token")
    return api.put("/movie/MovieSource/UpdateMovieSource", data, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const deleteMovieSource = id => {
    const token = localStorage.getItem("token")
    return api.delete(`/movie/MovieSource/DeleteMovieSource/${id}`, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const getVipSourceByMovieId = movieId => {
    const token = localStorage.getItem("token")
    return api.get(`/api/movies/${movieId}/vip-source`, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const getMovieSourceById = id => {
    const token = localStorage.getItem("token")
    return api.get(`/movie/MovieSource/GetMovieSourceById/${id}`, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

// MovieTag APIs
export const addTagToMovie = data => {
    const token = localStorage.getItem("token")
    return api.post("/movie/MovieTag/AddTagToMovie", data, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const updateMovieTag = data => {
    const token = localStorage.getItem("token")
    return api.put("/movie/MovieTag/UpdateMovieTag", data, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const deleteMovieTag = id => {
    const token = localStorage.getItem("token")
    return api.delete(`/movie/MovieTag/DeleteMovieTag/${id}`, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const getMoviesByTag = tagID => {
    const token = localStorage.getItem("token")
    return api.get(`/movie/MovieTag/GetMoviesByTag/${tagID}`, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const getTagsByMovie = movieID => {
    const token = localStorage.getItem("token")
    return api.get(`/movie/MovieTag/GetTagsByMovie/${movieID}`, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const getMoviesByTagIds = data => {
    const token = localStorage.getItem("token")
    return api.get("/movie/MovieTag/GetMoviesByTagIDs/getMovieByTagID", {
        params: data,
        headers: { Authorization: `Bearer ${token}` },
    })
}

// Payment APIs
export const vnpayCheckout = data => {
    const token = localStorage.getItem("token")
    return api.post("/api/payment/vnpay/checkout", data, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const vnpayCallback = () => {
    const token = localStorage.getItem("token")
    return api.get("/api/payment/vnpay/callback", {
        headers: { Authorization: `Bearer ${token}` },
    })
}

// Person APIs
export const createPerson = formData => {
    const token = localStorage.getItem("token")
    return axios.post(`${API_BASE_URL}/movie/Person/CreatePerson`, formData, {
        headers: {
            Authorization: `Bearer ${token}`,
        },
    })
}

export const updatePerson = data => {
    const token = localStorage.getItem("token")
    return api.put("/movie/Person/UpdatePerson", data, {
        headers: {
            Authorization: `Bearer ${token}`,
            "Content-Type": "multipart/form-data",
        },
    })
}

export const deletePerson = id => {
    const token = localStorage.getItem("token")
    return api.delete(`/movie/Person/DeletePerson/${id}`, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const getPersonById = ID => {
    const token = localStorage.getItem("token")
    return api.get(`/movie/Person/GetPersonByID/${ID}`, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const getAllPersons = () => {
    const token = localStorage.getItem("token")
    return api.get("/movie/Person/GetAllPerson/getall", {
        headers: { Authorization: `Bearer ${token}` },
    })
}

// Plan APIs
export const createPlan = data => {
    const token = localStorage.getItem("token")
    return api.post("/api/plans/create", data, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const updatePlan = data => {
    const token = localStorage.getItem("token")
    return api.put("/api/plans/update", data, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const deletePlan = planID => {
    const token = localStorage.getItem("token")
    return api.delete(`/api/plans/delete/${planID}`, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const getPlanById = planID => {
    const token = localStorage.getItem("token")
    return api.get(`/api/plans/${planID}`, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const getAllPlans = () => {
    const token = localStorage.getItem("token")
    return api.get("/api/plans/all", {
        headers: { Authorization: `Bearer ${token}` },
    })
}

// Price APIs
export const createPrice = data => {
    const token = localStorage.getItem("token")
    return api.post("/api/price/Create", data, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const updatePrice = data => {
    const token = localStorage.getItem("token")
    return api.put("/api/price/Update", data, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const deletePrice = priceID => {
    const token = localStorage.getItem("token")
    return api.delete(`/api/price/Delete/${priceID}`, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const getPriceById = priceID => {
    const token = localStorage.getItem("token")
    return api.get(`/api/price/${priceID}`, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const getAllPrices = () => {
    const token = localStorage.getItem("token")
    return api.get("/api/price/all", {
        headers: { Authorization: `Bearer ${token}` },
    })
}

// Region APIs
export const createRegion = data => {
    const token = localStorage.getItem("token")
    return api.post("/movie/Region/CreateRegion", data, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const updateRegion = data => {
    const token = localStorage.getItem("token")
    return api.put("/movie/Region/UpdateRegion", data, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const deleteRegion = id => {
    const token = localStorage.getItem("token")
    return api.delete(`/movie/Region/DeleteRegion/${id}`, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const getRegionById = ID => {
    const token = localStorage.getItem("token")
    return api.get(`/movie/Region/GetRegionByID/${ID}`, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const getAllRegions = () => {
    const token = localStorage.getItem("token")
    return api.get("/movie/Region/GetAllRegions/getAll", {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const getMoviesByRegionId = regionID => {
    const token = localStorage.getItem("token")
    return api.get(
        `/movie/Region/GetMovieByRegionID/getMovieByRegionID/${regionID}`,
        {
            headers: { Authorization: `Bearer ${token}` },
        }
    )
}

export const getPersonsByRegionId = regionID => {
    const token = localStorage.getItem("token")
    return api.get(
        `/movie/Region/GetPersonByRegionID/getPersonByRegionID/${regionID}`,
        {
            headers: { Authorization: `Bearer ${token}` },
        }
    )
}

// Register APIs
export const register = data => {
    const token = localStorage.getItem("token")
    return api.post("/register", data, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const verifyRegisterEmail = data => {
    const token = localStorage.getItem("token")
    return api.post("/register/verifyRegisterEmail", data, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

// Role APIs
export const getAllRoles = () => {
    const token = localStorage.getItem("token")
    return api.get("/roles/getall", {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const getAllScopeUser = () => {
    const token = localStorage.getItem("token")
    return api.get("/roles/getallscope-user", {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const addRole = data => {
    const token = localStorage.getItem("token")
    const prefix = isAdmin() ? "/roles/admin" : "/roles"
    return api.post(`${prefix}/addRole`, data, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const updateRole = data => {
    const token = localStorage.getItem("token")
    const prefix = isAdmin() ? "/roles/admin" : "/roles"
    return api.put(`${prefix}/updateRole`, data, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const deleteRole = roleID => {
    const token = localStorage.getItem("token")
    const prefix = isAdmin() ? "/roles/admin" : "/roles"
    return api.delete(`${prefix}/deleteRole/${roleID}`, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const getRoleByUserId = userID => {
    const token = localStorage.getItem("token")
    return api.get(`/roles/getRoleByUserID/${userID}`, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

// SavedMovie APIs
export const createSavedMovie = data => {
    const token = localStorage.getItem("token")
    return api.post("/api/SavedMovie/CreateSavedMovie", data, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const updateSavedMovie = data => {
    const token = localStorage.getItem("token")
    return api.put("/api/SavedMovie/UpdateSavedMovie", data, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const deleteSavedMovie = id => {
    const token = localStorage.getItem("token")
    return api.delete(`/api/SavedMovie/DeleteSavedMovie/${id}`, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const getSavedMovieById = id => {
    const token = localStorage.getItem("token")
    return api.get(`/api/SavedMovie/GetSavedMovieByID/${id}`, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const getSavedMoviesByUserId = userId => {
    const token = localStorage.getItem("token")
    return api.get(`/api/SavedMovie/GetSavedMoviesByUserID/user/${userId}`, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const getSavedMoviesByMovieId = movieId => {
    const token = localStorage.getItem("token")
    return api.get(`/api/SavedMovie/GetSavedMoviesByMovieID/movie/${movieId}`, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

// Search APIs
export const searchMovies = params => {
    const token = localStorage.getItem("token")
    return api.get("/api/search/movies", {
        params,
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const suggestMovies = params => {
    const token = localStorage.getItem("token")
    return api.get("/api/search/movies/suggest", {
        params,
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const searchPersons = params => {
    const token = localStorage.getItem("token")
    return api.get("/api/search/persons", {
        params,
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const searchAllMovies = data => {
    const token = localStorage.getItem("token")
    return api.post("/api/search/movies/all", data, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

// Tag APIs
export const createTag = data => {
    const token = localStorage.getItem("token")
    return api.post("/movie/Tag/CreateTag", data, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const updateTag = data => {
    const token = localStorage.getItem("token")
    return api.put("/movie/Tag/UpdateTag", data, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const deleteTag = id => {
    const token = localStorage.getItem("token")
    return api.delete(`/movie/Tag/DeleteTag/${id}`, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const getTagById = TagID => {
    const token = localStorage.getItem("token")
    return api.get(`/movie/Tag/GetTagById/${TagID}`, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const getAllTags = () => {
    const token = localStorage.getItem("token")
    return api.get("/movie/Tag/GetAllTags/getALlTags", {
        headers: { Authorization: `Bearer ${token}` },
    })
}

// User APIs
export const getAllUsers = () => {
    const token = localStorage.getItem("token")
    const prefix = isAdmin() ? "/user/admin" : "/user"
    return api.get(`${prefix}/getAllUsers`, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const adminGetAllUsers = () => {
    const token = localStorage.getItem("token")
    return api.get("/user/admin/getAllUsers", {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const getUserByID = userID => {
    const token = localStorage.getItem("token")
    return api.get(`/user/getUserByID/${userID}`, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const getUserSlimByID = userID => {
    const token = localStorage.getItem("token")
    const prefix = isAdmin() ? "/user/admin" : "/user"
    return api.get(`${prefix}/GetUserSlimById${userID}`, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const deleteUser = params => {
    const token = localStorage.getItem("token")
    return api.delete("/user/deleteUser", {
        params,
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const updateUserProfile = formData => {
    const token = localStorage.getItem("token")
    return api.put("/user/update/profile", formData, {
        headers: {
            "Content-Type": "multipart/form-data",
            Authorization: `Bearer ${token}`,
        },
    })
}

export const getMe = () => {
    const token = localStorage.getItem("token")
    return api.get("/user/me", {
        headers: { Authorization: `Bearer ${token}` },
    })
}

// UserRating APIs
export const createUserRating = data => {
    const token = localStorage.getItem("token")
    return api.post("/api/UserRating/CreateUserRating", data, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const updateUserRating = data => {
    const token = localStorage.getItem("token")
    return api.put("/api/UserRating/UpdateUserRating", data, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const deleteUserRating = id => {
    const token = localStorage.getItem("token")
    return api.delete(`/api/UserRating/DeleteUserRating/${id}`, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const getUserRatingById = ID => {
    const token = localStorage.getItem("token")
    return api.get(`/api/UserRating/GetUserRatingById/${ID}`, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const getAllUserRatingsByUserId = userID => {
    const token = localStorage.getItem("token")
    return api.get(`/api/UserRating/GetAllUserRatingsByUserId/${userID}`, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const getAllUserRatingsByMovieId = movieID => {
    const token = localStorage.getItem("token")
    return api.get(`/api/UserRating/GetAllUserRatingsByMovieId/${movieID}`, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

// VimeoUpload APIs
export const uploadVimeoFile = data => {
    const token = localStorage.getItem("token")
    return api.post("/api/upload/vimeo/file", data, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const uploadVimeoLink = data => {
    const token = localStorage.getItem("token")
    return api.post("/api/upload/vimeo/link", data, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

// WatchProgress APIs
export const createWatchProgress = data => {
    const token = localStorage.getItem("token")
    return api.post("/movie/WatchProgress/CreateWatchProgress", data, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const updateWatchProgress = data => {
    const token = localStorage.getItem("token")
    return api.put("/movie/WatchProgress/UpdateWatchProgress", data, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const deleteWatchProgress = id => {
    const token = localStorage.getItem("token")
    return api.delete(`/movie/WatchProgress/DeleteWatchProgress/${id}`, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const getWatchProgressByUserId = userId => {
    const token = localStorage.getItem("token")
    return api.get(`/movie/WatchProgress/GetWatchProgressByUserId/${userId}`, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const getWatchProgressById = ID => {
    const token = localStorage.getItem("token")
    return api.get(`/movie/WatchProgress/GetWatchProgressByID/${ID}`, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const getWatchProgressByMovieId = movieId => {
    const token = localStorage.getItem("token")
    return api.get(
        `/movie/WatchProgress/GetWatchProgressByMovieId/${movieId}`,
        {
            headers: { Authorization: `Bearer ${token}` },
        }
    )
}

// YouTubeUpload APIs
export const uploadYoutubeFile = data => {
    const token = localStorage.getItem("token")
    return api.post("/api/upload/youtube/file", data, {
        headers: {
            Authorization: `Bearer ${token}`,
            "Content-Type": "multipart/form-data",
        },
    })
}

// Movie Subtitle APIs
export const updateMovieSubtitle = async data => {
    const token = localStorage.getItem("token")
    const response = await fetch(
        "https://filmzone-api.koyeb.app/api/MovieSubTitle/UpdateMovieSubTitle/movie/updateMovieSubTitle",
        {
            method: "PUT",
            headers: {
                "Content-Type": "application/json",
                accept: "*/*",
                Authorization: `Bearer ${token}`,
            },
            body: JSON.stringify(data),
        }
    )
    return response.json()
}

export const deleteMovieSubtitle = async id => {
    const token = localStorage.getItem("token")
    const response = await fetch(
        `https://filmzone-api.koyeb.app/api/MovieSubTitle/DeleteMovieSubTitle/movie/deleteMovieSubTitle/${id}`,
        {
            method: "DELETE",
            headers: {
                Authorization: `Bearer ${token}`,
            },
        }
    )
    return response.json()
}

// Episode Subtitle APIs
export const updateEpisodeSubtitle = async data => {
    const token = localStorage.getItem("token")
    const response = await fetch(
        "https://filmzone-api.koyeb.app/api/MovieSubTitle/UpdateEpisodeSubTitle/episode/updateEpisodeSubTitle",
        {
            method: "PUT",
            headers: {
                "Content-Type": "application/json",
                accept: "*/*",
                Authorization: `Bearer ${token}`,
            },
            body: JSON.stringify(data),
        }
    )
    return response.json()
}

export const deleteEpisodeSubtitle = async id => {
    const token = localStorage.getItem("token")
    const response = await fetch(
        `https://filmzone-api.koyeb.app/api/MovieSubTitle/DeleteEpisodeSubTitle/episode/deleteEpisodeSubTitle/${id}`,
        {
            method: "DELETE",
            headers: {
                Authorization: `Bearer ${token}`,
            },
        }
    )
    return response.json()
}

export const uploadMovieSubtitle = data => {
    const token = localStorage.getItem("token")
    return fetch(
        "https://filmzone-api.koyeb.app/api/MovieSubTitle/UploadMovieSubTitle/UploadMovieSubTitle",
        {
            method: "POST",
            headers: {
                Authorization: `Bearer ${token}`,
            },
            body: data,
        }
    )
}

// Permission APIs
export const getAllPermissions = () => {
    const token = localStorage.getItem("token")
    const prefix = isAdmin() ? "/permissions/admin" : "/permissions"
    return api.get(`${prefix}/getall`, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const addPermission = data => {
    const token = localStorage.getItem("token")
    const isBulk = !isAdmin()
    const prefix = isAdmin()
        ? "/permissions/admin/addPermission"
        : "/permissions/BulkCreate"
    const payload = isBulk ? [data] : data
    return api.post(prefix, payload, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const updatePermission = data => {
    const token = localStorage.getItem("token")
    const prefix = isAdmin() ? "/permissions/admin" : "/permissions"
    return api.put(`${prefix}/updatePermission`, data, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const deletePermission = permissionID => {
    const token = localStorage.getItem("token")
    const prefix = isAdmin() ? "/permissions/admin" : "/permissions"
    return api.delete(`${prefix}/delete?permissionId=${permissionID}`, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const getPermissionbyRoleId = roleID => {
    const token = localStorage.getItem("token")
    const prefix = isAdmin() ? "/permissions/admin" : "/permissions"
    return api.get(`${prefix}/getbyRoleID/${roleID}`, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

// Order APIs
export const getAllOrders = () => {
    const token = localStorage.getItem("token")
    return api.get("/api/payment/order/all", {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const getOrderById = orderId => {
    const token = localStorage.getItem("token")
    return api.get(`/api/payment/order/${orderId}`, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const createOrder = data => {
    const token = localStorage.getItem("token")
    return api.post("/api/payment/order/create", data, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export const updateOrderStatus = (orderId, data) => {
    const token = localStorage.getItem("token")
    return api.put(`/api/payment/order/${orderId}/status`, data, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

// Subtitle APIs
export const getAllSubtitlesByMovieSourceId = sourceId => {
    const token = localStorage.getItem("token")
    return api.get(
        `/api/MovieSubTitle/GetAllSubTitlesByMovieId/movie/GetAllSubTitlesBySourceID/${sourceId}`,
        {
            headers: { Authorization: `Bearer ${token}` },
        }
    )
}

export const getAllSubtitlesByEpisodeSourceId = sourceId => {
    const token = localStorage.getItem("token")
    return api.get(
        `/api/MovieSubTitle/GetAllSubTitlesByEpisodeId/episode/GetAllSubTitlesBySourceID/${sourceId}`,
        {
            headers: { Authorization: `Bearer ${token}` },
        }
    )
}

// Movie Source APIs
export const getMovieSourcesByMovieId = movieId => {
    const token = localStorage.getItem("token")
    return api.get(
        `/movie/MovieSource/GetMovieSourcesByMovieIdPublic/getByMovieId/${movieId}`,
        {
            headers: { Authorization: `Bearer ${token}` },
        }
    )
}

// Episode APIs
export const getEpisodesByMovieId = movieId => {
    const token = localStorage.getItem("token")
    return api.get(`/api/Episode/GetEpisodesByMovieId/getbyMovie/${movieId}`, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

// Invoice APIs
export const getAllInvoices = () => {
    return api.get("/api/payment/invoice/all")
}

export const getInvoiceByOrderId = orderID => {
    return api.get(`/api/payment/invoice/${orderID}`)
}

export const getInvoicesByUserId = userID => {
    return api.get(`/api/payment/invoice/user/${userID}`)
}

// Translate Subtitle API
export const translateSubtitleFromSource = data => {
    const token = localStorage.getItem("token")
    return api.post(
        "/api/MovieSubTitle/TranslateFromSource/Translate/AutoFromSource",
        data,
        {
            headers: { Authorization: `Bearer ${token}` },
        }
    )
}

export const cloneRole = data => {
    const prefix = isAdmin() ? "/roles/admin" : "/roles"
    return api.post(`${prefix}/clonerole`, data)
}

// RolePermission APIs
export const assignPermissionToRole = data => {
    const token = localStorage.getItem("token")
    const url = isAdmin()
        ? "/role-permissions/admin/assign-permissions"
        : "/role-permissions/assign-permissions"
    return api.post(url, data, {
        headers: { Authorization: `Bearer ${token}` },
    })
}

export default api
