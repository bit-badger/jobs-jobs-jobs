<template>
  <aside class="collapse show p-3">
    <p class="home-link pb-3"><router-link to="/">Jobs, Jobs, Jobs</router-link></p>
    <p>&nbsp;</p>
    <nav>
      <template v-if="isLoggedOn">
        <router-link to="/citizen/dashboard"><icon icon="view-dashboard-variant" />&nbsp; Dashboard</router-link>
        <router-link to="/citizen/profile"><icon icon="pencil" />&nbsp; My Profile</router-link>
        <router-link to="/profile/search" class="separator">
          <icon icon="view-list-outline" />&nbsp; View Profiles
        </router-link>
        <router-link to="/listings/mine"><icon icon="sign-text" />&nbsp; My Job Listings</router-link>
        <router-link to="/listings/search" class="separator">
          <icon icon="newspaper-variant-multiple-outline" />&nbsp; View Listings
        </router-link>
        <router-link to="/success-story/list"><icon icon="thumb-up" />&nbsp; Success Stories</router-link>
        <router-link to="/citizen/log-off"><icon icon="logout-variant" />&nbsp; Log Off</router-link>
      </template>
      <template v-else>
        <router-link to="/"><icon icon="home" />&nbsp; Home</router-link>
        <router-link to="/profile/seeking"><icon icon="view-list-outline" />&nbsp; Job Seekers</router-link>
        <router-link to="/citizen/log-on"><icon icon="login-variant" />&nbsp; Log On</router-link>
      </template>
      <router-link to="/how-it-works"><icon icon="help-circle-outline" />&nbsp; How It Works</router-link>
    </nav>
  </aside>
</template>

<script lang="ts">
import { computed, defineComponent } from 'vue'
import { useStore } from '@/store'

export default defineComponent({
  name: 'AppNav',
  setup () {
    const store = useStore()

    return {
      /** Whether a user is logged in or not */
      isLoggedOn: computed(() => store.state.user !== undefined)
    }
  }
})
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
  margin-bottom: 1rem
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
nav > a.separator
  border-bottom: solid 1px white
</style>
