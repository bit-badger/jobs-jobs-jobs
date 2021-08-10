<template>
  <div class="card">
    <div class="card-body">
      <h6 class="card-title">
          <a href="#" :class="{ 'cp-c': collapsed, 'cp-o': !collapsed }" @click.prevent="toggle">{{headerText}}</a>
      </h6>
      <slot v-if="!collapsed"></slot>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent } from 'vue'

export default defineComponent({
  name: 'CollapsePanel',
  emits: ['toggle'],
  props: {
    headerText: {
      type: String,
      default: 'Toggle'
    },
    collapsed: {
      type: Boolean,
      default: false
    }
  },
  setup (props, { emit }) {
    return {
      toggle: () => emit('toggle', !props.collapsed)
    }
  }
})
</script>

<style lang="sass" scoped>
a.cp-c,
a.cp-o
  text-decoration: none
  font-weight: bold
  color: black
a.cp-c:hover,
a.cp-o:hover
  cursor: pointer
.cp-c::before
  content: '\2b9e \00a0'
.cp-o::before
  content: '\2b9f \00a0'
</style>
