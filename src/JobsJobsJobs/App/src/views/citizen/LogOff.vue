<template lang="pug">
article
  p &nbsp;
  p.fst-italic Logging off&hellip;
</template>

<script setup lang="ts">
import { onMounted } from "vue"
import { useRouter } from "vue-router"
import { toastSuccess } from "@/components/layout/AppToaster.vue"
import { useStore, Mutations } from "@/store"

const store = useStore()
const router = useRouter()

onMounted(() => {
  store.commit(Mutations.ClearUser)
  toastSuccess("Log Off Successful &nbsp; | &nbsp; <strong>Have a Nice Day!</strong>")
  router.push("/")
})
</script>
