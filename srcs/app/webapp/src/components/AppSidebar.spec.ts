import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import { createRouter, createWebHistory } from 'vue-router'
import AppSidebar from '@/components/AppSidebar.vue'

function makeRouter() {
  return createRouter({
    history: createWebHistory(),
    routes: [
      { path: '/', name: 'home', component: { template: '<div/>' } },
      { path: '/document', name: 'document', component: { template: '<div/>' } }
    ]
  })
}

describe('AppSidebar.vue', () => {
  it('AppSidebar_Renders_BrandName', async () => {
    const router = makeRouter()
    await router.push('/')
    await router.isReady()
    const wrapper = mount(AppSidebar, { global: { plugins: [router] } })
    expect(wrapper.text()).toContain('DocStore')
  })

  it('AppSidebar_Renders_BrandIconSVG', async () => {
    const router = makeRouter()
    await router.push('/')
    await router.isReady()
    const wrapper = mount(AppSidebar, { global: { plugins: [router] } })
    expect(wrapper.find('.brand-icon svg').exists()).toBe(true)
  })

  it('AppSidebar_Renders_HomeNavItem', async () => {
    const router = makeRouter()
    await router.push('/')
    await router.isReady()
    const wrapper = mount(AppSidebar, { global: { plugins: [router] } })
    expect(wrapper.text()).toContain('Home')
  })

  it('AppSidebar_Renders_DocumentsNavItem', async () => {
    const router = makeRouter()
    await router.push('/')
    await router.isReady()
    const wrapper = mount(AppSidebar, { global: { plugins: [router] } })
    expect(wrapper.text()).toContain('Documents')
  })

  it('AppSidebar_UsesRouterLink_ForNavigation', async () => {
    const router = makeRouter()
    await router.push('/')
    await router.isReady()
    const wrapper = mount(AppSidebar, { global: { plugins: [router] } })
    const hrefs = wrapper.findAll('a').map((l) => l.attributes('href'))
    expect(hrefs).toContain('/')
    expect(hrefs).toContain('/document')
  })

  it('AppSidebar_HomeRoute_HomeItemHasActiveClass', async () => {
    const router = makeRouter()
    await router.push('/')
    await router.isReady()
    const wrapper = mount(AppSidebar, { global: { plugins: [router] } })
    const homeLink = wrapper.find('a[href="/"]')
    expect(homeLink.exists()).toBe(true)
    expect(homeLink.classes()).toContain('active')
  })

  it('AppSidebar_DocumentRoute_DocumentsItemHasActiveClass', async () => {
    const router = makeRouter()
    await router.push('/document')
    await router.isReady()
    const wrapper = mount(AppSidebar, { global: { plugins: [router] } })
    const docLink = wrapper.find('a[href="/document"]')
    expect(docLink.exists()).toBe(true)
    expect(docLink.classes()).toContain('active')
  })

  it('AppSidebar_DocumentRoute_HomeItemDoesNotHaveActiveClass', async () => {
    const router = makeRouter()
    await router.push('/document')
    await router.isReady()
    const wrapper = mount(AppSidebar, { global: { plugins: [router] } })
    const homeLink = wrapper.find('a[href="/"]')
    expect(homeLink.classes()).not.toContain('active')
  })
})
