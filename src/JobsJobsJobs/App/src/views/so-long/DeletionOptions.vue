<template lang="pug">
article
  page-title(title="Account Deletion Options")
  h3.pb-3 Account Deletion Options
  h4.pb-3 Option 1 &ndash; Delete Your Profile
  p.
    Utilizing this option will remove your current employment profile and skills. This will preserve any success stories
    you may have written, and preserves this application&rsquo;s knowledge of you. This is what you want to use if you
    want to clear out your profile and start again (and remove the current one from others&rsquo; view).
  p.text-center: button.btn.btn-danger(@click.prevent="deleteProfile") Delete Your Profile
  hr
  h4.pb-3 Option 2 &ndash; Delete Your Account
  p.
    This option will make it like you never visited this site. It will delete your profile, skills, success stories, and
    account. This is what you want to use if you want to disappear from this application. Clicking the button below
    #[strong will not] affect your No Agenda Social account in any way; its effects are limited to Jobs, Jobs, Jobs.
  p: em.
    (This will not revoke this application&rsquo;s permissions on No Agenda Social; you will have to remove this
    yourself. The confirmation message has a link where you can do this; once the page loads, find the
    #[strong Jobs, Jobs, Jobs] entry, and click the #[strong &times; Revoke] link for that entry.)
  p.text-center: button.btn.btn-danger(@click.prevent="deleteAccount") Delete Your Entire Account
</template>

<script lang="ts">
import { useRouter } from "vue-router"
import api, { LogOnSuccess } from "@/api"
import { toastError, toastSuccess } from "@/components/layout/AppToaster.vue"
import { useStore } from "@/store"
</script>

<script setup lang="ts">
const store = useStore()
const router = useRouter()

/** Delete the profile only; redirect to home page on success */
const deleteProfile = async () => {
  const resp = await api.profile.delete(store.state.user as LogOnSuccess)
  if (typeof resp === "string") {
    toastError(resp, "Deleting Profile")
  } else {
    toastSuccess("Profile Deleted Successfully")
    router.push("/citizen/dashboard")
  }
}

/** Delete everything pertaining to the user's account */
const deleteAccount = async () => {
  const resp = await api.citizen.delete(store.state.user as LogOnSuccess)
  if (typeof resp === "string") {
    toastError(resp, "Deleting Account")
  } else {
    store.commit("clearUser")
    toastSuccess("Account Deleted Successfully")
    router.push("/so-long/success")
  }
}
</script>
