<template lang="pug">
article
  page-title(title="Success Stories")
  h3.pb-3 Success Stories
  load-data(:load="retrieveStories")
    table.table.table-sm.table-hover(v-if="stories?.length > 0")
      thead: tr
        th(scope="col") Story
        th(scope="col") From
        th(scope="col") Found Here?
        th(scope="col") Recorded On
      tbody: tr(v-for="story in stories" :key="story.id")
        td
          router-link(v-if="story.hasStory" :to="`/success-story/${story.id}/view`") View
          em(v-else) None
          template(v-if="story.citizenId === user.citizenId")
            |  ~ #[router-link(:to="`/success-story/${story.id}/edit`") Edit]
        td {{story.citizenName}}
        td
          strong(v-if="story.fromHere") Yes
          template(v-else) No
        td: full-date(:date="story.recordedOn")
    p(v-else) There are no success stories recorded #[em (yet)]
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
