<template>
  <div class="jjj-app">
    <app-nav />
    <div class="jjj-main">
      <title-bar />
      <main class="jjj-content container-fluid">
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
import { defineComponent, onMounted } from "vue"

import "bootstrap/dist/css/bootstrap.min.css"

import { Citizen } from "./api"
import { Mutations, useStore } from "./store"
import AppFooter from "./components/layout/AppFooter.vue"
import AppNav from "./components/layout/AppNav.vue"
import AppToaster from "./components/layout/AppToaster.vue"
import TitleBar from "./components/layout/TitleBar.vue"

export default defineComponent({
  components: {
    AppFooter,
    AppNav,
    AppToaster,
    TitleBar
  }
})

const store = useStore()

onMounted(() => store.commit(Mutations.SetTitle, "Jobs, Jobs, Jobs"))

/**
 * Return "Yes" for true and "No" for false
 *
 * @param cond The condition to be checked
 * @returns "Yes" for true, "No" for false
 */
export function yesOrNo (cond : boolean) : string {
  return cond ? "Yes" : "No"
}

/**
 * Get the display name for a citizen
 *
 * @param cit The citizen
 * @returns The citizen's display name
 */
export function citizenName (cit : Citizen) : string {
  return cit.displayName ?? `${cit.firstName} ${cit.lastName}`
}
</script>

<style lang="sass">
// Overall app styles
html
  scroll-behavior: smooth
a:link,
a:visited
  text-decoration: none
a:not(.btn):hover
  text-decoration: underline
label.jjj-required::after
  color: red
  content: ' *'
.jjj-heading-label
  display: inline-block
  font-size: 1rem
  text-transform: uppercase
// Styles for this component
.jjj-app
  display: flex
  flex-direction: row
.jjj-main
  flex-grow: 1
  display: flex
  flex-flow: column
  min-height: 100vh
.jjj-content
  flex-grow: 2
// Route transitions
.fade-enter-active,
.fade-leave-active
  transition: opacity 0.125s ease
.fade-enter-from,
.fade-leave-to
  opacity: 0
</style>
