<template>
  <NuxtLink :to="`/calls/${call.id}`" class="card hover:border-gray-700 transition-colors block cursor-pointer group">
    <div class="flex items-start justify-between gap-4">
      <div class="flex-1 min-w-0">
        <div class="flex items-center gap-2 mb-1">
          <StatusBadge :status="call.status" type="call" />
          <span class="text-xs text-gray-500">{{ call.type }}</span>
        </div>
        <h3 class="font-medium text-gray-100 group-hover:text-brand-400 transition-colors truncate">{{ call.title }}</h3>
        <div class="flex items-center gap-3 mt-2 text-sm text-gray-400 flex-wrap">
          <span v-if="call.accountName" class="flex items-center gap-1"><Icon name="heroicons:building-office-2" class="w-3.5 h-3.5" />{{ call.accountName }}</span>
          <span class="flex items-center gap-1"><Icon name="heroicons:user" class="w-3.5 h-3.5" />{{ call.ownerName }}</span>
          <span class="flex items-center gap-1"><Icon name="heroicons:clock" class="w-3.5 h-3.5" />{{ formatDate(call.meetingDate) }}</span>
          <span v-if="call.durationSeconds" class="flex items-center gap-1"><Icon name="heroicons:play" class="w-3.5 h-3.5" />{{ formatDuration(call.durationSeconds) }}</span>
        </div>
      </div>
      <div v-if="call.dealHealthScore != null" class="shrink-0">
        <ScoreRing :score="call.dealHealthScore" :size="52" :stroke-width="4" label="Health" />
      </div>
    </div>
    <div v-if="call.actionItemCount > 0" class="mt-3 flex items-center gap-1.5 text-xs text-amber-400">
      <Icon name="heroicons:check-circle" class="w-3.5 h-3.5" />
      {{ call.actionItemCount }} action {{ call.actionItemCount === 1 ? 'item' : 'items' }}
    </div>
  </NuxtLink>
</template>
<script setup lang="ts">
import type { Call } from '~/types'
defineProps<{ call: Call }>()
const { formatDate, formatDuration } = useFormatting()
</script>
