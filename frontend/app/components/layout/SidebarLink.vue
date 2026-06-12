<template>
  <NuxtLink :to="to" class="flex items-center gap-3 px-3 py-2 rounded-lg text-sm transition-colors"
    :class="isActive ? 'bg-brand-600/20 text-brand-400 font-medium' : 'text-gray-400 hover:text-gray-100 hover:bg-gray-800'">
    <Icon :name="icon" class="w-4 h-4 shrink-0" />
    <span>{{ label }}</span>
  </NuxtLink>
</template>
<script setup lang="ts">
const props = defineProps<{ to: string; label: string; icon: string }>()
const route = useRoute()
const isActive = computed(() => route.path === props.to || (props.to !== '/dashboard' && route.path.startsWith(props.to)))
</script>
