<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import Dialog from 'primevue/dialog'
import Select from 'primevue/select'
import FileUpload from 'primevue/fileupload'
import Button from 'primevue/button'
import { useToast } from 'primevue/usetoast'
import axios from 'axios'
import SignedUrlService from '../services/SignedUrlService'
import DocumentTypeService from '../services/DocumentTypeService'
import DocumentService from '../services/DocumentService'
import DocumentCreationRequestModel from '../models/DocumentCreationRequestModel'

const props = defineProps<{ visible: boolean }>()
const emit = defineEmits<{
  (e: 'update:visible', value: boolean): void
  (e: 'uploaded'): void
}>()

const toast = useToast()

const signedUrlService = new SignedUrlService()
const documentTypeService = new DocumentTypeService()
const documentService = new DocumentService()

const fileName = ref<string | null>(null)
const selectedFile = ref<File | null>(null)
const uploadError = ref<string | null>(null)
const selectedDocumentType = ref()
const documentTypes = ref()
const fileupload = ref()

const localVisible = computed({
  get: () => props.visible,
  set: (val: boolean) => emit('update:visible', val)
})

onMounted(() => {
  documentTypeService.getDocumentTypeFromAPI().then((data) => (documentTypes.value = data))
})

function onFileSelect(event: { files: File[] }) {
  const file = event.files[0]
  if (file) {
    fileName.value = file.name
    selectedFile.value = file
    uploadError.value = null
  }
}

async function uploadAndSave() {
  uploadError.value = null

  if (!selectedFile.value || !fileName.value) {
    uploadError.value = 'Please select a file before uploading.'
    return
  }

  if (!selectedDocumentType.value) {
    uploadError.value = 'Please select a document type.'
    return
  }

  try {
    const signedUrlResponse = await signedUrlService.requestSignedUrl(
      selectedDocumentType.value.docTypeCode,
      fileName.value
    )

    if (!signedUrlResponse?.uploadUrl) {
      uploadError.value = 'Failed to obtain upload URL. Please try again.'
      return
    }

    const isEmulator = signedUrlResponse.uploadUrl.startsWith('/')
    if (isEmulator) {
      // fake-gcs supports uploadType=multipart only — build a multipart/related body
      // with JSON metadata part + binary file part.
      const boundary = `boundary_${Date.now()}`
      const meta = JSON.stringify({ name: signedUrlResponse.objectName })
      const fileType = selectedFile.value.type || 'application/octet-stream'
      const encoder = new TextEncoder()
      const preamble = encoder.encode(
        `--${boundary}\r\nContent-Type: application/json; charset=UTF-8\r\n\r\n${meta}\r\n` +
          `--${boundary}\r\nContent-Type: ${fileType}\r\n\r\n`
      )
      const epilogue = encoder.encode(`\r\n--${boundary}--`)
      const fileBuffer = await selectedFile.value.arrayBuffer()
      const body = new Blob([preamble, new Uint8Array(fileBuffer), epilogue])

      await axios.post(signedUrlResponse.uploadUrl, body, {
        headers: { 'Content-Type': `multipart/related; boundary="${boundary}"` }
      })
    } else {
      // Production: PUT directly to the GCS signed URL with raw binary.
      await axios.put(signedUrlResponse.uploadUrl, selectedFile.value, {
        headers: { 'Content-Type': selectedFile.value.type || 'application/octet-stream' }
      })
    }

    await documentService.addNewDocument(
      new DocumentCreationRequestModel(
        selectedDocumentType.value.docTypeCode,
        signedUrlResponse.objectName
      )
    )

    emit('uploaded')
    emit('update:visible', false)
    toast.add({ severity: 'success', summary: 'Uploaded', detail: 'Document saved successfully.', life: 3000 })
  } catch (error: any) {
    const status = error?.response?.status
    if (status === 403) {
      uploadError.value = 'Upload permission denied. The upload URL may have expired.'
    } else if (status === 400) {
      uploadError.value = 'Invalid upload request. Please check the file and try again.'
    } else {
      uploadError.value = 'An error occurred during upload. Please try again.'
    }
    console.error('Upload failed:', error)
  }
}
</script>

<template>
  <Dialog v-model:visible="localVisible" header="Add document" :modal="true" :style="{ width: '400px' }">
    <div class="dialog-body">
      <div class="field">
        <label>Document type</label>
        <Select
          v-model="selectedDocumentType"
          :options="documentTypes"
          optionLabel="docTypeDescription"
          placeholder="Select a document type"
          class="w-full"
        />
      </div>
      <div class="field">
        <label>File</label>
        <FileUpload
          ref="fileupload"
          mode="basic"
          @select="onFileSelect"
          customUpload
          name="upload[]"
          accept="image/*"
          :maxFileSize="1000000"
          class="w-full"
        />
      </div>
      <div v-if="uploadError" class="error-message">{{ uploadError }}</div>
    </div>
    <template #footer>
      <Button label="Cancel" severity="secondary" @click="localVisible = false" />
      <Button label="Upload & save" icon="ti ti-upload" @click="uploadAndSave" />
    </template>
  </Dialog>
</template>

<style scoped>
.dialog-body {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  padding: 0.25rem 0;
}

.field {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.field label {
  font-size: 12px;
  font-weight: 500;
  color: #555;
}

.error-message {
  color: var(--red-500, #ef4444);
  font-size: 13px;
  margin-top: 0.25rem;
}
</style>
