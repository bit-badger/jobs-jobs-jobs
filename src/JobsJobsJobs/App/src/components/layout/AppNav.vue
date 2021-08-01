<template>
  <aside>
    <p class="home-link"><router-link to="/">Jobs, Jobs, Jobs</router-link></p>
    <p>&nbsp;</p>
    <nav>
      <template v-if="isLoggedOn">
        <router-link to="/citizen/dashboard"><v-icon icon="mdi-view-dashboard-variant" />Dashboard</router-link>
        <router-link to="/citizen/profile"><v-icon icon="mdi-pencil" /> Edit Your Profile</router-link>
        <router-link to="/profile/search"><v-icon icon="mdi-view-list-outline" /> View Profiles</router-link>
        <router-link to="/success-story/list"><v-icon icon="mdi-thumb-up" /> Success Stories</router-link>
        <router-link to="/citizen/log-off"><v-icon icon="mdi-logout-variant" /> Log Off</router-link>
      </template>
      <template v-else>
        <router-link to="/"><v-icon icon="mdi-home" /> Home</router-link>
        <router-link to="/profile/seeking"><v-icon icon="mdi-view-list-outline" /> Job Seekers</router-link>
        <router-link to="/citizen/log-on"><v-icon icon="mdi-login-variant" /> Log On</router-link>
      </template>
      <router-link to="/how-it-works"><v-icon icon="mdi-help-circle-outline" /> How It Works</router-link>
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
  color: white
  margin: 1rem
  font-size: 1.2rem
a:link, a:visited
  text-decoration: none
  color: white
  font-weight: 500
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
</style>
