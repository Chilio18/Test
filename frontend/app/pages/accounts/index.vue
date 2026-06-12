<template>
  <div>
    <PageHeader title="Accounts" subtitle="Customer accounts overview">
      <button class="flex items-center gap-2 px-4 py-2 bg-brand-600 hover:bg-brand-500 text-white rounded-lg text-sm font-medium transition-colors">
        <Icon name="heroicons:plus" class="w-4 h-4" /> New Account
      </button>
    </PageHeader>
    <div class="mb-4">
      <input v-model="search" type="text" placeholder="Search accounts..." class="w-full max-w-sm px-3 py-2 bg-gray-800 border border-gray-700 rounded-lg text-gray-100 placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-brand-500" />
    </div>
    <LoadingSpinner v-if="loading" label="Loading accounts..." class="py-20" />
    <div v-else-if="accounts.length > 0" class="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
      <NuxtLink v-for="account in accounts" :key="account.id" :to="`/accounts/${account.id}`"
        class="card hover:border-gray-700 transition-colors block group cursor-pointer">
        <div class="flex items-start justify-between mb-3">
          <div class="w-10 h-10 rounded-xl bg-gray-800 flex items-center justify-center text-lg font-bold text-gray-300">
            {{ account.name.charAt(0) }}
          </div>
          <span v-if="account.industry" class="text-xs text-gray-500 bg-gray-800 px-2 py-0.5 rounded-full">{{ account.industry }}</span>
        </div>
        <h3 class="font-semibold text-gray-100 group-hover:text-brand-400 transition-colors">{{ account.name }}</h3>
        <div class="mt-2 space-y-1 text-sm text-gray-400">
          <p v-if="account.website" class="flex items-center gap-1"><Icon name="heroicons:globe-alt" class="w-3.5 h-3.5" />{{ account.website }}</p>
          <p v-if="account.country" class="flex items-center gap-1"><Icon name="heroicons:map-pin" class="w-3.5 h-3.5" />{{ account.country }}</p>
        </div>
      </NuxtLink>
    </div>
    <EmptyState v-else icon="heroicons:building-office-2" title="No accounts" description="Accounts are synced from your CRM or created manually." />
  </div>
</template>
<script setup lang="ts">
import type { Account } from '~/types'
definePageMeta({ layout: 'default' })
const loading = ref(true); const search = ref(''); const accounts = ref<Account[]>([])
onMounted(async () => {
  try { const { get } = useApi(); const r = await get<{ items: Account[] }>('/api/accounts'); accounts.value = r.items } catch {}
  finally { loading.value = false }
})
</script>
