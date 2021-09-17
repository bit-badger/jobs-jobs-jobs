<template lang="pug">
form.container
  .row
    .col-xs-12.col-sm-6.col-md-4.col-lg-3
      continent-list(v-model="criteria.continentId" topLabel="Any" @update:modelValue="updateContinent")
    .col-xs-12.col-sm-6.col-offset-md-2.col-lg-3.col-offset-lg-0
      label.jjj-label Seeking Remote Work?
      br
      .form-check.form-check-inline
        input.form-check-input(type="radio" id="remoteNull" name="remoteWork" :checked="criteria.remoteWork === ''"
                               @click="updateValue('remoteWork', '')")
        label.form-check-label(for="remoteNull") No Selection
      .form-check.form-check-inline
        input.form-check-input(type="radio" id="remoteYes" name="remoteWork" :checked="criteria.remoteWork === 'yes'"
                               @click="updateValue('remoteWork', 'yes')")
        label.form-check-label(for="remoteYes") Yes
      .form-check.form-check-inline
        input.form-check-input(type="radio" id="remoteNo" name="remoteWork" :checked="criteria.remoteWork === 'no'"
                               @click="updateValue('remoteWork', 'no')")
        label.form-check-label(for="remoteNo") No
    .col-xs-12.col-sm-6.col-lg-3
      .form-floating
        input.form-control(type="text" id="skillSearch" placeholder="(free-form text)" :value="criteria.skill"
                           @input="updateValue('skill', $event.target.value)")
        label(for="skillSearch") Skill
      .form-text (free-form text)
    .col-xs-12.col-sm-6.col-lg-3
      .form-floating
        input.form-control(type="text" id="bioSearch" placeholder="(free-form text)" :value="criteria.bioExperience"
                           @input="updateValue('bioExperience', $event.target.value)")
        label(for="bioSearch") Bio / Experience
      .form-text (free-form text)
  .row: .col
    br
    button.btn.btn-outline-primary(type="submit" @click.prevent="$emit('search')") Search
</template>

<script setup lang="ts">
import { ref } from "vue"
import { ProfileSearch } from "@/api"
import ContinentList from "../ContinentList.vue"

const props = defineProps<{
  modelValue: ProfileSearch
}>()

const emit = defineEmits<{
  (e: "search") : void
  (e: "update:modelValue", value : ProfileSearch) : void
}>()

/** The initial search criteria passed; this is what we'll update and emit when data changes */
const criteria = ref({ ...props.modelValue })

/** Emit a value update */
const updateValue = (key : string, value : string) => {
  criteria.value = { ...criteria.value, [key]: value }
  emit("update:modelValue", criteria.value)
}

/** Update the continent ID */
const updateContinent = (c : string) => updateValue("continentId", c)
</script>
