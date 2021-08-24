<template lang="pug">
article
  page-title(title='People Seeking Work')
  h3.pb-3 People Seeking Work
  p(v-if='!searched').
    Enter one or more criteria to filter results, or just click &ldquo;Search&rdquo; to list all profiles.
  collapse-panel(headerText='Search Criteria' :collapsed='isCollapsed' @toggle='toggleCollapse')
    profile-public-search-form(v-model='criteria' @search='doSearch')
  error-list(:errors='errors')
    p(v-if='searching') Searching profiles&hellip;
    template(v-else)
      template(v-if='results.length > 0')
        p.pb-3.pt-3.
          These profiles match your search criteria. To learn more about these people, join the merry band of human
          resources in the #[a(href='https://noagendashow.net' target='_blank') No Agenda] tribe!
        table.table.table-sm.table-hover
          thead: tr
            th(scope='col') Continent
            th.text-center(scope='col') Region
            th.text-center(scope='col') Remote?
            th.text-center(scope='col') Skills
          tbody: tr(v-for='(profile, idx) in results' :key='idx')
            td {{profile.continent}}
            td {{profile.region}}
            td.text-center {{yesOrNo(profile.remoteWork)}}
            td: template(v-for='(skill, idx) in profile.skills' :key='idx') {{skill}}#[br]
      p.pt-3(v-else-if='searched') No results found for the specified criteria
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
      continentId: '',
      region: undefined,
      skill: undefined,
      remoteWork: ''
    }

    /** The search criteria being built from the page */
    const criteria : Ref<PublicSearch> = ref(emptyCriteria)

    /** The search results */
    const results : Ref<PublicSearchResult[]> = ref([])

    /** Whether the search results are collapsed */
    const isCollapsed = ref(searched.value && results.value.length > 0)

    /** Set up the page to match its requested state */
    const setUpPage = async () => {
      if (queryValue(route, 'searched') === 'true') {
        searched.value = true
        try {
          searching.value = true
          const contId = queryValue(route, 'continentId')
          const searchParams : PublicSearch = {
            continentId: contId === '' ? undefined : contId,
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
            searchParams.continentId = searchParams.continentId || ''
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

    watch(() => route.query, setUpPage, { immediate: true })

    return {
      errors,
      criteria,
      isCollapsed,
      toggleCollapse: (it : boolean) => { isCollapsed.value = it },
      doSearch: () => router.push({ query: { searched: 'true', ...criteria.value } }),
      searching,
      searched,
      results,
      yesOrNo
    }
  }
})
</script>
