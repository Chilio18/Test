<template>
  <div class="flex items-center justify-center" :class="fullPage && 'min-h-64'">
    <div class="rounded-full border-2 border-current border-t-transparent animate-spin text-brand-400"
      :class="size === 'sm' ? 'w-4 h-4' : size === 'lg' ? 'w-10 h-10' : 'w-6 h-6'" />
    <span v-if="label" class="ml-3 text-sm text-gray-400">{{ label }}</span>
  </div>
</template>
<script setup lang="ts">
withDefaults(defineProps<{ size?: 'sm' | 'md' | 'lg'; label?: string; fullPage?: boolean }>(), { size: 'md' })
</script>
