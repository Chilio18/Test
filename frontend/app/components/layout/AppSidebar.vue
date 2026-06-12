<template>
  <aside class="fixed inset-y-0 left-0 w-64 bg-gray-900 border-r border-gray-800 flex flex-col z-30">
    <div class="h-16 flex items-center px-6 border-b border-gray-800">
      <span class="text-lg font-bold text-gradient">Revenue Intelligence</span>
    </div>
    <nav class="flex-1 p-4 space-y-1 overflow-y-auto scrollbar-thin">
      <SidebarLink v-for="item in nav" :key="item.to" v-bind="item" />
      <div class="pt-4 pb-1">
        <p class="px-3 text-xs font-semibold text-gray-500 uppercase tracking-wider">Settings</p>
      </div>
      <SidebarLink v-for="item in settingsNav" :key="item.to" v-bind="item" />
    </nav>
    <div class="p-4 border-t border-gray-800">
      <div class="flex items-center gap-3">
        <div class="w-8 h-8 rounded-full bg-brand-600 flex items-center justify-center text-sm font-medium text-white">
          {{ userInitials }}
        </div>
        <div class="flex-1 min-w-0">
          <p class="text-sm font-medium text-gray-100 truncate">{{ authStore.user?.fullName }}</p>
          <p class="text-xs text-gray-500 truncate">{{ authStore.user?.role }}</p>
        </div>
        <button @click="authStore.logout()" class="text-gray-500 hover:text-gray-300 transition-colors">
          <Icon name="heroicons:arrow-right-on-rectangle" class="w-4 h-4" />
        </button>
      </div>
    </div>
  </aside>
</template>
<script setup lang="ts">
const authStore = useAuthStore()
const nav = [
  { to: '/dashboard', label: 'Dashboard', icon: 'heroicons:chart-bar' },
  { to: '/calls', label: 'Calls', icon: 'heroicons:microphone' },
  { to: '/accounts', label: 'Accounts', icon: 'heroicons:building-office-2' },
  { to: '/contacts', label: 'Contacts', icon: 'heroicons:user-group' },
  { to: '/opportunities', label: 'Opportunities', icon: 'heroicons:currency-euro' },
  { to: '/scorecards', label: 'Scorecards', icon: 'heroicons:clipboard-document-check' },
  { to: '/search', label: 'Ask AI', icon: 'heroicons:sparkles' },
]
const settingsNav = [
  { to: '/settings/users', label: 'Users', icon: 'heroicons:users' },
  { to: '/settings/teams', label: 'Teams', icon: 'heroicons:user-group' },
  { to: '/settings/integrations', label: 'Integrations', icon: 'heroicons:puzzle-piece' },
]
const userInitials = computed(() => (authStore.user?.fullName ?? '').split(' ').map(n => n[0]).slice(0, 2).join('').toUpperCase())
</script>
