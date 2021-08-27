<template lang="pug">
aside.collapse.show.p-3
  p.home-link.pb-3: router-link(to="/") Jobs, Jobs, Jobs
  p &nbsp;
  nav
    template(v-if="isLoggedOn")
      router-link(to="/citizen/dashboard") #[icon(icon="view-dashboard-variant")]&nbsp; Dashboard
      router-link(to="/help-wanted") #[icon(icon="newspaper-variant-multiple-outline")]&nbsp; Help Wanted!
      router-link(to="/profile/search") #[icon(icon="view-list-outline")]&nbsp; Employment Profiles
      router-link(to="/success-story/list") #[icon(icon="thumb-up")]&nbsp; Success Stories
      .separator
      router-link(to="/listings/mine") #[icon(icon="sign-text")]&nbsp; My Job Listings
      router-link(to="/citizen/profile") #[icon(icon="pencil")]&nbsp; My Employment Profile
      .separator
      router-link(to="/citizen/log-off") #[icon(icon="logout-variant")]&nbsp; Log Off
    template(v-else)
      router-link(to="/") #[icon(icon="home")]&nbsp; Home
      router-link(to="/profile/seeking") #[icon(icon="view-list-outline")]&nbsp; Job Seekers
      router-link(to="/citizen/log-on") #[icon(icon="login-variant")]&nbsp; Log On
    router-link(to="/how-it-works") #[icon(icon="help-circle-outline")]&nbsp; How It Works
</template>

<script setup lang="ts">
import { computed } from "vue"
import { useStore } from "@/store"

const store = useStore()

/** Whether a user is logged in or not */
const isLoggedOn = computed(() => store.state.user !== undefined)
</script>

<style lang="sass" scoped>
aside
  background-image: linear-gradient(180deg, darkgreen 0%, green 70%)
  color: white
  font-size: 1.2rem
  min-height: 100vh
  width: 250px
  min-width: 250px
  position: sticky
  top: 0
a:link, a:visited
  text-decoration: none
  color: white
  // font-weight: 500
.home-link
  font-size: 1.2rem
  text-align: center
  background-color: rgba(0, 0, 0, .4)
  margin: -1rem
  padding: 1rem
nav > a
  display: block
  width: 100%
  border-radius: .25rem
  padding: .5rem
  margin: .5rem 0
  font-size: 1rem
  > i
    vertical-align: top
    margin-right: 1rem
  &.router-link-exact-active
    background-color: rgba(255, 255, 255, .2)
  &:hover
    background-color: rgba(255, 255, 255, .5)
    color: black
    text-decoration: none
nav > div.separator
  border-bottom: solid 1px rgba(255, 255, 255, .75)
  height: 1px
</style>
