<template lang="pug">
svg(viewbox="0 0 24 24"): path(:fill="color || 'white'" :d="icon")
</template>

<script setup lang="ts">
const props = defineProps<{
  color?: string
  icon: string
}>()
</script>

<style lang="sass" scoped>
svg
  width: 24px
  height: 24px
</style>
