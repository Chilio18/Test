<template>
  <div>
    <PageHeader title="Field Mappings" subtitle="Map CRM fields to platform fields">
      <button class="flex items-center gap-2 px-4 py-2 bg-brand-600 hover:bg-brand-500 text-white rounded-lg text-sm font-medium transition-colors">
        <Icon name="heroicons:plus" class="w-4 h-4" /> Add Mapping
      </button>
    </PageHeader>
    <EmptyState icon="heroicons:arrows-right-left" title="No field mappings" description="Add field mappings to control how data flows between the platform and your CRM." />
  </div>
</template>
<script setup lang="ts">
definePageMeta({ layout: 'default' })
</script>
