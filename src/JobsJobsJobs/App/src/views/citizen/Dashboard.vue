<template>
  <article>
    <page-title title="Dashboard" />
    <h3>Welcome, {{user.name}}</h3>
    <load-data :load="retrieveData">
      <v-row class="spaced">
        <v-col cols="12" md="6">
          <v-card elevation="6">
            <v-card-header>
              <v-card-header-text>
                <v-card-title>Your Profile</v-card-title>
                <v-card-subtitle>Last updated <full-date-time :date="profile.lastUpdatedOn" /></v-card-subtitle>
              </v-card-header-text>
            </v-card-header>
            <v-card-text>
              <div v-if="profile">
                Your profile currently lists {{profile.skills.length}}
                skill<template v-if="profile.skills.length !== 1">s</template>.
                <span v-if="profile.seekingEmployment">
                  <br><br>
                  Your profile indicates that you are seeking employment. Once you find it,
                  <router-link to="/success-story/add">tell your fellow citizens about it!</router-link>
                </span>
              </div>
              <div v-else>
                You do not have an employment profile established; click below (or &ldquo;Edit Profile&rdquo; in the
                menu) to get started!
              </div>
            </v-card-text>
            <v-card-actions>
              <template v-if="profile">
                <v-btn v-if="profile" @click="viewProfile">View Profile</v-btn>
                <v-btn @click="editProfile">Edit Profile</v-btn>
              </template>
              <v-btn v-else @click="editProfile">Create Profile</v-btn>
            </v-card-actions>
          </v-card>
        </v-col>
        <v-col cols="12" md="6">
          <v-card elevation="6">
            <v-card-header>
              <v-card-header-text>
                <v-card-title>Other Citizens</v-card-title>
                <v-card-subtitle>
                  <template v-if="profileCount === 0">No</template><template v-else>{{profileCount}} Total</template>
                  Employment Profile<template v-if="profileCount !== 1">s</template>
                </v-card-subtitle>
              </v-card-header-text>
            </v-card-header>
            <v-card-text>
              <div v-if="profileCount === 1 && profile">
                It looks like, for now, it&rsquo;s just you&hellip;
              </div>
              <div v-else-if="profileCount > 0">
                Take a look around and see if you can help them find work!
              </div>
              <div v-else>
                You can click below, but you will not find anything&hellip;
              </div>
            </v-card-text>
            <v-card-actions>
              <v-btn @click="searchProfiles">Search Profiles</v-btn>
            </v-card-actions>
          </v-card>
        </v-col>
      </v-row>
    </load-data>
    <p class="spaced">
      To see how this application works, check out &ldquo;How It Works&rdquo; in the sidebar (last updated June
      14<sup>th</sup>, 2021).
    </p>
  </article>
</template>

<script lang="ts">
import { defineComponent, Ref, ref } from 'vue'
import { useRouter } from 'vue-router'
import api, { LogOnSuccess, Profile } from '@/api'
import { useStore } from '@/store'
import FullDateTime from '@/components/FullDateTime.vue'
import LoadData from '@/components/LoadData.vue'

export default defineComponent({
  name: 'Dashboard',
  components: {
    LoadData,
    FullDateTime
  },
  setup () {
    const store = useStore()
    const router = useRouter()

    /** The currently logged-in user */
    const user = store.state.user as LogOnSuccess

    /** The user's profile */
    const profile : Ref<Profile | undefined> = ref(undefined)

    /** A count of profiles in the system */
    const profileCount = ref(0)

    const retrieveData = async (errors : string[]) => {
      const profileResult = await api.profile.retreive(undefined, user)
      if (typeof profileResult === 'string') {
        errors.push(profileResult)
      } else if (typeof profileResult !== 'undefined') {
        profile.value = profileResult
      }
      const count = await api.profile.count(user)
      if (typeof count === 'string') {
        errors.push(count)
      } else {
        profileCount.value = count
      }
    }

    return {
      retrieveData,
      user,
      profile,
      profileCount,
      viewProfile: () => router.push(`/profile/view/${user.citizenId}`),
      editProfile: () => router.push('/citizen/profile'),
      searchProfiles: () => router.push('/profile/search')
    }
  }
})
</script>

<style lang="sass" scoped>
.spaced
  margin-top: 1rem
</style>
