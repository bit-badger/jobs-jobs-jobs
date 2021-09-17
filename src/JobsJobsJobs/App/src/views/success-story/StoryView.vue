<template lang="pug">
article
  load-data(:load="retrieveStory")
    h3
      | {{citizenName}}&rsquo;s Success Story
      .jjj-heading-label(v-if="story.fromHere")
        | &nbsp; &nbsp;#[span.badge.bg-success Via {{profileOrListing}} on Jobs, Jobs, Jobs]
    h4.pb-3.text-muted: full-date-time(:date="story.recordedOn")
    div(v-if="story.story" v-html="successStory")
</template>

<script setup lang="ts">
import { computed, Ref, ref } from "vue"
import { useRoute } from "vue-router"

import api, { LogOnSuccess, Success } from "@/api"
import { citizenName as citName } from "@/App.vue"
import { toHtml } from "@/markdown"
import { useStore } from "@/store"

import FullDateTime from "@/components/FullDateTime.vue"
import LoadData from "@/components/LoadData.vue"

const store = useStore()
const route = useRoute()

/** The currently logged-on user */
const user = store.state.user as LogOnSuccess

/** The story to be displayed */
const story : Ref<Success | undefined> = ref(undefined)

/** The citizen's name (real, display, or Mastodon, whichever is found first) */
const citizenName = ref("")

/** Retrieve the success story */
const retrieveStory = async (errors : string []) => {
  const storyResponse = await api.success.retrieve(route.params.id as string, user)
  if (typeof storyResponse === "string") {
    errors.push(storyResponse)
    return
  }
  if (typeof storyResponse === "undefined") {
    errors.push("Success story not found")
    return
  }
  story.value = storyResponse
  const citResponse = await api.citizen.retrieve(story.value.citizenId, user)
  if (typeof citResponse === "string") {
    errors.push(citResponse)
  } else if (typeof citResponse === "undefined") {
    errors.push("Citizen not found")
  } else {
    citizenName.value = citName(citResponse)
  }
}

/** Whether this success is from an employment profile or a job listing */
const profileOrListing = computed(() => story.value?.source === "profile" ? "employment profile" : "job listing")

/** The HTML success story */
const successStory = computed(() => toHtml(story.value?.story ?? ""))
</script>
