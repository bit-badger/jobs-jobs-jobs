<template lang="pug">
template(v-if="errors.length > 0")
  p The following error#[template(v-if="errors.length !== 1") s] occurred:
  ul: li(v-for="(error, idx) in errors" :key="idx") {{error}}
slot(v-else)
</template>

<script setup lang="ts">
const props = defineProps<{
  errors: string[]
}>()
</script>

<style lang="sass" scoped>
ul li
  font-family: monospace
</style>
