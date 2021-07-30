<template>
  <article>
    <page-title title="Logging off..." />
    <p>Logging off&hellip;</p>
  </article>
</template>

<script lang="ts">
import { defineComponent, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useStore } from '@/store'

export default defineComponent({
  name: 'LogOff',
  setup () {
    const store = useStore()
    const router = useRouter()

    onMounted(() => {
      store.commit('clearUser')
      router.push('/')
      // TODO: toast
    })

    return { }
  }
})
</script>
