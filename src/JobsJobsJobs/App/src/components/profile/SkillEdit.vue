<template lang="pug">
.row.pb-3
  .col.col-xs-2.col-md-1.align-self-center
    button.btn.btn-sm.btn-outline-danger.rounded-pill(title='Delete' @click.prevent="$emit('remove')") &nbsp;&minus;&nbsp;
  .col.col-xs-10.col-md-6
    .form-floating
      input.form-control(type='text' :id='`skillDesc${skill.id}`' maxlength='100'
                         placeholder='A skill (language, design technique, process, etc.)' :value='skill.description'
                         @input="updateValue('description', $event.target.value)")
      label.jjj-label(:for='`skillDesc${skill.id}`') Skill
    .form-text A skill (language, design technique, process, etc.)
  .col.col-xs-12.col-md-5
    .form-floating
      input.form-control(type='text' :id='`skillNotes${skill.id}`' maxlength='100'
                         placeholder='A further description of the skill (100 characters max)' :value='skill.notes'
                         @input="updateValue('notes', $event.target.value)")
      label.jjj-label(:for='`skillNotes${skill.id}`') Notes
    .form-text A further description of the skill (100 characters max)
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
