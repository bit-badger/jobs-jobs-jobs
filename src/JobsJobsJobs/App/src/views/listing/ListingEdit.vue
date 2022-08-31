<template>
  <article>
    <h3 class="pb-3" v-if="isNew">Add a Job Listing</h3>
    <h3 class="pb-3" v-else>Edit Job Listing</h3>
    <load-data :load="retrieveData">
      <form class="row g-3">
        <div class="col-12 col-sm-10 col-md-8 col-lg-6">
          <div class="form-floating">
            <input type="text" id="title" :class="{ 'form-control': true, 'is-invalid': v$.title.$error }"
                   maxlength="255" v-model="v$.title.$model" placeholder="The title for the job listing">
            <div class="invalid-feedback">Please enter a title for the job listing</div>
            <label class="jjj-required" for="title">Title</label>
          </div>
          <div class="form-text">
            No need to put location here; it will always be show to seekers with continent and region
          </div>
        </div>
        <div class="col-12 col-sm-6 col-md-4">
          <continent-list v-model="v$.continentId.$model" :isInvalid="v$.continentId.$error"
                          @touch="v$.continentId.$touch() || true" />
        </div>
        <div class="col-12 col-sm-6 col-md-8">
          <div class="form-floating">
            <input type="text" id="region" :class="{ 'form-control': true, 'is-invalid': v$.region.$error }"
                   maxlength="255" v-model="v$.region.$model" placeholder="Country, state, geographic area, etc.">
            <div class="invalid-feedback">Please enter a region</div>
            <label class="jjj-required" for="region">Region</label>
          </div>
          <div class="form-text">Country, state, geographic area, etc.</div>
        </div>
        <div class="col-12">
          <div class="form-check">
            <input type="checkbox" id="isRemote" class="form-check-input" v-model="v$.remoteWork.$model">
            <label class="form-check-label" for="isRemote">This opportunity is for remote work</label>
          </div>
        </div>
        <markdown-editor id="description" label="Job Description" v-model:text="v$.text.$model"
                         :isInvalid="v$.text.$error" />
        <div class="col-12 col-md-4">
          <div class="form-floating">
            <input type="date" id="neededBy" class="form-control" v-model="v$.neededBy.$model"
                   placeholder="Date by which this position needs to be filled">
            <label for="neededBy">Needed By</label>
          </div>
        </div>
        <div class="col-12">
          <p v-if="v$.$error" class="text-danger">Please correct the errors above</p>
          <button class="btn btn-primary" @click.prevent="saveListing(true)">
            <icon :icon="mdiContentSaveOutline" />&nbsp; Save
          </button>
        </div>
      </form>
    </load-data>
    <maybe-save :saveAction="doSave" :validator="v$" />
  </article>
</template>

<script setup lang="ts">
import { computed, reactive } from "vue"
import { useRoute, useRouter } from "vue-router"
import { mdiContentSaveOutline } from "@mdi/js"
import useVuelidate from "@vuelidate/core"
import { required } from "@vuelidate/validators"

import api, { Listing, ListingForm, LogOnSuccess } from "@/api"
import { toastError, toastSuccess } from "@/components/layout/AppToaster.vue"
import { Mutations, useStore } from "@/store"

import ContinentList from "@/components/ContinentList.vue"
import LoadData from "@/components/LoadData.vue"
import MarkdownEditor from "@/components/MarkdownEditor.vue"
import MaybeSave from "@/components/MaybeSave.vue"

const store = useStore()
const route = useRoute()
const router = useRouter()

/** The currently logged-on user */
const user = store.state.user as LogOnSuccess

/** A new job listing */
const newListing : Listing = {
  id: "",
  citizenId: user.citizenId,
  createdOn: "",
  title: "",
  continentId: "",
  region: "",
  remoteWork: false,
  isExpired: false,
  updatedOn: "",
  text: "",
  neededBy: undefined,
  wasFilledHere: undefined
}

/** The backing object for the form */
const listing = reactive(new ListingForm())

/** The ID of the listing requested */
const id = route.params.id as string

/** Is this a new job listing? */
const isNew = computed(() => id === "new")

/** Validation rules for the form */
const rules = computed(() => ({
  id: { },
  title: { required },
  continentId: { required },
  region: { required },
  remoteWork: { },
  text: { required },
  neededBy: { }
}))

/** Initialize form validation */
const v$ = useVuelidate(rules, listing, { $lazy: true })

/** Retrieve the listing being edited (or set up the form for a new listing) */
const retrieveData = async (errors : string[]) => {
  if (isNew.value) store.commit(Mutations.SetTitle, "Add a Job Listing")
  const listResult = isNew.value ? newListing : await api.listings.retreive(id, user)
  if (typeof listResult === "string") {
    errors.push(listResult)
  } else if (typeof listResult === "undefined") {
    errors.push("Job listing not found")
  } else {
    listing.id = listResult.id
    listing.title = listResult.title
    listing.continentId = listResult.continentId
    listing.region = listResult.region
    listing.remoteWork = listResult.remoteWork
    listing.text = listResult.text
    listing.neededBy = listResult.neededBy
  }
}

/** Save the job listing */
const saveListing = async (navigate : boolean) => {
  v$.value.$touch()
  if (v$.value.$error) return
  const apiFunc = isNew.value ? api.listings.add : api.listings.update
  if (listing.neededBy === "") listing.neededBy = undefined
  const result = await apiFunc(listing, user)
  if (typeof result === "string") {
    toastError(result, "saving job listing")
  } else {
    toastSuccess(`Job Listing ${isNew.value ? "Add" : "Updat"}ed Successfully`)
    v$.value.$reset()
    if (navigate) await router.push("/listings/mine")
  }
}

/** Parameterless save function (used to save when navigating away) */
const doSave = async () => await saveListing(false)
</script>
