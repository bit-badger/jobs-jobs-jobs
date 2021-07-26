<template>
  <article>
    <page-title title="Logging on..." />
    <p>{{message}}</p>
  </article>
</template>

<script lang="ts">
import { computed, defineComponent, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useStore } from '../../store'

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
        if (store.state.user !== undefined) { router.push('/citizen/dashboard') }
      } else {
        store.commit('setLogOnState', 'Did not receive a token from No Agenda Social (perhaps you clicked "Cancel"?)')
      }
    }

    onMounted(logOn)

    return {
      message: computed(() => store.state.logOnState)
    }
  }
})
</script>
