<template>
  <nav class="nav nav-pills">
    <v-btn rounded="pill" :color="sourceColor" @click="showMarkdown">Markdown</v-btn> &nbsp;
    <v-btn rounded="pill" :color="previewColor" @click="showPreview">Preview</v-btn>
  </nav>
  <section v-if="preview" class="preview" v-html="previewHtml">
  </section>
  <textarea v-else :id="id" class="form-control" rows="10" v-text="text"
            @input="$emit('update:text', $event.target.value)"></textarea>
</template>

<script lang="ts">
import { computed, defineComponent, ref } from 'vue'
import marked from 'marked'
import { markedOptions } from '../api'

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
    }
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

    return {
      preview,
      previewHtml,
      showMarkdown,
      showPreview,
      sourceColor: computed(() => preview.value ? '' : 'primary'),
      previewColor: computed(() => preview.value ? 'primary' : '')
    }
  }
})
</script>

<style lang="sass" scoped>
textarea
  width: 100%
</style>
