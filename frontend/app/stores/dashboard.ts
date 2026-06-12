import { defineStore } from 'pinia'
import type { LeadershipDashboard } from '~/types'

export const useDashboardStore = defineStore('dashboard', {
  state: () => ({ leadership: null as LeadershipDashboard | null, isLoading: false }),
  actions: {
    async fetchLeadershipDashboard(from?: string, to?: string) {
      this.isLoading = true
      try { const { getLeadershipDashboard } = useDashboardApi(); this.leadership = await getLeadershipDashboard(from, to) }
      finally { this.isLoading = false }
    },
  },
})
