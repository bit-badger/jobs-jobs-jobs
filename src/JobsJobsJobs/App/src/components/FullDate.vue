<template lang="pug">
template(v-if="true") {{formatted}}
</template>

<script setup lang="ts">
import { format } from "date-fns"
import { parseToUtc } from "./"

const props = defineProps<{
  date: string
}>()

/** The formatted date */
const formatted = format(parseToUtc(props.date), "PPP")
</script>
