<script setup lang="ts">
</script>

<template>
  <div class="content">
    <div class="hero">
      <div class="hero-badge">
        <i class="ti ti-sparkles"></i>Welcome
      </div>
      <h1>Storage mini app</h1>
      <p>Upload and manage your documents securely.<br />More features coming soon.</p>
    </div>
  </div>
</template>

<style scoped>
.content {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
}

.hero {
  text-align: center;
  max-width: 420px;
  padding: 2rem;
}

.hero-badge {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  background: #f5f5f3;
  border: 0.5px solid #e0e0da;
  border-radius: 99px;
  padding: 4px 14px;
  font-size: 12px;
  color: #666;
  margin-bottom: 1.5rem;
}

.hero-badge i {
  font-size: 13px;
  color: var(--color-green, #41b883);
}

h1 {
  font-size: 28px;
  font-weight: 500;
  color: #1a1a1a;
  line-height: 1.3;
  margin-bottom: 0.75rem;
}

p {
  font-size: 14px;
  color: #888;
  line-height: 1.7;
}
</style>
