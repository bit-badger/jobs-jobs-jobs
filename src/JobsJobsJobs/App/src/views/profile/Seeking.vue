<template>
  <article>
    <page-title title="People Seeking Work" />
    <h3>People Seeking Work</h3>

    <error-list :errors="errors">
      <p v-if="searching">Searching profiles...</p>
      <template v-else>
        <p v-if="!searched">
          Enter one or more criteria to filter results, or just click &ldquo;Search&rdquo; to list all profiles.
        </p>
        <collapse-panel headerText="Search Criteria" :collapsed="searched && results.length > 0">
          <profile-public-search-form v-model="criteria" @search="doSearch" />
        </collapse-panel>
        <br>
        <template v-if="results.length > 0">
          <p>
            These profiles match your search criteria. To learn more about these people, join the merry band of human
            resources in the <a href="https://noagendashow.net" target="_blank">No Agenda</a> tribe!
          </p>
          <table class="table table-sm table-hover">
            <thead>
              <tr>
                <th scope="col">Continent</th>
                <th scope="col" class="text-center">Region</th>
                <th scope="col" class="text-center">Remote?</th>
                <th scope="col" class="text-center">Skills</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(profile, idx) in results" :key="idx">
                <td>{{profile.continent}}</td>
                <td>{{profile.region}}</td>
                <td class="text-center">{{yesOrNo(profile.remoteWork)}}</td>
                <td>
                  <template v-for="(skill, idx) in profile.skills" :key="idx">{{skill}}<br></template>
                </td>
              </tr>
            </tbody>
          </table>
        </template>
        <template v-else>
          <p v-if="searched">No results found for the specified criteria</p>
        </template>
      </template>
    </error-list>
  </article>
</template>

<script lang="ts">
import { defineComponent, Ref, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { yesOrNo } from '@/App.vue'
import api, { PublicSearch, PublicSearchResult } from '@/api'
import { queryValue } from '@/router'

import CollapsePanel from '@/components/CollapsePanel.vue'
import ErrorList from '@/components/ErrorList.vue'
import ProfilePublicSearchForm from '@/components/profile/PublicSearchForm.vue'

export default defineComponent({
  components: {
    CollapsePanel,
    ErrorList,
    ProfilePublicSearchForm
  },
  setup () {
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
      continentId: undefined,
      region: undefined,
      skill: undefined,
      remoteWork: ''
    }

    /** The search criteria being built from the page */
    const criteria : Ref<PublicSearch> = ref(emptyCriteria)

    /** The search results */
    const results : Ref<PublicSearchResult[]> = ref([])

    /** Set up the page to match its requested state */
    const setUpPage = async () => {
      if (queryValue(route, 'searched') === 'true') {
        searched.value = true
        try {
          searching.value = true
          const searchParams : PublicSearch = {
            continentId: queryValue(route, 'continentId'),
            region: queryValue(route, 'region'),
            skill: queryValue(route, 'skill'),
            remoteWork: queryValue(route, 'remoteWork') || ''
          }
          const searchResult = await api.profile.publicSearch(searchParams)
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
