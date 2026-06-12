<template>
  <div class="card">
    <h2 class="text-xl font-bold text-gray-100 mb-6">Sign in</h2>
    <div class="space-y-4">
      <div>
        <label class="block text-sm text-gray-400 mb-1">Email</label>
        <input v-model="email" type="email" placeholder="you@unameit.nl"
          class="w-full px-3 py-2 bg-gray-800 border border-gray-700 rounded-lg text-gray-100 placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-brand-500" />
      </div>
      <div>
        <label class="block text-sm text-gray-400 mb-1">Password</label>
        <input v-model="password" type="password" placeholder="••••••••"
          class="w-full px-3 py-2 bg-gray-800 border border-gray-700 rounded-lg text-gray-100 placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-brand-500"
          @keyup.enter="login" />
      </div>
      <button @click="login" :disabled="loading"
        class="w-full py-2 px-4 bg-brand-600 hover:bg-brand-500 disabled:opacity-50 text-white rounded-lg font-medium transition-colors">
        {{ loading ? 'Signing in...' : 'Sign in' }}
      </button>
      <p v-if="error" class="text-red-400 text-sm text-center">{{ error }}</p>
    </div>
    <div class="mt-4 pt-4 border-t border-gray-800">
      <button class="w-full flex items-center justify-center gap-2 py-2 px-4 bg-gray-800 hover:bg-gray-700 text-gray-300 rounded-lg transition-colors text-sm">
        <Icon name="heroicons:building-office" class="w-4 h-4" />
        Sign in with Microsoft
      </button>
    </div>
  </div>
</template>
<script setup lang="ts">
definePageMeta({ layout: 'auth' })
const email = ref(''); const password = ref(''); const loading = ref(false); const error = ref('')
const authStore = useAuthStore()
async function login() {
  loading.value = true; error.value = ''
  try {
    // In production, call OIDC flow. For dev, mock token.
    authStore.setUser({ userId: crypto.randomUUID(), tenantId: crypto.randomUUID(), email: email.value, fullName: email.value, role: 'SalesManager', avatarUrl: null }, 'mock-token')
    await navigateTo('/dashboard')
  } catch { error.value = 'Invalid credentials' } finally { loading.value = false }
}
</script>
