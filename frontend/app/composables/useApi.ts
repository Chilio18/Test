import type { PagedResult } from '~/types'

export function useApi() {
  const config = useRuntimeConfig()
  const baseURL = config.public.apiBase

  function getHeaders(): Record<string, string> {
    const token = process.client ? localStorage.getItem('auth_token') : null
    return token ? { Authorization: `Bearer ${token}` } : {}
  }

  async function get<T>(path: string, params?: Record<string, unknown>): Promise<T> {
    return await $fetch<T>(`${baseURL}${path}`, { method: 'GET', params, headers: getHeaders() })
  }

  async function post<T>(path: string, body?: unknown): Promise<T> {
    return await $fetch<T>(`${baseURL}${path}`, { method: 'POST', body, headers: getHeaders() })
  }

  async function put<T>(path: string, body?: unknown): Promise<T> {
    return await $fetch<T>(`${baseURL}${path}`, { method: 'PUT', body, headers: getHeaders() })
  }

  async function del(path: string): Promise<void> {
    await $fetch(`${baseURL}${path}`, { method: 'DELETE', headers: getHeaders() })
  }

  return { get, post, put, del }
}

export function useCallsApi() {
  const { get, post } = useApi()
  return {
    getCalls: (params?: Record<string, unknown>) => get<PagedResult<import('~/types').Call>>('/api/calls', params),
    getCall: (id: string) => get<import('~/types').CallDetail>(`/api/calls/${id}`),
    uploadCall: (data: unknown) => post<{ callId: string; recordingId: string; presignedUploadUrl: string }>('/api/calls', data),
    syncToCrm: (id: string, data: unknown) => post(`/api/calls/${id}/sync-crm`, data),
  }
}

export function useDashboardApi() {
  const { get } = useApi()
  return {
    getLeadershipDashboard: (from?: string, to?: string) =>
      get<import('~/types').LeadershipDashboard>('/api/dashboard/leadership', { from, to }),
  }
}

export function useSearchApi() {
  const { post } = useApi()
  return {
    ask: (question: string, filters?: Record<string, unknown>) =>
      post<import('~/types').SemanticSearchResponse>('/api/search/ask', { question, ...filters }),
  }
}
