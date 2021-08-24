<template lang="pug">
footer: p.text-muted.
  Jobs, Jobs, Jobs v{{appVersion}} &bull; #[router-link(to='/privacy-policy') Privacy Policy]
  &bull; #[router-link(to='/terms-of-service') Terms of Service]
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
footer
  display: flex
  flex-direction: row-reverse
p
  padding-top: 2rem
  padding-right: .5rem
  font-style: italic
  font-size: .8rem
</style>
