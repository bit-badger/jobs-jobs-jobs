<template lang="pug">
article
  h3.pb-3 Help Wanted
  p(v-if="!searched").
    Enter relevant criteria to find results, or just click &ldquo;Search&rdquo; to see all current job listings.
  collapse-panel(headerText="Search Criteria" :collapsed="isCollapsed" @toggle="toggleCollapse")
    listing-search-form(v-model="criteria" @search="doSearch")
  error-list(:errors="errors")
    p.pt-3(v-if="searching") Searching job listings&hellip;
    template(v-else)
      table.table.table-sm.table-hover.pt-3(v-if="results.length > 0")
        thead: tr
          th(scope="col") Listing
          th(scope="col") Title
          th(scope="col") Location
          th.text-center(scope="col") Remote?
          th.text-center(scope="col") Needed By
        tbody: tr(v-for="it in results" :key="it.listing.id")
          td: router-link(:to="`/listing/${it.listing.id}/view`") View
          td {{it.listing.title}}
          td {{it.continent.name}} / {{it.listing.region}}
          td.text-center {{yesOrNo(it.listing.remoteWork)}}
          td.text-center(v-if="it.listing.neededBy") {{formatNeededBy(it.listing.neededBy)}}
          td.text-center(v-else) N/A
      p.pt-3(v-else-if="searched") No job listings found for the specified criteria
</template>

<script setup lang="ts">
import { ref, Ref, watch } from "vue"
import { useRoute, useRouter } from "vue-router"

import { formatNeededBy } from "./"
import { yesOrNo } from "@/App.vue"
import api, { ListingForView, ListingSearch, LogOnSuccess } from "@/api"
import { queryValue } from "@/router"
import { useStore } from "@/store"

import CollapsePanel from "@/components/CollapsePanel.vue"
import ErrorList from "@/components/ErrorList.vue"
import ListingSearchForm from "@/components/ListingSearchForm.vue"

const store = useStore()
const route = useRoute()
const router = useRouter()

/** Any errors encountered while retrieving data */
const errors : Ref<string[]> = ref([])

/** Whether we are currently searching (retrieving data) */
const searching = ref(false)

/** Whether a search has been performed on this page since it has been loaded */
const searched = ref(false)

/** An empty set of search criteria */
const emptyCriteria = {
  continentId: "",
  region: undefined,
  remoteWork: "",
  text: undefined
}

/** The search criteria being built from the page */
const criteria : Ref<ListingSearch> = ref(emptyCriteria)

/** The current search results */
const results : Ref<ListingForView[]> = ref([])

/** Whether the search criteria should be collapsed */
const isCollapsed = ref(searched.value && results.value.length > 0)

/** Set up the page to match its requested state */
const setUpPage = async () => {
  if (queryValue(route, "searched") === "true") {
    searched.value = true
    try {
      searching.value = true
      // Hold variable for ensuring continent ID is not undefined here, but excluded from search payload
      const contId = queryValue(route, "continentId")
      const searchParams : ListingSearch = {
        continentId: contId === "" ? undefined : contId,
        region: queryValue(route, "region"),
        remoteWork: queryValue(route, "remoteWork") ?? "",
        text: queryValue(route, "text")
      }
      const searchResult = await api.listings.search(searchParams, store.state.user as LogOnSuccess)
      if (typeof searchResult === "string") {
        errors.value.push(searchResult)
      } else if (searchResult === undefined) {
        errors.value.push(`The server returned a "Not Found" response (this should not happen)`)
      } else {
        results.value = searchResult
        searchParams.continentId = searchParams.continentId ?? ""
        criteria.value = searchParams
      }
    } finally {
      searching.value = false
    }
    isCollapsed.value = searched.value && results.value.length > 0
  } else {
    searched.value = false
    criteria.value = emptyCriteria
    errors.value = []
    results.value = []
  }
}

/** Refresh the page when the query string changes */
watch(() => route.query, setUpPage, { immediate: true })

/** Show or hide the search parameter panel */
const toggleCollapse = (it : boolean) => { isCollapsed.value = it }

/** Execute a search */
const doSearch = () => router.push({ query: { searched: "true", ...criteria.value } })
</script>
