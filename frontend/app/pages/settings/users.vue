<template>
  <div>
    <PageHeader title="Users" subtitle="Manage team members and permissions">
      <button class="flex items-center gap-2 px-4 py-2 bg-brand-600 hover:bg-brand-500 text-white rounded-lg text-sm font-medium transition-colors">
        <Icon name="heroicons:plus" class="w-4 h-4" /> Invite User
      </button>
    </PageHeader>
    <div class="card">
      <table class="min-w-full">
        <thead>
          <tr class="text-xs text-gray-500 uppercase tracking-wider border-b border-gray-800">
            <th class="pb-3 text-left">User</th><th class="pb-3 text-left">Role</th><th class="pb-3 text-left">Last Login</th><th class="pb-3"></th>
          </tr>
        </thead>
        <tbody class="divide-y divide-gray-800">
          <tr v-for="user in mockUsers" :key="user.email" class="hover:bg-gray-800/30 transition-colors">
            <td class="py-3">
              <div class="flex items-center gap-3">
                <div class="w-8 h-8 rounded-full bg-brand-600 flex items-center justify-center text-sm font-medium text-white">{{ user.name.charAt(0) }}</div>
                <div><p class="text-sm font-medium text-gray-100">{{ user.name }}</p><p class="text-xs text-gray-500">{{ user.email }}</p></div>
              </div>
            </td>
            <td class="py-3"><span class="badge-status bg-gray-700/50 text-gray-300">{{ user.role }}</span></td>
            <td class="py-3 text-sm text-gray-400">{{ user.lastLogin }}</td>
            <td class="py-3 text-right"><button class="text-gray-500 hover:text-gray-300"><Icon name="heroicons:ellipsis-horizontal" class="w-5 h-5" /></button></td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>
<script setup lang="ts">
definePageMeta({ layout: 'default' })
const mockUsers = [
  { name: 'Michiel Borst', email: 'michiel@unameit.nl', role: 'PlatformAdmin', lastLogin: '2 hours ago' },
  { name: 'Jan de Vries', email: 'jan@unameit.nl', role: 'SalesManager', lastLogin: '1 day ago' },
  { name: 'Anna Bakker', email: 'anna@unameit.nl', role: 'SalesRepresentative', lastLogin: '3 days ago' },
]
</script>
