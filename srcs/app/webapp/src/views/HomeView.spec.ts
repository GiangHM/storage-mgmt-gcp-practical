import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import HomeView from '@/views/HomeView.vue'

describe('HomeView.vue', () => {
  it('HomeView_Renders_WelcomeBadge', () => {
    const wrapper = mount(HomeView)
    expect(wrapper.text()).toContain('Welcome')
  })

  it('HomeView_Renders_H1Title', () => {
    const wrapper = mount(HomeView)
    expect(wrapper.find('h1').text()).toBe('Storage mini app')
  })

  it('HomeView_Renders_SubtitleText', () => {
    const wrapper = mount(HomeView)
    expect(wrapper.text()).toContain('Upload and manage your documents securely')
  })

  it('HomeView_DoesNotImport_TheWelcome', () => {
    const wrapper = mount(HomeView)
    expect(wrapper.findComponent({ name: 'TheWelcome' }).exists()).toBe(false)
  })

  it('HomeView_HasHeroBadgeChip', () => {
    const wrapper = mount(HomeView)
    expect(wrapper.find('.hero-badge').exists()).toBe(true)
  })

  it('HomeView_HeroIsCentred', () => {
    const wrapper = mount(HomeView)
    expect(wrapper.find('.hero').exists()).toBe(true)
  })
})
