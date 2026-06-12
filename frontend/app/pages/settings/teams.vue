<template>
  <div>
    <PageHeader title="Teams" subtitle="Organize your sales teams">
      <button class="flex items-center gap-2 px-4 py-2 bg-brand-600 hover:bg-brand-500 text-white rounded-lg text-sm font-medium transition-colors">
        <Icon name="heroicons:plus" class="w-4 h-4" /> Create Team
      </button>
    </PageHeader>
    <EmptyState icon="heroicons:user-group" title="No teams yet" description="Create teams to organize your sales reps and managers." />
  </div>
</template>
<script setup lang="ts">
definePageMeta({ layout: 'default' })
</script>
