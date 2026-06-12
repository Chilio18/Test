<template>
  <div class="relative inline-flex items-center justify-center">
    <svg :width="size" :height="size" class="transform -rotate-90">
      <circle :cx="size/2" :cy="size/2" :r="radius" fill="none" stroke="currentColor" class="text-gray-800" :stroke-width="strokeWidth" />
      <circle :cx="size/2" :cy="size/2" :r="radius" fill="none" :class="trackClass" :stroke-width="strokeWidth"
        :stroke-dasharray="`${circumference} ${circumference}`" :stroke-dashoffset="dashOffset" stroke-linecap="round"
        style="transition: stroke-dashoffset 0.5s ease-in-out" />
    </svg>
    <div class="absolute inset-0 flex flex-col items-center justify-center">
      <span class="font-bold" :class="[size >= 80 ? 'text-xl' : 'text-sm', scoreColor(score)]">{{ score != null ? score : '—' }}</span>
      <span v-if="label" class="text-xs text-gray-500">{{ label }}</span>
    </div>
  </div>
</template>
<script setup lang="ts">
const props = withDefaults(defineProps<{ score: number | null | undefined; max?: number; size?: number; strokeWidth?: number; label?: string }>(), { max: 100, size: 80, strokeWidth: 6 })
const { scoreColor } = useFormatting()
const radius = computed(() => (props.size - props.strokeWidth) / 2)
const circumference = computed(() => 2 * Math.PI * radius.value)
const dashOffset = computed(() => props.score != null ? circumference.value * (1 - props.score / props.max) : circumference.value)
const trackClass = computed(() => {
  if (props.score == null) return 'text-gray-700 stroke-current'
  if (props.score >= 80) return 'text-green-500 stroke-current'
  if (props.score >= 60) return 'text-yellow-500 stroke-current'
  if (props.score >= 40) return 'text-orange-500 stroke-current'
  return 'text-red-500 stroke-current'
})
</script>
