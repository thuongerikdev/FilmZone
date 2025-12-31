// services/api.js
import axios from "axios"

const isAdmin = () => {
    return localStorage.getItem("isAdmin") === "true";
};

const API_BASE_URL = "https://filmzone-api.koyeb.app"

const api = axios.create({
    baseURL: API_BASE_URL,
    headers: {
        "Content-Type": "application/json",
        "credentials": "include",
    },
})

// --- 1. Request Interceptor: Gắn token vào mọi request ---
api.interceptors.request.use(
    (config) => {
        const token = localStorage.getItem("token");
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => {
        return Promise.reject(error);
    }
);

// --- 2. Response Interceptor: Xử lý khi Token hết hạn (Lỗi 401) ---
api.interceptors.response.use(
    (response) => {
        return response;
    },
    async (error) => {
        const originalRequest = error.config;

        // Debug: Log ra để xem Axios thực sự nhìn thấy gì
        if (error.response) {
            console.log("Headers Axios nhận được:", error.response.headers);
        }

        if (error.response) {
            // 1. Kiểm tra header (nếu backend đã fix expose)
            // 2. HOẶC kiểm tra thông báo lỗi trong body (nếu có)
            // 3. HOẶC kiểm tra www-authenticate (nếu backend expose)
            // 4. Fallback: Nếu 401 và chưa retry -> Coi như hết hạn token
            
            const headers = error.response.headers;
            
            const isUnauthorized = error.response.status === 401;
            
            const isTokenExpired = isUnauthorized && !originalRequest._retry;

            if (isTokenExpired) {
                originalRequest._retry = true; 

                try {
                    console.log("Phát hiện lỗi 401, đang gọi Refresh Token...");
                    const refreshToken = localStorage.getItem("refreshToken");

                    if (!refreshToken) {
                        console.log("Không có refresh token, logout.");
                        handleLogout();
                        return Promise.reject(error);
                    }

                    const response = await axios.post(`${API_BASE_URL}/login/auth/refresh`, {
                        refreshToken: refreshToken
                    });

                    if (response.data && response.data.errorCode === 200) {
                        const { accessToken, accessTokenExpiresAt, refreshToken, refreshTokenExpiresAt } = response.data.data;

                        localStorage.setItem("token", accessToken);
                        localStorage.setItem("tokenExpiration", accessTokenExpiresAt);
                        if (refreshToken) {
                            localStorage.setItem("refreshToken", refreshToken);
                            localStorage.setItem("refreshTokenExpiration", refreshTokenExpiresAt);
                        }

                        api.defaults.headers.common["Authorization"] = `Bearer ${accessToken}`;
                        originalRequest.headers["Authorization"] = `Bearer ${accessToken}`;

                        console.log("Refresh thành công, gọi lại request cũ...");
                        return api(originalRequest);
                    }
                } catch (refreshError) {
                    // ... xử lý lỗi refresh ...
                    console.error("Lỗi khi Refresh Token:", refreshError);
                    handleLogout();
                    return Promise.reject(refreshError);
                }
            }
        }

        return Promise.reject(error);
    }
);

// Hàm Logout chung
const handleLogout = () => {
    localStorage.clear(); 
    window.location.href = "/login"; 
};

// Account APIs
export const startMfaTotp = data => api.post("/account/mfa/totp/start", data)
export const confirmMfaTotp = data =>
    api.post("/account/mfa/totp/confirm", data)
export const disableMfaTotp = data =>
    api.post("/account/mfa/totp/disable", data)
export const startPasswordChangeEmail = data =>
    api.post("/account/password/change/email/start", data)
export const verifyPasswordChangeEmail = data =>
    api.post("/account/password/change/email/verify", data)
export const verifyPasswordChangeMfa = data =>
    api.post("/account/password/change/mfa/verify", data)
export const commitPasswordChange = data =>
    api.post("/account/password/change/commit", data)
export const startPasswordForgotEmail = data =>
    api.post("/account/password/forgot/email/start", data)
export const verifyPasswordForgotEmail = data =>
    api.post("/account/password/forgot/email/verify", data)
export const verifyPasswordForgotMfa = data =>
    api.post("/account/password/forgot/mfa/verify", data)
export const commitPasswordForgot = data =>
    api.post("/account/password/forgot/commit", data)

// ArchiveUpload APIs
export const uploadArchiveFile = data =>
    api.post("/api/upload/archive/file", data)
export const uploadArchiveLink = data =>
    api.post("/api/upload/archive/link", data)

// Comment APIs
export const createComment = data =>
    api.post("/api/Comment/CreateComment", data)
export const updateComment = data => api.put("/api/Comment/UpdateComment", data)
export const deleteComment = id =>
    api.delete(`/api/Comment/DeleteComment/${id}`)
export const getCommentById = id => api.get(`/api/Comment/GetCommentByID/${id}`)
export const getCommentsByUserId = userID =>
    api.get(`/api/Comment/GetCommentsByUserID/${userID}?userID=${userID}`)
export const getCommentsByMovieId = movieID =>
    api.get(`/api/Comment/GetCommentsByMovieID/${movieID}`, {
        params: { movieID },
    })

// Episode APIs
export const createEpisode = data =>
    api.post("/api/Episode/CreateEpisode", data)
export const updateEpisode = data => api.put("/api/Episode/UpdateEpisode", data)
export const deleteEpisode = id =>
    api.delete(`/api/Episode/DeleteEpisode/${id}`)
export const getEpisodeById = id => api.get(`/api/Episode/GetEpisodeById/${id}`)
export const getAllEpisodes = () =>
    api.get("/api/Episode/GetAllEpisodes/getAll")

// EpisodeSource APIs
export const createEpisodeSource = data =>
    api.post("/movie/EpisodeSource/CreateEpisodeSource", data)
export const updateEpisodeSource = data =>
    api.put("/movie/EpisodeSource/UpdateEpisodeSource", data)
export const deleteEpisodeSource = id =>
    api.delete(`/movie/EpisodeSource/DeleteEpisodeSource/${id}`)
export const getEpisodeSourceById = id =>
    api.get(`/movie/EpisodeSource/GetEpisodeSourceById/${id}`)
export const getEpisodeSourcesByEpisodeId = episodeId =>
    api.get(`/movie/EpisodeSource/GetEpisodeSourcesByEpisodeId/${episodeId}`)

// EpisodeWatchProgress APIs
export const createEpisodeWatchProgress = data =>
    api.post("/api/EpisodeWatchProgress/CreateEpisodeWatchProgress", data)
export const updateEpisodeWatchProgress = data =>
    api.put("/api/EpisodeWatchProgress/UpdateEpisodeWatchProgress", data)
export const deleteEpisodeWatchProgress = id =>
    api.delete(`/api/EpisodeWatchProgress/DeleteEpisodeWatchProgress/${id}`)
export const getEpisodeWatchProgressById = id =>
    api.get(`/api/EpisodeWatchProgress/GetEpisodeWatchProgressByID/${id}`)
export const getEpisodeWatchProgressByUserId = userId =>
    api.get(
        `/api/EpisodeWatchProgress/GetEpisodeWatchProgressByUserID/user/${userId}`
    )
export const getEpisodeWatchProgressByEpisodeId = episodeId =>
    api.get(
        `/api/EpisodeWatchProgress/GetEpisodeWatchProgressByEpisodeID/episode/${episodeId}`
    )

// FZ.WebAPI Health
export const getHealthz = () => api.get("/healthz")

// ImageSource APIs
export const createImageSource = data =>
    api.post("/movie/ImageSource/CreateImageSource", data)
export const updateImageSource = data =>
    api.put("/movie/ImageSource/UpdateImageSource", data)
export const deleteImageSource = id =>
    api.delete(`/movie/ImageSource/DeleteImageSource/${id}`)
export const getImageSourcesByType = Type =>
    api.get(`/movie/ImageSource/GetImageSourcesByType/${Type}`)

// Login APIs
export const userLogin = data => api.post("/login/StaffLogin", data)
export const loginMobile = data => api.post("/login/login/mobile", data)
export const googleLogin = () => api.get("/login/google-login")
export const logout = data => api.post("/login/logout", data)
export const logoutSession = sessionId =>
    api.post(`/login/logout/session/${sessionId}`)
export const logoutAll = data => api.post("/login/logout/all", data)
export const googleCallback = () => api.get("/login/google/callback")
export const signinGoogle = () => api.get("/login/signin-google")
export const verifyMfa = data => api.post("/login/mfa/verify", data)
export const refreshAuth = data => api.post("/login/auth/refresh", data)

// Movie APIs
export const createMovie = data => api.post("/api/Movie/CreateMovie", data)
export const updateMovie = data => api.put("/api/Movie/UpdateMovie", data)
export const deleteMovie = id => api.delete(`/api/Movie/DeleteMovie/${id}`)
export const getMovieById = id => api.get(`/api/Movie/GetMovieById/${id}`)
export const getAllMovies = () => api.get("/api/Movie/GetAllMovies/gellAll")
export const getAllMoviesMainScreen = () =>
    api.get("/api/Movie/GetAllMoviesMainScreen/mainScreen")
export const getAllMoviesNewReleaseMainScreen = () =>
    api.get("/api/Movie/GetAllMoviesNewReleaseMainScreen/newReleaseMainScreen")
export const getWatchNowMovieById = id =>
    api.get(`/api/Movie/GetWatchNowMovieByID/watchNow/${id}`)

// MoviePerson APIs
export const addPersonToMovie = data =>
    api.post("/movie/MoviePerson/AddPersonToMovie", data)
export const removePersonFromMovie = id =>
    api.delete(`/movie/MoviePerson/RemovePersonFromMovie/${id}`)
export const getMoviesByPerson = personID =>
    api.get(`/movie/MoviePerson/GetMoviesByPerson/${personID}`)
export const getPersonsByMovie = movieID =>
    api.get(`/movie/MoviePerson/GetPersonsByMovie/${movieID}`)

// MovieSource APIs
export const createMovieSource = data =>
    api.post("/movie/MovieSource/CreateMovieSource", data)
export const updateMovieSource = data =>
    api.put("/movie/MovieSource/UpdateMovieSource", data)
export const deleteMovieSource = id =>
    api.delete(`/movie/MovieSource/DeleteMovieSource/${id}`)
export const getVipSourceByMovieId = movieId =>
    api.get(`/api/movies/${movieId}/vip-source`)
export const getMovieSourceById = id =>
    api.get(`/movie/MovieSource/GetMovieSourceById/${id}`)

// MovieTag APIs
export const addTagToMovie = data =>
    api.post("/movie/MovieTag/AddTagToMovie", data)
export const updateMovieTag = data =>
    api.put("/movie/MovieTag/UpdateMovieTag", data)
export const deleteMovieTag = id =>
    api.delete(`/movie/MovieTag/DeleteMovieTag/${id}`)
export const getMoviesByTag = tagID =>
    api.get(`/movie/MovieTag/GetMoviesByTag/${tagID}`)
export const getTagsByMovie = movieID =>
    api.get(`/movie/MovieTag/GetTagsByMovie/${movieID}`)
export const getMoviesByTagIds = data =>
    api.get("/movie/MovieTag/GetMoviesByTagIDs/getMovieByTagID", {
        params: data,
    })

// Payment APIs
export const vnpayCheckout = data =>
    api.post("/api/payment/vnpay/checkout", data)
export const vnpayCallback = () => api.get("/api/payment/vnpay/callback")

// Person APIs
export const createPerson = data => api.post("/movie/Person/CreatePerson", data)
export const updatePerson = data => api.put("/movie/Person/UpdatePerson", data)
export const deletePerson = id => api.delete(`/movie/Person/DeletePerson/${id}`)
export const getPersonById = ID => api.get(`/movie/Person/GetPersonByID/${ID}`)
export const getAllPersons = () => api.get("/movie/Person/GetAllPerson/getall")

// Plan APIs
export const createPlan = data => api.post("/api/plans/create", data)
export const updatePlan = data => api.put("/api/plans/update", data)
export const deletePlan = planID => api.delete(`/api/plans/delete/${planID}`)
export const getPlanById = planID => api.get(`/api/plans/${planID}`)
export const getAllPlans = () => api.get("/api/plans/all")

// Price APIs
export const createPrice = data => api.post("/api/price/Create", data)
export const updatePrice = data => api.put("/api/price/Update", data)
export const deletePrice = priceID => api.delete(`/api/price/Delete/${priceID}`)
export const getPriceById = priceID => api.get(`/api/price/${priceID}`)
export const getAllPrices = () => api.get("/api/price/all")

// Region APIs
export const createRegion = data => api.post("/movie/Region/CreateRegion", data)
export const updateRegion = data => api.put("/movie/Region/UpdateRegion", data)
export const deleteRegion = id => api.delete(`/movie/Region/DeleteRegion/${id}`)
export const getRegionById = ID => api.get(`/movie/Region/GetRegionByID/${ID}`)
export const getAllRegions = () => api.get("/movie/Region/GetAllRegions/getAll")
export const getMoviesByRegionId = regionID =>
    api.get(`/movie/Region/GetMovieByRegionID/getMovieByRegionID/${regionID}`)
export const getPersonsByRegionId = regionID =>
    api.get(`/movie/Region/GetPersonByRegionID/getPersonByRegionID/${regionID}`)

// Register APIs
export const register = data => api.post("/register", data)
export const verifyRegisterEmail = data =>
    api.post("/register/verifyRegisterEmail", data)

// Role APIs
export const getAllRoles = () => api.get("/roles/getall")
export const addRole = data => api.post("/roles/addRole", data)
export const updateRole = data => api.put("/roles/updateRole", data)
export const deleteRole = roleID => api.delete(`/roles/deleteRole/${roleID}`)
export const getRoleByUserId = userID =>
    api.get(`/roles/getRoleByUserID/${userID}`)

// SavedMovie APIs
export const createSavedMovie = data =>
    api.post("/api/SavedMovie/CreateSavedMovie", data)
export const updateSavedMovie = data =>
    api.put("/api/SavedMovie/UpdateSavedMovie", data)
export const deleteSavedMovie = id =>
    api.delete(`/api/SavedMovie/DeleteSavedMovie/${id}`)
export const getSavedMovieById = id =>
    api.get(`/api/SavedMovie/GetSavedMovieByID/${id}`)
export const getSavedMoviesByUserId = userId =>
    api.get(`/api/SavedMovie/GetSavedMoviesByUserID/user/${userId}`)
export const getSavedMoviesByMovieId = movieId =>
    api.get(`/api/SavedMovie/GetSavedMoviesByMovieID/movie/${movieId}`)

// Search APIs
export const searchMovies = params => api.get("/api/search/movies", { params })
export const suggestMovies = params =>
    api.get("/api/search/movies/suggest", { params })
export const searchPersons = params =>
    api.get("/api/search/persons", { params })
export const searchAllMovies = data => api.post("/api/search/movies/all", data)

// Tag APIs
export const createTag = data => api.post("/movie/Tag/CreateTag", data)
export const updateTag = data => api.put("/movie/Tag/UpdateTag", data)
export const deleteTag = id => api.delete(`/movie/Tag/DeleteTag/${id}`)
export const getTagById = TagID => api.get(`/movie/Tag/GetTagById/${TagID}`)
export const getAllTags = () => api.get("/movie/Tag/GetAllTags/getALlTags")

// User APIs
export const getAllUsers = () => {
    const prefix = isAdmin() ? "/user/admin" : "/user"; 
    return api.get(`${prefix}/getAllUsers`);
}
export const adminGetAllUsers = () => api.get("/user/admin/getAllUsers")
export const getUserByID = userID => api.get(`/user/getUserByID/${userID}`)
export const getUserSlimByID = userID => {
    const prefix = isAdmin() ? "/user/admin" : "/user"; 
    return api.get(`${prefix}/GetUserSlimById${userID}`);
}
export const deleteUser = params => api.delete("/user/deleteUser", { params })
export const updateUserProfile = (formData) => {
    return api.put("/user/update/profile", formData, {
        headers: {
            "Content-Type": "multipart/form-data",
        },
    });
};
export const getMe = () => api.get("/user/me")

// UserRating APIs
export const createUserRating = data =>
    api.post("/api/UserRating/CreateUserRating", data)
export const updateUserRating = data =>
    api.put("/api/UserRating/UpdateUserRating", data)
export const deleteUserRating = id =>
    api.delete(`/api/UserRating/DeleteUserRating/${id}`)
export const getUserRatingById = ID =>
    api.get(`/api/UserRating/GetUserRatingById/${ID}`)
export const getAllUserRatingsByUserId = userID =>
    api.get(`/api/UserRating/GetAllUserRatingsByUserId/${userID}`)
export const getAllUserRatingsByMovieId = movieID =>
    api.get(`/api/UserRating/GetAllUserRatingsByMovieId/${movieID}`)

// VimeoUpload APIs
export const uploadVimeoFile = data => api.post("/api/upload/vimeo/file", data)
export const uploadVimeoLink = data => api.post("/api/upload/vimeo/link", data)

// WatchProgress APIs
export const createWatchProgress = data =>
    api.post("/movie/WatchProgress/CreateWatchProgress", data)
export const updateWatchProgress = data =>
    api.put("/movie/WatchProgress/UpdateWatchProgress", data)
export const deleteWatchProgress = id =>
    api.delete(`/movie/WatchProgress/DeleteWatchProgress/${id}`)
export const getWatchProgressByUserId = userId =>
    api.get(`/movie/WatchProgress/GetWatchProgressByUserId/${userId}`)
export const getWatchProgressById = ID =>
    api.get(`/movie/WatchProgress/GetWatchProgressByID/${ID}`)
export const getWatchProgressByMovieId = movieId =>
    api.get(`/movie/WatchProgress/GetWatchProgressByMovieId/${movieId}`)

// YouTubeUpload APIs
export const uploadYoutubeFile = data =>
    api.post("/api/upload/youtube/file", data)

export const updateMovieSubtitle = async data => {
    const response = await fetch(
        "https://filmzone-api.koyeb.app/api/MovieSubTitle/UpdateMovieSubTitle/movie/updateMovieSubTitle",
        {
            method: "PUT",
            headers: { "Content-Type": "application/json", accept: "*/*" },
            body: JSON.stringify(data),
        }
    )
    return response.json()
}

// DELETE Movie Subtitle
export const deleteMovieSubtitle = async id => {
    const response = await fetch(
        `https://filmzone-api.koyeb.app/api/MovieSubTitle/DeleteMovieSubTitle/movie/deleteMovieSubTitle/${id}`,
        {
            method: "DELETE",
        }
    )
    return response.json()
}

// --- EPISODE SUBTITLE ---

// PUT Update Episode Subtitle
export const updateEpisodeSubtitle = async data => {
    const response = await fetch(
        "https://filmzone-api.koyeb.app/api/MovieSubTitle/UpdateEpisodeSubTitle/episode/updateEpisodeSubTitle",
        {
            method: "PUT",
            headers: { "Content-Type": "application/json", accept: "*/*" },
            body: JSON.stringify(data),
        }
    )
    return response.json()
}

// DELETE Episode Subtitle
export const deleteEpisodeSubtitle = async id => {
    const response = await fetch(
        `https://filmzone-api.koyeb.app/api/MovieSubTitle/DeleteEpisodeSubTitle/episode/deleteEpisodeSubTitle/${id}`,
        {
            method: "DELETE",
        }
    )
    return response.json()
}


// Permission APIs
export const getAllPermissions = () => {
    const prefix = isAdmin() ? "/permissions/admin" : "/permissions";
    return api.get(`${prefix}/getall`);
};
export const addPermission = data => api.post("/permissions/addPermission", data)
export const updatePermission = data => api.put("/permissions/updatePermission", data)
export const deletePermission = permissionID => api.delete(`/permissions/delate?permissionId=${permissionID}`)
export const getPermissionbyRoleId = (roleID) => {
    const prefix = isAdmin() ? "/permissions/admin" : "/permissions";
    return api.get(`${prefix}/getbyRoleID/${roleID}`);
};

// RolePermission APIs
export const assignPermissionToRole = (data) => {
    const url = isAdmin() 
        ? "/role-permissions/admin/assign-permissions" 
        : "/role-permissions/assign-permissions";
    return api.post(url, data);
};


export default api
