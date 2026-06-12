<template>
  <div>
    <PageHeader title="Calls" :subtitle="`${store.pagination.totalCount} total calls`">
      <button @click="showUpload = true" class="flex items-center gap-2 px-4 py-2 bg-brand-600 hover:bg-brand-500 text-white rounded-lg text-sm font-medium transition-colors">
        <Icon name="heroicons:plus" class="w-4 h-4" />
        Upload Call
      </button>
    </PageHeader>

    <!-- Filters -->
    <div class="flex flex-wrap gap-3 mb-6">
      <select v-model="statusFilter" @change="applyFilters" class="px-3 py-1.5 bg-gray-800 border border-gray-700 rounded-lg text-sm text-gray-300">
        <option value="">All statuses</option>
        <option v-for="s in statuses" :key="s" :value="s">{{ s }}</option>
      </select>
      <input v-model="fromFilter" type="date" @change="applyFilters" class="px-3 py-1.5 bg-gray-800 border border-gray-700 rounded-lg text-sm text-gray-300" />
      <input v-model="toFilter" type="date" @change="applyFilters" class="px-3 py-1.5 bg-gray-800 border border-gray-700 rounded-lg text-sm text-gray-300" />
      <button v-if="hasFilters" @click="clearFilters" class="px-3 py-1.5 text-sm text-gray-400 hover:text-gray-200 transition-colors flex items-center gap-1">
        <Icon name="heroicons:x-mark" class="w-4 h-4" /> Clear
      </button>
    </div>

    <LoadingSpinner v-if="store.isLoading" label="Loading calls..." class="py-20" />

    <template v-else-if="store.calls.length > 0">
      <div class="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
        <CallCard v-for="call in store.calls" :key="call.id" :call="call" />
      </div>
      <!-- Pagination -->
      <div v-if="store.pagination.totalPages > 1" class="flex justify-center gap-2 mt-6">
        <button v-for="p in pageRange" :key="p" @click="goToPage(p)"
          class="w-9 h-9 rounded-lg text-sm transition-colors"
          :class="p === store.pagination.page ? 'bg-brand-600 text-white' : 'bg-gray-800 text-gray-400 hover:bg-gray-700'">
          {{ p }}
        </button>
      </div>
    </template>

    <EmptyState v-else icon="heroicons:microphone" title="No calls yet" description="Upload your first call recording to get started with AI analysis." >
      <button @click="showUpload = true" class="px-4 py-2 bg-brand-600 hover:bg-brand-500 text-white rounded-lg text-sm font-medium transition-colors">
        Upload Call
      </button>
    </EmptyState>

    <!-- Upload Modal -->
    <UploadCallModal v-if="showUpload" @close="showUpload = false" @uploaded="onUploaded" />
  </div>
</template>
<script setup lang="ts">
definePageMeta({ layout: 'default' })
const store = useCallsStore()
const showUpload = ref(false)
const statusFilter = ref(''); const fromFilter = ref(''); const toFilter = ref('')
const statuses = ['Uploaded','Processing','Transcribing','Analyzing','Completed','Failed']
const hasFilters = computed(() => !!statusFilter.value || !!fromFilter.value || !!toFilter.value)
const pageRange = computed(() => Array.from({ length: Math.min(store.pagination.totalPages, 7) }, (_, i) => i + 1))

function applyFilters() {
  store.setFilter('status', statusFilter.value || null)
  store.setFilter('from', fromFilter.value || null)
  store.setFilter('to', toFilter.value || null)
  store.fetchCalls(1)
}
function clearFilters() { statusFilter.value = ''; fromFilter.value = ''; toFilter.value = ''; store.clearFilters(); store.fetchCalls(1) }
function goToPage(p: number) { store.fetchCalls(p) }
async function onUploaded() { showUpload.value = false; await store.fetchCalls() }
onMounted(() => store.fetchCalls())
</script>
