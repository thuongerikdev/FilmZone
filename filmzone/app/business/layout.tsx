// ==============================
// app/(site)/layout.tsx
// ==============================
'use client'

import React, { useState } from 'react'
import Link from 'next/link'
import { useRouter } from 'next/navigation'
import { Be_Vietnam_Pro } from 'next/font/google'
import { Film, Search, LogOut, Play, Clapperboard } from 'lucide-react'
import { http, clearAccessToken } from '../lib/apiClient' // giữ nguyên nếu bạn đã có

const beVietnam = Be_Vietnam_Pro({
    subsets: ['vietnamese', 'latin'],
    weight: ['400', '500', '600', '700', '800'],
    display: 'swap',
})

export default function MovieLayout({ children }: { children: React.ReactNode }) {
    const router = useRouter()
    const [loggingOut, setLoggingOut] = useState(false)

    const handleLogout = async () => {
        setLoggingOut(true)
        try {
            await http.post('/login/logout', {}, { skipAuth: true })
        } catch { /* ignore */ }
        finally {
            clearAccessToken()
            router.replace('/login?loggedout=1')
        }
    }

    return (
        <div className={`${beVietnam.className} antialiased flex min-h-screen flex-col bg-zinc-950 text-zinc-50 selection:bg-amber-200 selection:text-zinc-900`}>
            {/* ===== Header ===== */}
            <header className="sticky top-0 z-40 w-full border-b border-white/10 bg-zinc-950/70 backdrop-blur-xl supports-[backdrop-filter]:bg-zinc-950/60">
                <nav aria-label="Primary" className="mx-auto flex h-16 max-w-7xl items-center justify-between px-4 md:px-6">
                    <Link href="/" className="group inline-flex items-center gap-2">
                        <span className="grid h-9 w-9 place-items-center rounded-xl bg-amber-500 text-zinc-900 shadow-sm ring-1 ring-amber-300/50 transition-transform group-hover:scale-95">
                            <Film className="h-5 w-5" />
                        </span>
                        <span className="text-lg font-extrabold tracking-tight">
                            Film<span className="text-amber-400">Zone</span>
                        </span>
                    </Link>

                    <div className="hidden items-center gap-6 md:flex">
                        <NavLink href="/#trending">Xu hướng</NavLink>
                        <NavLink href="/#categories">Thể loại</NavLink>
                        <NavLink href="/#new">Mới phát hành</NavLink>
                        <NavLink href="/my-list">Danh sách của tôi</NavLink>
                    </div>

                    <div className="flex items-center gap-2">
                        <Link
                            href="/search"
                            className="inline-flex items-center gap-2 rounded-xl border border-white/15 bg-white/5 px-3 py-2 text-sm font-medium text-zinc-100 shadow-sm transition hover:bg-white/10 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-amber-400/40"
                            title="Tìm kiếm"
                        >
                            <Search className="h-4 w-4" />
                            <span className="hidden sm:inline">Tìm kiếm</span>
                        </Link>

                        <button
                            onClick={handleLogout}
                            disabled={loggingOut}
                            className="inline-flex items-center gap-2 rounded-xl border border-white/15 bg-white/5 px-3 py-2 text-sm font-medium text-zinc-100 shadow-sm transition hover:bg-white/10 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-amber-400/40"
                            title="Đăng xuất"
                        >
                            {loggingOut ? 'Đang đăng xuất…' : <><LogOut className="h-4 w-4" /><span className="hidden sm:inline">Đăng xuất</span></>}
                        </button>

                        <Link
                            href="/watch/now"
                            className="inline-flex items-center gap-2 rounded-xl bg-amber-500 px-4 py-2 text-sm font-semibold text-zinc-900 shadow-sm ring-1 ring-amber-300/60 transition hover:bg-amber-400 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-amber-400/60"
                        >
                            <Play className="h-4 w-4" /> Xem ngay
                        </Link>
                    </div>
                </nav>
            </header>

            {/* ===== Content ===== */}
            <div className="flex-1">
                {children}
            </div>

            {/* ===== Footer ===== */}
            <footer className="border-t border-white/10 bg-zinc-950 py-10">
                <div className="mx-auto max-w-7xl px-4 md:px-6">
                    <div className="grid gap-8 md:grid-cols-4">
                        <div>
                            <div className="mb-3 inline-flex items-center gap-2">
                                <span className="grid h-9 w-9 place-items-center rounded-xl bg-amber-500 text-zinc-900 ring-1 ring-amber-300">
                                    <Clapperboard className="h-5 w-5" />
                                </span>
                                <span className="text-lg font-extrabold tracking-tight">
                                    Film<span className="text-amber-400">Zone</span>
                                </span>
                            </div>
                            <p className="text-sm text-zinc-400">Nền tảng xem phim trực tuyến. Trải nghiệm nhanh, chất lượng ổn định.</p>
                        </div>

                        <div>
                            <p className="mb-3 font-semibold">Khám phá</p>
                            <ul className="space-y-2 text-sm text-zinc-400">
                                <li><FooterLink href="/movies">Phim lẻ</FooterLink></li>
                                <li><FooterLink href="/series">Phim bộ</FooterLink></li>
                                <li><FooterLink href="/genres">Thể loại</FooterLink></li>
                                <li><FooterLink href="/top">Bảng xếp hạng</FooterLink></li>
                            </ul>
                        </div>

                        <div>
                            <p className="mb-3 font-semibold">Hỗ trợ</p>
                            <ul className="space-y-2 text-sm text-zinc-400">
                                <li><FooterLink href="/about">Giới thiệu</FooterLink></li>
                                <li><FooterLink href="/contact">Liên hệ</FooterLink></li>
                                <li><FooterLink href="/terms">Điều khoản</FooterLink></li>
                                <li><FooterLink href="/privacy">Bảo mật</FooterLink></li>
                            </ul>
                        </div>

                        <div>
                            <p className="mb-3 font-semibold">Bắt đầu xem</p>
                            <Link
                                href="/signup"
                                className="inline-flex items-center justify-center rounded-2xl bg-amber-500 px-5 py-3 font-semibold text-zinc-900 shadow-sm ring-1 ring-amber-300/60 transition hover:bg-amber-400"
                            >
                                Tạo tài khoản
                            </Link>
                        </div>
                    </div>

                    <div className="mt-10 border-t border-white/10 pt-6 text-center text-sm text-zinc-500">
                        © {new Date().getFullYear()} FilmZone. All rights reserved.
                    </div>
                </div>
            </footer>
        </div>
    )
}

function NavLink({ href, children }: { href: string; children: React.ReactNode }) {
    return (
        <Link
            href={href}
            className="rounded text-sm font-medium text-zinc-300 transition hover:text-amber-400 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-amber-400/40"
        >
            {children}
        </Link>
    )
}
function FooterLink({ href, children }: { href: string; children: React.ReactNode }) {
    return (
        <Link
            href={href}
            className="rounded transition hover:text-amber-400 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-amber-400/30"
        >
            {children}
        </Link>
    )
}
