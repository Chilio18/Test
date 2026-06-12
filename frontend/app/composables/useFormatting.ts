import { format, formatDistanceToNow, parseISO } from 'date-fns'
import { nl } from 'date-fns/locale'

export function useFormatting() {
  function formatDate(date: string | null | undefined, fmt = 'd MMM yyyy'): string {
    if (!date) return '—'
    try { return format(parseISO(date), fmt, { locale: nl }) } catch { return '—' }
  }
  function formatDateTime(date: string | null | undefined): string {
    if (!date) return '—'
    try { return format(parseISO(date), 'd MMM yyyy HH:mm', { locale: nl }) } catch { return '—' }
  }
  function formatRelative(date: string | null | undefined): string {
    if (!date) return '—'
    try { return formatDistanceToNow(parseISO(date), { addSuffix: true, locale: nl }) } catch { return '—' }
  }
  function formatDuration(seconds: number | null | undefined): string {
    if (!seconds) return '—'
    const m = Math.floor(seconds / 60)
    if (m >= 60) { const h = Math.floor(m / 60); return `${h}h ${m % 60}m` }
    return `${m}m ${seconds % 60}s`
  }
  function formatCurrency(amount: number | null | undefined, currency = 'EUR'): string {
    if (amount == null) return '—'
    return new Intl.NumberFormat('nl-NL', { style: 'currency', currency }).format(amount)
  }
  function formatPercentage(value: number | null | undefined): string {
    if (value == null) return '—'
    return `${Math.round(value)}%`
  }
  function scoreColor(score: number | null | undefined): string {
    if (score == null) return 'text-gray-500'
    if (score >= 80) return 'text-green-400'
    if (score >= 60) return 'text-yellow-400'
    if (score >= 40) return 'text-orange-400'
    return 'text-red-400'
  }
  function scoreBgColor(score: number | null | undefined): string {
    if (score == null) return 'bg-gray-700 text-gray-400'
    if (score >= 80) return 'bg-green-500/20 text-green-400 border-green-500/30'
    if (score >= 60) return 'bg-yellow-500/20 text-yellow-400 border-yellow-500/30'
    if (score >= 40) return 'bg-orange-500/20 text-orange-400 border-orange-500/30'
    return 'bg-red-500/20 text-red-400 border-red-500/30'
  }
  function callStatusColor(status: string): string {
    const map: Record<string, string> = {
      Uploaded: 'bg-gray-500/20 text-gray-400', Processing: 'bg-blue-500/20 text-blue-400',
      Transcribing: 'bg-cyan-500/20 text-cyan-400', Analyzing: 'bg-purple-500/20 text-purple-400',
      Completed: 'bg-green-500/20 text-green-400', Failed: 'bg-red-500/20 text-red-400',
    }
    return map[status] ?? 'bg-gray-500/20 text-gray-400'
  }
  function stageColor(stage: string): string {
    const map: Record<string, string> = {
      Prospecting: 'bg-gray-500/20 text-gray-400', Qualification: 'bg-blue-500/20 text-blue-400',
      ProposalQuote: 'bg-orange-500/20 text-orange-400', NegotiationReview: 'bg-purple-500/20 text-purple-400',
      ClosedWon: 'bg-green-500/20 text-green-400', ClosedLost: 'bg-red-500/20 text-red-400',
    }
    return map[stage] ?? 'bg-gray-500/20 text-gray-400'
  }
  return { formatDate, formatDateTime, formatRelative, formatDuration, formatCurrency, formatPercentage, scoreColor, scoreBgColor, callStatusColor, stageColor }
}
