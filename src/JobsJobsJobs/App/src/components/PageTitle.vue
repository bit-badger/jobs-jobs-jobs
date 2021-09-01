<template lang="pug">
p(v-if="false")
</template>

<script setup lang="ts">
import { onMounted, watch } from "vue"

const props = defineProps<{
  title: string
}>()

/** The name of the application */
const appName = "Jobs, Jobs, Jobs"

/** Set the page title based on the input title attribute */
const setTitle = () => {
  if (props.title === "") {
    document.title = appName
  } else {
    document.title = `${props.title} | ${appName}`
  }
}

onMounted(setTitle)

/** Change the page title when the property changes */
watch(() => props.title, setTitle)
</script>
