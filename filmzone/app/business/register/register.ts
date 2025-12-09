export interface RegisterRequest {
  userName: string;
  password: string;
  email: string;
  phoneNumber: string;
  fullName: string;
  dateOfBirth: string; // ISO 8601
  gender: string;      // e.g. "Male" | "Female" | "Other"
}

export interface RegisterResponse {
  errorCode: number;
  errorMessager: string;
  data?: unknown;
}

export async function register(payload: RegisterRequest): Promise<RegisterResponse> {
  try {
    const baseURL = process.env.NEXT_PUBLIC_API_BASE_URL;
    const res = await fetch(`${baseURL}/api/AuthUser/register`, {
      method: 'POST',
      headers: {
        accept: '*/*',
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(payload),
    });

    const data: RegisterResponse = await res.json();
    return data;
  } catch (error) {
    return {
      errorCode: 500,
      errorMessager: 'Đã xảy ra lỗi. Vui lòng thử lại.',
    };
  }
}