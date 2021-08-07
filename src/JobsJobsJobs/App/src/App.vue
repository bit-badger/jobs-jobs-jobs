<template>
  <div class="jjj-app">
    <app-nav />
    <div class="jjj-main">
      <title-bar />
      <main class="container-fluid">
        <router-view v-slot="{ Component }">
          <transition name="fade" mode="out-in">
            <component :is="Component" />
          </transition>
        </router-view>
      </main>
      <app-footer />
      <app-toaster />
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent } from 'vue'
import AppFooter from './components/layout/AppFooter.vue'
import AppNav from './components/layout/AppNav.vue'
import AppToaster from './components/layout/AppToaster.vue'
import TitleBar from './components/layout/TitleBar.vue'

import 'bootstrap/dist/css/bootstrap.min.css'
import '@mdi/font/css/materialdesignicons.css'

export default defineComponent({
  name: 'App',
  components: {
    AppFooter,
    AppNav,
    AppToaster,
    TitleBar
  }
})

/**
 * Return "Yes" for true and "No" for false
 *
 * @param cond The condition to be checked
 * @returns "Yes" for true, "No" for false
 */
export function yesOrNo (cond : boolean) : string {
  return cond ? 'Yes' : 'No'
}
</script>

<style lang="sass">
// Overall app styles
html
  scroll-behavior: smooth
a:link,
a:visited
  text-decoration: none
a:hover
  text-decoration: underline
label.jjj-required::after
  color: red
  content: ' *'
// Styles for this component
.jjj-app
  display: flex
  flex-direction: row
.jjj-main
  flex-grow: 1
// Route transitions
.fade-enter-active,
.fade-leave-active
  transition: opacity 0.125s ease
.fade-enter-from,
.fade-leave-to
  opacity: 0
</style>
