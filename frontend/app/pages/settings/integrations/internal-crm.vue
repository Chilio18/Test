<template>
  <div>
    <div class="flex items-center gap-3 mb-6">
      <NuxtLink to="/settings/integrations" class="p-1.5 rounded-lg text-gray-400 hover:text-gray-200 hover:bg-gray-800 transition-colors">
        <Icon name="heroicons:arrow-left" class="w-5 h-5" />
      </NuxtLink>
      <h1 class="text-2xl font-bold text-gray-100">Internal CRM Integration</h1>
    </div>
    <div class="max-w-2xl space-y-6">
      <div class="card">
        <h2 class="font-semibold text-gray-100 mb-4">API Configuration</h2>
        <div class="space-y-4">
          <div><label class="block text-sm text-gray-400 mb-1">Base URL</label><input type="url" v-model="form.baseUrl" placeholder="https://crm.unameit.nl" class="w-full px-3 py-2 bg-gray-800 border border-gray-700 rounded-lg text-gray-100 focus:outline-none focus:ring-2 focus:ring-brand-500" /></div>
          <div><label class="block text-sm text-gray-400 mb-1">Authentication Type</label>
            <select v-model="form.authType" class="w-full px-3 py-2 bg-gray-800 border border-gray-700 rounded-lg text-gray-300">
              <option value="ApiKey">API Key</option><option value="OAuth2">OAuth2</option>
            </select>
          </div>
          <div v-if="form.authType === 'ApiKey'"><label class="block text-sm text-gray-400 mb-1">API Key</label><input type="password" v-model="form.apiKey" class="w-full px-3 py-2 bg-gray-800 border border-gray-700 rounded-lg text-gray-100 focus:outline-none focus:ring-2 focus:ring-brand-500" /></div>
        </div>
        <div class="flex gap-3 mt-6">
          <button class="px-4 py-2 bg-brand-600 hover:bg-brand-500 text-white rounded-lg text-sm font-medium transition-colors">Save Configuration</button>
          <button class="px-4 py-2 bg-gray-800 hover:bg-gray-700 text-gray-300 rounded-lg text-sm transition-colors">Test Connection</button>
        </div>
      </div>
      <div class="card">
        <h2 class="font-semibold text-gray-100 mb-4">Endpoint Mapping</h2>
        <div class="space-y-3 text-sm">
          <div v-for="ep in endpoints" :key="ep.key" class="flex items-center gap-3">
            <span class="text-gray-400 w-32 shrink-0">{{ ep.label }}</span>
            <input type="text" v-model="ep.value" class="flex-1 px-2 py-1.5 bg-gray-800 border border-gray-700 rounded text-gray-100 focus:outline-none focus:ring-1 focus:ring-brand-500" />
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
<script setup lang="ts">
definePageMeta({ layout: 'default' })
const form = ref({ baseUrl: '', authType: 'ApiKey', apiKey: '' })
const endpoints = ref([
  { key: 'accounts', label: 'Accounts', value: '/api/accounts' },
  { key: 'contacts', label: 'Contacts', value: '/api/contacts' },
  { key: 'opportunities', label: 'Opportunities', value: '/api/opportunities' },
  { key: 'notes', label: 'Notes', value: '/api/notes' },
  { key: 'tasks', label: 'Tasks', value: '/api/tasks' },
])
</script>
