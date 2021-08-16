<template>
  <article>
    <page-title :title="pageTitle" />
    <load-data :load="retrieveListing">
      <h3>{{it.listing.title}}</h3>
      <h4 class="pb-3 text-muted">{{it.continent.name}} / {{it.listing.region}}</h4>
      <p>
        <template v-if="it.listing.neededBy">
          <strong><em>NEEDED BY {{formatNeededBy(it.listing.neededBy)}}</em></strong> &bull;&nbsp;
        </template>
        Listed by {{citizenName(citizen)}} &ndash;
        <a :href="`https://noagendasocial.com/@${citizen.naUser}`" target="_blank">View No Agenda Social profile</a>
      </p>
      <hr>
      <div v-html="details"></div>
    </load-data>
  </article>
</template>

<script lang="ts">
import { computed, defineComponent, ref, Ref } from 'vue'
import { useRoute } from 'vue-router'
import { format } from 'date-fns'
import marked from 'marked'

import api, { Citizen, ListingForView, LogOnSuccess, markedOptions } from '@/api'
import { citizenName } from '@/App.vue'
import { useStore } from '@/store'
import LoadData from '@/components/LoadData.vue'

/**
 * Format the needed by date for display
 *
 * @param neededBy The defined needed by date
 * @returns The date to display
 */
export function formatNeededBy (neededBy : string) : string {
  return format(Date.parse(neededBy), 'PPP')
}

export default defineComponent({
  name: 'ListingView',
  components: { LoadData },
  setup () {
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
      if (typeof listingResp === 'string') {
        errors.push(listingResp)
      } else if (typeof listingResp === 'undefined') {
        errors.push('Job Listing not found')
      } else {
        it.value = listingResp
        const citizenResp = await api.citizen.retrieve(listingResp.listing.citizenId, user)
        if (typeof citizenResp === 'string') {
          errors.push(citizenResp)
        } else if (typeof citizenResp === 'undefined') {
          errors.push('Listing Citizen not found (this should not happen)')
        } else {
          citizen.value = citizenResp
        }
      }
    }

    return {
      pageTitle: computed(() => it.value ? `${it.value.listing.title} | Job Listing` : 'Loading Job Listing...'),
      retrieveListing,
      it,
      details: computed(() => marked(it.value?.listing.text || '', markedOptions)),
      citizen,
      citizenName,
      formatNeededBy
    }
  }
})
</script>
