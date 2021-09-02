<template lang="pug">
article
  p &nbsp;
  load-data(:load="retrieveInstances")
    p.fst-italic(v-if="selected") Sending you over to {{selected.name}} to log on; see you back in just a second&hellip;
    template(v-else)
      p.text-center Please select your No Agenda-affiliated Mastodon instance
      p.text-center(v-for="it in instances" :key="it.abbr")
        button.btn.btn-primary(@click.prevent="select(it.abbr)") {{it.name}}
</template>

<script setup lang="ts">
import { computed, Ref, ref } from "vue"
import api, { Instance } from "@/api"

import LoadData from "@/components/LoadData.vue"

/** The instances configured for Jobs, Jobs, Jobs */
const instances : Ref<Instance[]> = ref([])

/** Whether authorization is in progress */
const selected : Ref<Instance | undefined> = ref(undefined)

/** The authorization URL to which the user should be directed */
const authUrl = computed(() => {
  if (selected.value) {
    /** The client ID for Jobs, Jobs, Jobs at No Agenda Social */
    const client = `client_id=${selected.value.clientId}`
    const scope = "scope=read:accounts"
    const redirect = `redirect_uri=${document.location.origin}/citizen/${selected.value.abbr}/authorized`
    const respType = "response_type=code"
    return `${selected.value.url}/oauth/authorize?${client}&${scope}&${redirect}&${respType}`
  }
  return ""
})

/**
 * Select a given Mastadon instance
 *
 * @param abbr The abbreviation of the instance being selected
 */
const select = (abbr : string) => {
  selected.value = instances.value.find(it => it.abbr === abbr)
  document.location.assign(authUrl.value)
}

/** Load the instances we have configured */
const retrieveInstances = async (errors : string[]) => {
  const instancesResp = await api.instances.all()
  if (typeof instancesResp === "string") {
    errors.push(instancesResp)
  } else if (typeof instancesResp === "undefined") {
    errors.push("No instances found (this should not happen)")
  } else {
    instances.value = instancesResp
  }
}
</script>
