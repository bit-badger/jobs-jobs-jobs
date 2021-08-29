<template lang="pug">
article
  p &nbsp;
  p.fst-italic Sending you over to No Agenda Social to log on; see you back in just a second&hellip;
</template>

<script setup lang="ts">
/**
 * This component simply redirects the user to the No Agenda Social authorization page; it is separate here so that it
 * can be called from two different places, and allow the app to support direct links to authorized content.
 */

/** The authorization URL to which the user should be directed */
const authUrl = (() => {
  /** The client ID for Jobs, Jobs, Jobs at No Agenda Social */
  const id = "k_06zlMy0N451meL4AqlwMQzs5PYr6g3d2Q_dCT-OjU"
  const client = `client_id=${id}`
  const scope = "scope=read:accounts"
  const redirect = `redirect_uri=${document.location.origin}/citizen/authorized`
  const respType = "response_type=code"
  return `https://noagendasocial.com/oauth/authorize?${client}&${scope}&${redirect}&${respType}`
})()
document.location.assign(authUrl)
</script>
