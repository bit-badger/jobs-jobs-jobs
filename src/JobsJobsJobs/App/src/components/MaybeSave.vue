<template lang="pug">
.modal.fade(id='maybeSaveModal' tabindex='-1' aria-labelledby='maybeSaveLabel' aria-hidden='true'): .modal-dialog: .modal-content
  .modal-header: h5.modal-title(id='maybeSaveLabel') Unsaved Changes
  .modal-body You have modified the data on this page since it was last saved. What would you like to do?
  .modal-footer
    button.btn.btn-secondary(type='button' @click.prevent='onStay') Stay on This Page
    button.btn.btn-primary(type='button' @click.prevent='onSave') Save Changes
    button.btn.btn-danger(type='button' @click.prevent='onDiscard') Discard Changes
</template>

<script lang="ts">
import { computed, defineComponent, onMounted, ref, Ref, watch } from 'vue'
import { RouteLocationNormalized, useRouter } from 'vue-router'
import { Validation } from '@vuelidate/core'
import { Modal } from 'bootstrap'

export default defineComponent({
  name: 'MaybeSave',
  props: {
    isShown: {
      type: Boolean,
      required: true
    },
    toRoute: {
      // Can't type this because it's not filled until just before the modal is shown
      required: true
    },
    saveAction: {
      type: Function
    },
    validator: {
      type: Object
    }
  },
  emits: ['close', 'discard', 'cancel'],
  setup (props, { emit }) {
    const router = useRouter()

    /** The route where we tried to go */
    const newRoute = computed(() => props.toRoute as RouteLocationNormalized)

    /** Reference to the modal dialog (we can't get it until the component is rendered) */
    const modal : Ref<Modal | undefined> = ref(undefined)

    /** Save changes (if required) and go to the next route */
    const onSave = async () => {
      if (props.saveAction) await Promise.resolve(props.saveAction())
      emit('close')
      router.push(newRoute.value)
    }

    /** Discard changes (if required) and go to the next route */
    const onDiscard = () => {
      if (props.validator) (props.validator as Validation).$reset()
      emit('close')
      router.push(newRoute.value)
    }

    onMounted(() => {
      modal.value = new Modal(document.getElementById('maybeSaveModal') as HTMLElement,
        { backdrop: 'static', keyboard: false })
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

    return {
      onStay: () => emit('close'),
      onSave,
      onDiscard
    }
  }
})
</script>
