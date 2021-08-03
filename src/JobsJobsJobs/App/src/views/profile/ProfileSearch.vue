<template>
  <article>
    <page-title title="Search Profiles" />
    <h3>Search Profiles</h3>

    <error-list :errors="errors">
      <p v-if="searching">Searching profiles...</p>
      <template v-else>
        <p v-if="!searched">
          Enter one or more criteria to filter results, or just click &ldquo;Search&rdquo; to list all profiles.
        </p>
        <collapse-panel headerText="Search Criteria" :collapsed="searched && results.length > 0">
          <profile-search-form v-model="criteria" @search="doSearch" />
        </collapse-panel>
        <br>
        <table v-if="results.length > 0" class="table table-sm table-hover">
          <thead>
            <tr>
              <th scope="col">Profile</th>
              <th scope="col">Name</th>
              <th scope="col" class="text-center">Seeking?</th>
              <th scope="col" class="text-center">Remote?</th>
              <th scope="col" class="text-center">Full-Time?</th>
              <th scope="col">Last Updated</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="profile in results" :key="profile.citzenId">
              <td><router-link :to="`/profile/view/${profile.citizenId}`">View</router-link></td>
              <td :class="{ 'font-weight-bold' : profile.seekingEmployment }">{{profile.displayName}}</td>
              <td class="text-center">{{yesOrNo(profile.seekingEmployment)}}</td>
              <td class="text-center">{{yesOrNo(profile.remoteWork)}}</td>
              <td class="text-center">{{yesOrNo(profile.fullTime)}}</td>
              <td><full-date :date="profile.lastUpdatedOn" /></td>
            </tr>
          </tbody>
        </table>
        <p v-else-if="searched">No results found for the specified criteria</p>
      </template>
    </error-list>
  </article>
</template>

<script lang="ts">
import { defineComponent, Ref, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { yesOrNo } from '@/App.vue'
import api, { LogOnSuccess, ProfileSearch, ProfileSearchResult } from '@/api'
import { queryValue } from '@/router'
import { useStore } from '@/store'

import CollapsePanel from '@/components/CollapsePanel.vue'
import ErrorList from '@/components/ErrorList.vue'
import FullDate from '@/components/FullDate.vue'
import ProfileSearchForm from '@/components/profile/SearchForm.vue'

export default defineComponent({
  name: 'ProfileSearch',
  components: {
    CollapsePanel,
    ErrorList,
    FullDate,
    ProfileSearchForm
  },
  setup () {
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
      continentId: undefined,
      skill: undefined,
      bioExperience: undefined,
      remoteWork: ''
    }

    /** The search criteria being built from the page */
    const criteria : Ref<ProfileSearch> = ref(emptyCriteria)

    /** The current search results */
    const results : Ref<ProfileSearchResult[]> = ref([])

    /** Set up the page to match its requested state */
    const setUpPage = async () => {
      if (queryValue(route, 'searched') === 'true') {
        searched.value = true
        try {
          searching.value = true
          const searchParams : ProfileSearch = {
            continentId: queryValue(route, 'continentId'),
            skill: queryValue(route, 'skill'),
            bioExperience: queryValue(route, 'bioExperience'),
            remoteWork: queryValue(route, 'remoteWork') || ''
          }
          const searchResult = await api.profile.search(searchParams, store.state.user as LogOnSuccess)
          if (typeof searchResult === 'string') {
            errors.value.push(searchResult)
          } else if (searchResult === undefined) {
            errors.value.push('The server returned a "Not Found" response (this should not happen)')
          } else {
            results.value = searchResult
            criteria.value = searchParams
          }
        } finally {
          searching.value = false
        }
      } else {
        searched.value = false
        criteria.value = emptyCriteria
        errors.value = []
        results.value = []
      }
    }

    watch(() => route.query, setUpPage, { immediate: true })

    return {
      errors,
      criteria,
      doSearch: () => router.push({ query: { searched: 'true', ...criteria.value } }),
      searching,
      searched,
      results,
      yesOrNo
    }
  }
})
</script>
