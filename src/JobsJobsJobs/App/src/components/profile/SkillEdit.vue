<template>
  <div class="row pb-3">
    <div class="col-2 col-md-1 align-self-center">
      <button class="btn btn-sm btn-outline-danger rounded-pill" title="Delete"
              @click.prevent="$emit('remove')">&nbsp;&minus;&nbsp;</button>
    </div>
    <div class="col-10 col-md-6">
      <div class="form-floating">
        <input type="text" :id="`skillDesc${skill.id}`" class="form-control" maxlength="200"
               placeholder="A skill (language, design technique, process, etc.)" :value="skill.description"
               @input="updateValue('description', $event.target.value)">
        <label class="jjj-label" :for="`skillDesc${skill.id}`">Skill</label>
      </div>
      <div class="form-text">A skill (language, design technique, process, etc.)</div>
    </div>
    <div class="col-12 col-md-5">
      <div class="form-floating">
        <input class="form-control" type="text" :id="`skillNotes${skill.id}`" maxlength="1000"
               placeholder="A further description of the skill (100 characters max)" :value="skill.notes"
               @input="updateValue('notes', $event.target.value)">
        <label class="jjj-label" :for="`skillNotes${skill.id}`">Notes</label>
      </div>
      <div class="form-text">A further description of the skill</div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { Ref, ref } from "vue"
import { Skill } from "@/api"

const props = defineProps<{
  modelValue: Skill
}>()

const emit = defineEmits<{
  (e: "input") : void
  (e: "remove") : void
  (e: "update:modelValue", value: Skill) : void
}>()

/** The skill being edited */
const skill : Ref<Skill> = ref({ ...props.modelValue as Skill })

/** Update a value in the model */
const updateValue = (key : string, value : string) => {
  skill.value = { ...skill.value, [key]: value }
  emit("update:modelValue", skill.value)
  emit("input")
}
</script>
