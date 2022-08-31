<template>
  <article>
    <h3 class="pb-3">My Job Listings</h3>
    <p><router-link class="btn btn-outline-primary" to="/listing/new/edit">Add a New Job Listing</router-link></p>
    <load-data :load="getListings">
      <h4 v-if="expired.length > 0" class="pb-2">Active Job Listings</h4>
      <table v-if="active.length > 0" class="pb-3 table table-sm table-hover pt-3">
        <thead>
          <tr>
            <th scope="col">Action</th>
            <th scope="col">Title</th>
            <th scope="col">Continent / Region</th>
            <th scope="col">Created</th>
            <th scope="col">Updated</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="it in active" :key="it.listing.id">
            <td>
              <router-link :to="`/listing/${it.listing.id}/edit`">Edit</router-link>&nbsp;~
              <router-link :to="`/listing/${it.listing.id}/view`">View</router-link>&nbsp;~
              <router-link :to="`/listing/${it.listing.id}/expire`">Expire</router-link>
            </td>
            <td>{{it.listing.title}}</td>
            <td>{{it.continent.name}} / {{it.listing.region}}</td>
            <td><full-date-time :date="it.listing.createdOn" /></td>
            <td><full-date-time :date="it.listing.updatedOn" /></td>
          </tr>
        </tbody>
      </table>
      <p v-else class="pb-3 fst-italic">You have no active job listings</p>
      <template v-if="expired.length > 0">
        <h4 class="pb-2">Expired Job Listings</h4>
        <table class="table table-sm table-hover pt-3">
          <thead>
            <tr>
              <th scope="col">Action</th>
              <th scope="col">Title</th>
              <th scope="col">Filled Here?</th>
              <th scope="col">Expired</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="it in expired" :key="it.listing.id">
              <td><router-link :to="`/listing/${it.listing.id}/view`">View</router-link></td>
              <td>{{it.listing.title}}</td>
              <td>{{yesOrNo(it.listing.wasFilledHere)}}</td>
              <td><full-date-time :date="it.listing.updatedOn" /></td>
            </tr>
          </tbody>
        </table>
      </template>
    </load-data>
  </article>
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
