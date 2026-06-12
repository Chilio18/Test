<template>
  <div>
    <div class="flex items-center gap-3 mb-6">
      <button @click="navigateTo('/opportunities')" class="p-1.5 rounded-lg text-gray-400 hover:text-gray-200 hover:bg-gray-800 transition-colors">
        <Icon name="heroicons:arrow-left" class="w-5 h-5" />
      </button>
      <h1 class="text-2xl font-bold text-gray-100">Opportunity Detail</h1>
    </div>
    <EmptyState icon="heroicons:currency-euro" title="Opportunity not found" description="This opportunity does not exist." />
  </div>
</template>
<script setup lang="ts">
definePageMeta({ layout: 'default' })
</script>
