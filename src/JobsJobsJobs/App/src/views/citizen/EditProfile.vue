<template>
  <article>
    <page-title title="Edit Profile" />
    <h3>Employment Profile</h3>
    <load-data :load="retrieveData">
      <form>
        <v-container>
          <v-row>
            <v-col cols="12" sm="10" md="8" lg="6">
              <label for="realName">Real Name</label>
              <input type="text" id="realName" v-model="realName" maxlength="255"
                    placeholder="Leave blank to use your NAS display name">
            </v-col>
          </v-row>
          <v-row>
            <v-col>
              <label>
                <input type="checkbox" v-model="profile.seekingEmployment">
                I am currently seeking employment
              </label>
              <em v-if="profile?.seekingEmployment">&nbsp; &nbsp; If you have found employment, consider
                <router-link to="/success-story/add">telling your fellow citizens about it</router-link>
              </em>
            </v-col>
          </v-row>
          <v-row>
            <v-col cols="12" sm="6" md="4">
              <label for="continentId" class="jjj-required">Continent</label>
              <select id="continentId">
                <option v-for="c in continents" :key="c.id" :value="c.id"
                        :selected="c.id === profile?.continentId ? 'selected' : null">{{c.name}}</option>
              </select>
            </v-col>
            <v-col cols="12" sm="6" md="8">
              <label for="region" class="jjj-required">Region</label>
              <input type="text" id="region" v-model="profile.region" maxlength="255"
                    placeholder="Country, state, geographic area, etc.">
            </v-col>
          </v-row>
          <v-row>
            <v-col>
              <label for="bio" class="jjj-required">Professional Biography</label>
              <markdown-editor id="bio" v-model:text="profile.biography" />
            </v-col>
          </v-row>
          <v-row>
            <v-col cols="12" sm="12" offset-md="2" md="4">
              <label>
                <input type="checkbox" v-model="profile.remoteWork">
                I am looking for remote work
              </label>
            </v-col>
            <v-col cols="12" sm="12" md="4">
              <label>
                <input type="checkbox" v-model="profile.fullTime">
                I am looking for full-time work
              </label>
            </v-col>
          </v-row>
          <hr>
          <h4>
            Skills &nbsp;
            <button type="button" class="btn btn-outline-primary" @onclick="AddNewSkill">Add a Skill</button>
          </h4>
          @foreach (var skill in ProfileForm.Skills)
          {
            [SkillEdit Skill=@skill OnRemove=@RemoveSkill /]
          }
          <hr>
          <h4>Experience</h4>
          <p>
            This application does not have a place to individually list your chronological job history; however, you can
            use this area to list prior jobs, their dates, and anything else you want to include that&rsquo;s not
            already a part of your Professional Biography above.
          </p>
          <v-row>
            <v-col>
              <markdown-editor id="experience" v-model:text="profile.experience" />
            </v-col>
          </v-row>
          <v-row>
            <v-col>
              <label>
                <input type="checkbox" v-model="profile.isPublic">
                Allow my profile to be searched publicly (outside NA Social)
              </label>
            </v-col>
          </v-row>
          <v-row>
            <v-col>
              <br>
              <v-btn text color="primary">Save</v-btn>
            </v-col>
          </v-row>
        </v-container>
      </form>
      <p v-if="!isNew">
        <br>
        <v-btn color="primary" @click="viewProfile">
          <v-icon icon="mdi-file-account-outline" />&nbsp; View Your User Profile
        </v-btn>
      </p>
    </load-data>
    <p>
      <br>If you want to delete your profile, or your entire account, <router-link to="/so-long/options">see your
      deletion options here</router-link>.
    </p>
  </article>
</template>

<script lang="ts">
import { computed, defineComponent, Ref, ref } from 'vue'
import { useRouter } from 'vue-router'
import api, { LogOnSuccess, Profile } from '../../api'
import MarkdownEditor from '../../components/MarkdownEditor.vue'
import LoadData from '../../components/shared/LoadData.vue'
import { useStore } from '../../store'

export default defineComponent({
  name: 'EditProfile',
  components: {
    LoadData,
    MarkdownEditor
  },
  setup () {
    const store = useStore()
    const router = useRouter()

    /** The currently logged-on user */
    const user = store.state.user as LogOnSuccess

    /** Whether this is a new profile */
    const isNew = ref(false)

    /** The user's current profile */
    const profile : Ref<Profile | undefined> = ref(undefined)

    /** The user's real name */
    const realName : Ref<string | undefined> = ref(undefined)

    /** Retrieve the user's profile and their real name */
    const retrieveData = async (errors : string[]) => {
      await store.dispatch('ensureContinents')
      const profileResult = await api.profile.retreive(undefined, user)
      if (typeof profileResult === 'string') {
        errors.push(profileResult)
      } else if (typeof profileResult === 'undefined') {
        isNew.value = true
      } else {
        profile.value = profileResult
        // console.info(JSON.stringify(profile))
      }
      const nameResult = await api.citizen.retrieve(user.citizenId, user)
      if (typeof nameResult === 'string') {
        errors.push(nameResult)
      } else if (typeof nameResult !== 'undefined') {
        realName.value = nameResult.realName || ''
      }
    }

    return {
      retrieveData,
      user,
      isNew,
      profile,
      realName,
      continents: computed(() => store.state.continents),
      viewProfile: () => router.push(`/profile/view/${user.citizenId}`)
    }
  }
})
</script>
