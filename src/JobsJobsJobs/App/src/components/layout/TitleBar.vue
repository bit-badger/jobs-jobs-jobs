<template>
  <nav v-if="showMobileHeader" class="navbar navbar-dark">
    <span class="navbar-text"><router-link to="/">Jobs, Jobs, Jobs</router-link></span>
    <button class="btn" data-bs-toggle="offcanvas" data-bs-target="#mobileMenu" aria-controls="mobileMenu">
      <icon :icon="mdiMenu" />
    </button>
  </nav>
  <nav v-else class="navbar navbar-light bg-light">
    <span>&nbsp;</span>
    <span class="navbar-text">
      (&hellip;and Jobs &ndash; <audio-clip clip="pelosi-jobs">Let&rsquo;s Vote for Jobs!</audio-clip>)
    </span>
  </nav>
</template>

<script setup lang="ts">
import { mdiMenu } from "@mdi/js"
import { useBreakpoints, breakpointsBootstrapV5 } from "@vueuse/core"

import AudioClip from "@/components/AudioClip.vue"

const breakpoints = useBreakpoints(breakpointsBootstrapV5)

/** Whether to show the mobile or desktop header */
const showMobileHeader = breakpoints.smaller("md")
</script>

<style lang="sass" scoped>
.navbar-dark
  background-image: linear-gradient(0deg, green 0%, darkgreen 70%)
  padding-left: 1rem
  padding-right: 1rem
  button
    padding: 0
  .navbar-text
    font-weight: bold
    color: white
.navbar-light
  .navbar-text
    font-style: italic
    padding: 0 1rem 0 0
</style>
