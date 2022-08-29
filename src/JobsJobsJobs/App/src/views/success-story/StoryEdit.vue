<template>
  <article>
    <h3 class="pb-3">{{title}}</h3>
    <load-data :load="retrieveStory">
      <p v-if="isNew">
        Congratulations on your employment! Your fellow citizens would enjoy hearing how it all came about; tell us
        about it below! <em>(These will be visible to other users, but not to the general public.)</em>
      </p>
      <form class="row g-3">
        <div class="col-12">
          <div class="form-check">
            <input type="checkbox" id="fromHere" class="form-check-input" v-model="v$.fromHere.$model">
            <label class="form-check-label" for="fromHere">I found my employment here</label>
          </div>
        </div>
        <markdown-editor id="story" label="The Success Story" v-model:text="v$.story.$model" />
        <div class="col-12">
          <button class="btn btn-primary" type="submit" @click.prevent="saveStory(true)">
            <icon :icon="mdiContentSaveOutline" />&nbsp; Save
          </button>
          <p v-if="isNew">
            <em>(Saving this will set &ldquo;Seeking Employment&rdquo; to &ldquo;No&rdquo; on your profile.)</em>
          </p>
        </div>
      </form>
    </load-data>
    <maybe-save :saveAction="doSave" :validator="v$" />
  </article>
</template>

<script setup lang="ts">
import { computed, reactive } from "vue"
import { useRoute, useRouter } from "vue-router"
import { mdiContentSaveOutline } from "@mdi/js"
import useVuelidate from "@vuelidate/core"

import api, { LogOnSuccess, StoryForm } from "@/api"
import { toastError, toastSuccess } from "@/components/layout/AppToaster.vue"
import { Mutations, useStore } from "@/store"

import LoadData from "@/components/LoadData.vue"
import MarkdownEditor from "@/components/MarkdownEditor.vue"
import MaybeSave from "@/components/MaybeSave.vue"

const store = useStore()
const route = useRoute()
const router = useRouter()

/** The currently logged-on user */
const user = store.state.user as LogOnSuccess

/** The ID of the story being edited */
const id = route.params.id as string

/** Whether this is a new story */
const isNew = computed(() => id === "new")

/** The form for editing the story */
const story = reactive(new StoryForm())

/** Validator rules */
const rules = computed(() => ({
  fromHere: { },
  story: { }
}))

/** The validator */
const v$ = useVuelidate(rules, story, { $lazy: true })

/** Retrieve the specified story */
const retrieveStory = async (errors : string[]) => {
  if (isNew.value) {
    story.id = "new"
    store.commit(Mutations.SetTitle, "Tell Your Success Story")
  } else {
    const storyResult = await api.success.retrieve(id, user)
    if (typeof storyResult === "string") {
      errors.push(storyResult)
    } else if (typeof storyResult === "undefined") {
      errors.push("Story not found")
    } else if (storyResult.citizenId !== user.citizenId) {
      errors.push("Quit messing around")
    } else {
      story.id = storyResult.id
      story.fromHere = storyResult.fromHere
      story.story = storyResult.story ?? ""
    }
  }
}

/** Save the success story */
const saveStory = async (navigate : boolean) => {
  const saveResult = await api.success.save(story, user)
  if (typeof saveResult === "string") {
    toastError(saveResult, "saving success story")
  } else {
    if (isNew.value) {
      const foundResult = await api.profile.markEmploymentFound(user)
      if (typeof foundResult === "string") {
        toastError(foundResult, "clearing employment flag")
      } else {
        toastSuccess("Success Story saved and Seeking Employment flag cleared successfully")
        v$.value.$reset()
        if (navigate) {
          await router.push("/success-story/list")
        }
      }
    } else {
      toastSuccess("Success Story saved successfully")
      v$.value.$reset()
      if (navigate) {
        await router.push("/success-story/list")
      }
    }
  }
}

/** No-parameter save function (used for save-on-navigate) */
const doSave = async () => await saveStory(false)
</script>
