<template>
  <article>
    <page-title title="Welcome!" />
    <p>
      Welcome to Jobs, Jobs, Jobs (AKA No Agenda Careers), where citizens of Gitmo Nation can assist one another in
      finding employment. This will enable them to continue providing value-for-value to Adam and John, as they continue
      their work deconstructing the misinformation that passes for news on a day-to-day basis.
    </p>
    <p>
      Do you not understand the terms in the paragraph above? No worries; just head over to
      <a href="https://noagendashow.net" target="_blank">The Best Podcast in the Universe</a>
      <em><audio-clip clip="thats-true"> (that&rsquo;s true!)</audio-clip></em> and find out what you&rsquo;re missing.
    </p>
  </article>
</template>

<script lang="ts">
import { defineComponent } from 'vue'
import AudioClip from '../components/shared/AudioClip.vue'

export default defineComponent({
  name: 'Home',
  components: {
    AudioClip
  }
})
</script>
