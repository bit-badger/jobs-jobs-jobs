<template>
  <article>
    <page-title title="My Job Listings" />
    <h3 class="pb-3">My Job Listings</h3>
    <p>
      <router-link class="btn btn-primary-outline" to="/listing/new/edit">Add a New Job Listing</router-link>
    </p>
    <load-data :load="getListings">
      <table v-if="listings.length > 0">
        <thead>
          <tr>
            <th>Action</th>
            <th>Title</th>
            <th>Created</th>
            <th>Updated</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="listing in listings" :key="listing.id">
            <td><router-link :to="`/listing/${listing.Id}/edit`">Edit</router-link></td>
            <td>{{listing.Title}}</td>
            <td><full-date-time :date="listing.createdOn" /></td>
            <td><full-date-time :date="listing.updatedOn" /></td>
          </tr>
        </tbody>
      </table>
      <p v-else class="fst-italic">No job listings found</p>
    </load-data>
  </article>
</template>

<script lang="ts">
import { defineComponent, Ref, ref } from 'vue'
import api, { Listing, LogOnSuccess } from '@/api'
import { useStore } from '@/store'

import FullDateTime from '@/components/FullDateTime.vue'
import LoadData from '@/components/LoadData.vue'

export default defineComponent({
  name: 'MyListings',
  components: {
    FullDateTime,
    LoadData
  },
  setup () {
    const store = useStore()

    /** The listings for the user */
    const listings : Ref<Listing[]> = ref([])

    /** Retrieve the job listing posted by the current citizen */
    const getListings = async (errors : string[]) => {
      const listResult = await api.listings.mine(store.state.user as LogOnSuccess)
      if (typeof listResult === 'string') {
        errors.push(listResult)
      } else if (typeof listResult === 'undefined') {
        errors.push('API call returned 404 (this should not happen)')
      } else {
        listings.value = listResult
      }
    }

    return {
      getListings,
      listings
    }
  }
})
</script>
