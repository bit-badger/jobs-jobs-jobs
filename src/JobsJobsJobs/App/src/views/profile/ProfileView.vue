<template>
  <article>
    <page-title :title="pageTitle" />
    <load-data :load="retrieveProfile">
      <h2><a :href="it.citizen.profileUrl" target="_blank">{{it.citizen.citizenName()}}</a></h2>
      <h4 class="pb-3">{{it.continent.name}}, {{it.profile.region}}</h4>
      <p v-html="workTypes"></p>
      <hr>
      <div v-html="bioHtml"></div>

      <template v-if="it.profile.skills.length > 0">
        <hr>
        <h4 class="pb-3">Skills</h4>
        <ul>
          <li v-for="(skill, idx) in it.profile.skills" :key="idx">
            {{skill.description}}<template v-if="skill.notes"> ({{skill.notes}})</template>
          </li>
        </ul>
      </template>

      <template v-if="it.profile.experience">
        <hr>
        <h4 class="pb-3">Experience / Employment History</h4>
        <div v-html="expHtml"></div>
      </template>

      <template v-if="user.citizenId === it.citizen.id">
        <br><br>
        <router-link class="btn btn-primary" to="/citizen/profile">
          <icon icon="pencil" />&nbsp; Edit Your Profile
        </router-link>
      </template>
    </load-data>
  </article>
</template>

<script lang="ts">
import { computed, defineComponent, ref, Ref } from 'vue'
import { useRoute } from 'vue-router'
import marked from 'marked'

import api, { LogOnSuccess, markedOptions, ProfileForView } from '@/api'
import { citizenName } from '@/App.vue'
import { useStore } from '@/store'
import LoadData from '@/components/LoadData.vue'

export default defineComponent({
  name: 'ProfileView',
  components: { LoadData },
  setup () {
    const store = useStore()
    const route = useRoute()

    /** The currently logged-on user */
    const user = store.state.user as LogOnSuccess

    /** The requested profile */
    const it : Ref<ProfileForView | undefined> = ref(undefined)

    /** The work types for the top of the page */
    const workTypes = computed(() => {
      const parts : string[] = []
      if (it.value) {
        const p = it.value.profile
        if (p.seekingEmployment) {
          parts.push('<strong><em>CURRENTLY SEEKING EMPLOYMENT</em></strong>')
        } else {
          parts.push('Not actively seeking employment')
        }
        parts.push(`${p.fullTime ? 'I' : 'Not i'}nterested in full-time employment`)
        parts.push(`${p.remoteWork ? 'I' : 'Not i'}nterested in remote opportunities`)
      }
      return parts.join(' &bull; ')
    })

    /** Retrieve the profile and supporting data */
    const retrieveProfile = async (errors : string[]) => {
      const profileResp = await api.profile.retreiveForView(route.params.id as string, user)
      if (typeof profileResp === 'string') {
        errors.push(profileResp)
      } else if (typeof profileResp === 'undefined') {
        errors.push('Profile not found')
      } else {
        it.value = profileResp
      }
    }

    return {
      pageTitle: computed(() =>
        it.value ? `Employment profile for ${citizenName(it.value.citizen)}` : 'Loading Profile...'),
      user,
      retrieveProfile,
      it,
      workTypes,
      bioHtml: computed(() => marked(it.value?.profile.biography || '', markedOptions)),
      expHtml: computed(() => marked(it.value?.profile.experience || '', markedOptions))
    }
  }
})
</script>
