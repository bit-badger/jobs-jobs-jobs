<template>
  <template v-if="errors.length > 0">
    <p>The following error<template v-if="errors.length !== 1">s</template> occurred:</p>
    <ul>
      <li v-for="(error, idx) in errors" :key="idx">{{error}}</li>
    </ul>
  </template>
  <slot v-else></slot>
</template>

<script lang="ts">
import { defineComponent } from 'vue'

export default defineComponent({
  name: 'ErrorList',
  props: {
    errors: {
      type: Array,
      required: true
    }
  }
})
</script>

<style lang="sass" scoped>
ul li
  font-family: monospace
</style>
