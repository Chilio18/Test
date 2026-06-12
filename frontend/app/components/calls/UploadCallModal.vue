<template>
  <div class="fixed inset-0 bg-black/70 z-50 flex items-center justify-center p-4" @click.self="emit('close')">
    <div class="bg-gray-900 border border-gray-800 rounded-2xl w-full max-w-lg">
      <div class="flex items-center justify-between p-6 border-b border-gray-800">
        <h2 class="text-lg font-semibold text-gray-100">Upload Call Recording</h2>
        <button @click="emit('close')" class="text-gray-500 hover:text-gray-300">
          <Icon name="heroicons:x-mark" class="w-5 h-5" />
        </button>
      </div>
      <div class="p-6 space-y-4">
        <div>
          <label class="block text-sm text-gray-400 mb-1">Call Title *</label>
          <input v-model="form.title" type="text" placeholder="e.g. Discovery call with TechCorp"
            class="w-full px-3 py-2 bg-gray-800 border border-gray-700 rounded-lg text-gray-100 placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-brand-500" />
        </div>
        <div class="grid grid-cols-2 gap-4">
          <div>
            <label class="block text-sm text-gray-400 mb-1">Meeting Date *</label>
            <input v-model="form.meetingDate" type="datetime-local"
              class="w-full px-3 py-2 bg-gray-800 border border-gray-700 rounded-lg text-gray-100 focus:outline-none focus:ring-2 focus:ring-brand-500" />
          </div>
          <div>
            <label class="block text-sm text-gray-400 mb-1">Call Type</label>
            <select v-model="form.callType" class="w-full px-3 py-2 bg-gray-800 border border-gray-700 rounded-lg text-gray-300">
              <option v-for="t in callTypes" :key="t" :value="t">{{ t }}</option>
            </select>
          </div>
        </div>
        <div>
          <label class="block text-sm text-gray-400 mb-1">Recording File *</label>
          <div class="relative border-2 border-dashed border-gray-700 rounded-xl p-6 text-center hover:border-brand-500/50 transition-colors cursor-pointer" @click="fileInput?.click()">
            <input ref="fileInput" type="file" accept="audio/*,video/*" class="hidden" @change="onFileSelected" />
            <Icon name="heroicons:cloud-arrow-up" class="w-8 h-8 text-gray-500 mx-auto mb-2" />
            <p class="text-sm text-gray-400">
              <span v-if="file">{{ file.name }} ({{ (file.size / 1024 / 1024).toFixed(1) }} MB)</span>
              <span v-else>Click or drag file here · MP4, MP3, WAV, WebM</span>
            </p>
          </div>
        </div>
        <div class="flex items-center gap-2">
          <input id="consent" v-model="form.consentObtained" type="checkbox" class="w-4 h-4 rounded accent-brand-500" />
          <label for="consent" class="text-sm text-gray-400">Recording consent was obtained from all participants</label>
        </div>
        <div v-if="error" class="p-3 bg-red-500/10 border border-red-500/20 rounded-lg text-red-400 text-sm">{{ error }}</div>
      </div>
      <div class="flex gap-3 p-6 border-t border-gray-800">
        <button @click="emit('close')" class="flex-1 py-2 px-4 bg-gray-800 hover:bg-gray-700 text-gray-300 rounded-lg text-sm transition-colors">Cancel</button>
        <button @click="submit" :disabled="!canSubmit || uploading" class="flex-1 py-2 px-4 bg-brand-600 hover:bg-brand-500 disabled:opacity-50 text-white rounded-lg text-sm font-medium transition-colors">
          {{ uploading ? 'Uploading...' : 'Upload & Analyze' }}
        </button>
      </div>
    </div>
  </div>
</template>
<script setup lang="ts">
const emit = defineEmits<{ close: []; uploaded: [callId: string] }>()
const { uploadCall } = useCallsApi()
const fileInput = ref<HTMLInputElement>()
const file = ref<File | null>(null)
const uploading = ref(false)
const error = ref('')
const callTypes = ['Discovery','Demo','Negotiation','CheckIn','Onboarding','SupportCall','QBR','Other']
const form = ref({ title: '', meetingDate: new Date().toISOString().slice(0,16), callType: 'Discovery', consentObtained: false })
const canSubmit = computed(() => form.value.title && form.value.meetingDate && file.value && form.value.consentObtained)
function onFileSelected(e: Event) { file.value = (e.target as HTMLInputElement).files?.[0] ?? null }
async function submit() {
  if (!file.value || !canSubmit.value) return
  uploading.value = true; error.value = ''
  try {
    const result = await uploadCall({ title: form.value.title, meetingDate: new Date(form.value.meetingDate).toISOString(), callType: form.value.callType, fileName: file.value.name, contentType: file.value.type, fileSizeBytes: file.value.size, isVideo: file.value.type.startsWith('video/'), consentObtained: form.value.consentObtained, consentMethod: 'Verbal', language: 'nl' })
    // Upload file to presigned URL
    await $fetch(result.presignedUploadUrl, { method: 'PUT', body: file.value, headers: { 'Content-Type': file.value.type } })
    emit('uploaded', result.callId)
  } catch (e) { error.value = 'Upload failed. Please try again.' } finally { uploading.value = false }
}
</script>
