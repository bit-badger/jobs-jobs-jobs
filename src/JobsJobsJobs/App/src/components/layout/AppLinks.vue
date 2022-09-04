<template>
  <nav>
    <template v-if="isLoggedOn">
      <router-link to="/citizen/dashboard" @click="hide">
        <icon :icon="mdiViewDashboardVariant" />&nbsp; Dashboard
      </router-link>
      <router-link to="/help-wanted" @click="hide">
        <icon :icon="mdiNewspaperVariantMultipleOutline" />&nbsp; Help Wanted!
      </router-link>
      <router-link to="/profile/search" @click="hide">
        <icon :icon="mdiViewListOutline" />&nbsp; Employment Profiles
      </router-link>
      <router-link to="/success-story/list" @click="hide">
        <icon :icon="mdiThumbUp" />&nbsp; Success Stories
      </router-link>
      <div class="separator"></div>
      <router-link to="/citizen/account" @click="hide">
        <icon :icon="mdiAccountEdit" /> My Account
      </router-link>
      <router-link to="/listings/mine" @click="hide">
        <icon :icon="mdiSignText" />&nbsp; My Job Listings
      </router-link>
      <router-link to="/profile/edit" @click="hide">
        <icon :icon="mdiPencil" />&nbsp; My Employment Profile
      </router-link>
      <div class="separator"></div>
      <router-link to="/citizen/log-off" @click="hide">
        <icon :icon="mdiLogoutVariant" />&nbsp; Log Off
      </router-link>
    </template>
    <template v-else>
      <router-link to="/" @click="hide">
        <icon :icon="mdiHome" />&nbsp; Home
      </router-link>
      <router-link to="/profile/seeking" @click="hide">
        <icon :icon="mdiViewListOutline" />&nbsp; Job Seekers
      </router-link>
      <router-link to="/citizen/log-on" @click="hide">
        <icon :icon="mdiLoginVariant" />&nbsp; Log On
      </router-link>
    </template>
    <router-link to="/how-it-works" @click="hide">
      <icon :icon="mdiHelpCircleOutline" />&nbsp; How It Works
    </router-link>
  </nav>
</template>

<script setup lang="ts">
import { computed } from "vue"
import { useRouter } from "vue-router"
import { Offcanvas } from "bootstrap"
import {
  mdiAccountEdit,
  mdiHelpCircleOutline,
  mdiHome,
  mdiLoginVariant,
  mdiLogoutVariant,
  mdiNewspaperVariantMultipleOutline,
  mdiPencil,
  mdiSignText,
  mdiThumbUp,
  mdiViewDashboardVariant,
  mdiViewListOutline
} from "@mdi/js"

import { useStore } from "@/store"

import Icon from "@/components/Icon.vue"

const store = useStore()
const router = useRouter()

/** Whether a user is logged in or not */
const isLoggedOn = computed(() => store.state.user !== undefined)

/** The current mobile menu */
const menu = computed(() => {
  const elt = document.getElementById("mobileMenu")
  return elt ? Offcanvas.getOrCreateInstance(elt) : undefined
})

/** Hide the offcanvas menu (if it exists) when a link is clicked */
const hide = () => { if (menu.value) menu.value.hide() }
</script>

<style lang="sass" scoped>
path
  fill: white
path:hover
  fill: black
a:link, a:visited
  text-decoration: none
  color: white
nav > a
  display: block
  width: 100%
  border-radius: .25rem
  padding: .5rem
  margin: .5rem 0
  font-size: 1rem
  > i
    vertical-align: top
    margin-right: 1rem
  &.router-link-exact-active
    background-color: rgba(255, 255, 255, .2)
  &:hover
    background-color: rgba(255, 255, 255, .5)
    color: black
    text-decoration: none
nav > div.separator
  border-bottom: solid 1px rgba(255, 255, 255, .75)
  height: 1px
</style>
