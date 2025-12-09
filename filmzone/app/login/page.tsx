// app/business/login/page.tsx
'use client';

import React, { useEffect, useState } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import './style.css';
import Link from 'next/link';
import { http, setAccessToken } from '../lib/apiClient';

/* ===================== Types ===================== */
type LoginRequest = {
  userName: string;
  password: string;
};

type LoginResponse = {
  errorCode: number;
  errorMessage?: string;
  errorMessager?: string;
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
};

/* ===================== Helpers ===================== */
const pickMessage = (r: LoginResponse | null | undefined) =>
  (r?.errorMessage || r?.errorMessager || '').trim();

/* ===================== Component ===================== */
const LoginPage: React.FC = () => {
  const router = useRouter();
  const search = useSearchParams();
  const justLoggedOut = search.get('loggedout') === '1';

  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [isLoading, setIsLoading] = useState(false);

  // Guard: nếu đã đăng nhập thì rời trang /login
  const [checking, setChecking] = useState(true);

  // 1) Effect kiểm tra phiên (luôn được gọi)
  useEffect(() => {
    // Nếu vừa logout thì bỏ qua check refresh ở lần tải này
    if (justLoggedOut) { setChecking(false); return; }

    let cancelled = false;

    (async () => {
      try {
        // Public call: KHÔNG gắn Authorization, vẫn gửi cookie
        const res = await http.post<any>(
          '/login/auth/refresh',
          {},
          { skipAuth: true }
        );

        const token = res?.data?.token ?? res?.data?.data?.token ?? null;
        if (!cancelled && token) {
          setAccessToken(token);
          router.replace('/business/mainScreen');
        }
      } catch {
        // chưa có phiên -> hiển thị form
      } finally {
        if (!cancelled) setChecking(false);
      }
    })();

    return () => { cancelled = true; };
  }, [router, justLoggedOut]);

  // 2) Effect animation “mắt–miệng” (khai báo cố định; chỉ gắn listener khi checking=false)
  useEffect(() => {
    if (checking) return;

    const passwordField = document.getElementById('password') as HTMLInputElement | null;

    const handleMouseMove = (event: MouseEvent) => {
      if (
        !document.querySelector('#password:is(:focus)') &&
        !document.querySelector('#password:is(:user-invalid)')
      ) {
        const eyes = document.getElementsByClassName('eye') as HTMLCollectionOf<HTMLElement>;
        for (let i = 0; i < eyes.length; i++) {
          const eye = eyes[i];
          const rect = eye.getBoundingClientRect();
          const x = rect.left + 10;
          const y = rect.top + 10;
          const rad = Math.atan2(event.pageX - x, event.pageY - y);
          const rot = rad * (180 / Math.PI) * -1 + 180;
          eye.style.transform = `rotate(${rot}deg)`;
        }
      }
    };

    const handleFocusPassword = () => {
      const face = document.getElementById('face') as HTMLElement | null;
      if (face) face.style.transform = 'translateX(30px)';
      const eyes = document.getElementsByClassName('eye') as HTMLCollectionOf<HTMLElement>;
      for (let i = 0; i < eyes.length; i++) {
        eyes[i].style.transform = `rotate(100deg)`;
      }
    };

    const handleFocusOutPassword = (event: FocusEvent) => {
      const face = document.getElementById('face') as HTMLElement | null;
      const ball = document.getElementById('ball') as HTMLElement | null;
      if (face) face.style.transform = 'translateX(0)';
      const target = event.target as HTMLInputElement;
      if (ball) ball.classList.toggle('sad');
      if (!target.checkValidity()) {
        const eyes = document.getElementsByClassName('eye') as HTMLCollectionOf<HTMLElement>;
        for (let i = 0; i < eyes.length; i++) {
          eyes[i].style.transform = `rotate(215deg)`;
        }
      }
    };

    const handleSubmitHover = () => {
      const ball = document.getElementById('ball') as HTMLElement | null;
      if (ball) ball.classList.toggle('look_at');
    };

    document.addEventListener('mousemove', handleMouseMove);
    passwordField?.addEventListener('focus', handleFocusPassword);
    passwordField?.addEventListener('focusout', handleFocusOutPassword);
    const submitButton = document.getElementById('submit') as HTMLElement | null;
    submitButton?.addEventListener('mouseover', handleSubmitHover);
    submitButton?.addEventListener('mouseout', handleSubmitHover);

    return () => {
      document.removeEventListener('mousemove', handleMouseMove);
      passwordField?.removeEventListener('focus', handleFocusPassword);
      passwordField?.removeEventListener('focusout', handleFocusOutPassword);
      submitButton?.removeEventListener('mouseover', handleSubmitHover);
      submitButton?.removeEventListener('mouseout', handleSubmitHover);
    };
  }, [checking]);

  // Đăng nhập bằng axios client (public)
  const doLogin = async (credentials: LoginRequest): Promise<LoginResponse> => {
    try {
      const res = await http.post<LoginResponse>(
        '/login/userLogin',
        credentials,
        { skipAuth: true }
      );
      return res.data;
    } catch (err: any) {
      const msg =
        err?.response?.data?.errorMessage ||
        err?.response?.data?.errorMessager ||
        'Đã xảy ra lỗi. Vui lòng thử lại.';
      return { errorCode: 500, errorMessage: msg };
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    setError('');

    const result = await doLogin({ userName: username.trim(), password });

    if (result.errorCode === 200) {
      const requiresMfa = result.data?.requiresMfa === true;
      const ticket = result.data?.mfaTicket ?? '';

      if (requiresMfa && ticket) {
        router.replace(`/mfa/verify?ticket=${encodeURIComponent(ticket)}`);
        return;
      }

      const tk = result.data?.token ?? null;
      if (tk) setAccessToken(tk);

      router.replace('/business/mainScreen');
    } else {
      setError(pickMessage(result) || 'Đăng nhập thất bại');
      setIsLoading(false);
    }
  };

  const handleGoogleLogin = () => {
    const base = (process.env.NEXT_PUBLIC_API_BASE_URL ?? '').replace(/\/+$/, '');
    if (!base) { setError('Thiếu cấu hình NEXT_PUBLIC_API_BASE_URL'); return; }
    window.location.href = `${base}/login/google-login`;
  };

  return (
    <main>
      {checking ? (
        <div className="flex min-h-screen items-center justify-center opacity-70">
          Đang kiểm tra phiên...
        </div>
      ) : (
        <>
          <section className="form">
            <div className="logo">
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth="1.5" stroke="currentColor">
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  d="M21 7.5l-2.25-1.313M21 7.5v2.25m0-2.25l-2.25 1.313M3 7.5l2.25-1.313M3 7.5l2.25 1.313M3 7.5v2.25m9 3l2.25-1.313M12 12.75l-2.25-1.313M12 12.75V15m0 6.75l2.25-1.313M12 21.75V19.5m0 2.25l-2.25-1.313m0-16.875L12 2.25l2.25 1.313M21 14.25v2.25l-2.25 1.313m-13.5 0L3 16.5v-2.25"
                />
              </svg>
            </div>

            <h1 className="form__title">Đăng nhập vào tài khoản</h1>
            <p className="form__description">Chào mừng trở lại! Vui lòng nhập thông tin của bạn</p>

            <form onSubmit={handleSubmit}>
              <label className="form-control__label">Tên đăng nhập</label>
              <input
                type="text"
                className="form-control"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                required
              />

              <label className="form-control__label">Mật khẩu</label>
              <div className="password-field">
                <input
                  type="password"
                  className="form-control"
                  minLength={4}
                  id="password"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  required
                />
                <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth="1.5" stroke="currentColor">
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    d="M3.98 8.223A10.477 10.477 0 001.934 12C3.226 16.338 7.244 19.5 12 19.5c.993 0 1.953-.138 2.863-.395M6.228 6.228A10.45 10.45 0 0112 4.5c4.756 0 8.773 3.162 10.065 7.498a10.523 10.523 0 01-4.293 5.774M6.228 6.228L3 3m3.228 3.228l3.65 3.65m7.894 7.894L21 21m-3.228-3.228l-3.65-3.65m0 0a3 3 0 10-4.243-4.243m4.242 4.242L9.88 9.88"
                  />
                </svg>
              </div>

              <div className="password__settings">
                <label className="password__settings__remember">
                  <input type="checkbox" />
                  <span className="custom__checkbox">
                    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth="2" stroke="currentColor" className="w-6 h-6">
                      <path strokeLinecap="round" strokeLinejoin="round" d="M4.5 12.75l6 6 9-13.5" />
                    </svg>
                  </span>
                  Nhớ tôi
                </label>
                <a href="#">Quên mật khẩu?</a>
              </div>

              {error && <p className="form__error" style={{ color: 'red' }}>{error}</p>}

              <button type="submit" className="form__submit" id="submit" disabled={isLoading}>
                {isLoading ? 'Đang đăng nhập...' : 'Đăng Nhập'}
              </button>

              {/* Divider OAuth */}
              <div className="flex items-center gap-3 my-4">
                <span className="flex-1 h-px bg-gray-200"></span>
                <p className="text-sm text-gray-500">hoặc</p>
                <span className="flex-1 h-px bg-gray-200"></span>
              </div>

              {/* Google Login */}
              <button
                type="button"
                onClick={handleGoogleLogin}
                className="flex items-center justify-center w-full h-12 gap-3 bg-white border border-gray-300 rounded-full shadow-sm hover:bg-gray-50 transition"
              >
                <svg viewBox="0 0 48 48" width="20" height="20" aria-hidden="true">
                  <path fill="#FFC107" d="M43.6 20.5H42V20H24v8h11.3C33.8 32.6 29.4 36 24 36c-6.6 0-12-5.4-12-12s5.4-12 12-12c3.1 0 5.9 1.2 8 3.1l5.7-5.7C34.6 6.1 29.6 4 24 4 16.1 4 9.2 8.3 6.3 14.7z"></path>
                  <path fill="#FF3D00" d="M6.3 14.7l6.6 4.8C14.7 16.1 19 13 24 13c3.1 0 5.9 1.2 8 3.1l5.7-5.7C34.6 6.1 29.6 4 24 4 16.1 4 9.2 8.3 6.3 14.7z"></path>
                  <path fill="#4CAF50" d="M24 44c5.3 0 10.1-2 13.7-5.2l-6.3-5.3C29.4 36 27 37 24 37c-5.3 0-9.7-3.4-11.3-8.1l-6.6 5.1C9.2 39.7 16.1 44 24 44z"></path>
                  <path fill="#1976D2" d="M43.6 20.5H42V20H24v8h11.3c-1 3.1-3.4 5.6-6.6 6.8l-6.3 5.3C37.5 38.3 40 32.7 40 26c0-1.9-.2-3.1-.4-5.5z"></path>
                </svg>
                <span className="text-sm font-medium text-gray-700">Đăng nhập bằng Google</span>
              </button>
            </form>

            <p className="form__footer">
              Chưa có tài khoản?<br />
              <Link href="/business/register">Tạo tài khoản</Link>
            </p>
          </section>

          {/* Khu vực animation */}
          <section className="form__animation">
            <div id="ball">
              <div className="ball">
                <div id="face">
                  <div className="ball__eyes">
                    <div className="eye_wrap"><span className="eye"></span></div>
                    <div className="eye_wrap"><span className="eye"></span></div>
                  </div>
                  <div className="ball__mouth"></div>
                </div>
              </div>
            </div>
            <div className="ball__shadow"></div>
          </section>
        </>
      )}
    </main>
  );
};

export default LoginPage;
