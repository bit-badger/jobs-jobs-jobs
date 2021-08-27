<template lang="pug">
.col-12
  nav.nav.nav-pills.pb-1
    button(:class="sourceClass" @click.prevent="showMarkdown") Markdown
    | &nbsp;
    button(:class="previewClass" @click.prevent="showPreview") Preview
  section.preview(v-if="preview" v-html="previewHtml")
  .form-floating(v-else)
    textarea.form-control.md-edit(:id="id" :class="{ 'is-invalid': isInvalid }" rows="10" v-text="text"
                                  @input="$emit('update:text', $event.target.value)")
    .invalid-feedback Please enter some text for {{label}}
    label(:for="id") {{label}}
</template>

<script setup lang="ts">
import { computed, ref } from "vue"
import { toHtml } from "@/markdown"

const props = defineProps<{
  id: string
  text: string
  label: string
  isInvalid?: boolean
}>()

const emit = defineEmits<{
  (e: "update:text", value : string) : void
}>()

/** Whether to show the Markdown preview */
const preview = ref(false)

/** The HTML rendered for preview purposes */
const previewHtml = ref("")

/** Show the Markdown source */
const showMarkdown = () => {
  preview.value = false
}

/** Show the Markdown preview */
const showPreview = () => {
  previewHtml.value = toHtml(props.text)
  preview.value = true
}

/** Button classes for the selected button */
const selected = "btn btn-primary btn-sm rounded-pill"

/** Button classes for the unselected button */
const unselected = "btn btn-outline-secondary btn-sm rounded-pill"

/** The CSS class for the Markdown source button */
const sourceClass = computed(() => preview.value ? unselected : selected)

/** The CSS class for the Markdown preview button */
const previewClass = computed(() => preview.value ? selected : unselected)
</script>

<style lang="sass" scoped>
.md-edit
  width: 100%
  // When wrapping this with Bootstrap's floating label, it shrinks the input down to what a normal one-line input
  // would be; this overrides that for the textarea in this component specifically
  height: inherit !important
</style>
