<template>
  <div>
    <PageHeader title="Opportunities" subtitle="Active pipeline overview">
      <button @click="showFilter = !showFilter" class="flex items-center gap-2 px-3 py-1.5 bg-gray-800 border border-gray-700 text-gray-300 rounded-lg text-sm hover:bg-gray-700 transition-colors">
        <Icon name="heroicons:funnel" class="w-4 h-4" /> Filters
      </button>
    </PageHeader>

    <!-- Pipeline Kanban-style summary -->
    <div class="grid grid-cols-2 md:grid-cols-4 gap-4 mb-6">
      <div v-for="stage in stageSummary" :key="stage.name" class="card-sm">
        <p class="text-xs text-gray-500 truncate">{{ stage.name }}</p>
        <p class="text-xl font-bold text-gray-100 mt-1">{{ stage.count }}</p>
        <p class="text-xs text-gray-400">{{ formatCurrency(stage.value) }}</p>
      </div>
    </div>

    <LoadingSpinner v-if="loading" label="Loading opportunities..." class="py-20" />

    <div v-else-if="opportunities.length > 0" class="space-y-3">
      <NuxtLink v-for="opp in opportunities" :key="opp.id" :to="`/opportunities/${opp.id}`"
        class="card hover:border-gray-700 transition-colors block">
        <div class="flex items-center gap-4">
          <div class="flex-1 min-w-0">
            <div class="flex items-center gap-2 mb-1">
              <StatusBadge :status="opp.stage" type="opportunity" />
              <span v-if="opp.isStalled" class="badge-status bg-orange-500/20 text-orange-400">Stalled</span>
            </div>
            <h3 class="font-medium text-gray-100 truncate">{{ opp.name }}</h3>
            <p class="text-sm text-gray-400">{{ opp.accountName }} · {{ opp.ownerName }}</p>
          </div>
          <div class="text-right shrink-0">
            <p v-if="opp.amount" class="font-semibold text-gray-100">{{ formatCurrency(opp.amount) }}</p>
            <p v-if="opp.closeDate" class="text-xs text-gray-500 mt-0.5">Close {{ formatDate(opp.closeDate) }}</p>
          </div>
          <div class="shrink-0 ml-2">
            <ScoreRing :score="opp.dealHealthScore" :size="52" :stroke-width="4" label="Health" />
          </div>
        </div>
        <!-- Flags -->
        <div class="flex gap-2 mt-3">
          <span class="text-xs px-2 py-0.5 rounded" :class="opp.hasNextStep ? 'bg-green-500/10 text-green-400' : 'bg-red-500/10 text-red-400'">
            {{ opp.hasNextStep ? '✓' : '✗' }} Next step
          </span>
          <span class="text-xs px-2 py-0.5 rounded" :class="opp.hasBudgetDiscussion ? 'bg-green-500/10 text-green-400' : 'bg-red-500/10 text-red-400'">
            {{ opp.hasBudgetDiscussion ? '✓' : '✗' }} Budget
          </span>
          <span class="text-xs px-2 py-0.5 rounded" :class="opp.hasDecisionMaker ? 'bg-green-500/10 text-green-400' : 'bg-red-500/10 text-red-400'">
            {{ opp.hasDecisionMaker ? '✓' : '✗' }} Decision maker
          </span>
        </div>
      </NuxtLink>
    </div>

    <EmptyState v-else icon="heroicons:currency-euro" title="No opportunities" description="Opportunities are synced from your CRM." />
  </div>
</template>
<script setup lang="ts">
import type { Opportunity } from '~/types'
definePageMeta({ layout: 'default' })
const { formatCurrency, formatDate } = useFormatting()
const loading = ref(true); const showFilter = ref(false); const opportunities = ref<Opportunity[]>([])
const stageSummary = computed(() => {
  const stages = ['Prospecting','Qualification','ProposalQuote','ClosedWon']
  return stages.map(name => ({ name, count: opportunities.value.filter(o => o.stage === name).length, value: opportunities.value.filter(o => o.stage === name).reduce((s, o) => s + (o.amount ?? 0), 0) }))
})
onMounted(async () => { try { const { get } = useApi(); const r = await get<{ items: Opportunity[] }>('/api/opportunities'); opportunities.value = r.items } catch {} finally { loading.value = false } })
</script>
