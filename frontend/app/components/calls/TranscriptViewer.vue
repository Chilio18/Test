<template>
  <div class="space-y-1 max-h-[500px] overflow-y-auto scrollbar-thin pr-1">
    <div v-for="(seg, i) in segments" :key="i"
      class="flex gap-3 p-2 rounded-lg cursor-pointer hover:bg-gray-800/50 transition-colors"
      :class="isActive(seg) && 'bg-gray-800 ring-1 ring-brand-500/30'"
      @click="emit('seek', seg.startTime)">
      <div class="shrink-0 pt-0.5">
        <div class="w-7 h-7 rounded-full flex items-center justify-center text-xs font-bold" :class="bgColor(seg.speaker)">{{ seg.speaker.charAt(0).toUpperCase() }}</div>
      </div>
      <div class="flex-1 min-w-0">
        <div class="flex items-center gap-2 mb-0.5">
          <span class="text-xs font-medium" :class="textColor(seg.speaker)">{{ seg.speaker }}</span>
          <span class="text-xs text-gray-600 font-mono">{{ formatTime(seg.startTime) }}</span>
        </div>
        <p class="text-sm text-gray-300 leading-relaxed">{{ seg.text }}</p>
      </div>
    </div>
    <div v-if="segments.length === 0" class="text-center text-gray-500 py-8 text-sm">No transcript available</div>
  </div>
</template>
<script setup lang="ts">
import type { TranscriptSegment } from '~/types'
const props = defineProps<{ segments: TranscriptSegment[]; currentTime?: number }>()
const emit = defineEmits<{ seek: [time: number] }>()
const palette = ['bg-blue-600','bg-green-600','bg-purple-600','bg-orange-600','bg-pink-600']
const textPalette = ['text-blue-400','text-green-400','text-purple-400','text-orange-400','text-pink-400']
const speakerMap = new Map<string, number>()
function getSpeakerIdx(s: string) { if (!speakerMap.has(s)) speakerMap.set(s, speakerMap.size); return speakerMap.get(s)! % palette.length }
function bgColor(s: string) { return palette[getSpeakerIdx(s)] }
function textColor(s: string) { return textPalette[getSpeakerIdx(s)] }
function isActive(seg: TranscriptSegment) { return props.currentTime != null && props.currentTime >= seg.startTime && props.currentTime < seg.endTime }
function formatTime(s: number) { return `${Math.floor(s/60)}:${Math.floor(s%60).toString().padStart(2,'0')}` }
</script>
