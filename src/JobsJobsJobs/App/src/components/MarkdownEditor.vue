<template>
  <nav class="nav nav-pills">
    <a href="#" class="nav-link @MarkdownClass" @click.prevent="showMarkdown">Markdown</a>
    <a href="#" class="nav-link @PreviewClass" @click.prevent="showPreview">Preview</a>
  </nav>
  <section v-if="preview" class="preview" v-html="previewHtml">
  </section>
  <textarea v-else :id="id" class="form-control" rows="10" v-text="text"
            @input="$emit('update:text', $event.target.value)"></textarea>
</template>

<script lang="ts">
import { defineComponent, ref } from 'vue'
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
    const showMarkdown = () => { preview.value = false }

    /** Show the Markdown preview */
    const showPreview = () => {
      // TODO: render markdown as HTML
      previewHtml.value = props.text
      preview.value = true
    }

    return {
      preview,
      previewHtml,
      showMarkdown,
      showPreview
    }
  }
})
</script>
