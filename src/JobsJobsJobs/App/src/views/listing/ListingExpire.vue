<template lang="pug">
article
  page-title(title="Expire Job Listing")
  load-data(:load="retrieveListing")
    h3.pb-3 Expire Job Listing ({{listing.title}})
    p: em.
      Expiring this listing will remove it from search results. You will be able to see it via your &ldquo;My Job
      Listings&rdquo; page, but you will not be able to &ldquo;un-expire&rdquo; it.
    form.row.g-3
      .col-12: .form-check
        input.form-check-input(type="checkbox" id="fromHere" v-model="v$.fromHere.$model")
        label.form-check-label(for="fromHere") This job was filled due to its listing here
      template(v-if="expiration.fromHere")
        .col-12: p.
          Consider telling your fellow citizens about your experience! Comments entered here will be visible to
          logged-on users here, but not to the general public.
        markdown-editor(id="successStory" label="Your Success Story" v-model:text="v$.successStory.$model")
      .col-12
        button.btn.btn-primary(@click.prevent="expireListing").
          #[icon(icon="text-box-remove-outline")]&nbsp; Expire Listing
  maybe-save(:saveAction="doSave" :validator="v$")
</template>

<script setup lang="ts">
import { computed, reactive, Ref, ref } from "vue"
import { useRoute, useRouter } from "vue-router"
import useVuelidate from "@vuelidate/core"

import api, { Listing, ListingExpireForm, LogOnSuccess } from "@/api"
import { toastError, toastSuccess } from "@/components/layout/AppToaster.vue"
import { useStore } from "@/store"

import LoadData from "@/components/LoadData.vue"
import MarkdownEditor from "@/components/MarkdownEditor.vue"
import MaybeSave from "@/components/MaybeSave.vue"

const store = useStore()
const route = useRoute()
const router = useRouter()

/** The currently logged-on user */
const user = store.state.user as LogOnSuccess

/** The ID of the listing being expired */
const listingId = route.params.id as string

/** The listing being expired */
const listing : Ref<Listing | undefined> = ref(undefined)

/** The data needed to expire a job listing */
const expiration = reactive(new ListingExpireForm())
expiration.successStory = ""

/** The validation rules for the form */
const rules = computed(() => ({
  fromHere: { },
  successStory: { }
}))

/** Initialize form validation */
const v$ = useVuelidate(rules, expiration, { $lazy: true })

/** Retrieve the job listing being expired */
const retrieveListing = async (errors : string[]) => {
  const listingResp = await api.listings.retreive(listingId, user)
  if (typeof listingResp === "string") {
    errors.push(listingResp)
  } else if (typeof listingResp === "undefined") {
    errors.push("Listing not found")
  } else {
    listing.value = listingResp
  }
}

/** Expire the listing */
const expireListing = async (navigate : boolean) => {
  v$.value.$touch()
  if (v$.value.$error) return
  if ((expiration.successStory ?? "").trim() === "") expiration.successStory = undefined
  const expireResult = await api.listings.expire(listingId, expiration, user)
  if (typeof expireResult === "string") {
    toastError(expireResult, "expiring job listing")
  } else {
    toastSuccess(`Job Listing Expired${expiration.successStory ? " and Success Story Recorded" : ""} Successfully`)
    v$.value.$reset()
    if (navigate) {
      router.push("/listings/mine")
    }
  }
}

/** No-parameter save function (used for save-on-navigate) */
const doSave = async () => await expireListing(false)
</script>
