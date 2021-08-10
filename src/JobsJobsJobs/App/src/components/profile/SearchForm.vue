<template>
  <form class="container">
    <div class="row">
      <div class="col col-xs-12 col-sm-6 col-md-4 col-lg-3">
        <div class="form-floating">
          <select id="continentId" class="form-select"
                  :value="criteria.continentId" @change="updateValue('continentId', $event.target.value)">
            <option value="">&ndash; Any &ndash;</option>
            <option v-for="c in continents" :key="c.id" :value="c.id">{{c.name}}</option>
          </select>
          <label for="continentId">Continent</label>
        </div>
      </div>
      <div class="col col-xs-12 col-sm-6 col-offset-md-2 col-lg-3 col-offset-lg-0">
        <label class="jjj-label">Seeking Remote Work?</label><br>
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
          <input type="text" id="skillSearch" class="form-control" placeholder="(free-form text)"
                 :value="criteria.skill" @input="updateValue('skill', $event.target.value)">
          <label for="skillSearch" class="jjj-label">Skill</label>
        </div>
        <div class="form-text">(free-form text)</div>
      </div>
      <div class="col col-xs-12 col-sm-6 col-lg-3">
        <div class="form-floating">
          <input type="text" id="bioSearch" class="form-control" placeholder="(free-form text)"
                 :value="criteria.bioExperience" @input="updateValue('bioExperience', $event.target.value)">
          <label for="bioSearch" class="jjj-label">Bio / Experience</label>
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
import { computed, defineComponent, onMounted, Ref, ref } from 'vue'
import { ProfileSearch } from '@/api'
import { useStore } from '@/store'

export default defineComponent({
  name: 'ProfileSearchForm',
  props: {
    modelValue: {
      type: Object,
      required: true
    }
  },
  emits: ['search', 'update:modelValue'],
  setup (props, { emit }) {
    const store = useStore()

    /** The initial search criteria passed; this is what we'll update and emit when data changes */
    const criteria : Ref<ProfileSearch> = ref({ ...props.modelValue as ProfileSearch })

    /** Make sure we have continents */
    onMounted(async () => await store.dispatch('ensureContinents'))

    return {
      criteria,
      continents: computed(() => store.state.continents),
      updateValue: (key : string, value : string) => {
        criteria.value = { ...criteria.value, [key]: value }
        emit('update:modelValue', criteria.value)
      }
    }
  }
})
</script>
