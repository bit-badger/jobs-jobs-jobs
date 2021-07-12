<template>
  <span @click.prevent="playFile"><slot></slot></span><audio :id="clip"><source :src="clipSource"></audio>
</template>

<script lang="ts">
import { defineComponent } from 'vue'

export default defineComponent({
  name: 'AudioClip',
  props: {
    clip: {
      type: String,
      required: true
    }
  },
  computed: {
    clipSource () : string {
      return `/audio/${this.clip}.mp3`
    }
  },
  methods: {
    playFile () {
      const audio = document.getElementById(this.clip) as HTMLAudioElement
      audio.play()
    }
  }
})
</script>

<style lang="sass" scoped>
audio
  display: none
span
  border-bottom: dotted 1px lightgray
  &:hover
    cursor: pointer
</style>
