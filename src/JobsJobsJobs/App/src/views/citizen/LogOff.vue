<template>
  <article>
    <page-title title="Logging off..." />
    <p>&nbsp;</p>
    <p class="fst-italic">Logging off&hellip;</p>
  </article>
</template>

<script lang="ts">
import { defineComponent, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { toastSuccess } from '@/components/layout/AppToaster.vue'
import { useStore } from '@/store'

export default defineComponent({
  name: 'LogOff',
  setup () {
    const store = useStore()
    const router = useRouter()

    onMounted(() => {
      store.commit('clearUser')
      toastSuccess('Log Off Successful &nbsp; | &nbsp; <strong>Have a Nice Day!</strong>')
      router.push('/')
    })

    return { }
  }
})
</script>
