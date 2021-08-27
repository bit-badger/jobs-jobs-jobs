<template lang="pug">
.modal.fade(id="maybeSaveModal" tabindex="-1" aria-labelledby="maybeSaveLabel" aria-hidden="true"): .modal-dialog: .modal-content
  .modal-header: h5.modal-title(id="maybeSaveLabel") Unsaved Changes
  .modal-body You have modified the data on this page since it was last saved. What would you like to do?
  .modal-footer
    button.btn.btn-secondary(type="button" @click.prevent="onStay") Stay on This Page
    button.btn.btn-primary(type="button" @click.prevent="onSave") Save Changes
    button.btn.btn-danger(type="button" @click.prevent="onDiscard") Discard Changes
</template>

<script setup lang="ts">
import { onMounted, ref, Ref, watch } from "vue"
import { RouteLocationNormalized, useRouter } from "vue-router"
import { Validation } from "@vuelidate/core"
import { Modal } from "bootstrap"

const props = defineProps<{
  isShown: boolean
  toRoute: RouteLocationNormalized
  saveAction?: () => Promise<unknown>
  validator?: Validation
}>()

const emit = defineEmits<{
  (e: "close") : void
  (e: "discard") : void
  (e: "cancel") : void
}>()

const router = useRouter()

/** Reference to the modal dialog (we can't get it until the component is rendered) */
const modal : Ref<Modal | undefined> = ref(undefined)

/** Save changes (if required) and go to the next route */
const onSave = async () => {
  if (props.saveAction) await props.saveAction()
  emit("close")
  router.push(props.toRoute)
}

/** Discard changes (if required) and go to the next route */
const onDiscard = () => {
  if (props.validator) props.validator.$reset()
  emit("close")
  router.push(props.toRoute)
}

onMounted(() => {
  modal.value = new Modal(document.getElementById("maybeSaveModal") as HTMLElement,
    { backdrop: "static", keyboard: false })
})

/** Show or hide the modal based on the property value changing */
watch(() => props.isShown, (toShow) => {
  if (modal.value) {
    if (toShow) {
      modal.value.show()
    } else {
      modal.value.hide()
    }
  }
})

/** Stay on this page with no changes; just close the modal */
const onStay = () => emit("close")
</script>
