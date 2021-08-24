<template lang="pug">
template(v-if='true') {{formatted}}
</template>

<script lang="ts">
import { defineComponent } from 'vue'
import { format, parseJSON } from 'date-fns'
import { utcToZonedTime } from 'date-fns-tz'

/**
 * Parse a date from its JSON representation to a UTC-aligned date
 *
 * @param date The date string in JSON from JSON
 * @returns A UTC JavaScript date
 */
export function parseToUtc (date : string) : Date {
  return utcToZonedTime(parseJSON(date), Intl.DateTimeFormat().resolvedOptions().timeZone)
}

export default defineComponent({
  name: 'FullDate',
  props: {
    date: {
      type: String,
      required: true
    }
  },
  setup (props) {
    return {
      formatted: format(parseToUtc(props.date), 'PPP')
    }
  }
})
</script>
