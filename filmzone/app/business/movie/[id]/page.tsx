'use client'

import Image from 'next/image'
import { useEffect, useMemo, useRef, useState } from 'react'
import { useParams, useRouter } from 'next/navigation'
import { motion } from 'framer-motion'
import { httpPublic } from '../../../lib/apiClient'
import {
    ArrowLeft, CalendarDays, Clock, Globe2, Tag, Users,
    AlertTriangle, Play, ExternalLink
} from 'lucide-react'

/* ================= Types khớp với BE ================= */
type WatchNowApiResponse = {
    errorCode: number
    errorMessage: string
    data: WatchNowMovie
}
type WatchNowMovie = {
    movieID: number
    slug: string
    title: string
    originalTitle?: string
    description?: string
    movieType: 'movie' | 'series'
    image: string
    status: 'ongoing' | 'completed' | 'coming_soon' | string
    releaseDate?: string
    durationSeconds?: number
    totalSeasons?: number | null
    totalEpisodes?: number | null
    year?: number | null
    rated?: string | null
    popularity?: number | null
    region?: { regionID: number; regionName: string }
    /** <-- thêm dòng này */
    tags?: { tagID: number; tagName: string; tagDescription?: string | null }[]
    /** ---- */
    sources?: { movieSourceID: number; movieID: number; sourceName: string }[] | any
    actors?: { fullName: string; avatar?: string | null; personID: number; role: string; characterName?: string | null; creditOrder?: number | null }[]
    images?: { movieImageID: number; imageUrl: string }[]
}


type MovieSourceDetailResp = {
    errorCode: number
    errorMessage: string
    data: MovieSourceDetail
}
type MovieSourceDetail = {
    movieSourceID: number
    movieID: number
    sourceName: string
    sourceType: 'archive' | 'youtube' | 'hls' | 'mp4' | string
    sourceUrl: string
    sourceID?: string
    quality?: string
    language?: string
    isVipOnly?: boolean
    isActive?: boolean
    createdAt?: string
    updatedAt?: string
}

/* ================ Cloudinary-safe helper (giữ origin) ================ */
const cldVariant = (raw?: string | null, opts: { ar?: `${number}:${number}` | string } = {}) => {
    if (!raw) return null
    try {
        const u = new URL(raw, 'http://dummy')
        if (!u.hostname.includes('res.cloudinary.com')) return raw
        const [head, tail] = u.pathname.split('/upload/')
        const trans = ['c_fill', 'g_auto', 'q_auto:good', 'f_auto', 'dpr_auto']
        if (opts.ar) trans.push(`ar_${opts.ar}`)
        u.pathname = `${head}/upload/${trans.join(',')}/${tail}`
        return `${u.origin}${u.pathname}${u.search}${u.hash}`
    } catch { return raw }
}

/* ===================== Helpers nhận diện nguồn ===================== */
const isYouTube = (url?: string) =>
    !!url && (url.includes('youtube.com') || url.includes('youtu.be'))

const toYouTubeEmbed = (url: string) => {
    try {
        const u = new URL(url)
        if (u.hostname.includes('youtu.be')) {
            const id = u.pathname.slice(1)
            return `https://www.youtube.com/embed/${id}`
        }
        const id = u.searchParams.get('v')
        return `https://www.youtube.com/embed/${id ?? ''}`
    } catch { return url }
}

const isHls = (url?: string) => !!url && url.endsWith('.m3u8')
const isMp4 = (url?: string) => !!url && url.endsWith('.mp4')

/* ========================= Component chính ========================= */
export default function WatchNowPage() {
    const params = useParams<{ id: string }>()
    const router = useRouter()
    const movieId = useMemo(() => Number(params.id), [params.id])

    const [data, setData] = useState<WatchNowMovie | null>(null)
    const [loading, setLoading] = useState(true)
    const [error, setError] = useState<string | null>(null)

    const [activeSourceId, setActiveSourceId] = useState<number | null>(null)
    const [source, setSource] = useState<MovieSourceDetail | null>(null)
    const [sourceLoading, setSourceLoading] = useState(false)
    const [sourceError, setSourceError] = useState<string | null>(null)

    // video ref (cho HLS)
    const videoRef = useRef<HTMLVideoElement>(null)

    /* --------- Load thông tin phim + mặc định source đầu tiên --------- */
    useEffect(() => {
        if (!movieId || Number.isNaN(movieId)) {
            setError('ID phim không hợp lệ')
            setLoading(false)
            return
        }
        let cancelled = false
        async function load() {
            try {
                setLoading(true)
                const res = await httpPublic.get<WatchNowApiResponse>(`/api/Movie/GetWatchNowMovieByID/watchNow/${movieId}`)
                if (cancelled) return

                // Chuẩn hoá sources: chấp nhận cả 'sources' và 'Sources'
                const raw: any = res.data.data
                const normalized: WatchNowMovie = {
                    ...raw,
                    sources: Array.isArray(raw?.sources) ? raw.sources
                        : Array.isArray(raw?.Sources) ? raw.Sources
                            : [],
                }
                console.log('[watchNow] movie normalized:', raw)
                setData(normalized)

                // mặc định chọn source đầu tiên
                const first = (normalized.sources as any[])?.[0]?.movieSourceID ?? null
                setActiveSourceId(first)
                setError(null)
            } catch (e: any) {
                setError(e?.message || 'Không tải được dữ liệu')
            } finally {
                if (!cancelled) setLoading(false)
            }
        }
        load()
        return () => { cancelled = true }
    }, [movieId])

    /* -------------- Khi đổi activeSourceId thì tải chi tiết source -------------- */
    useEffect(() => {
        let cancelled = false
        async function loadSource() {
            if (!activeSourceId) { setSource(null); return }
            try {
                setSourceLoading(true)
                const res = await httpPublic.get<MovieSourceDetailResp>(`/movie/MovieSource/GetMovieSourceById/${activeSourceId}`)
                if (cancelled) return

                // Chuẩn hoá key PascalCase/camelCase
                const raw: any = res.data.data
                const normalized: MovieSourceDetail = {
                    ...raw,
                    sourceType: (raw?.sourceType ?? raw?.SourceType ?? '').toString().toLowerCase(),
                    sourceUrl: (raw?.sourceUrl ?? raw?.SourceUrl ?? '').toString(),
                    sourceID: (raw?.sourceID ?? raw?.SourceID) as string | undefined,
                }
                console.log('[watchNow] source detail normalized:', normalized)
                setSource(normalized)
                setSourceError(null)
            } catch (e: any) {
                setSource(null)
                setSourceError(e?.message || 'Không tải được nguồn phát')
            } finally {
                if (!cancelled) setSourceLoading(false)
            }
        }
        loadSource()
        return () => { cancelled = true }
    }, [activeSourceId])

    /* ---------- Khởi tạo HLS nếu cần (chỉ khi sourceUrl là .m3u8) ---------- */
    useEffect(() => {
        const url = source?.sourceUrl
        if (!url || !isHls(url)) return
        const video = videoRef.current
        if (!video) return

        if (video.canPlayType('application/vnd.apple.mpegURL')) {
            video.src = url
            return
        }
        let hls: any
            ; (async () => {
                try {
                    const mod = await import('hls.js') // npm i hls.js
                    if (mod?.default?.isSupported()) {
                        hls = new mod.default()
                        hls.loadSource(url)
                        hls.attachMedia(video)
                    } else {
                        console.warn('HLS.js không hỗ trợ, dùng fallback link.')
                    }
                } catch (err) {
                    console.warn('Không thể import hls.js', err)
                }
            })()

        return () => {
            if (hls) {
                try { hls.destroy() } catch { }
            }
        }
    }, [source?.sourceUrl])

    const hero = useMemo(() => {
        const img = data?.image || '/backdrops/placeholder.jpg'
        return cldVariant(img, { ar: '21:9' }) || img
    }, [data])

    const PlayerBlock = () => {
        if (sourceLoading) {
            return (
                <div className="relative w-full overflow-hidden rounded-xl bg-black/60" style={{ aspectRatio: '16/9' }}>
                    <div className="absolute inset-0 grid place-items-center text-zinc-300">
                        <Play className="h-10 w-10 mb-2 animate-pulse" />
                        <p className="text-sm">Đang tải nguồn phát…</p>
                    </div>
                </div>
            )
        }
        if (sourceError || !source) {
            return (
                <div className="relative w-full overflow-hidden rounded-xl bg-black/60" style={{ aspectRatio: '16/9' }}>
                    <div className="absolute inset-0 grid place-items-center text-zinc-300">
                        <AlertTriangle className="h-6 w-6 mb-1" />
                        <p className="text-sm">{sourceError || 'Chưa có nguồn phát khả dụng'}</p>
                    </div>
                </div>
            )
        }

        const { sourceType, sourceUrl, sourceID } = source

        if (sourceType === 'archive' && sourceID) {
            const embed = `https://archive.org/embed/${sourceID}`
            return (
                <div className="relative w-full overflow-hidden rounded-xl bg-black" style={{ aspectRatio: '16/9' }}>
                    <iframe
                        src={embed}
                        className="h-full w-full"
                        allow="autoplay; fullscreen; picture-in-picture"
                        loading="lazy"
                    />
                </div>
            )
        }

        if (sourceType === 'youtube' || isYouTube(sourceUrl)) {
            const embed = toYouTubeEmbed(sourceUrl)
            return (
                <div className="relative w-full overflow-hidden rounded-xl bg-black" style={{ aspectRatio: '16/9' }}>
                    <iframe
                        src={embed}
                        className="h-full w-full"
                        allow="autoplay; encrypted-media; picture-in-picture"
                        allowFullScreen
                        loading="lazy"
                    />
                </div>
            )
        }

        if (isHls(sourceUrl)) {
            return (
                <div className="relative w-full overflow-hidden rounded-xl bg-black" style={{ aspectRatio: '16/9' }}>
                    <video ref={videoRef} controls playsInline className="h-full w-full rounded-xl bg-black" />
                    <div className="mt-2 flex items-center justify-between text-xs text-zinc-400">
                        <span>Nếu video không phát, bạn có thể mở trực tiếp:</span>
                        <a href={sourceUrl} target="_blank" rel="noreferrer" className="inline-flex items-center gap-1 hover:text-amber-400">
                            Mở nguồn gốc <ExternalLink className="h-3.5 w-3.5" />
                        </a>
                    </div>
                </div>
            )
        }

        if (isMp4(sourceUrl)) {
            return (
                <div className="relative w-full overflow-hidden rounded-xl bg-black" style={{ aspectRatio: '16/9' }}>
                    <video src={sourceUrl} controls playsInline className="h-full w-full rounded-xl bg-black" />
                </div>
            )
        }

        return (
            <div className="relative w-full overflow-hidden rounded-xl bg-black/60 p-6" style={{ aspectRatio: '16/9' }}>
                <div className="grid place-items-center text-zinc-300 h-full">
                    <p className="text-sm mb-3">Không thể nhúng nguồn này.</p>
                    <a href={sourceUrl} target="_blank" rel="noreferrer" className="inline-flex items-center gap-2 rounded-lg bg-amber-500 px-3 py-1.5 text-sm font-semibold text-zinc-900 ring-1 ring-amber-300/60 hover:bg-amber-400">
                        Mở nguồn gốc <ExternalLink className="h-4 w-4" />
                    </a>
                </div>
            </div>
        )
    }

    return (
        <div className="min-h-[80vh]">
            <div className="mx-auto max-w-7xl px-4 md:px-6 py-4">
                <button onClick={() => router.back()} className="inline-flex items-center gap-2 text-sm text-zinc-300 hover:text-white">
                    <ArrowLeft className="h-4 w-4" /> Quay lại
                </button>
            </div>

            {/* Hero */}
            <section className="relative isolate min-h-[50vh]">
                <div className="absolute inset-0">
                    <Image src={hero || '/backdrops/placeholder.jpg'} alt={data?.title || 'FilmZone'} fill priority sizes="100vw" className="object-cover" />
                </div>
                <div className="absolute inset-0 bg-gradient-to-b from-black/70 via-black/40 to-black/70" />
                <div className="relative z-10 mx-auto max-w-7xl px-4 md:px-6 py-10">
                    <motion.h1 initial={{ opacity: 0, y: 12 }} animate={{ opacity: 1, y: 0 }} transition={{ duration: 0.6 }} className="text-3xl md:text-5xl font-extrabold">
                        {data?.title ?? 'Đang tải…'}
                    </motion.h1>
                    {data?.originalTitle && (<p className="mt-1 text-zinc-300 italic">{data.originalTitle}</p>)}

                    <div className="mt-4 flex flex-wrap items-center gap-2 text-sm text-zinc-200">
                        {data?.year && (<span className="rounded bg-white/10 px-2 py-0.5 ring-1 ring-white/20 inline-flex items-center gap-1"><CalendarDays className="h-4 w-4" /> {data.year}</span>)}
                        {data?.durationSeconds && (<span className="rounded bg-white/10 px-2 py-0.5 ring-1 ring-white/20 inline-flex items-center gap-1"><Clock className="h-4 w-4" /> {Math.max(1, Math.round(data.durationSeconds / 60))} phút</span>)}
                        {data?.region && (<span className="rounded bg-white/10 px-2 py-0.5 ring-1 ring-white/20 inline-flex items-center gap-1"><Globe2 className="h-4 w-4" /> {data.region.regionName}</span>)}
                    </div>

                    {data?.description && (<p className="mt-4 max-w-3xl text-zinc-200">{data.description}</p>)}
                </div>
            </section>

            <div className="mx-auto max-w-7xl px-4 md:px-6 py-8">
                {/* Player */}
                <div className="rounded-2xl border border-white/10 bg-white/5 p-4">
                    <div className="flex items-center justify-between mb-3">
                        <h2 className="text-lg font-semibold">Xem ngay</h2>
                        {source && (
                            <div className="text-xs text-zinc-400">
                                {source.quality ? <span className="mr-2">Chất lượng: <b className="text-zinc-200">{source.quality}</b></span> : null}
                                {source.language ? <span>Ngôn ngữ: <b className="text-zinc-200">{source.language.toUpperCase()}</b></span> : null}
                            </div>
                        )}
                    </div>

                    <PlayerBlock />

                    {/* Danh sách nguồn ở DƯỚI player */}
                    <div className="mt-4 flex flex-wrap items-center gap-2">
                        <span className="text-xs text-zinc-400 mr-2">
                            {Array.isArray((data as any)?.sources) ? `${(data as any).sources.length} nguồn` : '0 nguồn'}
                        </span>

                        {(data as any)?.sources?.map((s: any) => (
                            <button
                                key={s.movieSourceID}
                                onClick={() => {
                                    console.log('[source click]', s)
                                    setActiveSourceId(s.movieSourceID)
                                }}
                                className={`rounded-lg px-3 py-1.5 text-sm ring-1 transition ${activeSourceId === s.movieSourceID
                                    ? 'bg-amber-500 text-zinc-900 ring-amber-300'
                                    : 'bg-white/5 text-zinc-200 ring-white/10 hover:bg-white/10'
                                    }`}
                                title={`Nguồn: ${s.sourceName}`}
                            >
                                {s.sourceName}
                            </button>
                        ))}

                        {!Array.isArray((data as any)?.sources) || !(data as any)?.sources?.length ? (
                            <span className="text-sm text-zinc-400">Chưa có nguồn phát.</span>
                        ) : null}
                    </div>
                </div>

                {/* Tags */}
                {data?.tags && data.tags.length > 0 && (
                    <div className="mt-8">
                        <h3 className="text-base font-semibold mb-2 inline-flex items-center gap-2"><Tag className="h-4 w-4" /> Thể loại</h3>
                        <div className="flex flex-wrap gap-2">
                            {data.tags.map(t => (
                                <span key={t.tagID} className="rounded-full bg-white/5 px-3 py-1 text-sm ring-1 ring-white/10">{t.tagName}</span>
                            ))}
                        </div>
                    </div>
                )}

                {/* Cast */}
                {data?.actors && data.actors.length > 0 && (
                    <div className="mt-8">
                        <h3 className="text-base font-semibold mb-3 inline-flex items-center gap-2"><Users className="h-4 w-4" /> Diễn viên & ekip</h3>
                        <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-5 gap-3">
                            {data.actors.map(a => (
                                <div key={`${a.personID}-${a.role}`} className="rounded-xl border border-white/10 bg-white/5 p-3">
                                    <div className="relative mb-2 h-36 w-full overflow-hidden rounded-lg">
                                        <Image src={cldVariant(a.avatar || undefined, { ar: '1:1' }) || '/avatars/placeholder.png'} alt={a.fullName} fill className="object-cover" />
                                    </div>
                                    <div className="text-sm font-semibold line-clamp-1">{a.fullName}</div>
                                    <div className="text-xs text-zinc-400 line-clamp-1">{a.role}{a.characterName ? ` • ${a.characterName}` : ''}</div>
                                </div>
                            ))}
                        </div>
                    </div>
                )}

                {/* Gallery images */}
                {data?.images && data.images.length > 0 && (
                    <div className="mt-8">
                        <h3 className="text-base font-semibold mb-3">Hình ảnh</h3>
                        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
                            {data.images.map(img => {
                                const src = cldVariant(img.imageUrl, { ar: '16:9' }) || img.imageUrl
                                return (
                                    <div key={img.movieImageID} className="relative h-32 w-full overflow-hidden rounded-xl ring-1 ring-white/10">
                                        <Image src={src} alt={`image-${img.movieImageID}`} fill className="object-cover" />
                                    </div>
                                )
                            })}
                        </div>
                    </div>
                )}

                {/* Error */}
                {error && (
                    <div className="mt-6 bg-red-500/10 border border-red-500/30 text-red-200 rounded-xl px-4 py-3 flex items-center gap-2">
                        <AlertTriangle className="h-4 w-4" />
                        <span>{error}</span>
                    </div>
                )}
            </div>
        </div>
    )
}
