<template>
  <div>
    <PageHeader title="Dashboard" subtitle="Revenue intelligence overview">
      <select v-model="period" class="px-3 py-1.5 bg-gray-800 border border-gray-700 rounded-lg text-sm text-gray-300">
        <option value="7">Last 7 days</option>
        <option value="30">Last 30 days</option>
        <option value="90">Last 90 days</option>
      </select>
    </PageHeader>

    <LoadingSpinner v-if="store.isLoading" label="Loading dashboard..." class="py-20" />

    <template v-else-if="store.leadership">
      <!-- KPI row -->
      <div class="grid grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
        <MetricCard label="Total Calls" :value="store.leadership.totalCalls" icon="heroicons:microphone" iconBg="bg-blue-600/20" iconColor="text-blue-400" />
        <MetricCard label="Pipeline Value" :value="store.leadership.totalPipelineValue" format="currency" icon="heroicons:currency-euro" iconBg="bg-green-600/20" iconColor="text-green-400" />
        <MetricCard label="High Risk Deals" :value="store.leadership.highRiskDeals" icon="heroicons:shield-exclamation" iconBg="bg-red-600/20" iconColor="text-red-400" />
        <MetricCard label="Avg Call Duration" :value="store.leadership.averageCallDurationMinutes * 60" format="duration" icon="heroicons:clock" iconBg="bg-purple-600/20" iconColor="text-purple-400" />
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <!-- Deals at Risk -->
        <div class="lg:col-span-2 card">
          <h2 class="font-semibold text-gray-100 mb-4 flex items-center gap-2">
            <Icon name="heroicons:exclamation-triangle" class="w-5 h-5 text-red-400" />
            Deals at Risk
          </h2>
          <div class="space-y-3">
            <div v-for="deal in store.leadership.dealsAtRisk" :key="deal.opportunityId"
              class="flex items-center justify-between p-3 bg-gray-800/50 rounded-lg hover:bg-gray-800 transition-colors cursor-pointer"
              @click="navigateTo(`/opportunities/${deal.opportunityId}`)">
              <div class="flex-1 min-w-0">
                <p class="font-medium text-gray-100 truncate">{{ deal.name }}</p>
                <p class="text-sm text-gray-400">{{ deal.accountName }}</p>
                <div class="flex flex-wrap gap-1 mt-1">
                  <span v-for="reason in deal.riskReasons" :key="reason" class="px-1.5 py-0.5 bg-red-500/10 text-red-400 text-xs rounded">{{ reason }}</span>
                </div>
              </div>
              <div class="ml-4 text-right shrink-0">
                <p v-if="deal.amount" class="font-semibold text-gray-100">{{ $formatting.formatCurrency(deal.amount) }}</p>
                <div class="flex items-center gap-1 justify-end mt-1">
                  <span class="text-xs text-gray-500">Risk</span>
                  <span class="font-bold text-red-400">{{ deal.riskScore }}</span>
                </div>
              </div>
            </div>
            <div v-if="store.leadership.dealsAtRisk.length === 0" class="text-center text-gray-500 py-8 text-sm">No high-risk deals</div>
          </div>
        </div>

        <!-- Top Performers -->
        <div class="card">
          <h2 class="font-semibold text-gray-100 mb-4 flex items-center gap-2">
            <Icon name="heroicons:trophy" class="w-5 h-5 text-yellow-400" />
            Top Performers
          </h2>
          <div class="space-y-3">
            <div v-for="(rep, i) in store.leadership.topPerformers" :key="rep.userId" class="flex items-center gap-3">
              <div class="w-7 h-7 rounded-full flex items-center justify-center text-xs font-bold"
                :class="['bg-yellow-500 text-yellow-950','bg-gray-400 text-gray-950','bg-orange-600 text-orange-100'][i] ?? 'bg-gray-700 text-gray-300'">
                {{ i + 1 }}
              </div>
              <div class="flex-1 min-w-0">
                <p class="text-sm font-medium text-gray-100 truncate">{{ rep.name }}</p>
                <p class="text-xs text-gray-500">{{ rep.callCount }} calls · avg {{ rep.averageScore }}pts</p>
              </div>
              <ScoreRing :score="rep.averageScore" :max="10" :size="40" :stroke-width="4" />
            </div>
            <div v-if="store.leadership.topPerformers.length === 0" class="text-center text-gray-500 py-8 text-sm">No data yet</div>
          </div>
        </div>
      </div>
    </template>

    <EmptyState v-else icon="heroicons:chart-bar" title="No dashboard data" description="Start by recording and uploading calls." />
  </div>
</template>
<script setup lang="ts">
definePageMeta({ layout: 'default' })
const store = useDashboardStore()
const { formatCurrency } = useFormatting()
provide('$formatting', { formatCurrency })
const period = ref('30')
async function load() {
  const to = new Date().toISOString()
  const from = new Date(Date.now() - Number(period.value) * 86400000).toISOString()
  await store.fetchLeadershipDashboard(from, to)
}
watch(period, load)
onMounted(load)
</script>
