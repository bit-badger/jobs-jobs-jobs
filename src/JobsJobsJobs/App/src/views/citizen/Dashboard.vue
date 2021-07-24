<template>
  <h3>Welcome, {{user.name}}</h3>

  <template v-if="profile">
    <p>
      Your employment profile was last updated {{profile.lastUpdatedOn}}. Your profile currently lists
      {{profile.skills.length}} skill<span v-if="profile.skills.length !== 1">s</span>.
    </p>
    <p><router-link :to="'/profile/view/' + user.citizenId">View Your Employment Profile</router-link></p>
    <p v-if="profile.seekingEmployment">
      Your profile indicates that you are seeking employment. Once you find it,
      <router-link to="/success-story/add">tell your fellow citizens about it!</router-link>
    </p>
  </template>
  <template v-else>
    <p>
      You do not have an employment profile established; click &ldquo;Edit Profile&rdquo; in the menu to get
      started!
    </p>
  </template>
  <hr>
  <p>
    There <span v-if="profileCount === 1">is</span><span v-else>are</span> <span v-if="profileCount === 0">no</span><span v-else>{{profileCount}}</span>
    employment profile<span v-if="profileCount !== 1">s</span> from citizens of Gitmo Nation.
    <span v-if="profileCount > 0">Take a look around and see if you can help them find work!</span>
  </p>
  <hr>
  <p>
    To see how this application works, check out &ldquo;How It Works&rdquo; in the sidebar (last updated June
    14<sup>th</sup>, 2021).
  </p>
</template>

<script lang="ts">
import { defineComponent, onMounted, Ref, ref } from 'vue'
import api, { LogOnSuccess, Profile } from '../../api'
import { useStore } from '../../store'

export default defineComponent({
  name: 'Dashboard',
  setup () {
    const store = useStore()

    /** The currently logged-in user */
    const user = store.state.user as LogOnSuccess

    /** Error messages from data retrieval */
    const errorMessages : string[] = []

    /** The user's profile */
    const profile : Ref<Profile | undefined> = ref(undefined)

    /** A count of profiles in the system */
    const profileCount = ref(0)

    const retrieveData = async () => {
      const profileResult = await api.profile.retreive(undefined, user)
      if (typeof profileResult === 'string') {
        errorMessages.push(profileResult)
      } else if (typeof profileResult !== 'undefined') {
        profile.value = profileResult
      }
      const count = await api.profile.count(user)
      if (typeof count === 'string') {
        errorMessages.push(count)
      } else {
        profileCount.value = count
      }
    }

    onMounted(retrieveData)

    return {
      user,
      errorMessages,
      profile,
      profileCount
    }
  }
})
</script>
