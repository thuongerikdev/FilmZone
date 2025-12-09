'use client';

import { useState, useEffect } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import { http, httpPublic, setAccessToken } from '../../lib/apiClient';

type MfaVerifyResp = {
    errorCode: number;
    errorMessage: string;
    data?: {
        token?: string | null;
        refreshToken?: string | null;
        tokenExpiration?: string;
        refreshTokenExpiration?: string;
    };
};

export default function MfaVerifyPage() {
    const router = useRouter();
    const search = useSearchParams();
    const mfaTicket = search.get('ticket') || ''; // ticket lấy từ query: /mfa/verify?ticket=...

    const [otp, setOtp] = useState('');
    const [error, setError] = useState('');
    const [submitting, setSubmitting] = useState(false);

    useEffect(() => {
        if (!mfaTicket) setError('Thiếu mfaTicket. Vui lòng đăng nhập lại.');
    }, [mfaTicket]);

    async function handleSubmit(e: React.FormEvent) {
        e.preventDefault();
        if (!mfaTicket) return;
        if (otp.trim().length !== 6) {
            setError('Mã OTP phải gồm 6 số.');
            return;
        }

        setSubmitting(true);
        setError('');

        try {
            // ✅ Gửi đúng schema: { mfaTicket, code }
            const res = await http.post<MfaVerifyResp>('/login/mfa/verify', {
                mfaTicket,
                code: otp.trim(),
            });

            if (res.data?.errorCode !== 200) {
                throw new Error(res.data?.errorMessage || 'Xác thực MFA thất bại.');
            }

            // Nếu backend trả access token thì lưu vào RAM để dùng ngay
            const tk = res.data?.data?.token ?? null;
            if (tk) {
                setAccessToken(tk);
            } else {
                // Nếu không trả, thử refresh để lấy token qua refresh-cookie HttpOnly
                try {
                    const rf = await httpPublic.post<{ token?: string }>('/login/auth/refresh', {});
                    if (rf.data?.token) setAccessToken(rf.data.token);
                } catch {
                    /* ignore */
                }
            }

            // Thành công -> điều hướng
            router.push('/business/mainScreen');
        } catch (err: any) {
            const msg =
                err?.response?.data?.errorMessage ||
                err?.message ||
                'Có lỗi khi xác thực MFA.';
            setError(msg);
        } finally {
            setSubmitting(false);
        }
    }

    return (
        <div className="min-h-screen flex items-center justify-center p-6">
            <div className="w-full max-w-sm space-y-5">
                <h1 className="text-2xl font-semibold text-center">Xác thực MFA</h1>
                <p className="text-sm text-gray-600 text-center">
                    Nhập mã 6 số từ ứng dụng Google Authenticator.
                </p>

                {error && (
                    <div className="p-3 rounded border border-red-200 bg-red-50 text-red-700 text-sm">
                        {error}
                    </div>
                )}

                <form onSubmit={handleSubmit} className="space-y-3">
                    <input
                        value={otp}
                        onChange={(e) =>
                            setOtp(e.target.value.replace(/\D/g, '').slice(0, 6))
                        }
                        inputMode="numeric"
                        pattern="\d*"
                        maxLength={6}
                        placeholder="••••••"
                        autoComplete="one-time-code"
                        className="w-full px-4 py-2 border rounded-lg outline-none focus:ring-2 focus:ring-blue-500"
                        autoFocus
                    />

                    <button
                        type="submit"
                        disabled={!mfaTicket || submitting || otp.length !== 6}
                        className="w-full px-4 py-2 rounded-lg bg-blue-600 text-white font-semibold disabled:opacity-50"
                    >
                        {submitting ? 'Đang xác thực…' : 'Xác nhận'}
                    </button>
                </form>
            </div>
        </div>
    );
}
