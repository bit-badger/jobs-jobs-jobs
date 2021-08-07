<template>
  <article>
    <page-title title="Edit Profile" />
    <h3>Employment Profile</h3>
    <load-data :load="retrieveData">
      <form>
        <div class="row pb-3">
          <div class="col col-xs-12 col-sm-10 col-md-8 col-lg-6">
            <div class="form-floating">
              <input type="text" id="realName" class="form-control" v-model="profile.realName" maxlength="255"
                     placeholder="Leave blank to use your NAS display name">
              <label for="realName">Real Name</label>
            </div>
            <div class="form-text">Leave blank to use your NAS display name</div>
          </div>
        </div>
        <div class="row pb-3">
          <div class="col col-xs-12">
            <div class="form-check">
              <input type="checkbox" class="form-check-input" v-model="profile.seekingEmployment">
              <label class="form-check-label">I am currently seeking employment</label>
            </div>
            <p v-if="profile?.seekingEmployment">
              <em>If you have found employment, consider <router-link to="/success-story/add">telling your fellow
              citizens about it!</router-link></em>
            </p>
          </div>
        </div>
        <div class="row pb-3">
          <div class="col col-xs-12 col-sm-6 col-md-4">
            <div class="form-floating">
              <select id="continentId" class="form-select" :value="profile.continentId">
                <option v-for="c in continents" :key="c.id" :value="c.id">{{c.name}}</option>
              </select>
              <label for="continentId" class="jjj-required">Continent</label>
            </div>
          </div>
          <div class="col col-xs-12 col-sm-6 col-md-8">
            <div class="form-floating">
              <input type="text" id="region" class="form-control" v-model="profile.region" maxlength="255"
                     placeholder="Country, state, geographic area, etc.">
              <label for="region" class="jjj-required">Region</label>
            </div>
            <div class="form-text">Country, state, geographic area, etc.</div>
          </div>
        </div>
        <markdown-editor id="bio" label="Professional Biography" v-model:text="profile.biography" />
        <div class="row pb-3">
          <div class="col col-xs-12 col-offset-md-2 col-md-4">
            <div class="form-check">
              <input type="checkbox" id="isRemote" class="form-check-input" v-model="profile.remoteWork">
              <label class="form-check-label" for="isRemote">I am looking for remote work</label>
            </div>
          </div>
          <div class="col col-xs-12 col-md-4">
            <div class="form-check">
              <input type="checkbox" id="isFullTime" class="form-check-input" v-model="profile.fullTime">
              <label class="form-check-label" for="isFullTime">I am looking for full-time work</label>
            </div>
          </div>
        </div>
        <hr>
        <h4 class="pb-2">
          Skills &nbsp;
          <button class="btn btn-sm btn-outline-primary rounded-pill" @click.prevent="addSkill">Add a Skill</button>
        </h4>
        <profile-skill-edit v-for="(skill, idx) in profile.skills" :key="skill.id" v-model="profile.skills[idx]"
                            @remove="removeSkill(skill.id)" />
        <hr>
        <h4>Experience</h4>
        <p>
          This application does not have a place to individually list your chronological job history; however, you can
          use this area to list prior jobs, their dates, and anything else you want to include that&rsquo;s not
          already a part of your Professional Biography above.
        </p>
        <markdown-editor id="experience" label="Experience" v-model:text="profile.experience" />
        <div class="row pb-3">
          <div class="col col-xs-12">
            <div class="form-check">
              <input type="checkbox" id="isPublic" class="form-check-input" v-model="profile.isPublic">
              <label class="form-check-label" for="isPublic">
                Allow my profile to be searched publicly (outside NA Social)
              </label>
            </div>
          </div>
        </div>
        <div class="row pt-3">
          <div class="col col-xs-12">
            <button class="btn btn-primary">Save</button>
            <template v-if="!isNew">
              &nbsp; &nbsp;
              <button class="btn btn-outline-secondary" @click.prevent="viewProfile">
                <icon icon="file-account-outline" />&nbsp; View Your User Profile
              </button>
            </template>
          </div>
        </div>
      </form>
    </load-data>
    <hr>
    <p class="text-muted fst-italic">
      (If you want to delete your profile, or your entire account, <router-link to="/so-long/options">see your deletion
      options here</router-link>.)
    </p>
  </article>
</template>

<script lang="ts">
import { computed, defineComponent, Ref, ref } from 'vue'
import { useRouter } from 'vue-router'
import api, { Citizen, LogOnSuccess, Profile, ProfileForm } from '@/api'
import { useStore } from '@/store'

import LoadData from '@/components/LoadData.vue'
import MarkdownEditor from '@/components/MarkdownEditor.vue'
import ProfileSkillEdit from '@/components/profile/SkillEdit.vue'

export default defineComponent({
  name: 'EditProfile',
  components: {
    LoadData,
    MarkdownEditor,
    ProfileSkillEdit
  },
  setup () {
    const store = useStore()
    const router = useRouter()

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
    const profile : Ref<ProfileForm | undefined> = ref(undefined)

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
      const p = isNew.value ? newProfile : profileResult as Profile
      profile.value = {
        isSeekingEmployment: p.seekingEmployment,
        isPublic: p.isPublic,
        continentId: p.continentId,
        region: p.region,
        remoteWork: p.remoteWork,
        fullTime: p.fullTime,
        biography: p.biography,
        experience: p.experience,
        skills: p.skills,
        realName: typeof nameResult !== 'undefined' ? (nameResult as Citizen).realName || '' : ''
      }
    }

    /** The ID for new skills */
    let newSkillId = 0

    /** Add a skill to the profile */
    const addSkill = () => {
      const form = profile.value as ProfileForm
      form.skills.push({ id: `new${newSkillId}`, description: '', notes: undefined })
      newSkillId++
      profile.value = form
    }

    /** Remove the given skill from the profile */
    const removeSkill = (skillId : string) => {
      const form = profile.value as ProfileForm
      form.skills = form.skills.filter(s => s.id !== skillId)
      profile.value = form
    }

    /** Save the current profile values */
    const saveProfile = async () => {
      // TODO
    }

    return {
      retrieveData,
      user,
      isNew,
      profile,
      continents: computed(() => store.state.continents),
      addSkill,
      removeSkill,
      saveProfile,
      viewProfile: () => router.push(`/profile/view/${user.citizenId}`)
    }
  }
})
</script>
