<template>
  <div class="flex items-center justify-between mb-6">
    <div>
      <h1 class="text-2xl font-bold text-gray-100">{{ title }}</h1>
      <p v-if="subtitle" class="text-gray-400 mt-1">{{ subtitle }}</p>
    </div>
    <div class="flex items-center gap-3"><slot /></div>
  </div>
</template>
<script setup lang="ts">
defineProps<{ title: string; subtitle?: string }>()
</script>
