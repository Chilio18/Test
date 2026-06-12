import { defineStore } from 'pinia'
import type { CurrentUser } from '~/types'

export const useAuthStore = defineStore('auth', {
  state: () => ({ user: null as CurrentUser | null, token: null as string | null, isLoading: false }),
  getters: {
    isAuthenticated: (state) => !!state.user && !!state.token,
    isManager: (state) => ['SalesManager', 'OrganizationAdmin', 'PlatformAdmin'].includes(state.user?.role ?? ''),
    isAdmin: (state) => ['OrganizationAdmin', 'PlatformAdmin'].includes(state.user?.role ?? ''),
  },
  actions: {
    setUser(user: CurrentUser, token: string) {
      this.user = user; this.token = token
      if (process.client) localStorage.setItem('auth_token', token)
    },
    logout() {
      this.user = null; this.token = null
      if (process.client) localStorage.removeItem('auth_token')
      navigateTo('/login')
    },
    async fetchCurrentUser() {
      this.isLoading = true
      try {
        const { get } = useApi()
        const data = await get<{ userId: string; tenantId: string; email: string }>('/api/users/me')
        this.user = { userId: data.userId, tenantId: data.tenantId, email: data.email, fullName: data.email, role: 'SalesRepresentative', avatarUrl: null }
      } catch { this.logout() } finally { this.isLoading = false }
    },
  },
})
