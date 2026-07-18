import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import PrimeVue from 'primevue/config'
import Aura from '@primevue/themes/aura'
import DocumentTable from '@/components/DocumentTable.vue'
import DocumentService from '@/services/DocumentService'

vi.mock('@/services/DocumentService')

const mockDocuments = [
  { docTypeCode: 'IMP', docUrl: 'documents/IMP/storageresource.PNG' },
  { docTypeCode: 'REP', docUrl: 'documents/REP/report_q1.pdf' }
]

function makeWrapper() {
  return mount(DocumentTable, {
    global: {
      plugins: [[PrimeVue, { theme: { preset: Aura } }]]
    }
  })
}

describe('DocumentTable.vue', () => {
  let mockService: any

  beforeEach(() => {
    vi.clearAllMocks()
    mockService = { getDocumentFromAPI: vi.fn().mockResolvedValue(mockDocuments) }
    ;(DocumentService as any).mockImplementation(() => mockService)
  })

  afterEach(() => {
    vi.resetAllMocks()
  })

  it('DocumentTable_OnMount_LoadsDocuments', async () => {
    makeWrapper()
    await flushPromises()
    expect(mockService.getDocumentFromAPI).toHaveBeenCalledOnce()
  })

  it('DocumentTable_HasNoStandaloneRefreshButton', async () => {
    const wrapper = makeWrapper()
    await flushPromises()
    const buttons = wrapper.findAll('button')
    expect(buttons.every((b) => b.text() !== 'Refresh')).toBe(true)
  })

  it('DocumentTable_ExposesRefreshMethod', async () => {
    const wrapper = makeWrapper()
    await flushPromises()
    expect(typeof (wrapper.vm as any).refresh).toBe('function')
  })

  it('DocumentTable_Refresh_ReloadsDocuments', async () => {
    const wrapper = makeWrapper()
    await flushPromises()
    mockService.getDocumentFromAPI.mockResolvedValue(mockDocuments)
    await (wrapper.vm as any).refresh()
    expect(mockService.getDocumentFromAPI).toHaveBeenCalledTimes(2)
  })

  it('DocumentTable_Renders_TypeBadgeForEachRow', async () => {
    const wrapper = makeWrapper()
    await flushPromises()
    const badges = wrapper.findAll('.badge')
    expect(badges.length).toBeGreaterThanOrEqual(mockDocuments.length)
    expect(badges[0].text()).toBe('IMP')
  })

  it('DocumentTable_Renders_ActionButtonsForEachRow', async () => {
    const wrapper = makeWrapper()
    await flushPromises()
    const viewButtons = wrapper.findAll('[title="View"]')
    const deleteButtons = wrapper.findAll('[title="Delete"]')
    expect(viewButtons.length).toBeGreaterThanOrEqual(mockDocuments.length)
    expect(deleteButtons.length).toBeGreaterThanOrEqual(mockDocuments.length)
  })
})
