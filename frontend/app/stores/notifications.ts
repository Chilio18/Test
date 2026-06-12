import { defineStore } from 'pinia'
import type { Notification } from '~/types'

export const useNotificationStore = defineStore('notifications', {
  state: () => ({ notifications: [] as Notification[], isLoading: false }),
  getters: {
    unreadCount: (state) => state.notifications.filter(n => !n.isRead).length,
  },
  actions: {
    addNotification(n: Notification) { this.notifications.unshift(n) },
    markRead(id: string) { const n = this.notifications.find(n => n.id === id); if (n) n.isRead = true },
    markAllRead() { this.notifications.forEach(n => n.isRead = true) },
    async fetchNotifications() {
      this.isLoading = true
      try { const { get } = useApi(); this.notifications = await get<Notification[]>('/api/notifications') }
      finally { this.isLoading = false }
    },
  },
})
