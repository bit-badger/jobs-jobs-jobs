<template>
  <nav class="navbar navbar-light bg-light">
    <span></span>
    <span class="navbar-text">
      (...and Jobs &ndash; <audio-clip clip="pelosi-jobs">Let's Vote for Jobs!</audio-clip>)
    </span>
  </nav>
</template>

<script lang="ts">
import { defineComponent } from 'vue'
import AudioClip from '@/components/AudioClip.vue'

export default defineComponent({
  name: 'TitleBar',
  components: {
    AudioClip
  }
})
</script>

<style lang="sass" scoped>
span
  font-style: italic
  padding-right: 1rem
  // padding: 0 .5rem 0 0
</style>
