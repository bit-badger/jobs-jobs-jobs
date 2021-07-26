<template>
  <template v-if="true">{{formatted}}</template>
</template>

<script lang="ts">
import { defineComponent } from 'vue'
import { format, parseJSON } from 'date-fns'
import { utcToZonedTime } from 'date-fns-tz'

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
      formatted: format(
        utcToZonedTime(
          parseJSON(props.date), Intl.DateTimeFormat().resolvedOptions().timeZone),
        'PPPppp')
    }
  }
})
</script>
