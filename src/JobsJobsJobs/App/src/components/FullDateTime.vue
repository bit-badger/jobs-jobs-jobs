<template lang="pug">
template(v-if='true') {{formatted}}
</template>

<script lang="ts">
import { defineComponent } from 'vue'
import { format } from 'date-fns'
import { parseToUtc } from './FullDate.vue'

export default defineComponent({
  name: 'FullDateTime',
  props: {
    date: {
      type: String,
      required: true
    }
  },
  setup (props) {
    return {
      formatted: format(parseToUtc(props.date), 'PPPp')
    }
  }
})
</script>
