<template>
  <v-app>
    <v-navigation-drawer app>
      <app-nav />
    </v-navigation-drawer>
    <v-app-bar color="secondary" app>
      <title-bar />
    </v-app-bar>
    <v-main>
      <v-container fluid>
        <router-view></router-view>
      </v-container>
    </v-main>
    <v-footer app>
      <app-footer />
    </v-footer>
  </v-app>
</template>

<script lang="ts">
import { defineComponent } from 'vue'
import AppFooter from './components/shared/AppFooter.vue'
import AppNav from './components/shared/AppNav.vue'
import TitleBar from './components/shared/TitleBar.vue'

export default defineComponent({
  name: 'App',
  components: {
    AppFooter,
    AppNav,
    TitleBar
  },
  data () {
    return {
      //
    }
  }
})
</script>

<style lang="sass">
// Overall app styles
h3
  font-size: 1.75rem
h4
  font-size: 1.5rem
h1, h2, h3, h4, h5
  margin-bottom: .5rem
  font-weight: 500
  line-height: 1.2
p
  padding-bottom: 1rem
ul
  padding-bottom: 1rem
  margin-left: 1.5rem
  li
    list-style-type: disc
// Styles for this component
.v-navigation-drawer
  background-image: linear-gradient(180deg, darkgreen 0%, green 70%)
  height: 100vh
.v-app-bar
  height: 3.5rem !important
  display: flex
  align-items: center
  justify-content: center
.v-footer
  flex-direction: row-reverse
</style>
