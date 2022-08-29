<template>
  <article>
    <h3 class="pb-3">Account Deletion Options</h3>
    <h4 class="pb-3">Option 1 &ndash; Delete Your Profile</h4>
    <p>
      Utilizing this option will remove your current employment profile and skills. This will preserve any job listings
      you may have posted, or any success stories you may have written, and preserves this application&rsquo;s knowledge
      of you. This is what you want to use if you want to clear out your profile and start again (and remove the current
      one from others&rsquo; view).
    </p>
    <p class="text-center">
      <button class="btn btn-danger" @click.prevent="deleteProfile">Delete Your Profile</button>
    </p>
    <hr>
    <h4 class="pb-3">Option 2 &ndash; Delete Your Account</h4>
    <p>
      This option will make it like you never visited this site. It will delete your profile, skills, job listings,
      success stories, and account. This is what you want to use if you want to disappear from this application.
    </p>
    <p class="text-center">
      <button class="btn btn-danger" @click.prevent="deleteAccount">Delete Your Entire Account</button>
    </p>
  </article>
</template>

<script setup lang="ts">
import { useRouter } from "vue-router"

import api, { LogOnSuccess } from "@/api"
import { toastError, toastSuccess } from "@/components/layout/AppToaster.vue"
import { useStore, Mutations } from "@/store"

const store = useStore()
const router = useRouter()

/** The currently logged-on user */
const user = store.state.user as LogOnSuccess

/** Delete the profile only; redirect to home page on success */
const deleteProfile = async () => {
  const resp = await api.profile.delete(user)
  if (typeof resp === "string") {
    toastError(resp, "Deleting Profile")
  } else {
    toastSuccess("Profile Deleted Successfully")
    await router.push("/citizen/dashboard")
  }
}

/** Delete everything pertaining to the user's account */
const deleteAccount = async () => {
  const resp = await api.citizen.delete(user)
  if (typeof resp === "string") {
    toastError(resp, "Deleting Account")
  } else {
    store.commit(Mutations.ClearUser)
    toastSuccess("Account Deleted Successfully")
    await router.push("/so-long/success")
  }
}
</script>
