<template>
  <div class="space-y-3">
    <div v-for="(insight, i) in insights" :key="i" class="p-3 rounded-lg border text-sm" :class="insightStyle(insight.type)">
      <div class="flex items-center gap-2 mb-1">
        <Icon :name="insightIcon(insight.type)" class="w-4 h-4 shrink-0" />
        <span class="font-medium text-xs uppercase tracking-wide">{{ insight.type.replace(/([A-Z])/g, ' $1').trim() }}</span>
        <span class="ml-auto text-xs opacity-60">{{ Math.round(insight.confidence * 100) }}%</span>
      </div>
      <p class="text-gray-300">{{ insight.content }}</p>
      <blockquote v-if="insight.quote" class="mt-2 pl-3 border-l-2 border-current opacity-60 italic text-xs">"{{ insight.quote }}"</blockquote>
    </div>
    <div v-if="insights.length === 0" class="text-center text-gray-500 py-8 text-sm">No insights extracted</div>
  </div>
</template>
<script setup lang="ts">
import type { CallInsight } from '~/types'
defineProps<{ insights: CallInsight[] }>()
function insightStyle(type: string) {
  const map: Record<string,string> = { PainPoint:'bg-red-950/30 border-red-900/50 text-red-300', Objection:'bg-orange-950/30 border-orange-900/50 text-orange-300', BuyingSignal:'bg-green-950/30 border-green-900/50 text-green-300', CompetitorMention:'bg-yellow-950/30 border-yellow-900/50 text-yellow-300', BudgetDiscussion:'bg-blue-950/30 border-blue-900/50 text-blue-300', TechnicalRequirement:'bg-purple-950/30 border-purple-900/50 text-purple-300', Risk:'bg-red-950/30 border-red-800/50 text-red-300', Opportunity:'bg-teal-950/30 border-teal-900/50 text-teal-300' }
  return map[type] ?? 'bg-gray-800 border-gray-700 text-gray-300'
}
function insightIcon(type: string) {
  const map: Record<string,string> = { PainPoint:'heroicons:exclamation-triangle', Objection:'heroicons:hand-raised', BuyingSignal:'heroicons:check-badge', CompetitorMention:'heroicons:flag', BudgetDiscussion:'heroicons:currency-euro', TechnicalRequirement:'heroicons:cpu-chip', Risk:'heroicons:shield-exclamation', Opportunity:'heroicons:light-bulb' }
  return map[type] ?? 'heroicons:information-circle'
}
</script>
