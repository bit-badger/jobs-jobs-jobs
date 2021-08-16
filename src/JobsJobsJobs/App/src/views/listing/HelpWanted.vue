<template>
  <article>
    <page-title title="Help Wanted" />
    <h3 class="pb-3">Help Wanted</h3>

    <p v-if="!searched">
      Enter relevant criteria to find results, or just click &ldquo;Search&rdquo; to see all current job listings.
    </p>
    <collapse-panel headerText="Search Criteria" :collapsed="isCollapsed" @toggle="toggleCollapse">
      <listing-search-form v-model="criteria" @search="doSearch" />
    </collapse-panel>
    <error-list :errors="errors">
      <p v-if="searching" class="pt-3">Searching job listings...</p>
      <template v-else>
        <table v-if="results.length > 0" class="table table-sm table-hover pt-3">
          <thead>
            <tr>
              <th scope="col">Listing</th>
              <th scope="col">Title</th>
              <th scope="col">Location</th>
              <th scope="col" class="text-center">Remote?</th>
              <th scope="col" class="text-center">Needed By</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="it in results" :key="it.listing.id">
              <td><router-link :to="`/listing/${it.listing.id}/view`">View</router-link></td>
              <td>{{it.listing.title}}</td>
              <td>{{it.continent.name}} / {{it.listing.region}}</td>
              <td class="text-center">{{yesOrNo(it.listing.remoteWork)}}</td>
              <td v-if="it.listing.neededBy" class="text-center">{{formatNeededBy(it.listing.neededBy)}}</td>
              <td v-else class="text-center">N/A</td>
            </tr>
          </tbody>
        </table>
        <p v-else-if="searched" class="pt-3">No job listings found for the specified criteria</p>
      </template>
    </error-list>

  </article>
</template>

<script lang="ts">
import { defineComponent, ref, Ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'

import { formatNeededBy } from './ListingView.vue'
import { yesOrNo } from '@/App.vue'
import api, { ListingForView, ListingSearch, LogOnSuccess } from '@/api'
import { queryValue } from '@/router'
import { useStore } from '@/store'

import CollapsePanel from '@/components/CollapsePanel.vue'
import ErrorList from '@/components/ErrorList.vue'
import ListingSearchForm from '@/components/ListingSearchForm.vue'

export default defineComponent({
  components: {
    CollapsePanel,
    ErrorList,
    ListingSearchForm
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
      continentId: '',
      region: undefined,
      remoteWork: '',
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
      if (queryValue(route, 'searched') === 'true') {
        searched.value = true
        try {
          searching.value = true
          // Hold variable for ensuring continent ID is not undefined here, but excluded from search payload
          const contId = queryValue(route, 'continentId')
          const searchParams : ListingSearch = {
            continentId: contId === '' ? undefined : contId,
            region: queryValue(route, 'region'),
            remoteWork: queryValue(route, 'remoteWork') || '',
            text: queryValue(route, 'text')
          }
          const searchResult = await api.listings.search(searchParams, store.state.user as LogOnSuccess)
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
      yesOrNo,
      formatNeededBy
    }
  }
})
</script>
