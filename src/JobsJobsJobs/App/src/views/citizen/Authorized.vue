<template lang="pug">
article
  p &nbsp;
  p(v-html="message")
</template>

<script setup lang="ts">
import { computed, onMounted } from "vue"
import { useRoute, useRouter } from "vue-router"
import { useStore, Actions, Mutations } from "@/store"
import { AFTER_LOG_ON_URL } from "@/router"

const store = useStore()
const route = useRoute()
const router = useRouter()

/** The abbreviation of the instance from which we received the code */
const abbr = route.params.abbr as string

/** Set the message for this component */
const setMessage = (msg : string) => store.commit(Mutations.SetLogOnState, msg)

/** Pass the code to the API and exchange it for a user and a JWT */
const logOn = async () => {
  await store.dispatch(Actions.EnsureInstances)
  const instance = store.state.instances.find(it => it.abbr === abbr)
  if (typeof instance === "undefined") {
    setMessage(`Mastodon instance ${abbr} not found`)
  } else {
    setMessage(`<em>Welcome back! Verifying your ${instance.name} account&hellip;</em>`)
    const code = route.query.code
    if (code) {
      await store.dispatch(Actions.LogOn, { abbr, code })
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
