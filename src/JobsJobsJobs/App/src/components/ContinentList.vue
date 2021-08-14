<template>
  <div class="form-floating">
    <select id="continentId" :class="{ 'form-select': true, 'is-invalid': isInvalid}"
            :value="continentId" @change="continentChanged">
      <option value="">&ndash; {{emptyLabel}} &ndash;</option>
      <option v-for="c in continents" :key="c.id" :value="c.id">{{c.name}}</option>
    </select>
    <label for="continentId" class="jjj-required">Continent</label>
  </div>
  <div class="invalid-feedback">Please select a continent</div>
</template>

<script lang="ts">
import { useStore } from '@/store'
import { computed, defineComponent, onMounted, ref } from 'vue'
export default defineComponent({
  name: 'ContinentList',
  props: {
    modelValue: {
      type: String,
      required: true
    },
    topLabel: { type: String },
    isInvalid: { type: Boolean }
  },
  emits: ['update:modelValue', 'touch'],
  setup (props, { emit }) {
    const store = useStore()

    /** The continent ID, which this component can change */
    const continentId = ref(props.modelValue)

    /**
     * Mark the continent field as changed
     *
     * (This works around a really strange sequence where, if the "touch" call is directly wired up to the onChange
     * event, the first time a value is selected, it doesn't stick (although the field is marked as touched). On second
     * and subsequent times, it worked. The solution here is to grab the value and update the reactive source for the
     * form, then manually set the field to touched; this restores the expected behavior. This is probably why the
     * library doesn't hook into the onChange event to begin with...)
     */
    const continentChanged = (e : Event) : boolean => {
      continentId.value = (e.target as HTMLSelectElement).value
      emit('touch')
      emit('update:modelValue', continentId.value)
      return true
    }

    onMounted(async () => await store.dispatch('ensureContinents'))

    return {
      continentId,
      continents: computed(() => store.state.continents),
      emptyLabel: props.topLabel || 'Select',
      continentChanged
    }
  }
})
</script>
