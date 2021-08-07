<template>
  <span :class="iconClass"></span>
</template>

<script lang="ts">
import { defineComponent } from 'vue'

export default defineComponent({
  name: 'Icon',
  props: {
    icon: {
      type: String,
      required: true
    }
  },
  setup (props) {
    return {
      iconClass: `mdi mdi-${props.icon}`
    }
  }
})
</script>
