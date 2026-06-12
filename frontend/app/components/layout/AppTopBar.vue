<template>
  <header class="h-16 bg-gray-900 border-b border-gray-800 flex items-center justify-between px-6 shrink-0">
    <h1 class="text-lg font-semibold text-gray-100">{{ pageTitle }}</h1>
    <div class="flex items-center gap-3">
      <button @click="navigateTo('/search')" class="flex items-center gap-2 px-3 py-1.5 bg-gray-800 rounded-lg text-sm text-gray-400 hover:text-gray-200 transition-colors">
        <Icon name="heroicons:magnifying-glass" class="w-4 h-4" />
        <span class="hidden sm:inline">Ask AI...</span>
      </button>
      <NotificationBell />
    </div>
  </header>
</template>
<script setup lang="ts">
const route = useRoute()
const titles: Record<string, string> = { '/dashboard': 'Dashboard', '/calls': 'Calls', '/accounts': 'Accounts', '/contacts': 'Contacts', '/opportunities': 'Opportunities', '/scorecards': 'Scorecards', '/search': 'Ask AI', '/settings/users': 'Users', '/settings/teams': 'Teams', '/settings/integrations': 'Integrations' }
const pageTitle = computed(() => {
  for (const [path, title] of Object.entries(titles))
    if (route.path === path || route.path.startsWith(path + '/')) return title
  return 'Revenue Intelligence'
})
</script>
