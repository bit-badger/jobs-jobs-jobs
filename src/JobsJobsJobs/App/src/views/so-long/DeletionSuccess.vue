<template lang="pug">
article
  page-title(title="Account Deletion Success")
  h3.pb-3 Account Deletion Success
  p.
    Your account has been successfully deleted. To revoke the permissions you have previously granted to this
    application, find it in #[a(:href="`${url}/oauth/authorized_applications`") this list] and click
    #[strong &times; Revoke]. Otherwise, clicking &ldquo;Log On&rdquo; in the left-hand menu will create a new, empty
    account without prompting you further.
  p Thank you for participating, and thank you for your courage. #GitmoNation
</template>

<script setup lang="ts">
import { useRoute } from "vue-router"

const route = useRoute()

/** The URL of the instance from which the deleted user had authorized access */
const url = route.params.url as string

</script>
