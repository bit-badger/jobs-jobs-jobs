<template>
  <div class="hello">
    <h1>
      Jobs, Jobs, Jobs<br>
      <small><small><small><em>
        (and Jobs - <a class="audio" @click="playJobs">Let's Vote for Jobs!</a>)
        </em></small></small></small>
    </h1>
    <p>
      Future home of No Agenda Jobs, where citizens of Gitmo Nation can assist one another in finding or enhancing
      their employment. This will enable them to continue providing value for value to Adam and John, as they continue
      their work deconstructing the misinformation that passes for news on a day-to-day basis.
    </p>
    <p>
      Do you not understand the terms in the paragraph above? No worries; just head over to
      <a href="https://noagendashow.net">The Best Podcast in the Universe</a> <em><a class="audio" @click="playTrue">(it's true!)</a></em> and find out what
      you&rsquo;re missing.
    </p>
    <audio ref="jobsAudio">
      <source src="/pelosi-jobs.mp3">
    </audio>
    <audio ref="trueAudio">
      <source src="/thats-true.mp3">
    </audio>
  </div>
</template>

<script lang="ts">
import { ref } from 'vue'

export default {
  props: {
    msg: String
  },
  setup() {
    const jobsAudio = ref(null)
    const trueAudio = ref(null)

    const playJobs = () => jobsAudio.value.play()
    const playTrue = () => trueAudio.value.play()
    return {
      jobsAudio,
      trueAudio,
      playJobs,
      playTrue
    }
  },
}
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>

h3 {
  margin: 40px 0 0;
}
ul {
  list-style-type: none;
  padding: 0;
}
li {
  display: inline-block;
  margin: 0 10px;
}
a {
  color: #42b983;
}
a.audio {
  color: inherit;
  border-bottom: dotted 1px lightgray;
}
a.audio:hover {
  cursor: pointer;
}
</style>
