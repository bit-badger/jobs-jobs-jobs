<template>
  <div class="row pb-3">
    <div class="col col-xs-2 col-md-1 align-self-center">
      <button class="btn btn-sm btn-outline-danger rounded-pill" title="Delete" @click.prevent="$emit('remove')">
        &nbsp;&minus;&nbsp;
      </button>
    </div>
    <div class="col col-xs-10 col-md-6">
      <div class="form-floating">
        <input type="text" :id="`skillDesc${skill.id}`" class="form-control" maxlength="100"
               placeholder="A skill (language, design technique, process, etc.)"
               :value="skill.description" @input="updateValue('description', $event.target.value)">
        <label :for="`skillDesc${skill.id}`" class="jjj-label">Skill</label>
      </div>
      <div class="form-text">A skill (language, design technique, process, etc.)</div>
    </div>
    <div class="col col-xs-12 col-md-5">
      <div class="form-floating">
        <input type="text" :id="`skillNotes${skill.id}`" class="form-control" maxlength="100"
               placeholder="A further description of the skill (100 characters max)"
               :value="skill.notes" @input="updateValue('notes', $event.target.value)">
        <label :for="`skillNotes${skill.id}`" class="jjj-label">Notes</label>
      </div>
      <div class="form-text">A further description of the skill (100 characters max)</div>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent, Ref, ref } from 'vue'
import { Skill } from '@/api'

export default defineComponent({
  name: 'ProfileSkillEdit',
  props: {
    modelValue: {
      type: Object,
      required: true
    }
  },
  emits: ['input', 'remove', 'update:modelValue'],
  setup (props, { emit }) {
    /** The skill being edited */
    const skill : Ref<Skill> = ref({ ...props.modelValue as Skill })

    return {
      skill,
      updateValue: (key : string, value : string) => {
        skill.value = { ...skill.value, [key]: value }
        emit('update:modelValue', skill.value)
        emit('input')
      }
    }
  }
})
</script>
