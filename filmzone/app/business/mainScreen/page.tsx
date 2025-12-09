// app/(site)/page.tsx
// ==============================
'use client'

import Image from 'next/image'
import Link from 'next/link'
import { useEffect, useMemo, useState } from 'react'
import { motion } from 'framer-motion'
import { Play, Info, Flame, CalendarDays, AlertTriangle } from 'lucide-react'
import { httpPublic } from '../../lib/apiClient'

/* Types */
type ApiMovie = {
  movieID: number
  slug: string
  title: string
  originalTitle?: string
  description?: string
  movieType?: string
  image?: string
}

// UI movie item (mapped từ API)
type MovieItem = {
  id: number
  title: string
  overview?: string
  poster: string
  backdrop: string
  year?: number
  rating?: number
  quality?: 'CAM' | 'HD' | 'FullHD' | '4K'
  slug?: string
}

type RowProps = {
  title: string
  items: MovieItem[]
  variant?: 'poster' | 'landscape'
}

/** Ensure a URL is absolute (thêm protocol/host khi thiếu) */
const ensureAbsolute = (raw?: string | null) => {
  if (!raw) return null
  const s = String(raw).trim()
  // //res.cloudinary.com/...
  if (s.startsWith('//')) return 'https:' + s
  // http(s)://...
  if (/^https?:\/\//i.test(s)) return s
  // res.cloudinary.com/...
  if (/^res\.cloudinary\.com\//i.test(s)) return 'https://' + s
  // các đường dẫn local (/posters/..., /backdrops/...) giữ nguyên
  return s
}

/** Cloudinary-safe helper (hoạt động với ảnh local + external) */
const cldVariant = (
  raw?: string | null,
  opts: { ar?: `${number}:${number}` | string } = {}
) => {
  if (!raw) return null
  const abs = ensureAbsolute(raw)
  if (!abs) return raw

  try {
    const u = new URL(abs)
    // không phải Cloudinary → trả FULL url nguyên vẹn
    if (!/(^|\.)res\.cloudinary\.com$/i.test(u.hostname)) return u.toString()

    // chèn transformation sau /upload/
    const parts = u.pathname.split('/upload/')
    if (parts.length !== 2) return u.toString()

    const trans = ['c_fill', 'g_auto', 'q_auto:good', 'f_auto', 'dpr_auto']
    if (opts.ar) trans.push(`ar_${opts.ar}`)
    u.pathname = `${parts[0]}/upload/${trans.join(',')}/${parts[1]}`
    return u.toString() // 🔑 quan trọng: trả về FULL url
  } catch {
    return abs
  }
}

// Map dữ liệu API -> UI MovieItem
function mapApiMovie(m: ApiMovie): MovieItem {
  const img = m.image || '/posters/placeholder.jpg'
  const safe = ensureAbsolute(img) ?? img
  return {
    id: m.movieID,
    slug: m.slug,
    title: m.title || m.originalTitle || 'Untitled',
    overview: m.description,
    poster: safe,
    backdrop: safe,
    // có thể bổ sung year/rating/quality khi BE sẵn sàng
  }
}

export default function Page() {
  const [heroReady, setHeroReady] = useState(false)
  const [trending, setTrending] = useState<MovieItem[]>([])
  const [newReleases, setNewReleases] = useState<MovieItem[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  // Fetch 2 API public (skipAuth)
  useEffect(() => {
    let cancelled = false
    async function load() {
      try {
        setLoading(true)
        const [tRes, nRes] = await Promise.all([
          httpPublic.get<{ errorCode: number; data: ApiMovie[] }>(
            '/api/Movie/GetAllMoviesMainScreen/mainScreen'
          ),
          httpPublic.get<{ errorCode: number; data: ApiMovie[] }>(
            '/api/Movie/GetAllMoviesNewReleaseMainScreen/newReleaseMainScreen'
          ),
        ])
        if (cancelled) return
        const t = (tRes.data.data || []).map(mapApiMovie)
        const n = (nRes.data.data || []).map(mapApiMovie)
        setTrending(t)
        setNewReleases(n)
        setError(null)
      } catch (e: any) {
        setError(e?.message || 'Không tải được dữ liệu')
      } finally {
        if (!cancelled) setLoading(false)
      }
    }
    load()
    return () => {
      cancelled = true
    }
  }, [])

  // Chọn HERO: ưu tiên phần tử đầu của trending
  const HERO: MovieItem = useMemo(() => {
    return (
      trending[0] || {
        id: 0,
        title: 'FilmZone – Xem phim trực tuyến',
        overview:
          'Khám phá kho phim mới mỗi ngày: phim lẻ, phim bộ, đủ thể loại.',
        poster: '/posters/placeholder.jpg',
        backdrop: '/backdrops/placeholder.jpg',
        quality: 'FullHD',
      }
    )
  }, [trending])

  const deskSrc = cldVariant(HERO.backdrop, { ar: '21:9' }) || HERO.backdrop
  const mobSrc = cldVariant(HERO.backdrop, { ar: '4:5' }) || HERO.backdrop

  return (
    <>
      {/* ===== Hero ===== */}
      <section
        id="hero"
        className="relative isolate flex min-h-[82vh] items-center overflow-hidden"
      >
        <div className="absolute inset-0 z-0">
          <Image
            key={deskSrc + '-desk'}
            src={deskSrc}
            alt={HERO.title}
            fill
            priority
            sizes="100vw"
            quality={85}
            className={`hidden md:block object-cover transition-opacity duration-500 ${heroReady ? 'opacity-100' : 'opacity-0'
              }`}
            onLoad={() => setHeroReady(true)}
          />
          <Image
            key={mobSrc + '-mob'}
            src={mobSrc}
            alt={HERO.title}
            fill
            priority
            sizes="100vw"
            quality={85}
            className={`md:hidden object-cover transition-opacity duration-500 ${heroReady ? 'opacity-100' : 'opacity-0'
              }`}
            onLoad={() => setHeroReady(true)}
          />
        </div>

        {/* overlays để chữ dễ đọc */}
        <div className="pointer-events-none absolute inset-0 z-10 bg-gradient-to-b from-black/70 via-black/40 to-black/60" />
        <div className="pointer-events-none absolute inset-y-0 left-0 z-20 w-[60%] max-w-[900px] bg-gradient-to-r from-black/75 via-black/40 to-transparent" />

        <div className="relative z-30 mx-auto w-full max-w-7xl px-4 md:px-6">
          <div className="md:max-w-2xl md:rounded-2xl md:bg-black/35 md:backdrop-blur-[2px] md:ring-1 md:ring-white/10 md:p-6">
            <motion.h1
              initial={{ opacity: 0, y: 12 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.6 }}
              className="text-4xl font-extrabold leading-[1.15] tracking-tight md:text-6xl text-white drop-shadow-[0_2px_6px_rgba(0,0,0,0.6)]"
            >
              {HERO.title}
            </motion.h1>

            {HERO.overview && (
              <motion.p
                initial={{ opacity: 0, y: 12 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ duration: 0.6, delay: 0.1 }}
                className="mt-3 max-w-2xl text-base text-white/90 md:text-lg"
              >
                {HERO.overview}
              </motion.p>
            )}

            <motion.div
              initial={{ opacity: 0, y: 12 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.6, delay: 0.2 }}
              className="mt-6 flex flex-col gap-3 sm:flex-row"
            >
              <Link
                href={HERO.id ? `/business/movie/${HERO.id}` : '/movies'}
                className="inline-flex items-center justify-center gap-2 rounded-2xl bg-amber-500 px-6 py-3 text-base font-semibold text-stone-900 shadow-md ring-1 ring-amber-200 transition hover:bg-amber-400 hover:shadow-lg focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-amber-300"
              >
                <Play className="h-5 w-5" /> Xem ngay
              </Link>
              {HERO.id ? (
                <Link
                  href={`/title/${HERO.slug ?? HERO.id}`}
                  className="inline-flex items-center justify-center gap-2 rounded-2xl border border-white/60 px-6 py-3 text-base font-semibold text-white/95 backdrop-blur transition hover:bg-white/10 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-white/50"
                >
                  <Info className="h-5 w-5" /> Chi tiết
                </Link>
              ) : null}
            </motion.div>

            <div className="mt-6 inline-flex items-center gap-3 text-sm text-white/80">
              {typeof HERO.rating === 'number' && (
                <span className="inline-flex items-center gap-1">
                  <Flame className="h-4 w-4" /> {HERO.rating}
                </span>
              )}
              {HERO.year && (
                <span className="inline-flex items-center gap-1">
                  <CalendarDays className="h-4 w-4" /> {HERO.year}
                </span>
              )}
              {HERO.quality && (
                <span className="rounded bg-white/10 px-2 py-0.5 text-xs font-semibold ring-1 ring-white/20">
                  {HERO.quality}
                </span>
              )}
            </div>
          </div>
        </div>
      </section>

      {/* ===== States ===== */}
      {error && (
        <div className="bg-red-500/10 border border-red-500/30 text-red-200 mx-auto my-4 max-w-7xl rounded-xl px-4 py-3 flex items-center gap-2">
          <AlertTriangle className="h-4 w-4" />
          <span>{error}</span>
        </div>
      )}

      {/* ===== Rows ===== */}
      <section id="trending" className="scroll-mt-24 bg-zinc-950 py-10">
        <div className="mx-auto max-w-7xl px-4 md:px-6">
          <SectionTitle
            title="Đang thịnh hành"
            subtitle="Cập nhật theo thời gian thực"
          />
          <Row title="Xu hướng hôm nay" items={trending} />
        </div>
      </section>

      <section id="new" className="scroll-mt-24 bg-zinc-950 py-10">
        <div className="mx-auto max-w-7xl px-4 md:px-6">
          <SectionTitle
            title="Mới phát hành"
            subtitle="Đừng bỏ lỡ những cái tên hot"
          />
          <Row title="Vừa lên sóng" items={newReleases} />
        </div>
      </section>

      <section
        id="categories"
        className="scroll-mt-24 bg-gradient-to-b from-zinc-950 to-zinc-900 py-14"
      >
        <div className="mx-auto max-w-7xl px-4 md:px-6">
          <SectionTitle
            title="Khám phá theo thể loại"
            subtitle="Hành động • Phiêu lưu • Kinh dị • Tình cảm • Hoạt hình…"
          />
          <div className="grid grid-cols-2 gap-3 md:grid-cols-4 md:gap-4">
            {[
              'Hành động',
              'Phiêu lưu',
              'Tâm lý',
              'Kinh dị',
              'Hoạt hình',
              'Viễn tưởng',
              'Hài',
              'Tình cảm',
            ].map((g) => (
              <Link
                key={g}
                href={`/genres/${encodeURIComponent(g)}`}
                className="group relative h-28 overflow-hidden rounded-2xl bg-white/5 ring-1 ring-white/10 transition hover:ring-amber-400/40"
              >
                <div className="absolute inset-0 bg-gradient-to-br from-amber-400/10 to-transparent" />
                <span className="absolute left-4 top-4 z-10 text-sm font-semibold">
                  {g}
                </span>
              </Link>
            ))}
          </div>

          {/* Loading shimmer đơn giản */}
          {loading && (
            <div className="mt-6 grid grid-cols-2 gap-3 sm:grid-cols-3 md:grid-cols-5 lg:grid-cols-6">
              {Array.from({ length: 6 }).map((_, i) => (
                <div key={i} className="h-60 animate-pulse rounded-2xl bg-white/5" />
              ))}
            </div>
          )}
        </div>
      </section>
    </>
  )
}

/* Reusable */
function SectionTitle({ title, subtitle }: { title: string; subtitle?: string }) {
  return (
    <div className="mx-auto mb-6 max-w-2xl text-left md:text-center">
      <h2 className="text-2xl md:text-3xl font-extrabold tracking-tight">
        {title}
      </h2>
      {subtitle ? <p className="mt-2 text-zinc-400">{subtitle}</p> : null}
    </div>
  )
}

function Row({ title, items, variant = 'poster' }: RowProps) {
  return (
    <div className="space-y-3">
      <h3 className="text-lg font-semibold text-zinc-100">{title}</h3>
      <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 md:grid-cols-5 lg:grid-cols-6">
        {items.map((m) => (
          <MovieCard key={m.id} item={m} variant={variant} />
        ))}
        {!items.length && (
          <p className="col-span-full text-sm text-zinc-400">
            Chưa có dữ liệu.
          </p>
        )}
      </div>
    </div>
  )
}

function MovieCard({
  item,
  variant,
}: {
  item: MovieItem
  variant: 'poster' | 'landscape'
}) {
  const img =
    variant === 'poster'
      ? cldVariant(item.poster, { ar: '2:3' }) || item.poster
      : cldVariant(item.backdrop, { ar: '16:9' }) || item.backdrop

  return (
    <motion.article
      initial={{ opacity: 0, y: 10 }}
      whileInView={{ opacity: 1, y: 0 }}
      viewport={{ once: true }}
      transition={{ duration: 0.45 }}
      className="group overflow-hidden rounded-2xl border border-white/10 bg-white/5 shadow-sm ring-1 ring-transparent transition hover:-translate-y-0.5 hover:shadow-md hover:ring-amber-400/40"
    >
      <div
        className="relative w-full"
        style={{ aspectRatio: variant === 'poster' ? '2/3' : '16/9' }}
      >
        <Image
          src={img}
          alt={item.title}
          fill
          sizes="(min-width: 1024px) 16vw, (min-width: 768px) 20vw, 45vw"
          className="object-cover transition duration-500 group-hover:scale-105"
        />
        {item.quality && (
          <span className="absolute left-2 top-2 rounded bg-black/70 px-1.5 py-0.5 text-[10px] font-semibold ring-1 ring-white/20">
            {item.quality}
          </span>
        )}
      </div>
      <div className="p-3">
        <Link
          href={item.slug ? `/title/${item.slug}` : `/title/${item.id}`}
          className="line-clamp-1 text-sm font-semibold hover:text-amber-400"
        >
          {item.title}
        </Link>
        <div className="mt-1 flex items-center justify-between text-[12px] text-zinc-400">
          <span>{item.year ?? ''}</span>
          {typeof item.rating === 'number' && <span>★ {item.rating}</span>}
        </div>
        <Link
          href={`/business/movie/${item.id}`}
          className="mt-2 inline-flex w-full items-center justify-center rounded-xl bg-amber-500 px-3 py-1.5 text-xs font-semibold text-zinc-900 shadow-sm ring-1 ring-amber-300/60 transition hover:bg-amber-400"
        >
          Xem phim
        </Link>
      </div>
    </motion.article>
  )
}
