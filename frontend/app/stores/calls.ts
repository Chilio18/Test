import { defineStore } from 'pinia'
import type { Call, CallDetail, PagedResult } from '~/types'

export const useCallsStore = defineStore('calls', {
  state: () => ({
    calls: [] as Call[], currentCall: null as CallDetail | null,
    pagination: { page: 1, pageSize: 20, totalCount: 0, totalPages: 0 },
    isLoading: false, isLoadingDetail: false,
    filters: { status: null as string | null, ownerId: null as string | null, accountId: null as string | null, from: null as string | null, to: null as string | null },
  }),
  getters: {
    completedCalls: (state) => state.calls.filter(c => c.status === 'Completed'),
    pendingCalls: (state) => state.calls.filter(c => ['Processing', 'Transcribing', 'Analyzing'].includes(c.status)),
  },
  actions: {
    async fetchCalls(page = 1) {
      this.isLoading = true
      try {
        const { getCalls } = useCallsApi()
        const params: Record<string, unknown> = { page, pageSize: this.pagination.pageSize, ...Object.fromEntries(Object.entries(this.filters).filter(([, v]) => v !== null)) }
        const result = await getCalls(params)
        this.calls = result.items
        this.pagination = { page: result.page, pageSize: result.pageSize, totalCount: result.totalCount, totalPages: result.totalPages }
      } finally { this.isLoading = false }
    },
    async fetchCallDetail(id: string) {
      this.isLoadingDetail = true; this.currentCall = null
      try { const { getCall } = useCallsApi(); this.currentCall = await getCall(id) }
      finally { this.isLoadingDetail = false }
    },
    setFilter(key: keyof typeof this.filters, value: string | null) { this.filters[key] = value },
    clearFilters() { this.filters = { status: null, ownerId: null, accountId: null, from: null, to: null } },
  },
})
