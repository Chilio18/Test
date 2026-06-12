<template>
  <div>
    <div class="flex items-center gap-3 mb-6">
      <button @click="navigateTo('/accounts')" class="p-1.5 rounded-lg text-gray-400 hover:text-gray-200 hover:bg-gray-800 transition-colors">
        <Icon name="heroicons:arrow-left" class="w-5 h-5" />
      </button>
      <h1 class="text-2xl font-bold text-gray-100">Account Detail</h1>
    </div>
    <EmptyState icon="heroicons:building-office-2" title="Account not found" description="This account does not exist or you don't have access." />
  </div>
</template>
<script setup lang="ts">
definePageMeta({ layout: 'default' })
</script>
