<template>
  <div class="flex items-center gap-2">
    <span class="text-xs text-gray-400 w-28 shrink-0">{{ label }}</span>
    <div class="flex-1 h-1.5 bg-gray-800 rounded-full overflow-hidden">
      <div class="h-full rounded-full transition-all duration-500" :class="barColor" :style="{ width: `${score * 10}%` }" />
    </div>
    <span class="text-xs font-medium w-8 text-right" :class="textColor">{{ score }}</span>
  </div>
</template>
<script setup lang="ts">
const props = defineProps<{ label: string; score: number }>()
const barColor = computed(() => props.score >= 8 ? 'bg-green-500' : props.score >= 6 ? 'bg-yellow-500' : props.score >= 4 ? 'bg-orange-500' : 'bg-red-500')
const textColor = computed(() => props.score >= 8 ? 'text-green-400' : props.score >= 6 ? 'text-yellow-400' : props.score >= 4 ? 'text-orange-400' : 'text-red-400')
</script>
