<script setup lang="ts">
import { computed } from 'vue'
import { RouterView, useRoute } from 'vue-router'
import Toast from 'primevue/toast'
import AppSidebar from './components/AppSidebar.vue'

const route = useRoute()

const pageTitles: Record<string, string> = {
  home: 'Home',
  document: 'Documents'
}

const pageTitle = computed(() => pageTitles[route.name as string] ?? '')
</script>

<template>
  <Toast />
  <div class="shell">
    <AppSidebar />
    <div class="main">
      <div class="topbar">
        <span class="topbar-title">{{ pageTitle }}</span>
      </div>
      <div class="content">
        <RouterView />
      </div>
    </div>
  </div>
</template>

<style scoped>
.shell {
  display: flex;
  height: 100vh;
  overflow: hidden;
}

.main {
  flex: 1;
  display: flex;
  flex-direction: column;
  min-width: 0;
  overflow: hidden;
}

.topbar {
  height: 48px;
  border-bottom: 0.5px solid #e8e8e4;
  display: flex;
  align-items: center;
  padding: 0 1.5rem;
  flex-shrink: 0;
}

.topbar-title {
  font-size: 13px;
  color: #888;
}

.content {
  flex: 1;
  overflow: auto;
}
</style>
