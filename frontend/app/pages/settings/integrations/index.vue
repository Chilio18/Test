<template>
  <div>
    <PageHeader title="Integrations" subtitle="Connect CRM and other services" />
    <div class="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
      <NuxtLink v-for="integration in integrations" :key="integration.id" :to="integration.href" class="card hover:border-gray-700 transition-colors block group">
        <div class="flex items-start justify-between mb-3">
          <div class="w-12 h-12 rounded-xl flex items-center justify-center text-2xl" :class="integration.bg">{{ integration.icon }}</div>
          <span class="badge-status" :class="integration.connected ? 'bg-green-500/20 text-green-400' : 'bg-gray-500/20 text-gray-500'">
            {{ integration.connected ? 'Connected' : 'Not connected' }}
          </span>
        </div>
        <h3 class="font-semibold text-gray-100 group-hover:text-brand-400 transition-colors">{{ integration.name }}</h3>
        <p class="text-sm text-gray-400 mt-1">{{ integration.description }}</p>
      </NuxtLink>
    </div>
  </div>
</template>
<script setup lang="ts">
definePageMeta({ layout: 'default' })
const integrations = [
  { id: 'zoho', name: 'Zoho CRM', icon: 'Z', bg: 'bg-red-600/20', description: 'Sync accounts, contacts, deals and activities', connected: false, href: '/settings/integrations/zoho' },
  { id: 'internal-crm', name: 'Internal CRM', icon: '🏢', bg: 'bg-blue-600/20', description: 'Connect to the UnameIT proprietary CRM platform', connected: false, href: '/settings/integrations/internal-crm' },
  { id: 'ai', name: 'AI Providers', icon: '🤖', bg: 'bg-purple-600/20', description: 'Configure OpenAI, Anthropic, or Azure OpenAI', connected: true, href: '/settings/integrations' },
]
</script>
