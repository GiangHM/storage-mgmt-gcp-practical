import { describe, it, expect } from 'vitest'
import { existsSync } from 'fs'
import { dirname, resolve } from 'path'
import { fileURLToPath } from 'url'

const dir = dirname(fileURLToPath(import.meta.url))

describe('Unused component cleanup (Task 8)', () => {
  it('HelloWorld.vue is deleted', () => {
    expect(existsSync(resolve(dir, './HelloWorld.vue'))).toBe(false)
  })

  it('SignedUrlUpload.vue is deleted', () => {
    expect(existsSync(resolve(dir, './SignedUrlUpload.vue'))).toBe(false)
  })
})
