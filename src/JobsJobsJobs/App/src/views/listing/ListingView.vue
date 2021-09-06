<template lang="pug">
article
  page-title(:title="title")
  load-data(:load="retrieveListing")
    h3
      | {{it.listing.title}}
      .jjj-heading-label(v-if="it.listing.isExpired")
        | &nbsp; &nbsp; #[span.badge.bg-warning.text-dark Expired]
        template(v-if="it.listing.wasFilledHere") &nbsp; &nbsp;#[span.badge.bg-success Filled via Jobs, Jobs, Jobs]
    h4.pb-3.text-muted {{it.continent.name}} / {{it.listing.region}}
    p
      template(v-if="it.listing.neededBy").
        #[strong #[em NEEDED BY {{neededBy(it.listing.neededBy)}}]] &bull;
      |  Listed by #[a(:href="profileUrl" target="_blank") {{citizenName(citizen)}}]
    hr
    div(v-html="details")
</template>

<script setup lang="ts">
import { computed, ref, Ref } from "vue"
import { useRoute } from "vue-router"

import { formatNeededBy } from "./"
import api, { Citizen, ListingForView, LogOnSuccess } from "@/api"
import { citizenName } from "@/App.vue"
import { toHtml } from "@/markdown"
import { useStore } from "@/store"
import LoadData from "@/components/LoadData.vue"

const store = useStore()
const route = useRoute()

/** The currently logged-on user */
const user = store.state.user as LogOnSuccess

/** The requested job listing */
const it : Ref<ListingForView | undefined> = ref(undefined)

/** The citizen who posted this job listing */
const citizen : Ref<Citizen | undefined> = ref(undefined)

/** Retrieve the job listing and supporting data */
const retrieveListing = async (errors : string[]) => {
  const listingResp = await api.listings.retreiveForView(route.params.id as string, user)
  if (typeof listingResp === "string") {
    errors.push(listingResp)
  } else if (typeof listingResp === "undefined") {
    errors.push("Job Listing not found")
  } else {
    it.value = listingResp
    const citizenResp = await api.citizen.retrieve(listingResp.listing.citizenId, user)
    if (typeof citizenResp === "string") {
      errors.push(citizenResp)
    } else if (typeof citizenResp === "undefined") {
      errors.push("Listing Citizen not found (this should not happen)")
    } else {
      citizen.value = citizenResp
    }
  }
}

/** The page title (changes once the listing is loaded) */
const title = computed(() => it.value ? `${it.value.listing.title} | Job Listing` : "Loading Job Listing...")

/** The HTML details of the job listing */
const details = computed(() => toHtml(it.value?.listing.text ?? ""))

/** The NAS profile URL for the citizen who posted this job listing */
const profileUrl = computed(() => citizen.value ? citizen.value.profileUrl : "")

/** The needed by date, formatted in SHOUTING MODE */
const neededBy = (nb : string) => formatNeededBy(nb).toUpperCase()
</script>
