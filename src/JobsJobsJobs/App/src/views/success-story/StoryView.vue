<template lang="pug">
article
  page-title(title='Success Story')
  load-data(:load='retrieveStory')
    h3.pb-3 {{citizenName}}&rsquo;s Success Story
    h4.text-muted: full-date-time(:date='story.recordedOn')
    p.fst-italic(v-if='story.fromHere'): strong Found via Jobs, Jobs, Jobs
    hr
    div(v-if='story.story' v-html='successStory')
</template>

<script lang="ts">
import { computed, defineComponent, Ref, ref } from 'vue'
import { useRoute } from 'vue-router'
import marked from 'marked'
import api, { LogOnSuccess, markedOptions, Success } from '@/api'
import { useStore } from '@/store'

import FullDateTime from '@/components/FullDateTime.vue'
import LoadData from '@/components/LoadData.vue'

export default defineComponent({
  name: 'StoryView',
  components: {
    FullDateTime,
    LoadData
  },
  setup () {
    const store = useStore()
    const route = useRoute()

    /** The currently logged-on user */
    const user = store.state.user as LogOnSuccess

    /** The story to be displayed */
    const story : Ref<Success | undefined> = ref(undefined)

    /** The citizen's name (real, display, or NAS, whichever is found first) */
    const citizenName = ref('')

    /** Retrieve the success story */
    const retrieveStory = async (errors : string []) => {
      const storyResponse = await api.success.retrieve(route.params.id as string, user)
      if (typeof storyResponse === 'string') {
        errors.push(storyResponse)
        return
      }
      if (typeof storyResponse === 'undefined') {
        errors.push('Success story not found')
        return
      }
      story.value = storyResponse
      const citResponse = await api.citizen.retrieve(story.value.citizenId, user)
      if (typeof citResponse === 'string') {
        errors.push(citResponse)
      } else if (typeof citResponse === 'undefined') {
        errors.push('Citizen not found')
      } else {
        citizenName.value = citResponse.realName || citResponse.displayName || citResponse.naUser
      }
    }

    return {
      story,
      retrieveStory,
      citizenName,
      successStory: computed(() => marked(story.value?.story || '', markedOptions))
    }
  }
})
</script>
