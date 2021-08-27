<template lang="pug">
article
  page-title(title="My Job Listings")
  h3.pb-3 My Job Listings
  p: router-link.btn.btn-outline-primary(to="/listing/new/edit") Add a New Job Listing
  load-data(:load="getListings")
    table.table.table-sm.table-hover.pt-3(v-if='listings.length > 0')
      thead: tr
        th(scope="col") Action
        th(scope="col") Title
        th(scope="col") Continent / Region
        th(scope="col") Created
        th(scope="col") Updated
      tbody: tr(v-for='it in listings' :key='it.listing.id')
        td: router-link(:to="`/listing/${it.listing.id}/edit`") Edit
        td {{it.listing.title}}
        td {{it.continent.name}} / {{it.listing.region}}
        td: full-date-time(:date='it.listing.createdOn')
        td: full-date-time(:date='it.listing.updatedOn')
    p.fst-italic(v-else) No job listings found
</template>

<script setup lang="ts">
import { Ref, ref } from "vue"
import api, { ListingForView, LogOnSuccess } from "@/api"
import { useStore } from "@/store"

import FullDateTime from "@/components/FullDateTime.vue"
import LoadData from "@/components/LoadData.vue"

const store = useStore()

/** The listings for the user */
const listings : Ref<ListingForView[]> = ref([])

/** Retrieve the job listing posted by the current citizen */
const getListings = async (errors : string[]) => {
  const listResult = await api.listings.mine(store.state.user as LogOnSuccess)
  if (typeof listResult === "string") {
    errors.push(listResult)
  } else if (typeof listResult === "undefined") {
    errors.push("API call returned 404 (this should not happen)")
  } else {
    listings.value = listResult
  }
}
</script>
