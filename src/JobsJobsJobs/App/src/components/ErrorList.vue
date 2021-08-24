<template lang="pug">
template(v-if='errors.length > 0')
  p The following error#[template(v-if='errors.length !== 1') s] occurred:
  ul: li(v-for='(error, idx) in errors' :key='idx') {{error}}
slot(v-else)
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
