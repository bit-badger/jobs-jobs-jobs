<template>
  <form class="container">
    <div class="row">
      <div class="col col-xs-12 col-sm-6 col-md-4 col-lg-3">
        <continent-list v-model="criteria.continentId" topLabel="Any"
                        @update:modelValue="(c) => updateValue('continentId', c)" />
      </div>
      <div class="col col-xs-12 col-sm-6 col-lg-3">
        <div class="form-floating">
          <input type="text" id="regionSearch" class="form-control" placeholder="(free-form text)"
                 :value="criteria.region" @input="updateValue('region', $event.target.value)">
          <label for="regionSearch" class="jjj-label">Region</label>
        </div>
        <div class="form-text">(free-form text)</div>
      </div>
      <div class="col col-xs-12 col-sm-6 col-offset-md-2 col-lg-3 col-offset-lg-0">
        <label class="jjj-label">Remote Work Opportunity?</label><br>
        <div class="form-check form-check-inline">
          <input type="radio" id="remoteNull" name="remoteWork" class="form-check-input"
                  :checked="criteria.remoteWork === ''" @click="updateValue('remoteWork', '')">
          <label for="remoteNull" class="form-check-label">No Selection</label>
        </div>
        <div class="form-check form-check-inline">
          <input type="radio" id="remoteYes" name="remoteWork" class="form-check-input"
                  :checked="criteria.remoteWork === 'yes'" @click="updateValue('remoteWork', 'yes')">
          <label for="remoteYes" class="form-check-label">Yes</label>
        </div>
        <div class="form-check form-check-inline">
          <input type="radio" id="remoteNo" name="remoteWork" class="form-check-input"
                  :checked="criteria.remoteWork === 'no'" @click="updateValue('remoteWork', 'no')">
          <label for="remoteNo" class="form-check-label">No</label>
        </div>
      </div>
      <div class="col col-xs-12 col-sm-6 col-lg-3">
        <div class="form-floating">
          <input type="text" id="textSearch" class="form-control" placeholder="(free-form text)"
                 :value="criteria.text" @input="updateValue('text', $event.target.value)">
          <label for="textSearch" class="jjj-label">Job Listing Text</label>
        </div>
        <div class="form-text">(free-form text)</div>
      </div>
    </div>
    <div class="row">
      <div class="col col-xs-12">
        <br>
        <button type="submit" class="btn btn-outline-primary" @click.prevent="$emit('search')">Search</button>
      </div>
    </div>
  </form>
</template>

<script lang="ts">
import { ListingSearch } from '@/api'
import { defineComponent, ref, Ref } from 'vue'
import ContinentList from './ContinentList.vue'

export default defineComponent({
  name: 'ListingSearchForm',
  components: { ContinentList },
  props: {
    modelValue: {
      type: Object,
      required: true
    }
  },
  emits: ['search', 'update:modelValue'],
  setup (props, { emit }) {
    /** The initial search criteria passed; this is what we'll update and emit when data changes */
    const criteria : Ref<ListingSearch> = ref({ ...props.modelValue as ListingSearch })

    return {
      criteria,
      updateValue: (key : string, value : string) => {
        criteria.value = { ...criteria.value, [key]: value }
        emit('update:modelValue', criteria.value)
      }
    }
  }
})
</script>
