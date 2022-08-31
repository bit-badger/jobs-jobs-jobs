<template>
  <div v-if="showMobileMenu" id="mobileMenu" class="offcanvas offcanvas-end" tabindex="-1"
       aria-labelledby="mobileMenuLabel">
    <div class="offcanvas-header">
      <h5 id="mobileMenuLabel">Menu</h5>
      <button class="btn-close text-reset" type="button" data-bs-dismiss="offcanvas" aria-label="Close" />
    </div>
    <div class="offcanvas-body"><app-links /></div>
  </div>
  <aside v-else class="collapse show p-3">
    <p class="home-link pb-3"><router-link to="/">Jobs, Jobs, Jobs</router-link></p>
    <p>&nbsp;</p>
    <app-links />
  </aside>
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
