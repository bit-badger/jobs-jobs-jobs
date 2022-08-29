<template>
  <div id="maybeSaveModal" class="modal fade" tabindex="-1" aria-labelledby="maybeSaveLabel" aria-hidden="true">
    <div class="modal-dialog">
      <div class="modal-content">
        <div class="modal-header"><h5 id="maybeSaveLabel" class="modal-title">Unsaved Changes</h5></div>
        <div class="modal-body">
          You have modified the data on this page since it was last saved. What would you like to do?
        </div>
        <div class="modal-footer">
          <button class="btn btn-secondary" type="button" @click.prevent="close">Stay on This Page</button>
          <button class="btn btn-primary" type="button" @click.prevent="save">Save Changes</button>
          <button class="btn btn-danger" type="button" @click.prevent="discard">Discard Changes</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref, Ref } from "vue"
import { onBeforeRouteLeave, RouteLocationNormalized, useRouter } from "vue-router"
import { Validation } from "@vuelidate/core"
import { Modal } from "bootstrap"

const props = defineProps<{
  saveAction: () => Promise<unknown>
  validator?: Validation
}>()

const router = useRouter()

/** Reference to the modal dialog (we can't get it until the component is rendered) */
const modal : Ref<Modal | undefined> = ref(undefined)

/** The route to which navigation was intercepted, and will be resumed */
let nextRoute : RouteLocationNormalized

/** Close the modal window */
const close = () => modal.value?.hide()

/** Save changes and go to the next route */
const save = async () => {
  await props.saveAction()
  close()
  router.push(nextRoute)
}

/** Discard changes and go to the next route */
const discard = () => {
  if (props.validator) props.validator.$reset()
  close()
  router.push(nextRoute)
}

onMounted(() => {
  modal.value = new Modal(document.getElementById("maybeSaveModal") as HTMLElement,
    { backdrop: "static", keyboard: false })
})

/** Prompt for save if the user navigates away with unsaved changes */
onBeforeRouteLeave(async (to, from) => { // eslint-disable-line
  if (!props.validator || !props.validator.$anyDirty) return true
  nextRoute = to
  modal.value?.show()
  return false
})
</script>
