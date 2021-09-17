<template lang="pug">
#mobileMenu.offcanvas.offcanvas-end(v-if="showMobileMenu" tabindex="-1" aria-labelledby="mobileMenuLabel")
  .offcanvas-header
    h5#mobileMenuLabel Menu
    button.btn-close.text-reset(type="button" data-bs-dismiss="offcanvas" aria-label="Close")
  .offcanvas-body: app-links
aside.collapse.show.p-3(v-else)
  p.home-link.pb-3: router-link(to="/") Jobs, Jobs, Jobs
  p &nbsp;
  app-links
</template>

<script setup lang="ts">
import { useBreakpoints, breakpointsBootstrapV5 } from "@vueuse/core"
import AppLinks from "./AppLinks.vue"

const breakpoints = useBreakpoints(breakpointsBootstrapV5)

/** Whether the mobile menu or the desktop menu should be shown */
const showMobileMenu = breakpoints.smaller("md")
</script>

<style lang="sass" scoped>
aside,
#mobileMenu
  background-image: linear-gradient(180deg, darkgreen 0%, green 70%)
  color: white
  font-size: 1.2rem
aside
  min-height: 100vh
  width: 250px
  min-width: 250px
  position: sticky
  top: 0
  .home-link
    font-size: 1.2rem
    text-align: center
    background-color: rgba(0, 0, 0, .4)
    margin: -1rem
    padding: 1rem
  a:link,
  a:visited
    text-decoration: none
    color: white
</style>
