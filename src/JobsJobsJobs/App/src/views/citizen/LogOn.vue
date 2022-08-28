<template lang="pug">
article
  p &nbsp;
  p.fst-italic(v-if="selected") Sending you over to {{selected.name}} to log on; see you back in just a second&hellip;
  template(v-else)
    p.text-center Please select your No Agenda-affiliated Mastodon instance
    p.text-center(v-for="it in instances" :key="it.abbr")
      template(v-if="it.isEnabled")
        button.btn.btn-primary(@click.prevent="select(it.abbr)") {{it.name}}
      template(v-else).
        #[button.btn.btn-secondary(disabled="disabled") {{it.name}}]#[br]#[em {{it.reason}}]
  p: router-link(to="/citizen/register") Register
</template>

<script setup lang="ts">
import { computed, onMounted, Ref, ref } from "vue"
import { Instance } from "@/api"
import { useStore, Actions } from "@/store"

import LoadData from "@/components/LoadData.vue"

const store = useStore()

/** The instances configured for Jobs, Jobs, Jobs */
const instances = computed(() => store.state.instances)

/** Whether authorization is in progress */
const selected : Ref<Instance | undefined> = ref(undefined)

/** The authorization URL to which the user should be directed */
const authUrl = computed(() => {
  if (selected.value) {
    const client = `client_id=${selected.value.clientId}`
    const scope = "scope=read:accounts"
    const redirect = `redirect_uri=${document.location.origin}/citizen/${selected.value.abbr}/authorized`
    const respType = "response_type=code"
    return `${selected.value.url}/oauth/authorize?${client}&${scope}&${redirect}&${respType}`
  }
  return ""
})

/**
 * Select a given Mastodon instance
 *
 * @param abbr The abbreviation of the instance being selected
 */
const select = (abbr : string) => {
  selected.value = instances.value.find(it => it.abbr === abbr)
  document.location.assign(authUrl.value)
}

onMounted(async () => { await store.dispatch(Actions.EnsureInstances) })

</script>
