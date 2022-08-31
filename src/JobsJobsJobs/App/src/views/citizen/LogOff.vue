<template>
  <article>
    <p>&nbsp;</p>
    <p class="fst-italic">Logging off&hellip;</p>
  </article>
</template>

<script setup lang="ts">
import { onMounted } from "vue"
import { useRouter } from "vue-router"
import { toastSuccess } from "@/components/layout/AppToaster.vue"
import { useStore, Mutations } from "@/store"

const store = useStore()
const router = useRouter()

onMounted(async () => {
  store.commit(Mutations.ClearUser)
  toastSuccess("Log Off Successful &nbsp; | &nbsp; <strong>Have a Nice Day!</strong>")
  await router.push("/")
})
</script>
