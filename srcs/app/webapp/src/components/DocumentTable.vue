<script setup lang="ts">
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import { ref, onMounted } from 'vue'
import DocumentService from '@/services/DocumentService'
import DocumentResponseModel from '@/models/DocumentResponseModel'

const documentService = new DocumentService()
const documents = ref<DocumentResponseModel[]>([])

onMounted(async () => {
  await loadDocuments()
})

async function loadDocuments() {
  documents.value = await documentService.getDocumentFromAPI()
}

async function refresh() {
  await loadDocuments()
}

defineExpose({ refresh })

function extractFileName(docUrl: string): string {
  return docUrl.split('/').pop() ?? docUrl
}
</script>

<template>
  <div class="card">
    <DataTable :value="documents" tableStyle="width: 100%">
      <Column field="docTypeCode" header="Type">
        <template #body="{ data }">
          <span class="badge">{{ data.docTypeCode }}</span>
        </template>
      </Column>
      <Column header="File name">
        <template #body="{ data }">
          <span class="file-name">{{ extractFileName(data.docUrl) }}</span>
        </template>
      </Column>
      <Column header="Uploaded">
        <template #body>
          <span class="date">—</span>
        </template>
      </Column>
      <Column header="Actions" style="width: 80px">
        <template #body>
          <div class="actions">
            <button class="btn-icon" title="View"><i class="ti ti-eye"></i></button>
            <button class="btn-icon danger" title="Delete"><i class="ti ti-trash"></i></button>
          </div>
        </template>
      </Column>
    </DataTable>
  </div>
</template>

<style scoped>
.card {
  background: #fff;
  border: 0.5px solid #e8e8e4;
  border-radius: 10px;
  overflow: hidden;
}

.badge {
  display: inline-block;
  background: #e8f6f0;
  color: #1a7a52;
  font-size: 11px;
  font-weight: 600;
  padding: 2px 8px;
  border-radius: 99px;
}

.file-name {
  color: #555;
  font-size: 13px;
}

.date {
  color: #aaa;
  font-size: 12px;
}

.actions {
  display: flex;
  gap: 6px;
}

.btn-icon {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 28px;
  height: 28px;
  border: 0.5px solid #e0e0da;
  border-radius: 6px;
  background: transparent;
  color: #888;
  cursor: pointer;
}

.btn-icon i {
  font-size: 14px;
}

.btn-icon.danger {
  color: #d44;
  border-color: #f5c0c0;
}
</style>
