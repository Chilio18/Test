<template>
  <div class="card">
    <div class="flex items-start justify-between">
      <div>
        <p class="text-sm text-gray-400">{{ label }}</p>
        <p class="text-3xl font-bold text-gray-100 mt-1">{{ displayValue }}</p>
        <div v-if="trend != null" class="flex items-center gap-1 mt-2 text-sm">
          <Icon :name="trend >= 0 ? 'heroicons:arrow-trending-up' : 'heroicons:arrow-trending-down'" class="w-4 h-4" :class="trend >= 0 ? 'text-green-400' : 'text-red-400'" />
          <span :class="trend >= 0 ? 'text-green-400' : 'text-red-400'">{{ Math.abs(trend) }}% vs last period</span>
        </div>
      </div>
      <div class="w-12 h-12 rounded-xl flex items-center justify-center" :class="iconBg ?? 'bg-brand-600/20'">
        <Icon :name="icon" class="w-6 h-6" :class="iconColor ?? 'text-brand-400'" />
      </div>
    </div>
  </div>
</template>
<script setup lang="ts">
const props = defineProps<{ label: string; value: number | string | null; trend?: number | null; icon: string; iconBg?: string; iconColor?: string; format?: 'number' | 'currency' | 'percentage' | 'duration' }>()
const { formatCurrency, formatPercentage, formatDuration } = useFormatting()
const displayValue = computed(() => {
  if (props.value == null) return '—'
  if (props.format === 'currency') return formatCurrency(Number(props.value))
  if (props.format === 'percentage') return formatPercentage(Number(props.value))
  if (props.format === 'duration') return formatDuration(Number(props.value))
  return String(props.value)
})
</script>
