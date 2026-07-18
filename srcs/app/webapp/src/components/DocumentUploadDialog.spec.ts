import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import PrimeVue from 'primevue/config'
import Aura from '@primevue/themes/aura'
import ToastService from 'primevue/toastservice'
import DocumentUploadDialog from '@/components/DocumentUploadDialog.vue'
import SignedUrlService from '@/services/SignedUrlService'
import DocumentService from '@/services/DocumentService'
import DocumentTypeService from '@/services/DocumentTypeService'
import axios from 'axios'

vi.mock('@/services/SignedUrlService')
vi.mock('@/services/DocumentService')
vi.mock('@/services/DocumentTypeService')
vi.mock('axios')

const mockDocTypes = [
  { docTypeCode: 'IMP', docTypeDescription: 'Important' },
  { docTypeCode: 'REP', docTypeDescription: 'Report' }
]

function makeWrapper(visible = true) {
  return mount(DocumentUploadDialog, {
    props: { visible },
    global: {
      plugins: [
        [PrimeVue, { theme: { preset: Aura } }],
        ToastService
      ]
    }
  })
}

describe('DocumentUploadDialog.vue', () => {
  let mockSignedUrlSvc: any
  let mockDocSvc: any
  let mockDocTypeSvc: any
  const mockAxios = axios as any

  beforeEach(() => {
    vi.clearAllMocks()

    mockSignedUrlSvc = { requestSignedUrl: vi.fn() }
    ;(SignedUrlService as any).mockImplementation(() => mockSignedUrlSvc)

    mockDocSvc = { addNewDocument: vi.fn() }
    ;(DocumentService as any).mockImplementation(() => mockDocSvc)

    mockDocTypeSvc = { getDocumentTypeFromAPI: vi.fn().mockResolvedValue(mockDocTypes) }
    ;(DocumentTypeService as any).mockImplementation(() => mockDocTypeSvc)

    mockAxios.put = vi.fn()
    mockAxios.post = vi.fn()
  })

  afterEach(() => {
    vi.resetAllMocks()
  })

  it('Dialog_OnMount_LoadsDocumentTypes', async () => {
    makeWrapper()
    await flushPromises()
    expect(mockDocTypeSvc.getDocumentTypeFromAPI).toHaveBeenCalledOnce()
  })

  it('Dialog_AcceptsVisibleProp', () => {
    const wrapper = makeWrapper(false)
    expect(wrapper.props('visible')).toBe(false)
  })

  it('Dialog_NoFile_ShowsValidationError', async () => {
    const wrapper = makeWrapper()
    await flushPromises()
    await (wrapper.vm as any).uploadAndSave()
    expect((wrapper.vm as any).uploadError).toBe('Please select a file before uploading.')
  })

  it('Dialog_NoDocType_ShowsValidationError', async () => {
    const wrapper = makeWrapper()
    await flushPromises()
    const file = new File(['data'], 'test.png', { type: 'image/png' })
    ;(wrapper.vm as any).selectedFile = file
    ;(wrapper.vm as any).fileName = 'test.png'
    ;(wrapper.vm as any).selectedDocumentType = null
    await (wrapper.vm as any).uploadAndSave()
    expect((wrapper.vm as any).uploadError).toBe('Please select a document type.')
  })

  it('Dialog_NoSignedUrl_ShowsError', async () => {
    const wrapper = makeWrapper()
    await flushPromises()
    ;(wrapper.vm as any).selectedFile = new File(['d'], 'f.png', { type: 'image/png' })
    ;(wrapper.vm as any).fileName = 'f.png'
    ;(wrapper.vm as any).selectedDocumentType = { docTypeCode: 'IMP' }
    mockSignedUrlSvc.requestSignedUrl.mockResolvedValueOnce(null)
    await (wrapper.vm as any).uploadAndSave()
    await flushPromises()
    expect((wrapper.vm as any).uploadError).toBe('Failed to obtain upload URL. Please try again.')
  })

  it('Dialog_ProductionPath_CallsPutWithBinary', async () => {
    const wrapper = makeWrapper()
    await flushPromises()
    const file = new File(['data'], 'photo.png', { type: 'image/png' })
    ;(wrapper.vm as any).selectedFile = file
    ;(wrapper.vm as any).fileName = 'photo.png'
    ;(wrapper.vm as any).selectedDocumentType = { docTypeCode: 'IMP' }
    mockSignedUrlSvc.requestSignedUrl.mockResolvedValueOnce({
      uploadUrl: 'https://storage.googleapis.com/bucket/obj?sig=abc',
      objectName: 'documents/IMP/photo.png'
    })
    mockAxios.put.mockResolvedValueOnce({ status: 200 })
    mockDocSvc.addNewDocument.mockResolvedValueOnce(201)
    await (wrapper.vm as any).uploadAndSave()
    await flushPromises()
    expect(mockAxios.put).toHaveBeenCalledWith(
      'https://storage.googleapis.com/bucket/obj?sig=abc',
      file,
      expect.objectContaining({ headers: expect.objectContaining({ 'Content-Type': 'image/png' }) })
    )
    expect((wrapper.vm as any).uploadError).toBeNull()
  })

  it('Dialog_EmulatorPath_CallsPostWithMultipart', async () => {
    const wrapper = makeWrapper()
    await flushPromises()
    const file = new File(['data'], 'photo.png', { type: 'image/png' })
    // jsdom 24 does not implement File.arrayBuffer(); add it directly on this instance
    // so the emulator multipart code path can run without hitting a missing API.
    Object.defineProperty(file, 'arrayBuffer', {
      value: vi.fn().mockResolvedValue(new ArrayBuffer(4)),
      configurable: true
    })
    ;(wrapper.vm as any).selectedFile = file
    ;(wrapper.vm as any).fileName = 'photo.png'
    ;(wrapper.vm as any).selectedDocumentType = { docTypeCode: 'IMP' }
    mockSignedUrlSvc.requestSignedUrl.mockResolvedValueOnce({
      uploadUrl: '/storage-proxy/upload/storage/v1/b/bucket/o?uploadType=multipart',
      objectName: 'documents/IMP/photo.png'
    })
    mockAxios.post.mockResolvedValueOnce({ status: 200 })
    mockDocSvc.addNewDocument.mockResolvedValueOnce(201)
    await (wrapper.vm as any).uploadAndSave()
    await flushPromises()

    expect((wrapper.vm as any).uploadError).toBeNull()
    expect(mockAxios.post).toHaveBeenCalled()
    expect(mockAxios.put).not.toHaveBeenCalled()
  })

  it('Dialog_OnSuccess_EmitsUploaded', async () => {
    const wrapper = makeWrapper()
    await flushPromises()
    const file = new File(['data'], 'photo.png', { type: 'image/png' })
    ;(wrapper.vm as any).selectedFile = file
    ;(wrapper.vm as any).fileName = 'photo.png'
    ;(wrapper.vm as any).selectedDocumentType = { docTypeCode: 'IMP' }
    mockSignedUrlSvc.requestSignedUrl.mockResolvedValueOnce({
      uploadUrl: 'https://gcs.example.com/obj',
      objectName: 'documents/IMP/photo.png'
    })
    mockAxios.put.mockResolvedValueOnce({ status: 200 })
    mockDocSvc.addNewDocument.mockResolvedValueOnce(201)
    await (wrapper.vm as any).uploadAndSave()
    await flushPromises()
    expect(wrapper.emitted('uploaded')).toBeTruthy()
  })

  it('Dialog_OnSuccess_EmitsUpdateVisibleFalse', async () => {
    const wrapper = makeWrapper()
    await flushPromises()
    const file = new File(['data'], 'photo.png', { type: 'image/png' })
    ;(wrapper.vm as any).selectedFile = file
    ;(wrapper.vm as any).fileName = 'photo.png'
    ;(wrapper.vm as any).selectedDocumentType = { docTypeCode: 'IMP' }
    mockSignedUrlSvc.requestSignedUrl.mockResolvedValueOnce({
      uploadUrl: 'https://gcs.example.com/obj',
      objectName: 'documents/IMP/photo.png'
    })
    mockAxios.put.mockResolvedValueOnce({ status: 200 })
    mockDocSvc.addNewDocument.mockResolvedValueOnce(201)
    await (wrapper.vm as any).uploadAndSave()
    await flushPromises()
    expect(wrapper.emitted('update:visible')).toBeTruthy()
    expect(wrapper.emitted('update:visible')![0]).toEqual([false])
  })

  it('Dialog_On403_ShowsPermissionError', async () => {
    const wrapper = makeWrapper()
    await flushPromises()
    ;(wrapper.vm as any).selectedFile = new File(['d'], 'f.png', { type: 'image/png' })
    ;(wrapper.vm as any).fileName = 'f.png'
    ;(wrapper.vm as any).selectedDocumentType = { docTypeCode: 'IMP' }
    mockSignedUrlSvc.requestSignedUrl.mockResolvedValueOnce({
      uploadUrl: 'https://gcs.example.com/obj',
      objectName: 'x'
    })
    mockAxios.put.mockRejectedValueOnce({ response: { status: 403 } })
    await (wrapper.vm as any).uploadAndSave()
    await flushPromises()
    expect((wrapper.vm as any).uploadError).toBe('Upload permission denied. The upload URL may have expired.')
  })

  it('Dialog_On400_ShowsInvalidRequestError', async () => {
    const wrapper = makeWrapper()
    await flushPromises()
    ;(wrapper.vm as any).selectedFile = new File(['d'], 'f.png', { type: 'image/png' })
    ;(wrapper.vm as any).fileName = 'f.png'
    ;(wrapper.vm as any).selectedDocumentType = { docTypeCode: 'IMP' }
    mockSignedUrlSvc.requestSignedUrl.mockResolvedValueOnce({
      uploadUrl: 'https://gcs.example.com/obj',
      objectName: 'x'
    })
    mockAxios.put.mockRejectedValueOnce({ response: { status: 400 } })
    await (wrapper.vm as any).uploadAndSave()
    await flushPromises()
    expect((wrapper.vm as any).uploadError).toBe('Invalid upload request. Please check the file and try again.')
  })

  it('Dialog_OnGenericError_ShowsGenericError', async () => {
    const wrapper = makeWrapper()
    await flushPromises()
    ;(wrapper.vm as any).selectedFile = new File(['d'], 'f.png', { type: 'image/png' })
    ;(wrapper.vm as any).fileName = 'f.png'
    ;(wrapper.vm as any).selectedDocumentType = { docTypeCode: 'IMP' }
    mockSignedUrlSvc.requestSignedUrl.mockResolvedValueOnce({
      uploadUrl: 'https://gcs.example.com/obj',
      objectName: 'x'
    })
    mockAxios.put.mockRejectedValueOnce(new Error('Network error'))
    await (wrapper.vm as any).uploadAndSave()
    await flushPromises()
    expect((wrapper.vm as any).uploadError).toBe('An error occurred during upload. Please try again.')
  })
})
