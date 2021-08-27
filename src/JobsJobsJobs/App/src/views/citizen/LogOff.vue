<template lang="pug">
article
  page-title(title="Logging off...")
  p &nbsp;
  p.fst-italic Logging off&hellip;
</template>

<script setup lang="ts">
import { onMounted } from "vue"
import { useRouter } from "vue-router"
import { toastSuccess } from "@/components/layout/AppToaster.vue"
import { useStore } from "@/store"

const store = useStore()
const router = useRouter()

onMounted(() => {
  store.commit("clearUser")
  toastSuccess("Log Off Successful &nbsp; | &nbsp; <strong>Have a Nice Day!</strong>")
  router.push("/")
})
</script>
