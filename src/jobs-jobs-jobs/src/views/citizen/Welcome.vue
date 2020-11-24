<template>
  <div>
    <p>Welcome!</p>
    <p>Profile Established: <strong><span v-if="profile?.value">Yes</span><span v-else>No</span></strong></p>
  </div>
</template>

<script lang="ts">
import { onBeforeMount, ref } from 'vue'

import { userProfile } from '@/api'
import { Profile } from '@/api/types'

export default {
  setup() {
    const profile = ref<Profile | undefined>(undefined)

    onBeforeMount(async () => {
      profile.value = await userProfile()
    })

    return {
      profile
    }
  }
}
</script>