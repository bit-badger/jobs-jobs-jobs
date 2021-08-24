<template lang="pug">
nav.navbar.navbar-light.bg-light
  span &nbsp;
  span.navbar-text.
    (...and Jobs &ndash; #[audio-clip(clip='pelosi-jobs') Let's Vote for Jobs!])
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
.navbar-text
  font-style: italic
  padding-right: 1rem
</style>
