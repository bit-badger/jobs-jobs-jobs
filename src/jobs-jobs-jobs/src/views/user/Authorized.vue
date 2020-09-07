<template>
  <p>Logging you on with No Agenda Social...</p>
</template>

<script lang="ts">
import { logOn } from '@/auth'

export default {
  props: {
    code: {
      type: String
    }
  },
  setup(props) {
    logOn(props.code)
  }
}
</script>