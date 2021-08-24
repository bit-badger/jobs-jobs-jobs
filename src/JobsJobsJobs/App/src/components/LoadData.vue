<template lang="pug">
div(v-if='loading') Loading&hellip;
error-list(v-else :errors='errors')
  slot
</template>

<script lang="ts">
import { defineComponent, onMounted, ref } from 'vue'
import ErrorList from './ErrorList.vue'

export default defineComponent({
  name: 'LoadData',
  components: { ErrorList },
  props: {
    load: {
      type: Function,
      required: true
    }
  },
  setup (props) {
    /** Type the input function */
    const func = props.load as (errors: string[]) => Promise<unknown>

    /** Errors encountered during loading */
    const errors : string[] = []

    /** Whether we are currently loading data */
    const loading = ref(true)

    /** Call the data load function */
    const loadData = async () => {
      try {
        await func(errors)
      } finally {
        loading.value = false
      }
    }

    onMounted(loadData)

    return {
      loading,
      errors
    }
  }
})
</script>
