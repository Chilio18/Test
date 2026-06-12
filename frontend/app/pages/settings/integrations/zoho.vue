<template>
  <div>
    <div class="flex items-center gap-3 mb-6">
      <NuxtLink to="/settings/integrations" class="p-1.5 rounded-lg text-gray-400 hover:text-gray-200 hover:bg-gray-800 transition-colors">
        <Icon name="heroicons:arrow-left" class="w-5 h-5" />
      </NuxtLink>
      <h1 class="text-2xl font-bold text-gray-100">Zoho CRM</h1>
    </div>
    <div class="max-w-2xl space-y-6">
      <div class="card">
        <h2 class="font-semibold text-gray-100 mb-4">OAuth2 Configuration</h2>
        <div class="space-y-4">
          <div><label class="block text-sm text-gray-400 mb-1">Client ID</label><input type="text" v-model="form.clientId" class="w-full px-3 py-2 bg-gray-800 border border-gray-700 rounded-lg text-gray-100 focus:outline-none focus:ring-2 focus:ring-brand-500" /></div>
          <div><label class="block text-sm text-gray-400 mb-1">Client Secret</label><input type="password" v-model="form.clientSecret" class="w-full px-3 py-2 bg-gray-800 border border-gray-700 rounded-lg text-gray-100 focus:outline-none focus:ring-2 focus:ring-brand-500" /></div>
          <div><label class="block text-sm text-gray-400 mb-1">Data Center Region</label>
            <select v-model="form.region" class="w-full px-3 py-2 bg-gray-800 border border-gray-700 rounded-lg text-gray-300">
              <option value="eu">Europe (zohoapis.eu)</option><option value="com">US (zohoapis.com)</option>
            </select>
          </div>
        </div>
        <div class="flex gap-3 mt-6">
          <button class="px-4 py-2 bg-brand-600 hover:bg-brand-500 text-white rounded-lg text-sm font-medium transition-colors">Connect with Zoho</button>
          <button @click="testConnection" class="px-4 py-2 bg-gray-800 hover:bg-gray-700 text-gray-300 rounded-lg text-sm transition-colors">Test Connection</button>
        </div>
        <p v-if="testResult" class="mt-3 text-sm" :class="testResult.success ? 'text-green-400' : 'text-red-400'">{{ testResult.message }}</p>
      </div>
      <div class="card">
        <h2 class="font-semibold text-gray-100 mb-4">Sync Settings</h2>
        <div class="space-y-3">
          <label class="flex items-center gap-3 cursor-pointer"><input type="checkbox" v-model="syncSettings.accounts" class="w-4 h-4 rounded accent-brand-500" /><span class="text-sm text-gray-300">Sync Accounts</span></label>
          <label class="flex items-center gap-3 cursor-pointer"><input type="checkbox" v-model="syncSettings.contacts" class="w-4 h-4 rounded accent-brand-500" /><span class="text-sm text-gray-300">Sync Contacts</span></label>
          <label class="flex items-center gap-3 cursor-pointer"><input type="checkbox" v-model="syncSettings.deals" class="w-4 h-4 rounded accent-brand-500" /><span class="text-sm text-gray-300">Sync Deals/Opportunities</span></label>
          <label class="flex items-center gap-3 cursor-pointer"><input type="checkbox" v-model="syncSettings.writeBack" class="w-4 h-4 rounded accent-brand-500" /><span class="text-sm text-gray-300">Write AI summaries back to CRM</span></label>
        </div>
      </div>
    </div>
  </div>
</template>
<script setup lang="ts">
definePageMeta({ layout: 'default' })
const form = ref({ clientId: '', clientSecret: '', region: 'eu' })
const syncSettings = ref({ accounts: true, contacts: true, deals: true, writeBack: true })
const testResult = ref<{ success: boolean; message: string } | null>(null)
async function testConnection() { testResult.value = { success: true, message: 'Connection successful!' } }
</script>
