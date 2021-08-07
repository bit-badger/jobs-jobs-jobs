<template>
  <article class="container">
    <page-title title="Dashboard" />
    <h3 class="pb-4">Welcome, {{user.name}}</h3>
    <load-data :load="retrieveData">
      <div class="row row-cols-1 row-cols-md-2">
        <div class="col">
          <div class="card h-100">
            <h5 class="card-header">Your Profile</h5>
            <div class="card-body">
              <h6 class="card-subtitle mb-3 text-muted fst-italic">
                Last updated <full-date :date="profile.lastUpdatedOn" />
              </h6>
              <p v-if="profile" class="card-text">
                Your profile currently lists {{profile.skills.length}}
                skill<template v-if="profile.skills.length !== 1">s</template>.
                <span v-if="profile.seekingEmployment">
                  <br><br>
                  Your profile indicates that you are seeking employment. Once you find it,
                  <router-link to="/success-story/add">tell your fellow citizens about it!</router-link>
                </span>
              </p>
              <p v-else class="card-text">
                You do not have an employment profile established; click below (or &ldquo;Edit Profile&rdquo; in the
                menu) to get started!
              </p>
            </div>
            <div class="card-footer">
              <template v-if="profile">
                <button class="btn btn-outline-secondary" @click="viewProfile">View Profile</button> &nbsp; &nbsp;
                <button class="btn btn-outline-secondary" @click="editProfile">Edit Profile</button>
              </template>
              <button v-else class="btn btn-primary" @click="editProfile">Create Profile</button>
            </div>
          </div>
        </div>
        <div class="col">
          <div class="card h-100">
            <h5 class="card-header">Other Citizens</h5>
            <div class="card-body">
              <h6 class="card-subtitle mb-3 text-muted fst-italic">
                <template v-if="profileCount === 0">No</template><template v-else>{{profileCount}} Total</template>
                Employment Profile<template v-if="profileCount !== 1">s</template>
              </h6>
              <p v-if="profileCount === 1 && profile" class="card-text">
                It looks like, for now, it&rsquo;s just you&hellip;
              </p>
              <p v-else-if="profileCount > 0" class="card-text">
                Take a look around and see if you can help them find work!
              </p>
              <p v-else class="card-text">
                You can click below, but you will not find anything&hellip;
              </p>
            </div>
            <div class="card-footer">
              <button class="btn btn-outline-secondary" @click="searchProfiles">Search Profiles</button>
            </div>
          </div>
        </div>
      </div>
    </load-data>
    <p>&nbsp;</p>
    <p>
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

import FullDate from '@/components/FullDate.vue'
import LoadData from '@/components/LoadData.vue'

export default defineComponent({
  name: 'Dashboard',
  components: {
    FullDate,
    LoadData
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
