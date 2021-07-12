<template>
  <p>(...and Jobs &ndash; <audio-clip clip="pelosi-jobs">Let's Vote for Jobs!</audio-clip>)</p>
</template>

<script lang="ts">
import { defineComponent } from 'vue'
import AudioClip from './AudioClip.vue'

export default defineComponent({
  name: 'TitleBar',
  components: {
    AudioClip
  }
})
</script>

<style lang="sass" scoped>
p
  width: 100%
  text-align: right
  font-style: italic
  padding: 0 .5rem 0 0
</style>
