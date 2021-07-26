<template>
  <article>
    <page-title :title="pageTitle" />
    <load-data :load="retrieveProfile">
      <h2><a :href="it.citizen.profileUrl" target="_blank">{{citizenName}}</a></h2>
      <h4>{{it.continent.name}}, {{it.profile.region}}</h4>
      <p v-html="workTypes"></p>
      <hr>
      <div v-html="bioHtml"></div>

      <template v-if="it.profile.skills.length > 0">
        <hr>
        <h4>Skills</h4>
        <ul>
          <li v-for="(skill, idx) in it.profile.skills" :key="idx">
            {{skill.description}}<template v-if="skill.notes"> ({{skill.notes}})</template>
          </li>
        </ul>
      </template>

      <template v-if="it.profile.experience">
        <hr>
        <h4>Experience / Employment History</h4>
        <div v-html="expHtml"></div>
      </template>

      <template v-if="user.citizenId === it.citizen.id">
        <br><br>
        <v-btn color="primary" @click="editProfile"><v-icon icon="mdi-pencil" />&nbsp; Edit Your Profile</v-btn>
      </template>
    </load-data>
  </article>
</template>

<script lang="ts">
import { computed, defineComponent, ref, Ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import marked from 'marked'
import LoadData from '../../components/shared/LoadData.vue'
import { useStore } from '../../store'
import api, { LogOnSuccess, markedOptions, ProfileForView } from '../../api'

export default defineComponent({
  name: 'ProfileEdit',
  components: { LoadData },
  setup () {
    const store = useStore()
    const route = useRoute()
    const router = useRouter()

    /** The currently logged-on user */
    const user = store.state.user as LogOnSuccess

    /** The requested profile */
    const it : Ref<ProfileForView | undefined> = ref(undefined)

    /** The citizen's name (real, display, or NAS, whichever is found first) */
    const citizenName = computed(() => {
      const c = it.value?.citizen
      return c?.realName || c?.displayName || c?.naUser || ''
    })

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
      pageTitle: computed(() => it.value ? `Employment profile for ${citizenName.value}` : 'Loading Profile...'),
      user,
      retrieveProfile,
      it,
      workTypes,
      citizenName,
      bioHtml: computed(() => marked(it.value?.profile.biography || '', markedOptions)),
      expHtml: computed(() => marked(it.value?.profile.experience || '', markedOptions)),
      editProfile: () => router.push('/citizen/profile')
    }
  }
})
</script>
