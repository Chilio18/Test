<template>
  <div>
    <PageHeader title="Ask AI" subtitle="Search across all your call transcripts using natural language" />
    <div class="max-w-3xl mx-auto">
      <!-- Question input -->
      <div class="card mb-6">
        <label class="block text-sm text-gray-400 mb-2">Ask anything about your calls...</label>
        <div class="flex gap-2">
          <input v-model="question" type="text" placeholder="Which customers mentioned integration challenges? Which deals lack a next step?"
            class="flex-1 px-4 py-3 bg-gray-800 border border-gray-700 rounded-xl text-gray-100 placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-brand-500"
            @keyup.enter="search" />
          <button @click="search" :disabled="!question.trim() || loading"
            class="px-5 py-3 bg-brand-600 hover:bg-brand-500 disabled:opacity-50 text-white rounded-xl font-medium transition-colors flex items-center gap-2">
            <Icon name="heroicons:sparkles" class="w-5 h-5" />
            <span class="hidden sm:inline">Ask</span>
          </button>
        </div>
        <!-- Suggestions -->
        <div class="flex flex-wrap gap-2 mt-3">
          <button v-for="suggestion in suggestions" :key="suggestion" @click="question = suggestion"
            class="px-2.5 py-1 bg-gray-800 hover:bg-gray-700 text-xs text-gray-400 rounded-lg transition-colors">
            {{ suggestion }}
          </button>
        </div>
      </div>

      <!-- Loading -->
      <LoadingSpinner v-if="loading" label="Searching transcripts..." class="py-12" />

      <!-- Results -->
      <template v-else-if="result">
        <!-- AI Answer -->
        <div class="card mb-6">
          <div class="flex items-center gap-2 mb-3">
            <Icon name="heroicons:sparkles" class="w-5 h-5 text-brand-400" />
            <span class="font-semibold text-gray-100">AI Answer</span>
          </div>
          <p class="text-gray-300 leading-relaxed whitespace-pre-wrap">{{ result.answer }}</p>
        </div>

        <!-- Sources -->
        <div v-if="result.sources.length">
          <h3 class="text-sm font-medium text-gray-400 mb-3">{{ result.sources.length }} relevant call{{ result.sources.length > 1 ? 's' : '' }} found</h3>
          <div class="space-y-3">
            <div v-for="source in result.sources" :key="source.documentId" class="card-sm">
              <div class="flex items-center justify-between mb-2">
                <span class="text-xs font-medium text-gray-400 uppercase">{{ source.sourceType }}</span>
                <span class="text-xs text-gray-500">Relevance: {{ Math.round(source.score * 100) }}%</span>
              </div>
              <p class="text-sm text-gray-300 line-clamp-3">{{ source.content }}</p>
              <NuxtLink v-if="source.sourceId" :to="`/calls/${source.sourceId}`"
                class="mt-2 text-xs text-brand-400 hover:text-brand-300 flex items-center gap-1">
                View call <Icon name="heroicons:arrow-right" class="w-3 h-3" />
              </NuxtLink>
            </div>
          </div>
        </div>
      </template>
    </div>
  </div>
</template>
<script setup lang="ts">
import type { SemanticSearchResponse } from '~/types'
definePageMeta({ layout: 'default' })
const { ask } = useSearchApi()
const question = ref(''); const loading = ref(false); const result = ref<SemanticSearchResponse | null>(null)
const suggestions = ['Which customers mentioned integration issues?', 'Which deals are missing a next step?', 'What objections are most common?', 'Which opportunities discussed budget?', 'Who are the decision makers in active deals?']
async function search() {
  if (!question.value.trim()) return
  loading.value = true; result.value = null
  try { result.value = await ask(question.value) } finally { loading.value = false }
}
</script>
