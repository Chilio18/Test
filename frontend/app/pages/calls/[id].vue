<template>
  <div>
    <LoadingSpinner v-if="store.isLoadingDetail" label="Loading call..." fullPage />

    <template v-else-if="call">
      <!-- Header -->
      <div class="flex items-start gap-4 mb-6">
        <button @click="navigateTo('/calls')" class="mt-1 p-1.5 rounded-lg text-gray-400 hover:text-gray-200 hover:bg-gray-800 transition-colors">
          <Icon name="heroicons:arrow-left" class="w-5 h-5" />
        </button>
        <div class="flex-1 min-w-0">
          <div class="flex items-center gap-2 mb-1">
            <StatusBadge :status="call.status" type="call" />
            <span class="text-sm text-gray-500">{{ call.type }} · {{ formatDate(call.meetingDate) }}</span>
            <span v-if="call.durationSeconds" class="text-sm text-gray-500">· {{ formatDuration(call.durationSeconds) }}</span>
          </div>
          <h1 class="text-2xl font-bold text-gray-100">{{ call.title }}</h1>
          <div class="flex items-center gap-4 mt-2 text-sm text-gray-400">
            <span class="flex items-center gap-1"><Icon name="heroicons:user" class="w-4 h-4" />{{ call.owner.fullName }}</span>
            <span v-if="call.account" class="flex items-center gap-1"><Icon name="heroicons:building-office-2" class="w-4 h-4" />{{ call.account.name }}</span>
            <span v-if="call.opportunity" class="flex items-center gap-1"><Icon name="heroicons:currency-euro" class="w-4 h-4" />{{ call.opportunity.name }}</span>
          </div>
        </div>
        <!-- CRM Actions -->
        <div class="flex gap-2 shrink-0">
          <button @click="syncToCrm" :disabled="syncing" class="flex items-center gap-2 px-3 py-1.5 bg-gray-800 hover:bg-gray-700 border border-gray-700 text-gray-300 rounded-lg text-sm transition-colors disabled:opacity-50">
            <Icon name="heroicons:arrow-path" class="w-4 h-4" :class="syncing && 'animate-spin'" />
            Sync to CRM
          </button>
          <button @click="refreshAnalysis" class="flex items-center gap-2 px-3 py-1.5 bg-gray-800 hover:bg-gray-700 border border-gray-700 text-gray-300 rounded-lg text-sm transition-colors">
            <Icon name="heroicons:sparkles" class="w-4 h-4" />
            Re-analyze
          </button>
        </div>
      </div>

      <div class="grid grid-cols-1 xl:grid-cols-3 gap-6">
        <!-- Left: Media + Transcript -->
        <div class="xl:col-span-2 space-y-6">
          <!-- Player -->
          <div v-if="call.recordings.length > 0">
            <h2 class="text-sm font-medium text-gray-400 mb-2">Recording</h2>
            <AudioPlayer ref="audioPlayer" :src="call.recordings[0].presignedUrl" @time-update="currentTime = $event" />
          </div>

          <!-- Tabs -->
          <div class="card !p-0 overflow-hidden">
            <div class="flex border-b border-gray-800">
              <button v-for="tab in tabs" :key="tab.id" @click="activeTab = tab.id"
                class="px-4 py-3 text-sm font-medium transition-colors border-b-2"
                :class="activeTab === tab.id ? 'border-brand-500 text-brand-400' : 'border-transparent text-gray-400 hover:text-gray-200'">
                {{ tab.label }}
                <span v-if="tab.count" class="ml-1.5 px-1.5 py-0.5 bg-gray-800 text-gray-400 text-xs rounded-full">{{ tab.count }}</span>
              </button>
            </div>
            <div class="p-4">
              <!-- Transcript -->
              <TranscriptViewer v-if="activeTab === 'transcript'" :segments="call.transcript?.segments ?? []" :current-time="currentTime" @seek="seekAudio" />
              <!-- Summary -->
              <div v-if="activeTab === 'summary' && call.summary" class="space-y-4">
                <div>
                  <h3 class="text-xs font-medium text-gray-500 uppercase tracking-wider mb-2">Executive Summary</h3>
                  <p class="text-gray-300 leading-relaxed">{{ call.summary.executiveSummary }}</p>
                </div>
                <div>
                  <h3 class="text-xs font-medium text-gray-500 uppercase tracking-wider mb-2">Meeting Recap</h3>
                  <pre class="text-gray-300 text-sm whitespace-pre-wrap font-sans">{{ call.summary.meetingRecap }}</pre>
                </div>
                <div class="flex items-center gap-3 p-3 bg-gray-800/50 rounded-lg">
                  <span class="text-sm text-gray-400">Sentiment:</span>
                  <span class="font-medium" :class="call.summary.sentimentLabel === 'Positive' ? 'text-green-400' : call.summary.sentimentLabel === 'Negative' ? 'text-red-400' : 'text-yellow-400'">
                    {{ call.summary.sentimentLabel }} ({{ call.summary.sentimentScore }})
                  </span>
                </div>
              </div>
              <div v-else-if="activeTab === 'summary'" class="text-center text-gray-500 py-8 text-sm">Summary not yet generated</div>
              <!-- Insights -->
              <InsightPanel v-if="activeTab === 'insights'" :insights="call.insights" />
              <!-- Action Items -->
              <div v-if="activeTab === 'actions'" class="space-y-2">
                <div v-for="item in call.actionItems" :key="item.id"
                  class="flex items-start gap-3 p-3 bg-gray-800/50 rounded-lg">
                  <div class="mt-0.5 w-5 h-5 rounded border-2 cursor-pointer shrink-0"
                    :class="item.status === 'Completed' ? 'bg-green-500 border-green-500' : 'border-gray-600'">
                    <Icon v-if="item.status === 'Completed'" name="heroicons:check" class="w-3 h-3 text-white m-0.5" />
                  </div>
                  <div class="flex-1">
                    <p class="text-sm font-medium text-gray-100" :class="item.status === 'Completed' && 'line-through text-gray-500'">{{ item.title }}</p>
                    <p v-if="item.description" class="text-xs text-gray-400 mt-0.5">{{ item.description }}</p>
                    <div class="flex items-center gap-3 mt-1 text-xs text-gray-500">
                      <span v-if="item.assigneeName">→ {{ item.assigneeName }}</span>
                      <span v-if="item.dueDate">Due {{ formatDate(item.dueDate) }}</span>
                      <span v-if="item.isCrmSynced" class="text-green-400 flex items-center gap-0.5"><Icon name="heroicons:check-circle" class="w-3 h-3" />CRM synced</span>
                    </div>
                  </div>
                  <StatusBadge :status="item.status" />
                </div>
                <div v-if="call.actionItems.length === 0" class="text-center text-gray-500 py-8 text-sm">No action items</div>
              </div>
              <!-- Follow-up email -->
              <div v-if="activeTab === 'email'">
                <div v-if="call.followUpEmail" class="space-y-3">
                  <div class="flex justify-end">
                    <button @click="copyEmail" class="text-xs text-brand-400 hover:text-brand-300 flex items-center gap-1">
                      <Icon name="heroicons:clipboard" class="w-3.5 h-3.5" />Copy
                    </button>
                  </div>
                  <pre class="text-sm text-gray-300 whitespace-pre-wrap font-sans bg-gray-800/50 rounded-lg p-4">{{ call.followUpEmail }}</pre>
                </div>
                <div v-else class="text-center text-gray-500 py-8 text-sm">Follow-up email not yet generated</div>
              </div>
            </div>
          </div>
        </div>

        <!-- Right: AI Analysis Panel -->
        <div class="space-y-4">
          <!-- Deal Health -->
          <div v-if="call.dealRisk" class="card">
            <h2 class="font-semibold text-gray-100 mb-4 flex items-center gap-2">
              <Icon name="heroicons:heart" class="w-5 h-5 text-red-400" />
              Deal Intelligence
            </h2>
            <div class="grid grid-cols-3 gap-3 mb-4">
              <div class="text-center">
                <ScoreRing :score="call.dealRisk.healthScore" :size="64" :stroke-width="5" />
                <p class="text-xs text-gray-500 mt-1">Health</p>
              </div>
              <div class="text-center">
                <ScoreRing :score="call.dealRisk.riskScore" :size="64" :stroke-width="5" />
                <p class="text-xs text-gray-500 mt-1">Risk</p>
              </div>
              <div class="text-center">
                <ScoreRing :score="call.dealRisk.confidenceScore" :size="64" :stroke-width="5" />
                <p class="text-xs text-gray-500 mt-1">Confidence</p>
              </div>
            </div>
            <div class="grid grid-cols-3 gap-2 text-center text-xs mb-4">
              <div class="p-2 rounded-lg" :class="call.dealRisk.hasNextStep ? 'bg-green-500/10 text-green-400' : 'bg-red-500/10 text-red-400'">
                <Icon :name="call.dealRisk.hasNextStep ? 'heroicons:check-circle' : 'heroicons:x-circle'" class="w-4 h-4 mx-auto mb-0.5" />
                Next Step
              </div>
              <div class="p-2 rounded-lg" :class="call.dealRisk.hasBudgetDiscussion ? 'bg-green-500/10 text-green-400' : 'bg-red-500/10 text-red-400'">
                <Icon :name="call.dealRisk.hasBudgetDiscussion ? 'heroicons:check-circle' : 'heroicons:x-circle'" class="w-4 h-4 mx-auto mb-0.5" />
                Budget
              </div>
              <div class="p-2 rounded-lg" :class="call.dealRisk.hasDecisionMaker ? 'bg-green-500/10 text-green-400' : 'bg-red-500/10 text-red-400'">
                <Icon :name="call.dealRisk.hasDecisionMaker ? 'heroicons:check-circle' : 'heroicons:x-circle'" class="w-4 h-4 mx-auto mb-0.5" />
                Decision Maker
              </div>
            </div>
            <div v-if="call.dealRisk.risks.length" class="space-y-1">
              <p class="text-xs font-medium text-gray-500 uppercase tracking-wider">Risks</p>
              <div v-for="risk in call.dealRisk.risks" :key="risk" class="flex items-start gap-2 text-sm text-red-300">
                <Icon name="heroicons:exclamation-circle" class="w-4 h-4 shrink-0 mt-0.5" />{{ risk }}
              </div>
            </div>
          </div>

          <!-- Coaching Scores -->
          <div v-if="call.coaching" class="card">
            <h2 class="font-semibold text-gray-100 mb-4 flex items-center gap-2">
              <Icon name="heroicons:academic-cap" class="w-5 h-5 text-purple-400" />
              Coaching Scorecard
            </h2>
            <div class="space-y-2 mb-4">
              <CoachingBar label="Discovery" :score="call.coaching.discoveryScore" />
              <CoachingBar label="Qualification" :score="call.coaching.qualificationScore" />
              <CoachingBar label="Objection Handling" :score="call.coaching.objectionHandlingScore" />
              <CoachingBar label="Closing" :score="call.coaching.closingScore" />
              <CoachingBar label="Communication" :score="call.coaching.communicationScore" />
            </div>
            <div class="flex justify-between items-center p-2 bg-gray-800/50 rounded-lg">
              <span class="text-sm text-gray-400">Overall</span>
              <span class="font-bold text-lg" :class="scoreColor(call.coaching.overallScore * 10)">{{ call.coaching.overallScore }}/10</span>
            </div>
          </div>

          <!-- CRM Suggestions -->
          <div v-if="call.crmSuggestions" class="card">
            <h2 class="font-semibold text-gray-100 mb-4 flex items-center gap-2">
              <Icon name="heroicons:puzzle-piece" class="w-5 h-5 text-cyan-400" />
              CRM Suggestions
            </h2>
            <div class="space-y-3 text-sm">
              <div v-if="call.crmSuggestions.suggestedNextStep" class="p-2 bg-gray-800/50 rounded-lg">
                <p class="text-xs text-gray-500 mb-1">Next Step</p>
                <p class="text-gray-300">{{ call.crmSuggestions.suggestedNextStep }}</p>
              </div>
              <div v-if="call.crmSuggestions.suggestedStageChange" class="p-2 bg-yellow-500/10 border border-yellow-500/20 rounded-lg">
                <p class="text-xs text-yellow-500 mb-1">Suggested Stage Change</p>
                <p class="text-yellow-300">→ {{ call.crmSuggestions.suggestedStageChange }}</p>
              </div>
              <div v-if="call.crmSuggestions.suggestedTasks.length" class="space-y-1">
                <p class="text-xs text-gray-500">Suggested Tasks</p>
                <div v-for="task in call.crmSuggestions.suggestedTasks" :key="task" class="flex items-start gap-2 text-gray-300">
                  <Icon name="heroicons:check-circle" class="w-4 h-4 text-brand-400 shrink-0 mt-0.5" />{{ task }}
                </div>
              </div>
              <button @click="pushToCrm" class="w-full py-2 px-3 bg-brand-600/20 hover:bg-brand-600/30 border border-brand-500/30 text-brand-400 rounded-lg text-sm transition-colors">
                Push Note to CRM
              </button>
            </div>
          </div>
        </div>
      </div>
    </template>

    <EmptyState v-else icon="heroicons:microphone" title="Call not found" description="This call may have been deleted or you don't have access." />
  </div>
</template>
<script setup lang="ts">
import type { CallDetail } from '~/types'
definePageMeta({ layout: 'default' })
const route = useRoute()
const store = useCallsStore()
const { formatDate, formatDuration, scoreColor } = useFormatting()
const call = computed(() => store.currentCall)
const audioPlayer = ref()
const currentTime = ref(0)
const activeTab = ref('transcript')
const syncing = ref(false)

const tabs = computed(() => [
  { id: 'transcript', label: 'Transcript', count: call.value?.transcript?.segments.length },
  { id: 'summary', label: 'Summary', count: null },
  { id: 'insights', label: 'Insights', count: call.value?.insights.length },
  { id: 'actions', label: 'Actions', count: call.value?.actionItems.length },
  { id: 'email', label: 'Follow-up Email', count: null },
])

function seekAudio(time: number) { audioPlayer.value?.seekTo(time) }

async function syncToCrm() { syncing.value = true; await new Promise(r => setTimeout(r, 1500)); syncing.value = false }
async function refreshAnalysis() { await store.fetchCallDetail(route.params.id as string) }
function pushToCrm() { /* trigger CRM push */ }
function copyEmail() { if (call.value?.followUpEmail) navigator.clipboard.writeText(call.value.followUpEmail) }

onMounted(() => store.fetchCallDetail(route.params.id as string))
</script>
