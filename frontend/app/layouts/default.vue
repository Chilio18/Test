<template>
  <div class="min-h-screen bg-gray-950 flex">
    <AppSidebar />
    <div class="flex-1 flex flex-col min-w-0 ml-64">
      <AppTopBar />
      <main class="flex-1 p-6 overflow-auto scrollbar-thin">
        <slot />
      </main>
    </div>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ middleware: 'auth' })
</script>
