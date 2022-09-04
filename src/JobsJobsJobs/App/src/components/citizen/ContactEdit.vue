<template>
  <div class="row pb-3">
    <div class="col-2 col-md-1 align-self-center">
      <button class="btn btn-sm btn-outline-danger rounded-pill" title="Delete"
              @click.prevent="$emit('remove')">&nbsp;&minus;&nbsp;</button>
    </div>
    <div class="col-10 col-md-6">
      <div class="form-floating">
        <select :id="`contactType${contact.id}`" class="form-control" :value="contact.contactType"
                @input="updateValue('contactType', $event.target.value)">
          <option value="Website">Website</option>
          <option value="Email">E-mail Address</option>
          <option value="Phone">Phone Number</option>
        </select>
        <label class="jjj-label" :for="`contactType${contact.id}`">Type</label>
      </div>
    </div>
    <div class="col-12 col-md-5">
      <div class="form-floating">
        <input type="text" :id="`contactName${contact.id}`" class="form-control" maxlength="1000"
               placeholder="The name of this contact" :value="contact.name"
               @input="updateValue('name', $event.target.value)">
        <label class="jjj-label" :for="`contactName${contact.id}`">Name</label>
      </div>
      <div class="form-text">Optional; will link sites and e-mail, qualify phone numbers</div>
    </div>
    <div class="col-12 col-md-5">
      <div class="form-floating">
        <input type="text" :id="`contactValue${contact.id}`" class="form-control" maxlength="1000"
               placeholder="The value forthis contact" :value="contact.value"
               @input="updateValue('value', $event.target.value)">
        <label class="jjj-label" :for="`contactValue${contact.id}`">Contact</label>
      </div>
      <div class="form-text">The URL, e-mail address, or phone number</div>
    </div>
    <div class="col-12 col-offset-md-2 col-md-4">
      <div class="form-check">
        <input type="checkbox" :id="`contactIsPublic${contact.id}`" class="form-check-input" value="true"
               :checked="contact.isPublic">
        <label class="form-check-label" :for="`contactIsPublic${contact.id}`">Public</label>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { OtherContact } from "@/api"
import { ref, Ref } from "vue"

const props = defineProps<{
  modelValue: OtherContact
}>()

const emit = defineEmits<{
  (e: "input") : void
  (e: "remove") : void
  (e: "update:modelValue", value: OtherContact) : void
}>()

/** The contact being edited */
const contact : Ref<OtherContact> = ref({ ...props.modelValue as OtherContact })

/** Update a value in the model */
const updateValue = (key : string, value : string) => {
  contact.value = { ...contact.value, [key]: value }
  emit("update:modelValue", contact.value)
  emit("input")
}

</script>

<style scoped>

</style>
