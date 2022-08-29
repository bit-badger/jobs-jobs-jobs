<template>
  <article>
    <h3 class="pb-3">Account Confirmation</h3>
    <load-data :load="confirmToken">
      <p v-if="isConfirmed">
        Your account was confirmed successfully! You may <router-link to="/citizen/log-on">log on here</router-link>.
      </p>
      <p v-else>
        The confirmation token did not match any pending accounts. Confirmation tokens are only valid for 3 days; if
        the token expired, you will need to re-register,
        which <router-link to="/citzen/register">you can do here</router-link>.
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

/** Whether the account was confirmed */
const isConfirmed = ref(false)

/** Confirm the account via the token */
const confirmToken = async (errors: string[]) => {
  const resp = await api.citizen.confirmToken(route.params.token as string)
  if (typeof resp === "string") {
    errors.push(resp)
  } else {
    isConfirmed.value = resp
  }
}
</script>
