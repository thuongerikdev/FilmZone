// src/app/(auth)/login/login.api.ts
export interface LoginRequest {
  userName: string;
  password: string;
}

export interface LoginResponse {
  errorCode: number;
  errorMessage?: string;   // BE có thể dùng key này
  errorMessager?: string;  // hoặc key này
  data?: {
    userID?: number;
    userName?: string;
    email?: string;
    isEmailVerified?: boolean;

    // MFA
    requiresMfa?: boolean;
    mfaTicket?: string | null;

    // Token (có thể null khi requiresMfa = true)
    token?: string | null;
    refreshToken?: string | null;
    tokenExpiration?: string;
    refreshTokenExpiration?: string;

    // Khác
    sessionId?: number;
    deviceId?: string | null;
  };
}

function getApiBase(): string {
  const base = process.env.NEXT_PUBLIC_API_BASE_URL;
  if (!base) throw new Error('Thiếu cấu hình NEXT_PUBLIC_API_BASE_URL');
  return base.replace(/\/+$/, '');
}

/** Gọi API đăng nhập (nhận cookie HttpOnly nếu BE set). */
export async function login(credentials: LoginRequest): Promise<LoginResponse> {
  try {
    const base = getApiBase();
    const res = await fetch(`${base}/login/userLogin`, {
      method: 'POST',
      credentials: 'include', // để nhận Set-Cookie
      headers: {
        accept: '*/*',
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(credentials),
    });

    const data = (await res.json().catch(() => null)) as LoginResponse | null;
    if (!data) return { errorCode: 500, errorMessage: 'Phản hồi không hợp lệ' };
    return data;
  } catch {
    return { errorCode: 500, errorMessage: 'Đã xảy ra lỗi. Vui lòng thử lại.' };
  }
}

/** Điều hướng qua BE để OAuth Google (BE set cookie trong callback). */
export function redirectToGoogle(returnUrl?: string) {
  const base = getApiBase();
  // NOTE: nếu BE của bạn là /login/google-login thì đổi lại endpoint ở dòng dưới
  const url = new URL(`${base}/login/google-login`);
  if (returnUrl) url.searchParams.set('returnUrl', returnUrl);
  window.location.href = url.toString();
}

/** (tuỳ nhu cầu) gọi refresh để lấy access token mới dựa trên refresh-cookie */
export async function refresh(): Promise<{ token: string; expiresIn?: number } | null> {
  try {
    const base = getApiBase();
    const res = await fetch(`${base}/login/auth/refresh`, {
      method: 'POST',
      credentials: 'include',
    });
    if (!res.ok) return null;
    return (await res.json()) as { token: string; expiresIn?: number };
  } catch {
    return null;
  }
}

/** Helper lấy message chuẩn từ response */
export const pickMessage = (r: LoginResponse) =>
  r.errorMessage || r.errorMessager || '';
