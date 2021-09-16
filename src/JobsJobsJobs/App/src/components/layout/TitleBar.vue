<template lang="pug">
nav.navbar.navbar-dark.mobile
  span.navbar-text: router-link(to="/") Jobs, Jobs, Jobs
  button.btn(data-bs-toggle="offcanvas" data-bs-target="#mobileMenu" aria-controls="mobileMenu")
    icon(:icon="mdiMenu")
nav.navbar.navbar-light.bg-light.wide
  span &nbsp;
  span.navbar-text.
    (&hellip;and Jobs &ndash; #[audio-clip(clip="pelosi-jobs") Let&rsquo;s Vote for Jobs!])
</template>

<script setup lang="ts">
import { mdiMenu } from "@mdi/js"
import AudioClip from "@/components/AudioClip.vue"
</script>

<style lang="sass" scoped>
.mobile
  background-image: linear-gradient(0deg, green 0%, darkgreen 70%)
  button
    padding: 0 1rem 0 0
  .navbar-text
    font-weight: bold
    color: white
.wide
  display: none
@media (min-width: 768px)
  .wide
    display: flex
  .mobile
    display: none
  .navbar-text
    font-style: italic
</style>
