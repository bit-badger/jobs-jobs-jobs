<template>
  <article>
    <h3 class="pb-3">Success Stories</h3>
    <load-data :load="retrieveStories">
      <table v-if="stories?.length > 0" class="table table-sm table-hover">
        <thead>
          <tr>
            <th scope="col">Story</th>
            <th scope="col">From</th>
            <th scope="col">Found Here?</th>
            <th scope="col">Recorded On</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="story in stories" :key="story.id">
            <td>
              <router-link v-if="story.hasStory" :to="`/success-story/${story.id}/view`">View</router-link>
              <em v-else>None</em>
              <template v-if="story.citizenId === user.citizenId">
                ~ <router-link :to="`/success-story/${story.id}/edit`">Edit</router-link>
              </template>
            </td>
            <td>{{story.citizenName}}</td>
            <td><strong v-if="story.fromHere">Yes</strong><template v-else>No</template></td>
            <td><full-date :date="story.recordedOn" /></td>
          </tr>
        </tbody>
      </table>
      <p v-else>There are no success stories recorded <em>(yet)</em></p>
    </load-data>
  </article>
</template>

<script setup lang="ts">
import { ref, Ref } from "vue"
import api, { LogOnSuccess, StoryEntry } from "@/api"
import { useStore } from "@/store"

import FullDate from "@/components/FullDate.vue"
import LoadData from "@/components/LoadData.vue"

const store = useStore()

/** The currently logged-on user */
const user = store.state.user as LogOnSuccess

/** The success stories to be displayed */
const stories : Ref<StoryEntry[] | undefined> = ref(undefined)

/** Get all currently recorded stories */
const retrieveStories = async (errors : string[]) => {
  const listResult = await api.success.list(user)
  if (typeof listResult === "string") {
    errors.push(listResult)
  } else if (typeof listResult === "undefined") {
    stories.value = []
  } else {
    stories.value = listResult
  }
}
</script>
