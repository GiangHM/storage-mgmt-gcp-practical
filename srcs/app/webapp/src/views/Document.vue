<script setup lang="ts">
import { ref } from 'vue'
import DocumentTable from '../components/DocumentTable.vue'
import DocumentUploadDialog from '../components/DocumentUploadDialog.vue'

const tableRef = ref<InstanceType<typeof DocumentTable> | null>(null)
const dialogVisible = ref(false)

function onUploaded() {
  tableRef.value?.refresh()
}
</script>

<template>
  <div class="page">
    <div class="page-header">
      <h2>Documents</h2>
      <button class="btn-primary" @click="dialogVisible = true">
        <i class="ti ti-plus"></i>Add document
      </button>
    </div>

    <DocumentTable ref="tableRef" />

    <DocumentUploadDialog v-model:visible="dialogVisible" @uploaded="onUploaded" />
  </div>
</template>

<style scoped>
.page {
  padding: 1.5rem;
  display: flex;
  flex-direction: column;
  gap: 1rem;
  height: 100%;
  overflow: auto;
}

.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.page-header h2 {
  font-size: 18px;
  font-weight: 500;
  color: #1a1a1a;
}

.btn-primary {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  background: var(--color-green, #41b883);
  color: white;
  border: none;
  border-radius: 8px;
  padding: 7px 14px;
  font-size: 13px;
  font-weight: 500;
  cursor: pointer;
}

.btn-primary i {
  font-size: 15px;
}
</style>
