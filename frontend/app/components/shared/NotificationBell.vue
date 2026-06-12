<template>
  <div class="relative" ref="container">
    <button @click="open = !open" class="relative p-2 rounded-lg text-gray-400 hover:text-gray-200 hover:bg-gray-800 transition-colors">
      <Icon name="heroicons:bell" class="w-5 h-5" />
      <span v-if="store.unreadCount > 0" class="absolute top-1 right-1 w-4 h-4 bg-red-500 rounded-full text-[10px] font-bold text-white flex items-center justify-center">
        {{ store.unreadCount > 9 ? '9+' : store.unreadCount }}
      </span>
    </button>
    <Transition name="dropdown">
      <div v-if="open" class="absolute right-0 mt-2 w-80 bg-gray-900 border border-gray-800 rounded-xl shadow-2xl z-50">
        <div class="flex items-center justify-between p-4 border-b border-gray-800">
          <span class="font-medium text-gray-100">Notifications</span>
          <button @click="store.markAllRead()" class="text-xs text-brand-400 hover:text-brand-300">Mark all read</button>
        </div>
        <div class="max-h-96 overflow-y-auto scrollbar-thin divide-y divide-gray-800">
          <div v-for="n in store.notifications.slice(0, 10)" :key="n.id" @click="handleClick(n)"
            class="p-4 hover:bg-gray-800 cursor-pointer transition-colors" :class="!n.isRead && 'bg-blue-950/20'">
            <div class="flex items-start gap-3">
              <div class="w-2 h-2 mt-1.5 rounded-full shrink-0" :class="n.isRead ? 'bg-gray-600' : 'bg-brand-400'" />
              <div>
                <p class="text-sm font-medium text-gray-100">{{ n.title }}</p>
                <p v-if="n.body" class="text-xs text-gray-400 mt-0.5 line-clamp-2">{{ n.body }}</p>
              </div>
            </div>
          </div>
          <div v-if="store.notifications.length === 0" class="p-8 text-center text-gray-500 text-sm">No notifications</div>
        </div>
      </div>
    </Transition>
  </div>
</template>
<script setup lang="ts">
import type { Notification } from '~/types'
const store = useNotificationStore()
const open = ref(false)
const container = ref<HTMLElement>()
function handleClick(n: Notification) { store.markRead(n.id); if (n.actionUrl) navigateTo(n.actionUrl); open.value = false }
onMounted(() => document.addEventListener('click', (e) => { if (container.value && !container.value.contains(e.target as Node)) open.value = false }))
</script>
<style scoped>
.dropdown-enter-active, .dropdown-leave-active { transition: opacity 0.1s ease, transform 0.1s ease; }
.dropdown-enter-from, .dropdown-leave-to { opacity: 0; transform: translateY(-4px); }
</style>
