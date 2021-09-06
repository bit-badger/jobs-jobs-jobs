<template lang="pug">
article
  page-title(title="Terms of Service")
  h3 Terms of Service
  p: em (as of September 6#[sup th], 2021)

  h4 Acceptance of Terms
  p.
    By accessing this web site, you are agreeing to be bound by these Terms and Conditions, and that you are responsible
    to ensure that your use of this site complies with all applicable laws. Your continued use of this site implies your
    acceptance of these terms.

  h4 Description of Service and Registration
  p
    | Jobs, Jobs, Jobs is a service that allows individuals to enter and amend employment profiles, restricting access
    | to the details of these profiles to other users of
    = " "
    template(v-for="(it, idx) in instances" :key="idx")
      a(:href="it.url" target="_blank") {{it.name}}
      template(v-if="idx + 2 < instances.length")= ", "
      template(v-else-if="idx + 1 < instances.length")= ", and "
      template(v-else)= ". "
    | Registration is accomplished by allowing Jobs, Jobs, Jobs to read one&rsquo;s Mastodon profile. See our
    = " "
    router-link(to="/privacy-policy") privacy policy
    = " "
    | for details on the personal (user) information we maintain.

  h4 Liability
  p.
    This service is provided &ldquo;as is&rdquo;, and no warranty (express or implied) exists. The service and its
    developers may not be held liable for any damages that may arise through the use of this service.

  h4 Updates to Terms
  p.
    These terms and conditions may be updated at any time. When these terms are updated, users will be notified via a
    notice on the dashboard page. Additionally, the date at the top of this page will be updated, and any substantive
    updates will also be accompanied by a summary of those changes.

  hr

  p.
    You may also wish to review our #[router-link(to="/privacy-policy") privacy policy] to learn how we handle your
    data.

  hr

  p: em.
    Change on September 6#[sup th], 2021 &ndash; replaced &ldquo;No Agenda Social&rdquo; with a list of all No
    Agenda-affiliated Mastodon instances.
</template>

<script setup lang="ts">
import { onMounted, Ref, ref } from "vue"

import api, { Instance } from "@/api"
import { toastError } from "@/components/layout/AppToaster.vue"

const instances : Ref<Instance[]> = ref([])

onMounted(async () => {
  const apiResp = await api.instances.all()
  if (typeof apiResp === "string") {
    toastError(apiResp, "retrieving instances")
  } else if (typeof apiResp === "undefined") {
    toastError("No instances to display", undefined)
  } else {
    instances.value = apiResp
  }
})
</script>
