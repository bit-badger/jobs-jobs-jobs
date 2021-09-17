<template lang="pug">
article
  h3.pb-3 Search Profiles
  p(v-if="!searched").
    Enter one or more criteria to filter results, or just click &ldquo;Search&rdquo; to list all profiles.
  collapse-panel(headerText="Search Criteria" :collapsed="isCollapsed" @toggle="toggleCollapse")
    profile-search-form(v-model="criteria" @search="doSearch")
  error-list(:errors="errors")
    p.pt-3(v-if="searching") Searching profiles&hellip;
    template(v-else)
      table.table.table-sm.table-hover.pt-3(v-if="results.length > 0")
        thead: tr
          th(scope="col") Profile
          th(scope="col") Name
          th.text-center(scope="col" v-if="wideDisplay") Seeking?
          th.text-center(scope="col") Remote?
          th.text-center(scope="col" v-if="wideDisplay") Full-Time?
          th(scope="col" v-if="wideDisplay") Last Updated
        tbody: tr(v-for="profile in results" :key="profile.citzenId")
          td: router-link(:to="`/profile/${profile.citizenId}/view`") View
          td(:class="{ 'fw-bold' : profile.seekingEmployment }") {{profile.displayName}}
          td.text-center(v-if="wideDisplay") {{yesOrNo(profile.seekingEmployment)}}
          td.text-center {{yesOrNo(profile.remoteWork)}}
          td.text-center(v-if="wideDisplay") {{yesOrNo(profile.fullTime)}}
          td(v-if="wideDisplay"): full-date(:date="profile.lastUpdatedOn")
      p.pt-3(v-else-if="searched") No results found for the specified criteria
</template>

<script setup lang="ts">
import { Ref, ref, watch } from "vue"
import { useRoute, useRouter } from "vue-router"
import { useBreakpoints, breakpointsBootstrapV5 } from "@vueuse/core"

import { yesOrNo } from "@/App.vue"
import api, { LogOnSuccess, ProfileSearch, ProfileSearchResult } from "@/api"
import { queryValue } from "@/router"
import { useStore } from "@/store"

import CollapsePanel from "@/components/CollapsePanel.vue"
import ErrorList from "@/components/ErrorList.vue"
import FullDate from "@/components/FullDate.vue"
import ProfileSearchForm from "@/components/profile/SearchForm.vue"

const store = useStore()
const route = useRoute()
const router = useRouter()
const breakpoints = useBreakpoints(breakpointsBootstrapV5)

/** Any errors encountered while retrieving data */
const errors : Ref<string[]> = ref([])

/** Whether we are currently searching (retrieving data) */
const searching = ref(false)

/** Whether a search has been performed on this page since it has been loaded */
const searched = ref(false)

/** An empty set of search criteria */
const emptyCriteria = {
  continentId: "",
  skill: undefined,
  bioExperience: undefined,
  remoteWork: ""
}

/** The search criteria being built from the page */
const criteria : Ref<ProfileSearch> = ref(emptyCriteria)

/** The current search results */
const results : Ref<ProfileSearchResult[]> = ref([])

/** Whether the search criteria should be collapsed */
const isCollapsed = ref(searched.value && results.value.length > 0)

/** Hide certain columns if the display is too narrow */
const wideDisplay = breakpoints.greater("sm")

/** Set up the page to match its requested state */
const setUpPage = async () => {
  if (queryValue(route, "searched") === "true") {
    searched.value = true
    try {
      searching.value = true
      // Hold variable for ensuring continent ID is not undefined here, but excluded from search payload
      const contId = queryValue(route, "continentId")
      const searchParams : ProfileSearch = {
        continentId: contId === "" ? undefined : contId,
        skill: queryValue(route, "skill"),
        bioExperience: queryValue(route, "bioExperience"),
        remoteWork: queryValue(route, "remoteWork") ?? ""
      }
      const searchResult = await api.profile.search(searchParams, store.state.user as LogOnSuccess)
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

/** Show and hide the search parameter panel */
const toggleCollapse = (it : boolean) => { isCollapsed.value = it }

/** Execute a search */
const doSearch = () => router.push({ query: { searched: "true", ...criteria.value } })
</script>
