<template lang="pug">
article
  h3.pb-3 My Job Listings
  p: router-link.btn.btn-outline-primary(to="/listing/new/edit") Add a New Job Listing
  load-data(:load="getListings")
    h4.pb-2(v-if="expired.length > 0") Active Job Listings
    table.pb-3.table.table-sm.table-hover.pt-3(v-if="active.length > 0")
      thead: tr
        th(scope="col") Action
        th(scope="col") Title
        th(scope="col") Continent / Region
        th(scope="col") Created
        th(scope="col") Updated
      tbody: tr(v-for="it in active" :key="it.listing.id")
        td
          router-link(:to="`/listing/${it.listing.id}/edit`") Edit
          = " ~ "
          router-link(:to="`/listing/${it.listing.id}/view`") View
          = " ~ "
          router-link(:to="`/listing/${it.listing.id}/expire`") Expire
        td {{it.listing.title}}
        td {{it.continent.name}} / {{it.listing.region}}
        td: full-date-time(:date="it.listing.createdOn")
        td: full-date-time(:date="it.listing.updatedOn")
    p.pb-3.fst-italic(v-else) You have no active job listings
    template(v-if="expired.length > 0")
      h4.pb-2 Expired Job Listings
      table.table.table-sm.table-hover.pt-3
        thead: tr
          th(scope="col") Action
          th(scope="col") Title
          th(scope="col") Filled Here?
          th(scope="col") Expired
        tbody: tr(v-for="it in expired" :key="it.listing.id")
          td
            router-link(:to="`/listing/${it.listing.id}/view`") View
          td {{it.listing.title}}
          td {{yesOrNo(it.listing.wasFilledHere)}}
          td: full-date-time(:date="it.listing.updatedOn")
</template>

<script setup lang="ts">
import { computed, Ref, ref } from "vue"
import api, { ListingForView, LogOnSuccess } from "@/api"
import { yesOrNo } from "@/App.vue"
import { useStore } from "@/store"

import FullDateTime from "@/components/FullDateTime.vue"
import LoadData from "@/components/LoadData.vue"

const store = useStore()

/** The listings for the user */
const listings : Ref<ListingForView[]> = ref([])

/** The active (non-expired) listings entered by this user */
const active = computed(() => listings.value.filter(it => !it.listing.isExpired))

/** The expired listings entered by this user */
const expired = computed(() => listings.value.filter(it => it.listing.isExpired))

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
