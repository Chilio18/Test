<template>
  <div class="flex flex-col items-center justify-center py-20 text-center">
    <div class="w-16 h-16 rounded-2xl bg-gray-800 flex items-center justify-center mb-4">
      <Icon :name="icon" class="w-8 h-8 text-gray-500" />
    </div>
    <h3 class="text-lg font-medium text-gray-200 mb-2">{{ title }}</h3>
    <p class="text-gray-500 max-w-sm mb-6">{{ description }}</p>
    <slot />
  </div>
</template>
<script setup lang="ts">
defineProps<{ icon: string; title: string; description: string }>()
</script>
