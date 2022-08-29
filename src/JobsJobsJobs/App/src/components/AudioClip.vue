<template>
  <span @click="playFile">
    <slot />
    <audio :id="clip"><source :src="clipSource" /></audio>
  </span>
</template>

<script setup lang="ts">
const props = defineProps<{
  clip: string
}>()

/** The full relative URL for the audio clip */
const clipSource = `/audio/${props.clip}.mp3`

/** Play the audio file */
const playFile = () => {
  const audio = document.getElementById(props.clip) as HTMLAudioElement
  audio.play()
}
</script>

<style lang="sass" scoped>
audio
  display: none
span
  border-bottom: dotted 1px lightgray
  &:hover
    cursor: pointer
</style>
