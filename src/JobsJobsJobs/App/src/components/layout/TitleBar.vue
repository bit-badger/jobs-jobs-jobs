<template lang="pug">
nav.navbar.navbar-light.bg-light
  span &nbsp;
  span.navbar-text.
    (&hellip;and Jobs &ndash; #[audio-clip(clip="pelosi-jobs") Let&rsquo;s Vote for Jobs!])
</template>

<script setup lang="ts">
import AudioClip from "@/components/AudioClip.vue"
</script>

<style lang="sass" scoped>
.navbar-text
  font-style: italic
  padding-right: 1rem
</style>
