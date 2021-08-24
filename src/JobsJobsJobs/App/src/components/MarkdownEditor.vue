<template lang="pug">
.col-12
  nav.nav.nav-pills.pb-1
    button(:class='sourceClass' @click.prevent='showMarkdown') Markdown
    | &nbsp;
    button(:class='previewClass' @click.prevent='showPreview') Preview
  section.preview(v-if='preview' v-html='previewHtml')
  .form-floating(v-else)
    textarea(:id='id' :class="{ 'form-control': true, 'md-edit': true, 'is-invalid': isInvalid }" rows='10'
             v-text='text' @input="$emit('update:text', $event.target.value)")
    .invalid-feedback Please enter some text for {{label}}
    label(:for='id') {{label}}
</template>

<script lang="ts">
import { computed, defineComponent, ref } from 'vue'
import marked from 'marked'
import { markedOptions } from '@/api'

export default defineComponent({
  name: 'MarkdownEditor',
  props: {
    id: {
      type: String,
      required: true
    },
    text: {
      type: String,
      required: true
    },
    label: {
      type: String,
      required: true
    },
    isInvalid: { type: Boolean }
  },
  emits: ['update:text'],
  setup (props) {
    /** Whether to show the Markdown preview */
    const preview = ref(false)

    /** The HTML rendered for preview purposes */
    const previewHtml = ref('')

    /** Show the Markdown source */
    const showMarkdown = () => {
      preview.value = false
    }

    /** Show the Markdown preview */
    const showPreview = () => {
      previewHtml.value = marked(props.text, markedOptions)
      preview.value = true
    }

    /** Button classes for the selected button */
    const selected = 'btn btn-primary btn-sm rounded-pill'

    /** Button classes for the unselected button */
    const unselected = 'btn btn-outline-secondary btn-sm rounded-pill'

    return {
      preview,
      previewHtml,
      showMarkdown,
      showPreview,
      sourceClass: computed(() => preview.value ? unselected : selected),
      previewClass: computed(() => preview.value ? selected : unselected)
    }
  }
})
</script>

<style lang="sass" scoped>
.md-edit
  width: 100%
  // When wrapping this with Bootstrap's floating label, it shrinks the input down to what a normal one-line input
  // would be; this overrides that for the textarea in this component specifically
  height: inherit !important
</style>
