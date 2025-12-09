'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import QRCode from 'qrcode';
import { http } from '../../lib/apiClient';

type StartTotpResp = {
    errorCode: number;
    errorMessage: string;
    data?: {
        secretBase32: string;
        otpauthUri: string;
        label: string;
    };
};

type ConfirmTotpResp = {
    errorCode: number;
    errorMessage: string;
    data?: boolean; // true nếu bật thành công
};

export default function MfaQrPage() {
    const router = useRouter();

    const [dataUrl, setDataUrl] = useState<string>('');
    const [error, setError] = useState<string>('');
    const [otp, setOtp] = useState<string>('');
    const [loading, setLoading] = useState<boolean>(true);
    const [submitting, setSubmitting] = useState<boolean>(false);

    useEffect(() => {
        const controller = new AbortController();

        (async () => {
            try {
                // Khởi tạo TOTP
                const res = await http.post<StartTotpResp>(
                    '/account/mfa/totp/start',
                    {},
                    { signal: controller.signal }
                );

                if (res.data?.errorCode !== 200) {
                    throw new Error(res.data?.errorMessage || 'Start TOTP failed');
                }

                const otpauthUri = res.data?.data?.otpauthUri;
                if (!otpauthUri) throw new Error('Missing otpauthUri from API');

                // Render QR từ otpauthUri
                const url = await QRCode.toDataURL(otpauthUri);
                setDataUrl(url);
            } catch (e: any) {
                const msg =
                    e?.response?.data?.errorMessage ||
                    e?.message ||
                    'Unexpected error';
                setError(msg);
            } finally {
                setLoading(false);
            }
        })();

        return () => controller.abort();
    }, []);

    async function handleSubmit(e: React.FormEvent) {
        e.preventDefault();
        if (!otp || otp.trim().length < 6) return;

        setSubmitting(true);
        setError('');

        try {
            const res = await http.post<ConfirmTotpResp>(
                '/account/mfa/totp/confirm',
                { code: otp.trim() }
            );

            if (res.data?.errorCode === 200 && res.data?.data === true) {
                // Bật MFA thành công -> về trang business
                router.push('/business/mainScreen');
                return;
            }
            throw new Error(res.data?.errorMessage || 'Xác nhận MFA thất bại');
        } catch (e: any) {
            const msg =
                e?.response?.data?.errorMessage ||
                e?.message ||
                'Unexpected error';
            setError(msg);
        } finally {
            setSubmitting(false);
        }
    }

    return (
        <div className="min-h-screen flex flex-col items-center justify-center gap-6 p-8">
            <h1 className="text-xl font-semibold">Liên kết Google Authenticator</h1>

            {loading ? (
                <div className="opacity-70">Đang tạo mã QR...</div>
            ) : error ? (
                <div className="p-3 rounded bg-red-50 text-red-700 border border-red-200">
                    Lỗi: {error}
                </div>
            ) : (
                <>
                    {dataUrl && (
                        <img
                            src={dataUrl}
                            alt="Scan with Google Authenticator"
                            className="w-64 h-64 rounded-xl shadow"
                        />
                    )}

                    <form
                        onSubmit={handleSubmit}
                        className="w-full max-w-xs flex flex-col items-stretch gap-3"
                    >
                        <label className="text-sm text-gray-700">
                            Nhập mã OTP 6 số từ ứng dụng
                        </label>
                        <input
                            value={otp}
                            onChange={(e) =>
                                setOtp(e.target.value.replace(/\D/g, '').slice(0, 6))
                            }
                            inputMode="numeric"
                            pattern="\d*"
                            maxLength={6}
                            placeholder="••••••"
                            className="px-4 py-2 border rounded-lg outline-none focus:ring-2 focus:ring-blue-500"
                            autoFocus
                        />
                        <button
                            type="submit"
                            disabled={submitting || otp.length !== 6}
                            className="px-4 py-2 rounded-lg bg-blue-600 text-white font-semibold disabled:opacity-50"
                        >
                            {submitting ? 'Đang xác nhận…' : 'OK'}
                        </button>
                    </form>
                </>
            )}
        </div>
    );
}
