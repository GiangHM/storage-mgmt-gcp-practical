import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import { createRouter, createWebHistory } from 'vue-router'
import App from '@/App.vue'

function makeRouter() {
  return createRouter({
    history: createWebHistory(),
    routes: [
      { path: '/', name: 'home', component: { template: '<div/>' } },
      { path: '/document', name: 'document', component: { template: '<div/>' } }
    ]
  })
}

describe('App.vue', () => {
  it('App_Renders_AppSidebar', async () => {
    const router = makeRouter()
    await router.push('/')
    await router.isReady()
    const wrapper = mount(App, { global: { plugins: [router] } })
    expect(wrapper.findComponent({ name: 'AppSidebar' }).exists()).toBe(true)
  })

  it('App_DoesNotRender_Header', async () => {
    const router = makeRouter()
    await router.push('/')
    await router.isReady()
    const wrapper = mount(App, { global: { plugins: [router] } })
    expect(wrapper.find('header').exists()).toBe(false)
  })

  it('App_DoesNotRender_HelloWorld', async () => {
    const router = makeRouter()
    await router.push('/')
    await router.isReady()
    const wrapper = mount(App, { global: { plugins: [router] } })
    expect(wrapper.text()).not.toContain('Storage mini app - part 1: Upload document')
  })

  it('App_HomeRoute_TopbarTitleIsHome', async () => {
    const router = makeRouter()
    await router.push('/')
    await router.isReady()
    const wrapper = mount(App, { global: { plugins: [router] } })
    expect(wrapper.find('.topbar-title').text()).toBe('Home')
  })

  it('App_DocumentRoute_TopbarTitleIsDocuments', async () => {
    const router = makeRouter()
    await router.push('/document')
    await router.isReady()
    const wrapper = mount(App, { global: { plugins: [router] } })
    expect(wrapper.find('.topbar-title').text()).toBe('Documents')
  })

  it('App_Renders_RouterView', async () => {
    const router = makeRouter()
    await router.push('/')
    await router.isReady()
    const wrapper = mount(App, { global: { plugins: [router] } })
    expect(wrapper.findComponent({ name: 'RouterView' }).exists()).toBe(true)
  })
})
