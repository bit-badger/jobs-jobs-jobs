<template>
  <article>
    <h3 class="pb-3">{{title}}</h3>
    <load-data :load="retrieveProfile">
      <h2>
        <a :href="it.citizen.profileUrl" target="_blank" rel="noopener">{{citizenName(it.citizen)}}</a>
        <span v-if="it.profile.seekingEmployment" class="jjj-heading-label">
          &nbsp; &nbsp;<span class="badge bg-dark">Currently Seeking Employment</span>
        </span>
      </h2>
      <h4 class="pb-3">{{it.continent.name}}, {{it.profile.region}}</h4>
      <p v-html="workTypes" />
      <hr>
      <div v-html="bioHtml" />
      <template v-if="it.profile.skills.length > 0">
        <hr>
        <h4 class="pb-3">Skills</h4>
        <ul>
          <li v-for="(skill, idx) in it.profile.skills" :key="idx">
            {{skill.description}}<template v-if="skill.notes"> &nbsp;({{skill.notes}})</template>
          </li>
        </ul>
      </template>
      <template v-if="it.profile.experience">
        <hr>
        <h4 class="pb-3">Experience / Employment History</h4>
        <div v-html="expHtml" />
      </template>
      <template v-if="user.citizenId === it.citizen.id">
        <br><br>
        <router-link class="btn btn-primary" to="/profile/edit">
          <icon :icon="mdiPencil" />&nbsp; Edit Your Profile
        </router-link>
      </template>
    </load-data>
  </article>
</template>

<script setup lang="ts">
import { computed, ref, Ref } from "vue"
import { useRoute } from "vue-router"
import { mdiPencil } from "@mdi/js"

import api, { LogOnSuccess, ProfileForView } from "@/api"
import { citizenName } from "@/App.vue"
import { toHtml } from "@/markdown"
import { Mutations, useStore } from "@/store"
import LoadData from "@/components/LoadData.vue"

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
    parts.push(`${p.fullTime ? "I" : "Not i"}nterested in full-time employment`)
    parts.push(`${p.remoteWork ? "I" : "Not i"}nterested in remote opportunities`)
  }
  return parts.join(" &bull; ")
})

/** Retrieve the profile and supporting data */
const retrieveProfile = async (errors : string[]) => {
  const profileResp = await api.profile.retreiveForView(route.params.id as string, user)
  if (typeof profileResp === "string") {
    errors.push(profileResp)
  } else if (typeof profileResp === "undefined") {
    errors.push("Profile not found")
  } else {
    it.value = profileResp
    store.commit(Mutations.SetTitle, `Employment profile for ${citizenName(profileResp.citizen)}`)
  }
}

/** The HTML version of the citizen's professional biography */
const bioHtml = computed(() => toHtml(it.value?.profile.biography ?? ""))

/** The HTML version of the citizens Experience section */
const expHtml = computed(() => toHtml(it.value?.profile.experience ?? ""))
</script>
