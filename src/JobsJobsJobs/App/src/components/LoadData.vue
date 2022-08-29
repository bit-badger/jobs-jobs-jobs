<template>
  <div v-if="loading">Loading&hellip;</div>
  <error-list v-else :errors="errors">
    <slot />
  </error-list>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue"
import ErrorList from "./ErrorList.vue"

const props = defineProps<{
  load: (errors : string[]) => Promise<unknown>
}>()

/** Errors encountered during loading */
const errors : string[] = []

/** Whether we are currently loading data */
const loading = ref(true)

/** Call the data load function */
const loadData = async () => {
  try {
    await props.load(errors)
  } finally {
    loading.value = false
  }
}

onMounted(loadData)
</script>
