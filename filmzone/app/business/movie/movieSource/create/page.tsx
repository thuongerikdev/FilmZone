'use client'

import React, { useEffect, useMemo, useRef, useState } from 'react'
import { http } from '@/app/lib/apiClient'
import type { AxiosError } from 'axios'
import * as signalR from '@microsoft/signalr'

type Provider = 'archive-file' | 'archive-link' | 'youtube-file'

function clampPct(n: number | undefined | null) {
    if (typeof n !== 'number' || Number.isNaN(n)) return 0
    if (n < 0) return 0
    if (n > 100) return 100
    return Math.floor(n)
}

function ProgressBar({ label, percent, hint }: { label: string; percent: number; hint?: string }) {
    const p = clampPct(percent)
    return (
        <div className="w-full">
            <div className="mb-1 flex items-end justify-between">
                <span className="text-xs text-neutral-300">{label}</span>
                <span className="text-xs tabular-nums text-neutral-300">{p}%</span>
            </div>
            <div className="h-3 w-full overflow-hidden rounded-full bg-neutral-800">
                <div
                    className="h-full bg-blue-600 transition-[width] duration-300"
                    style={{ width: `${p}%` }}
                    aria-valuemin={0}
                    aria-valuemax={100}
                    aria-valuenow={p}
                    role="progressbar"
                />
            </div>
            {hint ? <div className="mt-1 text-[11px] text-neutral-400">{hint}</div> : null}
        </div>
    )
}

export default function UploadPage() {
    // ====== provider & common meta ======
    const [provider, setProvider] = useState<Provider>('archive-file')

    const [scope, setScope] = useState<'movie' | 'episode'>('movie')
    const [targetId, setTargetId] = useState<number | ''>('')
    const [quality, setQuality] = useState('1080p')
    const [language, setLanguage] = useState('vi')
    const [isVipOnly, setIsVipOnly] = useState(false)
    const [isActive, setIsActive] = useState(true)

    // ====== file/link state ======
    const [file, setFile] = useState<File | null>(null)
    const [linkUrl, setLinkUrl] = useState('')
    const fileInputRef = useRef<HTMLInputElement | null>(null)

    // ====== runtime / feedback ======
    const [submitting, setSubmitting] = useState(false)
    const [errorMsg, setErrorMsg] = useState<string | null>(null)
    const [successMsg, setSuccessMsg] = useState<string | null>(null)

    const [clientPct, setClientPct] = useState(0)
    const [serverPct, setServerPct] = useState(0)

    const [serverProgress, setServerProgress] = useState<
        Array<{ ts: number; type: 'progress' | 'completed' | 'error' | 'info'; text: string; percent?: number }>
    >([])

    const hubRef = useRef<signalR.HubConnection | null>(null)

    const endpointBase = http.defaults.baseURL ?? ''
    const uploadLabel = useMemo(() => {
        if (provider === 'archive-file') return `POST ${endpointBase}/api/upload/archive/file`
        if (provider === 'archive-link') return `POST ${endpointBase}/api/upload/archive/link`
        return `POST ${endpointBase}/api/upload/youtube/file`
    }, [provider, endpointBase])

    function pushLog(entry: { type: 'progress' | 'completed' | 'error' | 'info'; text: string; percent?: number }) {
        setServerProgress((prev) => [...prev, { ts: Date.now(), ...entry }])
    }

    async function connectHubAndJoin(jobId: string) {
        if (hubRef.current) {
            try {
                await hubRef.current.stop()
            } catch { }
            hubRef.current = null
        }

        const base = http.defaults.baseURL ?? ''
        const hubUrl = `${base}/hubs/upload`

        const conn = new signalR.HubConnectionBuilder().withUrl(hubUrl, { withCredentials: true }).withAutomaticReconnect().build()

        conn.on('upload.progress', (payload: any) => {
            const pct = clampPct(payload?.percent)
            const text = payload?.text ?? 'Uploading...'
            setServerPct(pct)
            pushLog({ type: 'progress', text, percent: pct })
        })

        conn.on('upload.done', (payload: any) => {
            setServerPct(100)
            const text = payload?.playerUrl ? `Done. Player: ${payload.playerUrl}` : 'Done.'
            pushLog({ type: 'completed', text, percent: 100 })
            setSuccessMsg(text)
        })

        conn.on('upload.error', (payload: any) => {
            const text = payload?.error || 'Server error'
            pushLog({ type: 'error', text })
            setErrorMsg(text)
        })

        await conn.start()
        await conn.invoke('JoinJob', jobId)
        hubRef.current = conn
    }

    useEffect(() => {
        return () => {
            if (hubRef.current) hubRef.current.stop().catch(() => { })
        }
    }, [])

    useEffect(() => {
        if (provider === 'archive-link') {
            setFile(null)
            if (fileInputRef.current) fileInputRef.current.value = ''
        } else {
            setLinkUrl('')
        }
        setClientPct(0)
        setServerPct(0)
        setServerProgress([])
    }, [provider])

    async function onSubmit(e: React.FormEvent) {
        e.preventDefault()
        setSubmitting(true)
        setErrorMsg(null)
        setSuccessMsg(null)
        setServerProgress([])
        setClientPct(0)
        setServerPct(0)

        if (provider !== 'archive-link' && !file) {
            setErrorMsg('Vui lòng chọn file.')
            setSubmitting(false)
            return
        }
        if (provider === 'archive-link' && !linkUrl.trim()) {
            setErrorMsg('Vui lòng nhập Link URL.')
            setSubmitting(false)
            return
        }
        if (targetId === '') {
            setErrorMsg('Vui lòng nhập Target ID.')
            setSubmitting(false)
            return
        }

        try {
            if (provider === 'archive-link') {
                const body = {
                    Scope: scope,
                    TargetId: Number(targetId),
                    Quality: quality,
                    Language: language,
                    IsVipOnly: isVipOnly,
                    IsActive: isActive,
                    LinkUrl: linkUrl.trim(),
                }
                const res = await http.post('/api/upload/archive/link', body, {
                    timeout: 0,
                    headers: { 'Content-Type': 'application/json' },
                })
                const jobId = res.data?.jobId as string
                pushLog({ type: 'info', text: `Job created: ${jobId}` })
                await connectHubAndJoin(jobId)
            } else {
                const fd = new FormData()
                fd.append('Scope', scope)
                fd.append('TargetId', String(targetId))
                fd.append('Quality', quality)
                fd.append('Language', language)
                fd.append('IsVipOnly', String(isVipOnly))
                fd.append('IsActive', String(isActive))
                if (file) fd.append('File', file)

                const url = provider === 'archive-file' ? '/api/upload/archive/file' : '/api/upload/youtube/file'

                const res = await http.post(url, fd, {
                    timeout: 0,
                    onUploadProgress: (pe) => {
                        if (!pe.total) return
                        const pct = Math.floor((pe.loaded * 100) / pe.total)
                        setClientPct(pct)
                        pushLog({ type: 'progress', text: `Browser uploading... ${pct}%`, percent: pct })
                    },
                })

                const jobId = res.data?.jobId as string
                pushLog({ type: 'info', text: `Job created: ${jobId}` })
                await connectHubAndJoin(jobId)
            }
        } catch (err: any) {
            const ax = err as AxiosError
            const msg =
                ax?.response?.data && typeof ax.response.data === 'object'
                    ? JSON.stringify(ax.response.data)
                    : (ax?.response?.data as string) || ax?.message || 'Request failed'
            setErrorMsg(msg)
            pushLog({ type: 'error', text: msg })
        } finally {
            setSubmitting(false)
        }
    }

    function fillDemo() {
        setScope('movie')
        setTargetId(1)
        setQuality('1080p')
        setLanguage('vi')
        setIsVipOnly(false)
        setIsActive(true)
        setLinkUrl('https://example.com/video.mp4')
        setFile(null)
        if (fileInputRef.current) fileInputRef.current.value = ''
        setServerProgress([])
        setClientPct(0)
        setServerPct(0)
        setErrorMsg(null)
        setSuccessMsg(null)
    }

    return (
        <div className="mx-auto max-w-4xl p-6 bg-neutral-950 text-neutral-100">
            <div className="mb-6 flex items-center gap-3">
                <h1 className="text-2xl font-bold">Upload Video</h1>
                <button type="button" onClick={fillDemo} className="rounded-xl border border-neutral-700 hover:bg-neutral-800 px-3 py-1.5" title="Điền mẫu nhanh">
                    Điền mẫu
                </button>
            </div>

            <form onSubmit={onSubmit} className="space-y-8">
                {/* Provider */}
                <section className="space-y-2">
                    <h2 className="text-lg font-semibold">Chọn nhà cung cấp</h2>
                    <div className="grid grid-cols-1 gap-2 md:grid-cols-3">
                        <label className="flex items-center gap-2 rounded-xl border border-neutral-700 bg-neutral-900 p-3">
                            <input
                                type="radio"
                                name="provider"
                                value="archive-file"
                                checked={provider === 'archive-file'}
                                onChange={() => setProvider('archive-file')}
                                className="accent-blue-600"
                            />
                            Archive (file)
                        </label>
                        <label className="flex items-center gap-2 rounded-xl border border-neutral-700 bg-neutral-900 p-3">
                            <input
                                type="radio"
                                name="provider"
                                value="archive-link"
                                checked={provider === 'archive-link'}
                                onChange={() => setProvider('archive-link')}
                                className="accent-blue-600"
                            />
                            Archive (link)
                        </label>
                        <label className="flex items-center gap-2 rounded-xl border border-neutral-700 bg-neutral-900 p-3">
                            <input
                                type="radio"
                                name="provider"
                                value="youtube-file"
                                checked={provider === 'youtube-file'}
                                onChange={() => setProvider('youtube-file')}
                                className="accent-blue-600"
                            />
                            YouTube (file)
                        </label>
                    </div>
                    <p className="text-xs text-neutral-400">Endpoint: {uploadLabel}</p>
                </section>

                {/* Meta */}
                <section className="grid grid-cols-1 gap-4 md:grid-cols-3">
                    <div>
                        <label className="block text-sm font-medium">Scope</label>
                        <select
                            className="mt-1 w-full rounded-xl border border-neutral-700 bg-neutral-900 text-neutral-100 p-2"
                            value={scope}
                            onChange={(e) => setScope(e.target.value as 'movie' | 'episode')}
                        >
                            <option value="movie">movie</option>
                            <option value="episode">episode</option>
                        </select>
                    </div>
                    <div>
                        <label className="block text-sm font-medium">Target ID</label>
                        <input
                            className="mt-1 w-full rounded-xl border border-neutral-700 bg-neutral-900 text-neutral-100 p-2"
                            type="number"
                            min={1}
                            value={targetId}
                            onChange={(e) => setTargetId(e.target.value === '' ? '' : Number(e.target.value))}
                            placeholder="VD: movieID/episodeID"
                        />
                    </div>
                    <div>
                        <label className="block text-sm font-medium">Quality</label>
                        <input className="mt-1 w-full rounded-xl border border-neutral-700 bg-neutral-900 text-neutral-100 p-2" value={quality} onChange={(e) => setQuality(e.target.value)} />
                    </div>
                    <div>
                        <label className="block text-sm font-medium">Language</label>
                        <input className="mt-1 w-full rounded-xl border border-neutral-700 bg-neutral-900 text-neutral-100 p-2" value={language} onChange={(e) => setLanguage(e.target.value)} />
                    </div>
                    <div className="flex items-center gap-2">
                        <input id="vip" type="checkbox" checked={isVipOnly} onChange={(e) => setIsVipOnly(e.target.checked)} className="accent-blue-600" />
                        <label htmlFor="vip" className="text-sm">
                            VIP only
                        </label>
                    </div>
                    <div className="flex items-center gap-2">
                        <input id="active" type="checkbox" checked={isActive} onChange={(e) => setIsActive(e.target.checked)} className="accent-blue-600" />
                        <label htmlFor="active" className="text-sm">
                            Active
                        </label>
                    </div>
                </section>

                {/* File/Link */}
                {provider === 'archive-link' ? (
                    <section className="space-y-2">
                        <label className="block text-sm font-medium">Link URL</label>
                        <input
                            className="w-full rounded-xl border border-neutral-700 bg-neutral-900 text-neutral-100 p-2"
                            value={linkUrl}
                            onChange={(e) => setLinkUrl(e.target.value)}
                            placeholder="https://example.com/video.mp4"
                        />
                    </section>
                ) : (
                    <section className="space-y-2">
                        <label className="block text-sm font-medium">File</label>
                        <input
                            key={`file-${provider}`}
                            ref={fileInputRef}
                            className="w-full rounded-xl border border-neutral-700 bg-neutral-900 text-neutral-100 p-2"
                            type="file"
                            accept="video/*"
                            onChange={(e) => setFile(e.target.files?.[0] ?? null)}
                        />
                        {file && <p className="text-xs text-neutral-400">{file.name} ({file.size.toLocaleString()} bytes)</p>}
                    </section>
                )}

                {/* Progress */}
                <section className="space-y-4">
                    <h3 className="text-lg font-semibold">Tiến trình</h3>
                    <ProgressBar label="Trình duyệt → Server" percent={clientPct} hint={clientPct > 0 && clientPct < 100 ? 'Đang tải lên server...' : clientPct === 100 ? 'Đã gửi lên server' : ''} />
                    <ProgressBar label="Server → Vendor" percent={serverPct} hint={serverPct > 0 && serverPct < 100 ? 'Server đang đẩy sang vendor...' : serverPct === 100 ? 'Vendor đang xử lý...' : ''} />
                </section>

                <div className="flex items-center gap-3">
                    <button type="submit" disabled={submitting} className="rounded-xl bg-blue-600 hover:bg-blue-700 px-5 py-2 text-white disabled:opacity-50">
                        {submitting ? 'Submitting…' : 'Start Upload'}
                    </button>
                </div>
            </form>

            {/* Alerts */}
            <section className="mt-6">
                {errorMsg && (
                    <div className="mb-4 rounded-xl border border-red-500/50 bg-red-950/40 p-4 text-red-300">
                        <div className="font-semibold">Error</div>
                        <div className="whitespace-pre-wrap text-sm">{String(errorMsg)}</div>
                    </div>
                )}
                {successMsg && (
                    <div className="mb-4 rounded-xl border border-green-500/50 bg-green-950/40 p-4 text-green-300">
                        <div className="font-semibold">Success</div>
                        <div className="whitespace-pre-wrap text-sm">{String(successMsg)}</div>
                    </div>
                )}
            </section>

            {/* Log */}
            <section className="mt-4">
                <details className="rounded-xl border border-neutral-700">
                    <summary className="cursor-pointer select-none p-3 text-sm font-semibold">Chi tiết log</summary>
                    <div className="divide-y divide-neutral-800">
                        {serverProgress.length === 0 && <div className="p-3 text-sm text-neutral-400">Chưa có log</div>}
                        {serverProgress.map((l, i) => (
                            <div key={l.ts + '-' + i} className="flex items-center gap-3 p-3 text-sm text-neutral-300">
                                {typeof l.percent === 'number' ? (
                                    <span className="inline-block w-16 tabular-nums">{clampPct(l.percent)}%</span>
                                ) : (
                                    <span className="inline-block w-16 text-neutral-500">—</span>
                                )}
                                <span className={l.type === 'error' ? 'text-red-400' : l.type === 'completed' ? 'text-green-400' : ''}>
                                    {l.text}
                                </span>
                            </div>
                        ))}
                    </div>
                </details>
            </section>
        </div>
    )
}
