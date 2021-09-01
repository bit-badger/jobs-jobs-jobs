<template lang="pug">
article
  page-title(title="Edit Profile")
  h3.pb-3 My Employment Profile
  load-data(:load="retrieveData"): form.row.g-3
    .col-12.col-sm-10.col-md-8.col-lg-6
      .form-floating
        input.form-control(type="text" id="realName" v-model="v$.realName.$model" maxlength="255"
                           placeholder="Leave blank to use your NAS display name")
        label(for="realName") Real Name
      .form-text Leave blank to use your NAS display name
    .col-12
      .form-check
        input.form-check-input(type="checkbox" id="isSeeking" v-model="v$.isSeekingEmployment.$model")
        label.form-check-label(for="isSeeking") I am currently seeking employment
      p(v-if="profile.isSeekingEmployment"): em.
        If you have found employment, consider
        #[router-link(to="/success-story/new/edit") telling your fellow citizens about it!]
    .col-12.col-sm-6.col-md-4
      continent-list(v-model="v$.continentId.$model" :isInvalid="v$.continentId.$error"
                     @touch="v$.continentId.$touch() || true")
    .col-12.col-sm-6.col-md-8
      .form-floating
        input.form-control(type="text" id="region" :class="{ 'is-invalid': v$.region.$error }"
                           v-model="v$.region.$model" maxlength="255"
                           placeholder="Country, state, geographic area, etc.")
        #regionFeedback.invalid-feedback Please enter a region
        label.jjj-required(for="region") Region
      .form-text Country, state, geographic area, etc.
    markdown-editor(id="bio" label="Professional Biography" v-model:text="v$.biography.$model"
                    :isInvalid="v$.biography.$error")
    .col-12.col-offset-md-2.col-md-4
      .form-check
        input.form-check-input(type="checkbox" id="isRemote" v-model="v$.remoteWork.$model")
        label.form-check-label(for="isRemote") I am looking for remote work
    .col-12.col-md-4
      .form-check
        input.form-check-input(type="checkbox" id="isFullTime" v-model="v$.fullTime.$model")
        label.form-check-label(for="isFullTime") I am looking for full-time work
    .col-12
      hr
      h4.pb-2 Skills &nbsp;#[button.btn.btn-sm.btn-outline-primary.rounded-pill(@click.prevent="addSkill") Add a Skill]
    profile-skill-edit(v-for="(skill, idx) in profile.skills" :key="skill.id" v-model="profile.skills[idx]"
                       @remove="removeSkill(skill.id)" @input="v$.skills.$touch")
    .col-12
      hr
      h4 Experience
      p.
        This application does not have a place to individually list your chronological job history; however, you can use
        this area to list prior jobs, their dates, and anything else you want to include that&rsquo;s not already a part
        of your Professional Biography above.
    markdown-editor(id="experience" label="Experience" v-model:text="v$.experience.$model")
    .col-12: .form-check
      input.form-check-input(type="checkbox" id="isPublic" v-model="v$.isPublic.$model")
      label.form-check-label(for="isPublic") Allow my profile to be searched publicly (outside NA Social)
    .col-12
      p.text-danger(v-if="v$.$error") Please correct the errors above
      button.btn.btn-primary(@click.prevent="saveProfile") #[icon(:icon="mdiContentSaveOutline")]&nbsp; Save
      template(v-if="!isNew")
        | &nbsp; &nbsp;
        router-link.btn.btn-outline-secondary(:to="`/profile/${user.citizenId}/view`").
          #[icon(color="#6c757d" :icon="mdiFileAccountOutline")]&nbsp; View Your User Profile
  hr
  p.text-muted.fst-italic.
    (If you want to delete your profile, or your entire account,
    #[router-link(to="/so-long/options") see your deletion options here].)
  maybe-save(:saveAction="saveProfile" :validator="v$")
</template>

<script setup lang="ts">
import { computed, ref, reactive } from "vue"
import { mdiContentSaveOutline, mdiFileAccountOutline } from "@mdi/js"
import useVuelidate from "@vuelidate/core"
import { required } from "@vuelidate/validators"

import api, { Citizen, LogOnSuccess, Profile, ProfileForm } from "@/api"
import { toastError, toastSuccess } from "@/components/layout/AppToaster.vue"
import { useStore } from "@/store"

import ContinentList from "@/components/ContinentList.vue"
import LoadData from "@/components/LoadData.vue"
import MarkdownEditor from "@/components/MarkdownEditor.vue"
import MaybeSave from "@/components/MaybeSave.vue"
import ProfileSkillEdit from "@/components/profile/SkillEdit.vue"

const store = useStore()

/** The currently logged-on user */
const user = store.state.user as LogOnSuccess

/** Whether this is a new profile */
const isNew = ref(false)

/** The starting values for a new employment profile */
const newProfile : Profile = {
  id: user.citizenId,
  seekingEmployment: false,
  isPublic: false,
  continentId: "",
  region: "",
  remoteWork: false,
  fullTime: false,
  biography: "",
  lastUpdatedOn: "",
  experience: undefined,
  skills: []
}

/** The user's current profile (plus a few items, adapted for editing) */
const profile = reactive(new ProfileForm())

/** The validation rules for the form */
const rules = computed(() => ({
  realName: { },
  isSeekingEmployment: { },
  isPublic: { },
  continentId: { required },
  region: { required },
  remoteWork: { },
  fullTime: { },
  biography: { required },
  experience: { },
  skills: { }
}))

/** Initialize form validation */
const v$ = useVuelidate(rules, profile, { $lazy: true })

/** Retrieve the user's profile and their real name */
const retrieveData = async (errors : string[]) => {
  const profileResult = await api.profile.retreive(undefined, user)
  if (typeof profileResult === "string") {
    errors.push(profileResult)
  } else if (typeof profileResult === "undefined") {
    isNew.value = true
  }
  const nameResult = await api.citizen.retrieve(user.citizenId, user)
  if (typeof nameResult === "string") {
    errors.push(nameResult)
  }
  if (errors.length > 0) return
  // Update the empty form with appropriate values
  const p = isNew.value ? newProfile : profileResult as Profile
  profile.isSeekingEmployment = p.seekingEmployment
  profile.isPublic = p.isPublic
  profile.continentId = p.continentId
  profile.region = p.region
  profile.remoteWork = p.remoteWork
  profile.fullTime = p.fullTime
  profile.biography = p.biography
  profile.experience = p.experience
  profile.skills = p.skills
  profile.realName = typeof nameResult !== "undefined" ? (nameResult as Citizen).realName ?? "" : ""
}

/** The ID for new skills */
let newSkillId = 0

/** Add a skill to the profile */
const addSkill = () => {
  profile.skills.push({ id: `new${newSkillId++}`, description: "", notes: undefined })
  v$.value.skills.$touch()
}

/** Remove the given skill from the profile */
const removeSkill = (skillId : string) => {
  profile.skills = profile.skills.filter(s => s.id !== skillId)
  v$.value.skills.$touch()
}

/** Save the current profile values */
const saveProfile = async () => {
  v$.value.$touch()
  if (v$.value.$error) return
  // Remove any blank skills before submitting
  profile.skills = profile.skills.filter(s => !(s.description.trim() === "" && (s.notes ?? "").trim() === ""))
  const saveResult = await api.profile.save(profile, user)
  if (typeof saveResult === "string") {
    toastError(saveResult, "saving profile")
  } else {
    toastSuccess("Profile Saved Successfuly")
    v$.value.$reset()
  }
}
</script>
