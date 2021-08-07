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
        <button @click.prevent="showToast('howdy', 'danger')">Show toast</button>
      </main>
      <app-footer />
      <div aria-live="polite" aria-atomic="true">
        <div class="toast-container position-absolute p-3 bottom-0 start-50 translate-middle-x" id="toasts"></div>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent } from 'vue'
import { Toast } from 'bootstrap'
import AppFooter from './components/layout/AppFooter.vue'
import AppNav from './components/layout/AppNav.vue'
import TitleBar from './components/layout/TitleBar.vue'

import 'bootstrap/dist/css/bootstrap.min.css'
import '@mdi/font/css/materialdesignicons.css'

export default defineComponent({
  name: 'App',
  components: {
    AppFooter,
    AppNav,
    TitleBar
  },
  setup () {
    return {
      showToast
    }
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

/**
 * Show a toast notification
 *
 * @param text The text of the notification
 * @param type The type of notification to show (defaults to 'success')
 * @param heading The optional text to use for the heading
 */
export function showToast (text : string, type = 'success', heading : string | undefined) : void {
  let header : HTMLDivElement | undefined
  if (heading) {
    header = document.createElement('div')
    header.className = 'toast-header'
    header.innerHTML = 'The Header'
  }
  const body = document.createElement('div')
  body.className = 'toast-body'
  body.innerHTML = text

  const toast = document.createElement('div')
  if (header) toast.appendChild(header)
  toast.appendChild(body)
  toast.className = `toast bg-${type} text-white`

  ;(document.getElementById('toasts') as HTMLDivElement).appendChild(toast)
  new Toast(toast).show()
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
