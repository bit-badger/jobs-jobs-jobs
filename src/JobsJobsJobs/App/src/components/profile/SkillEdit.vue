<template>
  <v-row>
    <v-col cols="2" md="1">
      <br>
      <v-btn color="danger" variant="outlined" title="Delete" @click="$emit('remove')">&minus;</v-btn>
    </v-col>
    <v-col cols="10" md="6">
      <label :for="`skillDesc${skill.id}`" class="jjj-label">Skill</label>
      <input type="text" :id="`skillDesc${skill.id}`" maxlength="100"
             placeholder="A skill (language, design technique, process, etc.)"
             :value="skill.description" @input="updateValue('description', $event.target.value)">
    </v-col>
    <v-col cols="12" md="5">
      <label :for="`skillNotes${skill.id}`" class="jjj-label">Notes</label>
      <input type="text" :id="`skillNotes${skill.id}`" maxlength="100"
             placeholder="A further description of the skill (100 characters max)"
             :value="skill.notes" @input="updateValue('notes', $event.target.value)">
    </v-col>
  </v-row>
</template>

<script lang="ts">
import { defineComponent } from 'vue'
import { Skill } from '@/api'

export default defineComponent({
  name: 'ProfileSkillEdit',
  props: {
    modelValue: {
      type: Object,
      required: true
    }
  },
  emits: ['remove', 'update:modelValue'],
  setup (props, { emit }) {
    /** The skill being edited */
    const skill : Skill = { ...props.modelValue as Skill }

    return {
      skill,
      updateValue: (key : string, value : string) => emit('update:modelValue', { ...skill, [key]: value })
    }
  }
})
</script>
