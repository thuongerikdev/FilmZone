// app/lib/apiClient.ts
import axios, {
    AxiosError,
    AxiosInstance,
    AxiosRequestConfig,
    AxiosResponse,
    InternalAxiosRequestConfig,
} from 'axios';

// ---- type augmentation: thêm cờ skipAuth & __retry vào AxiosRequestConfig ----
declare module 'axios' {
    export interface AxiosRequestConfig {
        /** Bỏ qua gắn Authorization và KHÔNG chạy refresh khi 401 (dùng cho public API) */
        skipAuth?: boolean;
        /** Cờ nội bộ để tránh lặp vô hạn khi retry sau refresh */
        __retry?: boolean;
    }
}

const baseURL = process.env.NEXT_PUBLIC_API_BASE_URL ?? '';

const http: AxiosInstance = axios.create({
    baseURL,
    withCredentials: true, // cần nếu BE set cookie HttpOnly cho refresh
    timeout: 15000,
});

// Token chỉ giữ trong RAM (không localStorage để tránh XSS)
let memoryAccessToken: string | null = null;
let refreshing: Promise<void> | null = null;

export function setAccessToken(token: string | null) {
    memoryAccessToken = token;
}
export function clearAccessToken() {
    memoryAccessToken = null;
}

// Gọi refresh bằng axios gốc (tránh interceptor) để lấy access token mới
async function doRefresh(): Promise<void> {
    const res = await axios.post(
        `${baseURL}/login/auth/refresh`,
        {},
        { withCredentials: true }
    );
    // BE trả { token, expiresIn }
    memoryAccessToken = (res.data as any)?.token ?? null;
}

// ---- Request interceptor: gắn Authorization trừ khi skipAuth ----
http.interceptors.request.use((config: InternalAxiosRequestConfig) => {
    if (!config.skipAuth && memoryAccessToken) {
        config.headers = config.headers ?? {};
        (config.headers as Record<string, string>)['Authorization'] =
            `Bearer ${memoryAccessToken}`;
    }
    return config;
});

// ---- Response interceptor: nếu 401 và KHÔNG skipAuth => refresh & retry 1 lần ----
http.interceptors.response.use(
    (res: AxiosResponse) => res,
    async (error: AxiosError) => {
        const cfg = error.config as AxiosRequestConfig | undefined;
        const status = error.response?.status;

        // Public request (skipAuth) hoặc không phải 401 -> ném lỗi luôn
        if (!cfg || cfg.skipAuth || status !== 401) throw error;

        // Tránh vòng lặp vô hạn
        if (cfg.__retry) throw error;

        // Khoá refresh để các request khác cùng chờ
        if (!refreshing) {
            refreshing = doRefresh().finally(() => { refreshing = null; });
        }
        await refreshing;

        // Refresh thất bại -> không có token -> chuyển về /login
        if (!memoryAccessToken) {
            if (typeof window !== 'undefined') window.location.href = '/login';
            throw error;
        }

        // Retry 1 lần với token mới
        cfg.__retry = true;
        cfg.headers = cfg.headers ?? {};
        (cfg.headers as Record<string, string>)['Authorization'] =
            `Bearer ${memoryAccessToken}`;

        return http.request(cfg);
    }
);

// ---- Helper cho public API: tự set skipAuth & withCredentials=false ----
export const httpPublic = {
    get<T = unknown>(url: string, config?: AxiosRequestConfig) {
        return http.get<T>(url, { ...config, skipAuth: true, withCredentials: false });
    },
    post<T = unknown>(url: string, data?: unknown, config?: AxiosRequestConfig) {
        return http.post<T>(url, data, { ...config, skipAuth: true, withCredentials: false });
    },
    put<T = unknown>(url: string, data?: unknown, config?: AxiosRequestConfig) {
        return http.put<T>(url, data, { ...config, skipAuth: true, withCredentials: false });
    },
    delete<T = unknown>(url: string, config?: AxiosRequestConfig) {
        return http.delete<T>(url, { ...config, skipAuth: true, withCredentials: false });
    },
};

// Mặc định export http cho các endpoint cần auth (tự refresh)
export { http };
