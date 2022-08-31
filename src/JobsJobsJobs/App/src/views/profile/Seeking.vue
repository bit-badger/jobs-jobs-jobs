<template>
  <article>
    <h3 class="pb-3">People Seeking Work</h3>
    <p v-if="!searched">
      Enter one or more criteria to filter results, or just click &ldquo;Search&rdquo; to list all profiles.
    </p>
    <collapse-panel headerText="Search Criteria" :collapsed="isCollapsed" @toggle="toggleCollapse">
      <profile-public-search-form v-model="criteria" @search="doSearch" />
    </collapse-panel>
    <error-list :errors="errors">
      <p v-if="searching">Searching profiles&hellip;</p>
      <template v-else>
        <template v-if="results.length > 0">
          <p class="py-3">
            These profiles match your search criteria. To learn more about these people, join the merry band of human
            resources in the <a href="https://noagendashow.net" target="_blank" rel="noopener">No Agenda</a> tribe!
          </p>
          <table class="table table-sm table-hover">
            <thead>
              <tr>
                <th scope="col">Continent</th>
                <th class="text-center" scope="col">Region</th>
                <th class="text-center" scope="col">Remote?</th>
                <th class="text-center" scope="col">Skills</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(profile, idx) in results" :key="idx">
                <td>{{profile.continent}}</td>
                <td>{{profile.region}}</td>
                <td class="text-center">{{yesOrNo(profile.remoteWork)}}</td>
                <td><template v-for="(skill, idx) in profile.skills" :key="idx">{{skill}}<br></template></td>
              </tr>
            </tbody>
          </table>
        </template>
        <p v-else-if="searched" class="pt-3">No results found for the specified criteria</p>
      </template>
    </error-list>
  </article>
</template>

<script setup lang="ts">
import { Ref, ref, watch } from "vue"
import { useRoute, useRouter } from "vue-router"
import { yesOrNo } from "@/App.vue"
import api, { PublicSearch, PublicSearchResult } from "@/api"
import { queryValue } from "@/router"

import CollapsePanel from "@/components/CollapsePanel.vue"
import ErrorList from "@/components/ErrorList.vue"
import ProfilePublicSearchForm from "@/components/profile/PublicSearchForm.vue"

const route = useRoute()
const router = useRouter()

/** Whether a search has been performed */
const searched = ref(false)

/** Indicates whether a request for matching profiles is in progress */
const searching = ref(false)

/** Error messages encountered while searching for profiles */
const errors : Ref<string[]> = ref([])

/** An empty set of search criteria */
const emptyCriteria = {
  continentId: "",
  region: undefined,
  skill: undefined,
  remoteWork: ""
}

/** The search criteria being built from the page */
const criteria : Ref<PublicSearch> = ref(emptyCriteria)

/** The search results */
const results : Ref<PublicSearchResult[]> = ref([])

/** Whether the search results are collapsed */
const isCollapsed = ref(searched.value && results.value.length > 0)

/** Set up the page to match its requested state */
const setUpPage = async () => {
  if (queryValue(route, "searched") === "true") {
    searched.value = true
    try {
      searching.value = true
      const contId = queryValue(route, "continentId")
      const searchParams : PublicSearch = {
        continentId: contId === "" ? undefined : contId,
        region: queryValue(route, "region"),
        skill: queryValue(route, "skill"),
        remoteWork: queryValue(route, "remoteWork") ?? ""
      }
      const searchResult = await api.profile.publicSearch(searchParams)
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

/** Open and closed the search parameter panel */
const toggleCollapse = (it : boolean) => { isCollapsed.value = it }

/** Execute a search */
const doSearch = () => router.push({ query: { searched: "true", ...criteria.value } })
</script>
