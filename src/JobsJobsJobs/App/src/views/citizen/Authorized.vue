<template lang="pug">
article
  page-title(title="Logging on...")
  p &nbsp;
  p(v-html="message")
</template>

<script setup lang="ts">
import { computed, onMounted } from "vue"
import { useRoute, useRouter } from "vue-router"
import api from "@/api"
import { useStore } from "@/store"
import { AFTER_LOG_ON_URL } from "@/router"

const store = useStore()
const route = useRoute()
const router = useRouter()

/** The abbreviation of the instance from which we received the code */
const abbr = route.params.abbr as string

/** Set the message for this component */
const setMessage = (msg : string) => store.commit("setLogOnState", msg)

/** Pass the code to the API and exchange it for a user and a JWT */
const logOn = async () => {
  const instance = await api.instances.byAbbr(abbr)
  if (typeof instance === "string") {
    setMessage(instance)
  } else if (typeof instance === "undefined") {
    setMessage(`Mastodon instance ${abbr} not found`)
  } else {
    const code = route.query.code
    if (code) {
      await store.dispatch("logOn", { abbr, code })
      if (store.state.user !== undefined) {
        const afterLogOnUrl = window.localStorage.getItem(AFTER_LOG_ON_URL)
        if (afterLogOnUrl) {
          window.localStorage.removeItem(AFTER_LOG_ON_URL)
          router.push(afterLogOnUrl)
        } else {
          router.push("/citizen/dashboard")
        }
      }
    } else {
      setMessage(`Did not receive a token from ${instance.name} (perhaps you clicked &ldquo;Cancel&rdquo;?)`)
    }
  }
}

onMounted(logOn)

/** Accessor for the log on state */
const message = computed(() => store.state.logOnState)
</script>
