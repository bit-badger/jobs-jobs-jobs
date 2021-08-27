<template lang="pug">
article.container
  page-title(title="Dashboard")
  h3.pb-4 Welcome, {{user.name}}
  load-data(:load="retrieveData"): .row.row-cols-1.row-cols-md-2
    .col: .card.h-100
      h5.card-header Your Profile
      .card-body
        h6.card-subtitle.mb-3.text-muted.fst-italic Last updated #[full-date-time(:date="profile.lastUpdatedOn")]
        p.card-text(v-if="profile")
          | Your profile currently lists {{profile.skills.length}}
          | skill#[template(v-if="profile.skills.length !== 1") s].
          span(v-if="profile.seekingEmployment")
            br
            br
            | Your profile indicates that you are seeking employment. Once you find it,
            router-link(to="/success-story/add") tell your fellow citizens about it!
        p.card-text(v-else).
          You do not have an employment profile established; click below (or &ldquo;Edit Profile&rdquo; in the menu) to
          get started!
      .card-footer
        template(v-if="profile")
          router-link.btn.btn-outline-secondary(:to="`/profile/${user.citizenId}/view`") View Profile
          | &nbsp; &nbsp;
          router-link.btn.btn-outline-secondary(to="/citizen/profile") Edit Profile
        router-link.btn.btn-primary(v-else to="/citizen/profile") Create Profile
    .col: .card.h-100
      h5.card-header Other Citizens
      .card-body
        h6.card-subtitle.mb-3.text-muted.fst-italic
          template(v-if="profileCount === 0") No
          template(v-else) {{profileCount}} Total
          |  Employment Profile#[template(v-if="profileCount !== 1") s]
        p.card-text(v-if="profileCount === 1 && profile") It looks like, for now, it&rsquo;s just you&hellip;
        p.card-text(v-else-if="profileCount > 0") Take a look around and see if you can help them find work!
        p.card-text(v-else) You can click below, but you will not find anything&hellip;
      .card-footer: router-link.btn.btn-outline-secondary(to="/profile/search") Search Profiles
  p &nbsp;
  p.
    To see how this application works, check out &ldquo;How It Works&rdquo; in the sidebar (last updated June
    14#[sup th], 2021).
</template>

<script setup lang="ts">
import { Ref, ref } from "vue"
import api, { LogOnSuccess, Profile } from "@/api"
import { useStore } from "@/store"

import FullDateTime from "@/components/FullDateTime.vue"
import LoadData from "@/components/LoadData.vue"

const store = useStore()

/** The currently logged-in user */
const user = store.state.user as LogOnSuccess

/** The user's profile */
const profile : Ref<Profile | undefined> = ref(undefined)

/** A count of profiles in the system */
const profileCount = ref(0)

const retrieveData = async (errors : string[]) => {
  const profileResult = await api.profile.retreive(undefined, user)
  if (typeof profileResult === "string") {
    errors.push(profileResult)
  } else if (typeof profileResult !== "undefined") {
    profile.value = profileResult
  }
  const count = await api.profile.count(user)
  if (typeof count === "string") {
    errors.push(count)
  } else {
    profileCount.value = count
  }
}
</script>
