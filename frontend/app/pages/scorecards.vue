<template>
  <div>
    <PageHeader title="Scorecards" subtitle="Sales coaching performance">
      <button class="flex items-center gap-2 px-4 py-2 bg-brand-600 hover:bg-brand-500 text-white rounded-lg text-sm font-medium transition-colors">
        <Icon name="heroicons:plus" class="w-4 h-4" /> New Template
      </button>
    </PageHeader>
    <div class="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
      <div v-for="template in mockTemplates" :key="template.id" class="card">
        <div class="flex items-start justify-between mb-3">
          <h3 class="font-semibold text-gray-100">{{ template.name }}</h3>
          <span v-if="template.isDefault" class="badge-status bg-brand-500/20 text-brand-400">Default</span>
        </div>
        <p class="text-sm text-gray-400 mb-3">{{ template.criteria.length }} criteria</p>
        <div class="space-y-1">
          <div v-for="c in template.criteria.slice(0,3)" :key="c.id" class="text-xs text-gray-500 flex items-center gap-1.5">
            <Icon name="heroicons:check-circle" class="w-3 h-3 text-gray-600" />{{ c.name }}
          </div>
          <div v-if="template.criteria.length > 3" class="text-xs text-gray-600">+{{ template.criteria.length - 3 }} more</div>
        </div>
      </div>
    </div>
  </div>
</template>
<script setup lang="ts">
definePageMeta({ layout: 'default' })
const mockTemplates = [
  { id: '1', name: 'MEDDICC', isDefault: true, criteria: [{ id:'1', name:'Metrics' }, { id:'2', name:'Economic Buyer' }, { id:'3', name:'Decision Criteria' }, { id:'4', name:'Decision Process' }, { id:'5', name:'Identify Pain' }, { id:'6', name:'Champion' }] },
  { id: '2', name: 'Discovery Quality', isDefault: false, criteria: [{ id:'1', name:'Problem Discovery' }, { id:'2', name:'Business Impact' }, { id:'3', name:'Timeline' }, { id:'4', name:'Budget' }] },
  { id: '3', name: 'Objection Handling', isDefault: false, criteria: [{ id:'1', name:'Acknowledge' }, { id:'2', name:'Clarify' }, { id:'3', name:'Response Quality' }] },
]
</script>
