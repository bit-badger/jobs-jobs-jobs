<template lang="pug">
article
  page-title(:title="title")
  h3.pb-3 {{title}}
  load-data(:load="retrieveStory")
    p(v-if="isNew").
      Congratulations on your employment! Your fellow citizens would enjoy hearing how it all came about; tell us
      about it below! #[em (These will be visible to other users, but not to the general public.)]
    form.row.g-3
      .col-12: .form-check
        input.form-check-input(type="checkbox" id="fromHere" v-model="v$.fromHere.$model")
        label.form-check-label(for="fromHere") I found my employment here
      markdown-editor(id="story" label="The Success Story" v-model:text="v$.story.$model")
      .col-12
        button.btn.btn-primary(type="submit" @click.prevent="saveStory(true)").
          #[icon(icon="content-save-outline")]&nbsp; Save
        p(v-if="isNew"): em (Saving this will set &ldquo;Seeking Employment&rdquo; to &ldquo;No&rdquo; on your profile.)
  maybe-save(:isShown="confirmNavShown" :toRoute="nextRoute" :saveAction="doSave" :validator="v$" @close="confirmClose")
</template>

<script setup lang="ts">
import { computed, reactive, ref, Ref } from "vue"
import { onBeforeRouteLeave, RouteLocationNormalized, useRoute, useRouter } from "vue-router"
import useVuelidate from "@vuelidate/core"

import api, { LogOnSuccess, StoryForm } from "@/api"
import { toastError, toastSuccess } from "@/components/layout/AppToaster.vue"
import { useStore } from "@/store"

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

/** The page title */
const title = computed(() => isNew.value ? "Tell Your Success Story" : "Edit Success Story")

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
          router.push("/success-story/list")
        }
      }
    } else {
      toastSuccess("Success Story saved successfully")
      v$.value.$reset()
      if (navigate) {
        router.push("/success-story/list")
      }
    }
  }
}

/** Whether the navigation confirmation is shown  */
const confirmNavShown = ref(false)

/** The "next" route (will be navigated or cleared) */
const nextRoute : Ref<RouteLocationNormalized | undefined> = ref(undefined)

/** Prompt for save if the user navigates away with unsaved changes */
onBeforeRouteLeave(async (to, from) => { // eslint-disable-line
  if (!v$.value.$anyDirty) return true
  nextRoute.value = to
  confirmNavShown.value = true
  return false
})

/** No-parameter save function (used for save-on-navigate) */
const doSave = async () => await saveStory(false)

/** Close the confirm navigation modal */
const confirmClose = () => { confirmNavShown.value = false }
</script>
