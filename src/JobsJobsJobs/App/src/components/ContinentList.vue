<template>
  <div class="form-floating">
    <select id="continentId" :class="{ 'form-select': true, 'is-invalid': isInvalid}" :value="continentId"
            @change="continentChanged">
      <option value="">&ndash; {{emptyLabel}} &ndash;</option>
      <option v-for="c in continents" :key="c.id" :value="c.id">{{c.name}}</option>
    </select>
    <label class="jjj-required" for="continentId">Continent</label>
  </div>
  <div class="invalid-feedback">Please select a continent</div>
</template>

<script setup lang="ts">
import { useStore } from "@/store"
import { computed, onMounted, ref } from "vue"

interface Props {
  modelValue: string
  topLabel?: string
  isInvalid?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  isInvalid: false
})

const emit = defineEmits<{
  (e: "update:modelValue", value : string) : void
  (e: "touch") : void
}>()

const store = useStore()

/** The continent ID, which this component can change */
const continentId = ref(props.modelValue)

/**
 * Mark the continent field as changed
 *
 * (This works around a really strange sequence where, if the "touch" call is directly wired up to the onChange event,
 * the first time a value is selected, it doesn't stick (although the field is marked as touched). On second and
 * subsequent times, it worked. The solution here is to grab the value and update the reactive source for the form, then
 * manually set the field to touched; this restores the expected behavior. This is probably why the library doesn't hook
 * into the onChange event to begin with...)
 */
const continentChanged = (e : Event) : boolean => {
  continentId.value = (e.target as HTMLSelectElement).value
  emit("touch")
  emit("update:modelValue", continentId.value)
  return true
}

onMounted(async () => await store.dispatch("ensureContinents"))

/** Accessor for the continent list */
const continents = computed(() => store.state.continents)

/** The label to use for the top entry in the list */
const emptyLabel = props.topLabel ?? "Select"
</script>
