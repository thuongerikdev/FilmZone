'use client'

import React, { useMemo, useState } from 'react'
import { http } from '@/app/lib/apiClient'
import type { AxiosError } from 'axios'

type PersonRole =
    | 'cast'
    | 'director'
    | 'writer'
    | 'producer'
    | 'editor'
    | 'cinematographer'
    | 'composer'

interface PersonRow {
    personID: number | ''
    role: PersonRole
    characterName?: string
    creditOrder?: number | ''
}

const CREATE_MOVIE_ENDPOINT = '/api/Movie/CreateMovie'

export default function Page() {
    // ====== PREFILL ======
    const [slug, setSlug] = useState('thuong-dep-trai')
    const [title, setTitle] = useState('string')
    const [image, setImage] = useState<File | null>(null) // poster (file không thể điền sẵn)
    const [originalTitle, setOriginalTitle] = useState('Oke')
    const [description, setDescription] = useState('Oke')
    const [movieType, setMovieType] = useState<'movie' | 'series'>('movie')
    const [status, setStatus] = useState<'ongoing' | 'completed' | 'coming_soon'>('completed')
    const [releaseDate, setReleaseDate] = useState('2025-09-11') // yyyy-mm-dd
    const [durationSeconds, setDurationSeconds] = useState<number | ''>(2)
    const [totalSeasons, setTotalSeasons] = useState<number | ''>('') // series dùng
    const [totalEpisodes, setTotalEpisodes] = useState<number | ''>('') // series dùng

    const [year, setYear] = useState<number | ''>(2020)
    const [rated, setRated] = useState('0')
    const [regionID, setRegionID] = useState<number | ''>(3) // BE: non-nullable
    const [popularity, setPopularity] = useState<number | ''>(4)

    const [tagIDsInput, setTagIDsInput] = useState('1')

    const [people, setPeople] = useState<PersonRow[]>([
        { personID: 1, role: 'director', characterName: 'Hà Lan', creditOrder: 2 },
    ])

    const [movieImages, setMovieImages] = useState<File[]>([]) // (file không thể điền sẵn)

    const [submitting, setSubmitting] = useState(false)
    const [serverResponse, setServerResponse] = useState<any>(null)
    const [errorMsg, setErrorMsg] = useState<string | null>(null)

    const parsedTagIDs = useMemo(() => {
        return tagIDsInput
            .split(',')
            .map((s) => s.trim())
            .filter(Boolean)
            .map((n) => Number(n))
            .filter((n) => !Number.isNaN(n))
    }, [tagIDsInput])

    function updatePerson<K extends keyof PersonRow>(index: number, key: K, value: PersonRow[K]) {
        setPeople((prev) => prev.map((p, i) => (i === index ? { ...p, [key]: value } : p)))
    }

    function addPerson() {
        setPeople((prev) => [...prev, { personID: '', role: 'cast', characterName: '', creditOrder: '' }])
    }

    function removePerson(index: number) {
        setPeople((prev) => prev.filter((_, i) => i !== index))
    }

    function fillDemo() {
        setSlug('thuong-dep-trai')
        setTitle('string')
        setOriginalTitle('Oke')
        setDescription('Oke')
        setMovieType('movie')
        setStatus('completed')
        setReleaseDate('2025-09-11')
        setDurationSeconds(2)
        setTotalSeasons('')
        setTotalEpisodes('')
        setYear(2020)
        setRated('0')
        setRegionID(3)
        setPopularity(4)
        setTagIDsInput('1')
        setPeople([{ personID: 1, role: 'director', characterName: 'Hà Lan', creditOrder: 2 }])
        setImage(null)
        setMovieImages([])
    }

    async function onSubmit(e: React.FormEvent) {
        e.preventDefault()
        setSubmitting(true)
        setServerResponse(null)
        setErrorMsg(null)

        // ====== FE validations khớp BE non-nullable ======
        if (!image) {
            setErrorMsg('Poster image (field "image") là bắt buộc.')
            setSubmitting(false)
            return
        }
        if (movieImages.length === 0) {
            setErrorMsg('Vui lòng chọn ít nhất 1 ảnh trong "Additional Images" (movieImages).')
            setSubmitting(false)
            return
        }
        if (regionID === '') {
            setErrorMsg('Region ID là bắt buộc.')
            setSubmitting(false)
            return
        }

        try {
            const fd = new FormData()

            // Scalars
            fd.append('slug', slug)
            fd.append('title', title)
            fd.append('image', image) // poster
            if (originalTitle) fd.append('originalTitle', originalTitle)
            if (description) fd.append('description', description)
            fd.append('movieType', movieType)
            fd.append('status', status)
            if (releaseDate) fd.append('releaseDate', releaseDate)
            if (movieType === 'movie' && durationSeconds !== '') {
                fd.append('durationSeconds', String(durationSeconds))
            }
            if (movieType === 'series') {
                if (totalSeasons !== '') fd.append('totalSeasons', String(totalSeasons))
                if (totalEpisodes !== '') fd.append('totalEpisodes', String(totalEpisodes))
            }
            if (year !== '') fd.append('year', String(year))
            if (rated) fd.append('rated', rated)
            fd.append('regionID', String(regionID))
            if (popularity !== '') fd.append('popularity', String(popularity))

            // tagIDs[]
            parsedTagIDs.forEach((id, idx) => {
                fd.append(`tagIDs[${idx}]`, String(id))
            })

            // person[] + index-hints
            const validPeople = people.filter((p) => p.personID !== '')
            validPeople.forEach((p, idx) => {
                fd.append('person.index', String(idx))
                fd.append(`person[${idx}].personID`, String(p.personID))
                fd.append(`person[${idx}].role`, p.role)
                if (p.characterName) fd.append(`person[${idx}].characterName`, p.characterName)
                if (p.creditOrder !== '') fd.append(`person[${idx}].creditOrder`, String(p.creditOrder))
            })

            // movieImages[] + index-hints
            movieImages.forEach((file, idx) => {
                fd.append('movieImages.index', String(idx))
                fd.append(`movieImages[${idx}].image`, file)
            })

            const res = await http.post(CREATE_MOVIE_ENDPOINT, fd, {
                timeout: 120_000,
                headers: {}, // KHÔNG set Content-Type cho FormData
            })

            setServerResponse(res.data)
        } catch (err: any) {
            const ax = err as AxiosError
            if ((ax as any)?.code === 'ECONNABORTED') {
                setErrorMsg('Upload/processing quá lâu (vượt timeout). Hãy tăng timeout hoặc kiểm tra BE.')
            } else {
                const msg =
                    ax?.response?.data && typeof ax.response.data === 'object'
                        ? JSON.stringify(ax.response.data, null, 2)
                        : (ax?.response?.data as string) || ax?.message || 'Request failed'
                setErrorMsg(msg)
            }
        } finally {
            setSubmitting(false)
        }
    }

    const endpointLabel = `${http.defaults.baseURL ?? ''}${CREATE_MOVIE_ENDPOINT}`

    // ===== helpers: class cho input/select/textarea dark mode =====
    const field =
        'mt-1 w-full rounded-xl border border-neutral-700 bg-neutral-900 text-neutral-100 placeholder-neutral-400 p-2 focus:outline-none focus:ring-2 focus:ring-blue-600 focus:border-transparent'
    const selectField =
        'mt-1 w-full rounded-xl border border-neutral-700 bg-neutral-900 text-neutral-100 p-2 focus:outline-none focus:ring-2 focus:ring-blue-600'
    const numberField = field

    return (
        <div className="min-h-screen bg-neutral-950 text-neutral-100">
            <div className="mx-auto max-w-5xl p-6">
                <div className="flex items-center gap-3 mb-4">
                    <h1 className="text-2xl font-bold">Create Movie</h1>
                    <button
                        type="button"
                        onClick={fillDemo}
                        className="rounded-xl border border-neutral-700 px-3 py-1.5 hover:bg-neutral-800"
                        title="Điền lại các giá trị mẫu"
                    >
                        Điền mẫu
                    </button>
                </div>

                <form onSubmit={onSubmit} className="space-y-8">
                    {/* Basics */}
                    <section className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <div>
                            <label className="block text-sm font-medium">Slug</label>
                            <input className={field} value={slug} onChange={(e) => setSlug(e.target.value)} required />
                        </div>
                        <div>
                            <label className="block text-sm font-medium">Title</label>
                            <input className={field} value={title} onChange={(e) => setTitle(e.target.value)} required />
                        </div>
                        <div>
                            <label className="block text-sm font-medium">Poster Image (image)</label>
                            <input
                                className={field}
                                type="file"
                                accept="image/*"
                                required
                                onChange={(e) => setImage(e.target.files?.[0] ?? null)}
                            />
                        </div>
                        <div>
                            <label className="block text-sm font-medium">Original Title</label>
                            <input className={field} value={originalTitle} onChange={(e) => setOriginalTitle(e.target.value)} />
                        </div>
                        <div className="md:col-span-2">
                            <label className="block text-sm font-medium">Description</label>
                            <textarea
                                className={`${field} min-h-28`}
                                rows={4}
                                value={description}
                                onChange={(e) => setDescription(e.target.value)}
                            />
                        </div>
                    </section>

                    {/* Movie meta */}
                    <section className="grid grid-cols-1 md:grid-cols-3 gap-4">
                        <div>
                            <label className="block text-sm font-medium">Movie Type</label>
                            <select
                                className={selectField + ' bg-neutral-900'}
                                value={movieType}
                                onChange={(e) => setMovieType(e.target.value as 'movie' | 'series')}
                            >
                                <option value="movie">movie</option>
                                <option value="series">series</option>
                            </select>
                        </div>

                        <div>
                            <label className="block text-sm font-medium">Status</label>
                            <select
                                className={selectField + ' bg-neutral-900'}
                                value={status}
                                onChange={(e) => setStatus(e.target.value as any)}
                            >
                                <option value="ongoing">ongoing</option>
                                <option value="completed">completed</option>
                                <option value="coming_soon">coming_soon</option>
                            </select>
                        </div>

                        <div>
                            <label className="block text-sm font-medium">Release Date</label>
                            <input className={field} type="date" value={releaseDate} onChange={(e) => setReleaseDate(e.target.value)} />
                        </div>

                        {movieType === 'movie' && (
                            <div>
                                <label className="block text-sm font-medium">Duration Seconds</label>
                                <input
                                    className={numberField}
                                    type="number"
                                    min={0}
                                    value={durationSeconds}
                                    onChange={(e) => setDurationSeconds(e.target.value === '' ? '' : Number(e.target.value))}
                                />
                            </div>
                        )}

                        {movieType === 'series' && (
                            <>
                                <div>
                                    <label className="block text-sm font-medium">Total Seasons</label>
                                    <input
                                        className={numberField}
                                        type="number"
                                        min={0}
                                        value={totalSeasons}
                                        onChange={(e) => setTotalSeasons(e.target.value === '' ? '' : Number(e.target.value))}
                                    />
                                </div>
                                <div>
                                    <label className="block text-sm font-medium">Total Episodes</label>
                                    <input
                                        className={numberField}
                                        type="number"
                                        min={0}
                                        value={totalEpisodes}
                                        onChange={(e) => setTotalEpisodes(e.target.value === '' ? '' : Number(e.target.value))}
                                    />
                                </div>
                            </>
                        )}

                        <div>
                            <label className="block text-sm font-medium">Year</label>
                            <input
                                className={numberField}
                                type="number"
                                min={1800}
                                value={year}
                                onChange={(e) => setYear(e.target.value === '' ? '' : Number(e.target.value))}
                            />
                        </div>

                        <div>
                            <label className="block text-sm font-medium">Rated</label>
                            <input className={field} value={rated} onChange={(e) => setRated(e.target.value)} />
                        </div>

                        <div>
                            <label className="block text-sm font-medium">Region ID</label>
                            <input
                                className={numberField}
                                type="number"
                                min={1}
                                value={regionID}
                                onChange={(e) => setRegionID(e.target.value === '' ? '' : Number(e.target.value))}
                                required
                            />
                        </div>

                        <div>
                            <label className="block text-sm font-medium">Popularity</label>
                            <input
                                className={numberField}
                                type="number"
                                step="0.1"
                                min={0}
                                value={popularity}
                                onChange={(e) => setPopularity(e.target.value === '' ? '' : Number(e.target.value))}
                            />
                        </div>

                        <div className="md:col-span-2">
                            <label className="block text-sm font-medium">Tag IDs (comma separated)</label>
                            <input
                                className={field}
                                placeholder="e.g. 1,2,3"
                                value={tagIDsInput}
                                onChange={(e) => setTagIDsInput(e.target.value)}
                            />
                            {!!parsedTagIDs.length && (
                                <p className="text-xs text-neutral-400 mt-1">Parsed: [{parsedTagIDs.join(', ')}]</p>
                            )}
                        </div>
                    </section>

                    {/* People */}
                    <section className="space-y-3">
                        <div className="flex items-center justify-between">
                            <h2 className="text-lg font-semibold">People</h2>
                            <button
                                type="button"
                                onClick={addPerson}
                                className="rounded-xl bg-blue-600 text-white px-3 py-2 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500"
                            >
                                + Add person
                            </button>
                        </div>

                        <div className="overflow-x-auto rounded-xl border border-neutral-800">
                            <table className="min-w-full">
                                <thead className="bg-neutral-800 text-neutral-100">
                                    <tr>
                                        <th className="text-left p-2 border border-neutral-800">personID</th>
                                        <th className="text-left p-2 border border-neutral-800">role</th>
                                        <th className="text-left p-2 border border-neutral-800">characterName</th>
                                        <th className="text-left p-2 border border-neutral-800">creditOrder</th>
                                        <th className="text-left p-2 border border-neutral-800">actions</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {people.map((p, idx) => (
                                        <tr key={idx} className="odd:bg-neutral-900 even:bg-neutral-900/70">
                                            <td className="p-2 border border-neutral-800">
                                                <input
                                                    className="w-40 rounded-xl border border-neutral-700 bg-neutral-900 text-neutral-100 p-2 focus:ring-2 focus:ring-blue-600 focus:border-transparent"
                                                    type="number"
                                                    min={1}
                                                    value={p.personID}
                                                    onChange={(e) =>
                                                        updatePerson(idx, 'personID', e.target.value === '' ? '' : Number(e.target.value))
                                                    }
                                                />
                                            </td>
                                            <td className="p-2 border border-neutral-800">
                                                <select
                                                    className="w-40 rounded-xl border border-neutral-700 bg-neutral-900 text-neutral-100 p-2 focus:ring-2 focus:ring-blue-600"
                                                    value={p.role}
                                                    onChange={(e) => updatePerson(idx, 'role', e.target.value as PersonRole)}
                                                >
                                                    <option value="cast">cast</option>
                                                    <option value="director">director</option>
                                                    <option value="writer">writer</option>
                                                    <option value="producer">producer</option>
                                                    <option value="editor">editor</option>
                                                    <option value="cinematographer">cinematographer</option>
                                                    <option value="composer">composer</option>
                                                </select>
                                            </td>
                                            <td className="p-2 border border-neutral-800">
                                                <input
                                                    className="w-56 rounded-xl border border-neutral-700 bg-neutral-900 text-neutral-100 p-2 focus:ring-2 focus:ring-blue-600 focus:border-transparent"
                                                    value={p.characterName ?? ''}
                                                    onChange={(e) => updatePerson(idx, 'characterName', e.target.value)}
                                                    placeholder="(optional)"
                                                />
                                            </td>
                                            <td className="p-2 border border-neutral-800">
                                                <input
                                                    className="w-36 rounded-xl border border-neutral-700 bg-neutral-900 text-neutral-100 p-2 focus:ring-2 focus:ring-blue-600 focus:border-transparent"
                                                    type="number"
                                                    min={0}
                                                    value={p.creditOrder ?? ''}
                                                    onChange={(e) =>
                                                        updatePerson(idx, 'creditOrder', e.target.value === '' ? '' : Number(e.target.value))
                                                    }
                                                    placeholder="(optional)"
                                                />
                                            </td>
                                            <td className="p-2 border border-neutral-800">
                                                <button
                                                    type="button"
                                                    onClick={() => removePerson(idx)}
                                                    className="rounded-xl border border-red-500/60 text-red-300 px-3 py-2 hover:bg-red-500/10 disabled:opacity-50"
                                                    disabled={people.length === 1}
                                                    title={people.length === 1 ? 'At least one row is kept for convenience' : 'Remove'}
                                                >
                                                    Remove
                                                </button>
                                            </td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        </div>
                    </section>

                    {/* Additional images */}
                    <section className="space-y-2">
                        <h2 className="text-lg font-semibold">Additional Images (movieImages)</h2>
                        <input
                            className={field}
                            type="file"
                            accept="image/*"
                            multiple
                            required
                            onChange={(e) => setMovieImages(Array.from(e.target.files ?? []))}
                        />
                        {movieImages.length > 0 && <p className="text-xs text-neutral-400">Selected: {movieImages.length} file(s)</p>}
                    </section>

                    <div className="flex items-center gap-3">
                        <button
                            type="submit"
                            disabled={submitting}
                            className="rounded-xl bg-blue-600 text-white px-5 py-2 hover:bg-blue-700 disabled:opacity-50 focus:outline-none focus:ring-2 focus:ring-blue-500"
                        >
                            {submitting ? 'Submitting…' : 'Create Movie'}
                        </button>
                        <span className="text-sm text-neutral-400">POST → {endpointLabel}</span>
                    </div>
                </form>

                {/* Response */}
                <section className="mt-8">
                    {errorMsg && (
                        <div className="rounded-xl border border-red-500/50 bg-red-950/40 p-4 mb-4">
                            <div className="font-semibold text-red-300">Error</div>
                            <div className="text-red-200 text-sm whitespace-pre-wrap">{String(errorMsg)}</div>
                        </div>
                    )}
                    {serverResponse && (
                        <div className="rounded-xl border border-neutral-800 bg-neutral-900 p-4">
                            <div className="font-semibold mb-2">Server response</div>
                            <pre className="text-sm whitespace-pre-wrap text-neutral-200">
                                {typeof serverResponse === 'string' ? serverResponse : JSON.stringify(serverResponse, null, 2)}
                            </pre>
                        </div>
                    )}
                </section>
            </div>
        </div>
    )
}
