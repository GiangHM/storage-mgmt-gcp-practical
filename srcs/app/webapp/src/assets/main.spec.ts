import { describe, it, expect } from 'vitest'
import { readFileSync } from 'fs'
import { dirname, resolve } from 'path'
import { fileURLToPath } from 'url'

const dir = dirname(fileURLToPath(import.meta.url))
const mainCss = readFileSync(resolve(dir, './main.css'), 'utf-8')
const baseCss = readFileSync(resolve(dir, './base.css'), 'utf-8')

describe('main.css design tokens', () => {
  it('Tokens_ColorGreen_IsDefined', () => {
    expect(mainCss).toMatch(/--color-green\s*:/)
  })

  it('Tokens_SidebarWidth_IsDefined', () => {
    expect(mainCss).toMatch(/--sidebar-width\s*:/)
  })

  it('Tokens_BorderColor_IsDefined', () => {
    expect(mainCss).toMatch(/--border-color\s*:/)
  })

  it('Tokens_ExistingColorBackground_StillInBaseCss', () => {
    expect(baseCss).toMatch(/--color-background\s*:/)
  })

  it('Tokens_ExistingColorText_StillInBaseCss', () => {
    expect(baseCss).toMatch(/--color-text\s*:/)
  })
})
