<template>
  <p>{{message}}</p>
</template>

<script lang="ts">
import { ref } from 'vue'
import { logOn } from '@/auth'

export default {
  props: {
    code: {
      type: String
    }
  },
  setup() {
    const message = ref('Logging you on with No Agenda Social...')
    return {
      message
    }
  },
  async mounted() {
    const result = await logOn(this.code)
    if (result === '') {
      this.$router.push('/citizen/welcome')
    } else {
      this.message = `Unable to log on via No Agenda Social:\n${result}`
    }
  }
}
</script>