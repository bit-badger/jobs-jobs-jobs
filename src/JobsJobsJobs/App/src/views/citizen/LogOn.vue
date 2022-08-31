<template>
  <article>
    <h3 class="pb-3">Log On</h3>
    <p v-if="message !== ''" class="pb-3 text-center">
      <span class="text-danger">{{message}}</span><br>
      <template v-if="message.indexOf('ocked') > -1">
        If this is a new account, it must be confirmed before it can be used; otherwise, you need to
        <router-link to="/citizen/forgot-password">request an unlock code</router-link> before you may log on.
      </template>
    </p>
    <form class="row g-3 pb-3">
      <div class="col-12 col-md-6">
        <div class="form-floating">
          <div class="form-floating">
            <input type="email" id="email" :class="{ 'form-control': true, 'is-invalid': v$.email.$error }"
                   v-model="v$.email.$model" placeholder="E-mail Address">
            <div class="invalid-feedback">Please enter a valid e-mail address</div>
            <label class="jjj-required" for="email">E-mail Address</label>
          </div>
        </div>
      </div>
      <div class="col-12 col-md-6">
        <div class="form-floating">
          <input type="password" id="password" :class="{ 'form-control': true, 'is-invalid': v$.password.$error }"
                 v-model="v$.password.$model" placeholder="Password">
          <div class="invalid-feedback">Please enter a password</div>
          <label class="jjj-required" for="password">Password</label>
        </div>
      </div>
      <div class="col-12">
        <p class="text-danger" v-if="v$.$error">Please correct the errors above</p>
        <button class="btn btn-primary" @click.prevent="logOn" :disabled="!logOnEnabled">
          <icon :icon="mdiLogin" />&nbsp; Log On
        </button>
      </div>
    </form>
    <p class="text-center">Need an account? <router-link to="/citizen/register">Register for one!</router-link></p>
    <p class="text-center">
      Forgot your password? <router-link to="/citizen/forgot-password">Request a reset.</router-link>
    </p>
  </article>
</template>

<script setup lang="ts">
import { computed, reactive, ref } from "vue"
import { useRouter } from "vue-router"
import { mdiLogin } from "@mdi/js"
import useVuelidate from "@vuelidate/core"
import { email, required } from "@vuelidate/validators"

import { LogOnForm } from "@/api"
import { toastSuccess } from "@/components/layout/AppToaster.vue"
import { AFTER_LOG_ON_URL } from "@/router"
import { useStore, Actions } from "@/store"

const store = useStore()
const router = useRouter()

/** The form to log on to Jobs, Jobs, Jobs */
const logOnForm = reactive(new LogOnForm())

/** Whether the log on button is enabled */
const logOnEnabled = ref(true)

/** The message returned from the log on attempt */
const message = computed(() => store.state.logOnState)

/** Validation rules for the log on form */
const rules = computed(() => ({
  email: { required, email },
  password: { required }
}))

/** Form and validation */
const v$ = useVuelidate(rules, logOnForm, { $lazy: true })

/** Log the citizen on */
const logOn = async () => {
  v$.value.$touch()
  if (v$.value.$error) return
  logOnEnabled.value = false
  await store.dispatch(Actions.LogOn, { form: logOnForm })
  logOnEnabled.value = true
  if (store.state.user !== undefined) {
    toastSuccess("Log On Successful")
    v$.value.$reset()
    const nextUrl = window.localStorage.getItem(AFTER_LOG_ON_URL) ?? "/citizen/dashboard"
    window.localStorage.removeItem(AFTER_LOG_ON_URL)
    await router.push(nextUrl)
  }
}
</script>
