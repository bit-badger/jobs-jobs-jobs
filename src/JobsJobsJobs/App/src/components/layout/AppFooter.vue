<template>
  <p>
    Jobs, Jobs, Jobs v{{appVersion}} &bull; <router-link to="/privacy-policy">Privacy Policy</router-link>
    &bull; <router-link to="/terms-of-service">Terms of Service</router-link>
  </p>
</template>

<script lang="ts">
import { defineComponent } from 'vue'
import { version } from '../../../package.json'

export default defineComponent({
  name: 'AppFooter',
  setup () {
    let appVersion : string = version
    while (appVersion.endsWith('.0')) {
      appVersion = appVersion.substring(0, appVersion.length - 2)
    }
    return {
      appVersion
    }
  }
})
</script>

<style lang="sass" scoped>
p
  padding-top: 2rem
  color: rgba(0, 0, 0, .4)
  font-style: italic
  font-size: .8rem
  a:link,
  a:visited
    color: rgba(0, 0, 0, .4)
    text-decoration: none
  a:hover
    text-decoration: underline
</style>
