<template>
  <span class="badge-status" :class="colorClass">{{ label ?? status }}</span>
</template>
<script setup lang="ts">
const props = defineProps<{ status: string; type?: 'call' | 'opportunity' | 'action'; label?: string }>()
const { callStatusColor, stageColor } = useFormatting()
const colorClass = computed(() => {
  if (props.type === 'call') return callStatusColor(props.status)
  if (props.type === 'opportunity') return stageColor(props.status)
  return 'bg-gray-500/20 text-gray-400'
})
</script>
