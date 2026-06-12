<template>
  <div>
    <PageHeader title="Sync Logs" subtitle="CRM synchronization history" />
    <EmptyState icon="heroicons:arrow-path" title="No sync logs" description="Sync logs will appear here after your first CRM synchronization." />
  </div>
</template>
<script setup lang="ts">
definePageMeta({ layout: 'default' })
</script>
