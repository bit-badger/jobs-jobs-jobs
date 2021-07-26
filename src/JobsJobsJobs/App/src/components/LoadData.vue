<template>
  <div v-if="loading">Loading&hellip;</div>
  <template v-else>
    <div v-if="errors.length > 0">
      <p v-for="(error, idx) in errors" :key="idx">{{error}}</p>
    </div>
    <slot v-else></slot>
  </template>
</template>

<script lang="ts">
import { defineComponent, onMounted, ref } from 'vue'
export default defineComponent({
  name: 'LoadData',
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
