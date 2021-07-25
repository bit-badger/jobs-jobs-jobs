<template>
  <p>TODO: convert this template</p>
</template>

<script lang="ts">
import { defineComponent } from 'vue'
import { useRoute } from 'vue-router'

export default defineComponent({
  name: 'ProfileEdit',
  setup () {
    const route = useRoute()
    const profileId = route.params.id

    return {
      profileId
    }
  }
})
</script>
