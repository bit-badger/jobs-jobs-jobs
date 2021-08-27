<template lang="pug">
span(:class="iconClass")
</template>

<script setup lang="ts">
const props = defineProps<{
  icon: string
}>()

/** The CSS class to display the requested icon */
const iconClass = `mdi mdi-${props.icon}`
</script>
