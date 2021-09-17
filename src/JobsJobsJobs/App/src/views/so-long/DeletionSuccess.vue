<template lang="pug">
article
  h3.pb-3 Account Deletion Success
  p.
    Your account has been successfully deleted. To revoke the permissions you have previously granted to this
    application, find it in #[a(:href="`${url}/oauth/authorized_applications`") this list] and click
    #[strong &times; Revoke]. Otherwise, clicking &ldquo;Log On&rdquo; in the left-hand menu will create a new, empty
    account without prompting you further.
  p Thank you for participating, and thank you for your courage. #GitmoNation
</template>

<script setup lang="ts">
import { computed, onMounted } from "vue"
import { useRoute } from "vue-router"
import { useStore, Actions } from "@/store"

const route = useRoute()
const store = useStore()

/** The abbreviation of the instance from which the deleted user had authorized access */
const abbr = route.params.abbr as string

/** The URL of that instance */
const url = computed(() => store.state.instances.find(it => it.abbr === abbr)?.url ?? "")

onMounted(async () => { await store.dispatch(Actions.EnsureInstances) })

</script>
