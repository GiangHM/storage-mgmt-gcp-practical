<script setup lang="ts">
import { RouterLink } from 'vue-router'
</script>

<template>
  <aside class="sidebar">
    <div class="brand">
      <div class="brand-icon">
        <svg viewBox="0 0 196 170"><polygon points="98,0 196,170 0,170" /></svg>
      </div>
      <span class="brand-name">DocStore</span>
    </div>
    <div class="nav-section-label">Navigation</div>
    <RouterLink to="/" class="nav-item" exact-active-class="active" active-class="">
      <i class="ti ti-home"></i>Home
    </RouterLink>
    <RouterLink to="/document" class="nav-item" active-class="active">
      <i class="ti ti-files"></i>Documents
    </RouterLink>
  </aside>
</template>

<style scoped>
.sidebar {
  width: 210px;
  background: #fafaf8;
  border-right: 0.5px solid #e8e8e4;
  padding: 1.25rem 0;
  display: flex;
  flex-direction: column;
  gap: 2px;
  flex-shrink: 0;
  height: 100%;
}

.brand {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 0 1.25rem 1.25rem;
  border-bottom: 0.5px solid #e8e8e4;
  margin-bottom: 0.5rem;
}

.brand-icon {
  width: 28px;
  height: 28px;
  background: var(--color-green, #41b883);
  border-radius: 6px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.brand-icon svg {
  width: 14px;
  height: 14px;
  fill: white;
}

.brand-name {
  font-size: 13px;
  font-weight: 600;
  color: #1a1a1a;
}

.nav-section-label {
  font-size: 10px;
  font-weight: 600;
  letter-spacing: 0.07em;
  text-transform: uppercase;
  color: #aaa;
  padding: 0.75rem 1.25rem 0.25rem;
}

.nav-item {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 8px 1.25rem;
  font-size: 13px;
  color: #666;
  text-decoration: none;
}

.nav-item i {
  font-size: 16px;
}

.nav-item.active {
  background: #fff;
  color: #1a1a1a;
  font-weight: 500;
  border-right: 2px solid var(--color-green, #41b883);
}
</style>
