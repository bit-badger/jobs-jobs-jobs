<template>
  <article>
    <page-title title="Logging on..." />
    <p v-html="message"></p>
  </article>
</template>

<script lang="ts">
import { computed, defineComponent, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useStore } from '@/store'
import { AFTER_LOG_ON_URL } from '@/router'

export default defineComponent({
  name: 'Authorized',
  setup () {
    const router = useRouter()
    const store = useStore()

    /** Pass the code to the API and exchange it for a user and a JWT */
    const logOn = async () => {
      const code = router.currentRoute.value.query.code
      if (code) {
        await store.dispatch('logOn', code)
        if (store.state.user !== undefined) {
          const afterLogOnUrl = window.localStorage.getItem(AFTER_LOG_ON_URL)
          if (afterLogOnUrl) {
            window.localStorage.removeItem(AFTER_LOG_ON_URL)
            router.push(afterLogOnUrl)
          } else {
            router.push('/citizen/dashboard')
          }
        }
      } else {
        store.commit('setLogOnState',
          'Did not receive a token from No Agenda Social (perhaps you clicked &ldquo;Cancel&rdquo;?)')
      }
    }

    onMounted(logOn)

    return {
      message: computed(() => store.state.logOnState)
    }
  }
})
</script>
