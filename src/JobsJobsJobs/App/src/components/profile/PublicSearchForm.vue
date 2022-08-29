<template>
  <form class="container">
    <div class="row">
      <div class="col-12 col-sm-6 col-md-4 col-lg-3">
        <continent-list v-model="criteria.continentId" topLabel="Any" @update:modelValue="updateContinent" />
      </div>
      <div class="col-12 col-sm-6 col-md-4 col-lg-3">
        <div class="form-floating">
          <input type="text" id="region" class="form-control form-control-sm" maxlength="1000"
                 placeholder="(free-form text)" :value="criteria.region"
                 @input="updateValue('region', $event.target.value)">
          <label for="region">Region</label>
        </div>
        <div class="form-text">(free-form text)</div>
      </div>
      <div class="col-12 col-sm-6 col-offset-md-2 col-lg-3 col-offset-lg-0">
        <label class="jjj-label">Seeking Remote Work?</label><br>
        <div class="form-check form-check-inline">
          <input type="radio" id="remoteNull" class="form-check-input" name="remoteWork"
                 :checked="criteria.remoteWork === ''" @click="updateValue('remoteWork', '')">
          <label class="form-check-label" for="remoteNull">No Selection</label>
        </div>
        <div class="form-check form-check-inline">
          <input type="radio" id="remoteYes" class="form-check-input" name="remoteWork"
                 :checked="criteria.remoteWork === 'yes'" @click="updateValue('remoteWork', 'yes')">
          <label class="form-check-label" for="remoteYes">Yes</label>
        </div>
        <div class="form-check form-check-inline">
          <input type="radio" id="remoteNo" class="form-check-input" name="remoteWork"
                 :checked="criteria.remoteWork === 'no'" @click="updateValue('remoteWork', 'no')">
          <label class="form-check-label" for="remoteNo">No</label>
        </div>
      </div>
      <div class="col-12 col-sm-6 col-lg-3">
        <div class="form-floating">
          <input type="text" id="skillSearch" class="form-control form-control-sm" maxlength="1000"
                 placeholder="(free-form text)" :value="criteria.skill"
                 @input="updateValue('skill', $event.target.value)">
          <label for="skillSearch">Skill</label>
        </div>
        <div class="form-text">(free-form text)</div>
      </div>
    </div>
    <div class="row">
      <div class="col">
        <br>
        <button class="btn btn-outline-primary" type="submit" @click.prevent="$emit('search')">Search</button>
      </div>
    </div>
  </form>
</template>

<script setup lang="ts">
import { ref } from "vue"
import { PublicSearch } from "@/api"
import ContinentList from "../ContinentList.vue"

const props = defineProps<{
  modelValue: PublicSearch
}>()

const emit = defineEmits<{
  (e: "search") : void
  (e: "update:modelValue", value : PublicSearch) : void
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
