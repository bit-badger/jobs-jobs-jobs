<template>
  <article>
    <page-title title="Account Deletion Options" />
    <h3>Account Deletion Options</h3>

    <p v-if="error !== ''">{{error}}</p>

    <h4>Option 1 &ndash; Delete Your Profile</h4>
    <p>
      Utilizing this option will remove your current employment profile and skills. This will preserve any success
      stories you may have written, and preserves this application&rsquo;s knowledge of you. This is what you want to
      use if you want to clear out your profile and start again (and remove the current one from others&rsquo; view).
    </p>
    <p class="text-center">
      <v-btn color="error" @click="deleteProfile">Delete Your Profile</v-btn>
    </p>

    <hr>

    <h4>Option 2 &ndash; Delete Your Account</h4>
    <p>
      This option will make it like you never visited this site. It will delete your profile, skills, success stories,
      and account. This is what you want to use if you want to disappear from this application. Clicking the button
      below <strong>will not</strong> affect your No Agenda Social account in any way; its effects are limited to Jobs,
      Jobs, Jobs.
    </p>
    <p>
      <em>
        (This will not revoke this application&rsquo;s permissions on No Agenda Social; you will have to remove this
        yourself. The confirmation message has a link where you can do this; once the page loads, find the
        <strong>Jobs, Jobs, Jobs</strong> entry, and click the <strong>&times; Revoke</strong> link for that entry.)
      </em>
    </p>
    <p class="text-center">
      <v-btn color="error" @click="deleteAccount">Delete Your Entire Account</v-btn>
    </p>
  </article>
</template>

<script lang="ts">
import { defineComponent, ref } from 'vue'
import { useRouter } from 'vue-router'
import api, { LogOnSuccess } from '@/api'
import { useStore } from '@/store'

export default defineComponent({
  name: 'DeletionOptions',
  setup () {
    const store = useStore()
    const router = useRouter()

    /** Error message encountered during actions */
    const error = ref('')

    /** Delete the profile only; redirect to home page on success */
    const deleteProfile = async () => {
      const resp = await api.profile.delete(store.state.user as LogOnSuccess)
      if (typeof resp === 'string') {
        error.value = resp
      } else {
        // TODO: notify
        router.push('/citizen/dashboard')
      }
    }

    /** Delete everything pertaining to the user's account */
    const deleteAccount = async () => {
      const resp = await api.citizen.delete(store.state.user as LogOnSuccess)
      if (typeof resp === 'string') {
        error.value = resp
      } else {
        store.commit('clearUser')
        // TODO: notify
        router.push('/so-long/success')
      }
    }

    return {
      error,
      deleteProfile,
      deleteAccount
    }
  }
})
</script>
