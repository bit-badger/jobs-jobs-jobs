<template>
  <h3>Employment Profile</h3>

  <v-form>
    <v-container>
      <v-row>
        <v-col cols="12" sm="10" md="8" lg="6">
          <v-text-field label="Real Name"
                        placeholder="Leave blank to use your NAS display name"
                        counter="255"
                        maxlength="255"
                        :value="realName"></v-text-field>
          <div class="form-group">
            <label for="realName" class="jjj-label">Real Name</label>
            [InputText id="realName" @bind-Value=@ProfileForm.RealName class="form-control"
                      placeholder="Leave blank to use your NAS display name" /]
            [ValidationMessage For=@(() => ProfileForm.RealName) /]
          </div>
        </v-col>
      </v-row>
      <div class="form-row">
        <div class="col">
          <div class="form-check">
            [InputCheckbox id="seeking" class="form-check-input" @bind-Value=@ProfileForm.IsSeekingEmployment /]
            <label for="seeking" class="form-check-label">I am currently seeking employment</label>
            <em v-if="profile?.seekingEmployment">&nbsp; &nbsp; If you have found employment, consider
              <router-link to="/success-story/add">telling your fellow citizens about it</router-link>
            </em>
          </div>
        </div>
      </div>
      <div class="form-row">
        <div class="col col-xs-12 col-sm-6 col-md-4">
          <div class="form-group">
            <label for="continentId" class="jjj-required">Continent</label>
            [InputSelect id="continentId" @bind-Value=@ProfileForm.ContinentId class="form-control"]
              <option>&ndash; Select &ndash;</option>
              @foreach (var (id, name) in Continents)
              {
                <option value="@id">@name</option>
              }
            [/InputSelect]
            [ValidationMessage For=@(() => ProfileForm.ContinentId) /]
          </div>
        </div>
        <div class="col col-xs-12 col-sm-6 col-md-8">
          <div class="form-group">
            <label for="region" class="jjj-required">Region</label>
            [InputText id="region" @bind-Value=@ProfileForm.Region class="form-control"
                        placeholder="Country, state, geographic area, etc." /]
            [ValidationMessage For=@(() => ProfileForm.Region) /]
          </div>
        </div>
      </div>
      <div class="form-row">
        <div class="col">
          <div class="form-group">
            <label for="bio" class="jjj-required">Professional Biography</label>
            [MarkdownEditor Id="bio" @bind-Text=@ProfileForm.Biography /]
            [ValidationMessage For=@(() => ProfileForm.Biography) /]
          </div>
        </div>
      </div>
      <div class="form-row">
        <div class="col col-xs-12 col-sm-12 offset-md-2 col-md-4">
          <div class="form-check">
            [InputCheckbox id="isRemote" class="form-check-input" @bind-Value=@ProfileForm.RemoteWork /]
            <label for="isRemote" class="form-check-label">I am looking for remote work</label>
          </div>
        </div>
        <div class="col col-xs-12 col-sm-12 col-md-4">
          <div class="form-check">
            [InputCheckbox id="isFull" class="form-check-input" @bind-Value=@ProfileForm.FullTime /]
            <label for="isFull" class="form-check-label">I am looking for full-time work</label>
          </div>
        </div>
      </div>
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
        use this area to list prior jobs, their dates, and anything else you want to include that&rsquo;s not already a
        part of your Professional Biography above.
      </p>
      <div class="form-row">
        <div class="col">
          [MarkdownEditor Id="experience" @bind-Text=@ProfileForm.Experience /]
        </div>
      </div>
      <div class="form-row">
        <div class="col">
          <div class="form-check">
            [InputCheckbox id="isPublic" class="form-check-input" @bind-Value=@ProfileForm.IsPublic /]
            <label for="isPublic" class="form-check-label">
              Allow my profile to be searched publicly (outside NA Social)
            </label>
          </div>
        </div>
      </div>
      <div class="form-row">
        <div class="col">
          <br>
          <button type="submit" class="btn btn-outline-primary">Save</button>
        </div>
      </div>
    </v-container>
  </v-form>
  <p v-if="!isNew">
    <br><router-link :to="'/profile/view/' + user.citizenId"><v-icon icon="file-account-outline" /> View Your User
    Profile</router-link>
  </p>
  <p>
    <br>If you want to delete your profile, or your entire account, <router-link to="/so-long/options">see your deletion
    options here</router-link>.
  </p>

</template>

<script lang="ts">
import { defineComponent, onMounted, ref } from 'vue'
import api, { LogOnSuccess, Profile } from '../../api'
import { useStore } from '../../store'

export default defineComponent({
  name: 'EditProfile',
  setup () {
    const store = useStore()

    /** The currently logged-on user */
    const user = store.state.user as LogOnSuccess

    /** Whether this is a new profile */
    const isNew = ref(false)

    /** Errors that may be encountered */
    const errorMessages : string[] = []

    /** The user's current profile */
    let profile : Profile | undefined

    /** The user's real name */
    let realName : string | undefined

    /** Retrieve the user's profile */
    const loadProfile = async () => {
      const profileResult = await api.profile.retreive(undefined, user)
      if (typeof profileResult === 'string') {
        errorMessages.push(profileResult)
      } else if (typeof profileResult === 'undefined') {
        isNew.value = true
      } else {
        profile = profileResult
      }
    }

    onMounted(loadProfile)

    return {
      user,
      isNew,
      errorMessages,
      profile,
      realName
    }
  }
})
</script>
