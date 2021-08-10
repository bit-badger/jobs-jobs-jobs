<template>
  <article>
    <page-title title="Edit Profile" />
    <h3 class="pb-3">Employment Profile</h3>
    <load-data :load="retrieveData">
      <form class="row g-3">
        <div class="col-12 col-sm-10 col-md-8 col-lg-6">
          <div class="form-floating">
            <input type="text" id="realName" class="form-control" v-model="v$.realName.$model" maxlength="255"
                    placeholder="Leave blank to use your NAS display name">
            <label for="realName">Real Name</label>
          </div>
          <div class="form-text">Leave blank to use your NAS display name</div>
        </div>
        <div class="col-12">
          <div class="form-check">
            <input type="checkbox" id="isSeeking" class="form-check-input" v-model="v$.isSeekingEmployment.$model">
            <label for="isSeeking" class="form-check-label">I am currently seeking employment</label>
          </div>
          <p v-if="profile.isSeekingEmployment">
            <em>If you have found employment, consider <router-link to="/success-story/new/edit">telling your fellow
            citizens about it!</router-link></em>
          </p>
        </div>
        <div class="col-12 col-sm-6 col-md-4">
          <div class="form-floating">
            <select id="continentId" :class="{ 'form-select': true, 'is-invalid': v$.continentId.$error }"
                    :value="v$.continentId.$model" @change="continentChanged">
              <option v-for="c in continents" :key="c.id" :value="c.id">{{c.name}}</option>
            </select>
            <label for="continentId" class="jjj-required">Continent</label>
          </div>
          <div class="invalid-feedback">Please select a continent</div>
        </div>
        <div class="col-12 col-sm-6 col-md-8">
          <div class="form-floating">
            <input type="text" id="region" :class="{ 'form-control': true, 'is-invalid': v$.region.$error }"
                   v-model="v$.region.$model" maxlength="255" placeholder="Country, state, geographic area, etc.">
            <div id="regionFeedback" class="invalid-feedback">Please enter a region</div>
            <label for="region" class="jjj-required">Region</label>
          </div>
          <div class="form-text">Country, state, geographic area, etc.</div>
        </div>
        <markdown-editor id="bio" label="Professional Biography" v-model:text="v$.biography.$model"
                         :isInvalid="v$.biography.$error" />
        <div class="col-12 col-offset-md-2 col-md-4">
          <div class="form-check">
            <input type="checkbox" id="isRemote" class="form-check-input" v-model="v$.remoteWork.$model">
            <label class="form-check-label" for="isRemote">I am looking for remote work</label>
          </div>
        </div>
        <div class="col-12 col-md-4">
          <div class="form-check">
            <input type="checkbox" id="isFullTime" class="form-check-input" v-model="v$.fullTime.$model">
            <label class="form-check-label" for="isFullTime">I am looking for full-time work</label>
          </div>
        </div>
        <div class="col-12">
          <hr>
          <h4 class="pb-2">
            Skills &nbsp;
            <button class="btn btn-sm btn-outline-primary rounded-pill" @click.prevent="addSkill">Add a Skill</button>
          </h4>
        </div>
        <profile-skill-edit v-for="(skill, idx) in profile.skills" :key="skill.id" v-model="profile.skills[idx]"
                            @remove="removeSkill(skill.id)" @input="v$.skills.$touch" />
        <div class="col-12">
          <hr>
          <h4>Experience</h4>
          <p>
            This application does not have a place to individually list your chronological job history; however, you can
            use this area to list prior jobs, their dates, and anything else you want to include that&rsquo;s not
            already a part of your Professional Biography above.
          </p>
        </div>
        <markdown-editor id="experience" label="Experience" v-model:text="v$.experience.$model" />
        <div class="col-12">
          <div class="form-check">
            <input type="checkbox" id="isPublic" class="form-check-input" v-model="v$.isPublic.$model">
            <label class="form-check-label" for="isPublic">
              Allow my profile to be searched publicly (outside NA Social)
            </label>
          </div>
        </div>
        <div class="col-12">
          <p v-if="v$.$error" class="text-danger">Please correct the errors above</p>
          <button class="btn btn-primary" @click.prevent="saveProfile">
            <icon icon="content-save-outline" />&nbsp; Save
          </button>
          <template v-if="!isNew">
            &nbsp; &nbsp;
            <router-link class="btn btn-outline-secondary" :to="`/profile/${user.citizenId}/view`">
              <icon icon="file-account-outline" />&nbsp; View Your User Profile
            </router-link>
          </template>
        </div>
      </form>
    </load-data>
    <hr>
    <p class="text-muted fst-italic">
      (If you want to delete your profile, or your entire account, <router-link to="/so-long/options">see your deletion
      options here</router-link>.)
    </p>
    <maybe-save :isShown="confirmNavShown" :toRoute="nextRoute" :saveAction="saveProfile" :validator="v$"
                @close="confirmClose" />
  </article>
</template>

<script lang="ts">
import { computed, defineComponent, ref, reactive, Ref } from 'vue'
import { onBeforeRouteLeave, RouteLocationNormalized } from 'vue-router'
import useVuelidate from '@vuelidate/core'
import { required } from '@vuelidate/validators'

import api, { Citizen, LogOnSuccess, Profile, ProfileForm } from '@/api'
import { toastError, toastSuccess } from '@/components/layout/AppToaster.vue'
import { useStore } from '@/store'

import LoadData from '@/components/LoadData.vue'
import MarkdownEditor from '@/components/MarkdownEditor.vue'
import MaybeSave from '@/components/MaybeSave.vue'
import ProfileSkillEdit from '@/components/profile/SkillEdit.vue'

export default defineComponent({
  name: 'EditProfile',
  components: {
    LoadData,
    MarkdownEditor,
    MaybeSave,
    ProfileSkillEdit
  },
  setup () {
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
      continentId: '',
      region: '',
      remoteWork: false,
      fullTime: false,
      biography: '',
      lastUpdatedOn: '',
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
      await store.dispatch('ensureContinents')
      const profileResult = await api.profile.retreive(undefined, user)
      if (typeof profileResult === 'string') {
        errors.push(profileResult)
      } else if (typeof profileResult === 'undefined') {
        isNew.value = true
      }
      const nameResult = await api.citizen.retrieve(user.citizenId, user)
      if (typeof nameResult === 'string') {
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
      profile.realName = typeof nameResult !== 'undefined' ? (nameResult as Citizen).realName || '' : ''
    }

    /**
     * Mark the continent field as changed
     *
     * (This works around a really strange sequence where, if the "touch" call is directly wired up to the onChange
     * event, the first time a value is selected, it doesn't stick (although the field is marked as touched). On second
     * and subsequent times, it worked. The solution here is to grab the value and update the reactive source for the
     * form, then manually set the field to touched; this restores the expected behavior. This is probably why the
     * library doesn't hook into the onChange event to begin with...)
     */
    const continentChanged = (e : Event) : boolean => {
      profile.continentId = (e.target as HTMLSelectElement).value
      v$.value.continentId.$touch()
      return true
    }

    /** The ID for new skills */
    let newSkillId = 0

    /** Add a skill to the profile */
    const addSkill = () => {
      profile.skills.push({ id: `new${newSkillId++}`, description: '', notes: undefined })
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
      profile.skills = profile.skills.filter(s => !(s.description.trim() === '' && (s.notes || '').trim() === ''))
      const saveResult = await api.profile.save(profile, user)
      if (typeof saveResult === 'string') {
        toastError(saveResult, 'saving profile')
      } else {
        toastSuccess('Profile Saved Successfuly')
        v$.value.$reset()
      }
    }

    /** Whether the navigation confirmation is shown  */
    const confirmNavShown = ref(false)

    /** The "next" route (will be navigated or cleared) */
    const nextRoute : Ref<RouteLocationNormalized | undefined> = ref(undefined)

    /** If the user has unsaved changes, give them an opportunity to save before moving on */
    onBeforeRouteLeave(async (to, from) => { // eslint-disable-line
      if (!v$.value.$anyDirty) return true
      nextRoute.value = to
      confirmNavShown.value = true
      return false
    })

    return {
      v$,
      retrieveData,
      user,
      isNew,
      profile,
      continents: computed(() => store.state.continents),
      continentChanged,
      addSkill,
      removeSkill,
      saveProfile,
      confirmNavShown,
      nextRoute,
      confirmClose: () => { confirmNavShown.value = false }
    }
  }
})
</script>
