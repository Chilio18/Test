<template>
  <div>
    <PageHeader title="Contacts" subtitle="People from your accounts" />
    <EmptyState icon="heroicons:user-group" title="No contacts" description="Contacts are imported from CRM or added manually." />
  </div>
</template>
<script setup lang="ts">
definePageMeta({ layout: 'default' })
</script>
