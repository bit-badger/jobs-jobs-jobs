<template>
  <article>
    <h3 class="pb-3">Account Deletion</h3>
    <load-data :load="denyAccount">
      <p v-if="isDeleted">
        The account was deleted successfully; sorry for the trouble.
      </p>
      <p v-else>
        The confirmation token did not match any pending accounts; if this was an inadvertently created account, it has
        likely already been deleted.
      </p>
    </load-data>
  </article>
</template>

<script setup lang="ts">
import { ref } from "vue"
import { useRoute } from "vue-router"

import api from "@/api"
import LoadData from "@/components/LoadData.vue"

const route = useRoute()

/** Whether the account was deleted */
const isDeleted = ref(false)

/** Deny the account after confirming the token */
const denyAccount = async (errors: string[]) => {
  const resp = await api.citizen.denyAccount(route.params.token as string)
  if (typeof resp === "string") {
    errors.push(resp)
  } else {
    isDeleted.value = resp
  }
}
</script>
