<template>
  <div class="card-sm">
    <div class="flex items-center gap-4">
      <button @click="togglePlay" class="w-10 h-10 rounded-full bg-brand-600 hover:bg-brand-500 flex items-center justify-center transition-colors shrink-0">
        <Icon :name="isPlaying ? 'heroicons:pause' : 'heroicons:play'" class="w-5 h-5 text-white" />
      </button>
      <div class="flex-1">
        <div class="flex items-center gap-2 text-xs text-gray-400 mb-1">
          <span>{{ formatTime(currentTime) }}</span>
          <div class="flex-1 h-1.5 bg-gray-800 rounded-full cursor-pointer relative overflow-hidden" @click="seek" ref="progressBar">
            <div class="absolute left-0 top-0 h-full bg-brand-500 rounded-full" :style="{ width: `${progress}%` }" />
          </div>
          <span>{{ formatTime(duration) }}</span>
        </div>
      </div>
      <select v-model="playbackRate" @change="setRate" class="text-xs bg-gray-800 border-0 rounded text-gray-300">
        <option value="0.75">0.75x</option><option value="1">1x</option>
        <option value="1.25">1.25x</option><option value="1.5">1.5x</option><option value="2">2x</option>
      </select>
    </div>
    <audio ref="audioEl" :src="src" @timeupdate="onTimeUpdate" @loadedmetadata="e => duration = (e.target as HTMLAudioElement).duration" @ended="isPlaying = false" />
  </div>
</template>
<script setup lang="ts">
defineProps<{ src: string }>()
const emit = defineEmits<{ timeUpdate: [time: number] }>()
const audioEl = ref<HTMLAudioElement>(); const progressBar = ref<HTMLElement>()
const isPlaying = ref(false); const currentTime = ref(0); const duration = ref(0); const playbackRate = ref('1')
const progress = computed(() => duration.value ? (currentTime.value / duration.value) * 100 : 0)
function togglePlay() { if (!audioEl.value) return; isPlaying.value ? audioEl.value.pause() : audioEl.value.play(); isPlaying.value = !isPlaying.value }
function onTimeUpdate() { currentTime.value = audioEl.value?.currentTime ?? 0; emit('timeUpdate', currentTime.value) }
function seek(e: MouseEvent) { if (!progressBar.value || !audioEl.value) return; audioEl.value.currentTime = ((e.clientX - progressBar.value.getBoundingClientRect().left) / progressBar.value.offsetWidth) * duration.value }
function setRate() { if (audioEl.value) audioEl.value.playbackRate = Number(playbackRate.value) }
function formatTime(s: number) { return `${Math.floor(s/60)}:${Math.floor(s%60).toString().padStart(2,'0')}` }
function seekTo(s: number) { if (audioEl.value) audioEl.value.currentTime = s }
defineExpose({ seekTo })
</script>
