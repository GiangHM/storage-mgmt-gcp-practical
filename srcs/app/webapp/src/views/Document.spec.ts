import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import PrimeVue from 'primevue/config'
import Aura from '@primevue/themes/aura'
import ToastService from 'primevue/toastservice'
import Document from '@/views/Document.vue'
import DocumentService from '@/services/DocumentService'
import DocumentTypeService from '@/services/DocumentTypeService'
import SignedUrlService from '@/services/SignedUrlService'

vi.mock('@/services/DocumentService')
vi.mock('@/services/DocumentTypeService')
vi.mock('@/services/SignedUrlService')

function makeWrapper() {
  return mount(Document, {
    global: {
      plugins: [
        [PrimeVue, { theme: { preset: Aura } }],
        ToastService
      ],
      stubs: {
        DocumentUploadDialog: { template: '<div class="stub-dialog" />', props: ['visible'] }
      }
    }
  })
}

describe('Document.vue', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    const mockDocSvc = { getDocumentFromAPI: vi.fn().mockResolvedValue([]) }
    ;(DocumentService as any).mockImplementation(() => mockDocSvc)
    ;(DocumentTypeService as any).mockImplementation(() => ({ getDocumentTypeFromAPI: vi.fn().mockResolvedValue([]) }))
    ;(SignedUrlService as any).mockImplementation(() => ({ requestSignedUrl: vi.fn() }))
  })

  it('Document_Renders_PageHeaderH2', async () => {
    const wrapper = makeWrapper()
    await flushPromises()
    expect(wrapper.find('h2').text()).toBe('Documents')
  })

  it('Document_Renders_AddDocumentButton', async () => {
    const wrapper = makeWrapper()
    await flushPromises()
    const buttons = wrapper.findAll('button')
    expect(buttons.some((b) => b.text().includes('Add document'))).toBe(true)
  })

  it('Document_DoesNotRender_SignedUrlUpload', async () => {
    const wrapper = makeWrapper()
    await flushPromises()
    expect(wrapper.findComponent({ name: 'SignedUrlUpload' }).exists()).toBe(false)
  })

  it('Document_InitialState_DialogIsHidden', async () => {
    const wrapper = makeWrapper()
    await flushPromises()
    expect((wrapper.vm as any).dialogVisible).toBe(false)
  })

  it('Document_AddDocumentButton_OpensDialog', async () => {
    const wrapper = makeWrapper()
    await flushPromises()
    const buttons = wrapper.findAll('button')
    const addBtn = buttons.find((b) => b.text().includes('Add document'))
    await addBtn!.trigger('click')
    expect((wrapper.vm as any).dialogVisible).toBe(true)
  })

  it('Document_Renders_DocumentTable', async () => {
    const wrapper = makeWrapper()
    await flushPromises()
    expect(wrapper.findComponent({ name: 'DocumentTable' }).exists()).toBe(true)
  })
})
