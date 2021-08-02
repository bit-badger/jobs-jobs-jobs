<template>
  <form Model=@Criteria OnValidSubmit=@OnSearch>
    <v-container>
      <v-row>
        <v-col cols="12" sm="6" md="4" lg="3">
          <label for="continentId" class="jjj-label">Continent</label>
          <select id="continentId" class="form-control form-control-sm"
                  :value="criteria.continentId" @change="updateValue('continentId', $event.target.value)">
            <option value="">&ndash; Any &ndash;</option>
            <option v-for="c in continents" :key="c.id" :value="c.id">{{c.name}}</option>
          </select>
        </v-col>
        <v-col cols="12" sm="6" offset-md="2" lg="3" offset-lg="0">
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
        </v-col>
        <v-col cols="12" sm="6" lg="3">
          <label for="skillSearch" class="jjj-label">Skill</label>
          <input type="text" id="skillSearch" class="form-control form-control-sm" placeholder="(free-form text)"
                 :value="criteria.skill" @input="updateValue('skill', $event.target.value)">
        </v-col>
        <v-col cols="12" sm="6" lg="3">
          <label for="bioSearch" class="jjj-label">Bio / Experience</label>
          <input type="text" id="bioSearch" class="form-control form-control-sm" placeholder="(free-form text)"
                 :value="criteria.bioExperience" @input="updateValue('bioExperience', $event.target.value)">
        </v-col>
      </v-row>
      <v-row class="form-row">
        <v-col cols="12">
          <br>
          <v-btn type="submit" color="primary" variant="outlined" @click.prevent="$emit('search')">Search</v-btn>
        </v-col>
      </v-row>
    </v-container>
  </form>
</template>

<script lang="ts">
import { computed, defineComponent, onMounted } from 'vue'
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
    const criteria : ProfileSearch = { ...props.modelValue as ProfileSearch }

    /** Make sure we have continents */
    onMounted(async () => await store.dispatch('ensureContinents'))

    return {
      criteria,
      continents: computed(() => store.state.continents),
      updateValue: (key : string, value : string) => emit('update:modelValue', { ...criteria, [key]: value })
    }
  }
})
</script>
