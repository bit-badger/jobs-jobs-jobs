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
  setup (props) {
    /** The full relative URL for the audio clip */
    const clipSource = `/audio/${props.clip}.mp3`

    /** Play the audio file */
    const playFile = () => {
      const audio = document.getElementById(props.clip) as HTMLAudioElement
      audio.play()
    }

    return {
      clipSource,
      playFile
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
