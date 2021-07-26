<template>
  <p v-if="false"></p>
</template>

<script lang="ts">
import { defineComponent, onMounted } from 'vue'

export default defineComponent({
  name: 'PageTitle',
  props: {
    title: {
      type: String,
      required: true
    }
  },
  setup (props) {
    /** The name of the application */
    const appName = 'Jobs, Jobs, Jobs'

    /** Set the page title based on the input title attribute */
    const setTitle = () => {
      if (props.title === '') {
        document.title = appName
      } else {
        document.title = `${props.title} | ${appName}`
      }
    }

    onMounted(setTitle)

    return {
      setTitle
    }
  },
  watch: {
    title: 'setTitle'
  }
})
</script>
