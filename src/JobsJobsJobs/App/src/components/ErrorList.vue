<template>
  <template v-if="errors.length > 0">
    <p>The following error<template v-if="errors.length !== 1">s</template> occurred:</p>
    <ul><li v-for="(error, idx) in errors" :key="idx">{{error}}</li></ul>
  </template>
  <slot v-else />
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
