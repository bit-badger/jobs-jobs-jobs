<template>
  <article>
    <h3 class="pb-3">Register</h3>
    <form class="row g-3">
      <div class="col-6 col-xl-4">
        <div class="form-floating">
          <input class="form-control" type="text" id="firstName" v-model="v$.firstName.$model" placeholder="First Name">
          <div v-if="v$.firstName.$error" class="text-danger">Please enter your first name</div>
          <label class="jjj-required" for="firstName">First Name</label>
        </div>
      </div>
      <div class="col-6 col-xl-4">
        <div class="form-floating">
          <input class="form-control" type="text" id="lastName" v-model="v$.lastName.$model" placeholder="Last Name">
          <div v-if="v$.lastName.$error" class="text-danger">Please enter your last name</div>
          <label class="jjj-required" for="firstName">Last Name</label>
        </div>
      </div>
      <div class="col-6 col-xl-4">
        <div class="form-floating">
          <input class="form-control" type="text" id="displayName" v-model="v$.displayName.$model"
                 placeholder="Display Name">
          <label for="displayName">Display Name</label>
          <div class="form-text"><em>Optional; overrides "FirstName LastName"</em></div>
        </div>
      </div>
      <div class="col-6 col-xl-4">
        <div class="form-floating">
          <input class="form-control" type="email" id="email" v-model="v$.email.$model" placeholder="E-mail Address">
          <div v-if="v$.email.$error" class="text-danger">Please enter a valid e-mail address</div>
          <label class="jjj-required" for="email">E-mail Address</label>
        </div>
      </div>
      <div class="col-6 col-xl-4">
        <div class="form-floating">
          <input class="form-control" type="password" id="password" v-model="v$.password.$model" placeholder="Password"
                 minlength="8">
          <div v-if="v$.password.$error" class="text-danger">Please enter a password at least 8 characters long</div>
          <label class="jjj-required" for="password">Password</label>
        </div>
      </div>
      <div class="col-6 col-xl-4">
        <div class="form-floating">
          <input class="form-control" type="password" id="confirmPassword" v-model="v$.confirmPassword.$model"
                 placeholder="Confirm Password">
          <div v-if="v$.confirmPassword.$error" class="text-danger">The passwords do not match</div>
          <label class="jjj-required" for="confirmPassword">Confirm Password</label>
        </div>
      </div>
      <div class="col-12">
        <p class="text-danger" v-if="v$.$error">Please correct the errors above</p>
        <button class="btn btn-primary" @click.prevent="saveProfile">
          <icon :icon="mdiContentSaveOutline" />&nbsp; Save
        </button>
      </div>
    </form>
  </article>
</template>

<script setup lang="ts">
import { computed, reactive } from "vue"
import { useRouter } from "vue-router"
import { mdiContentSaveOutline } from "@mdi/js"
import useVuelidate from "@vuelidate/core"
import { email, minLength, required, sameAs } from "@vuelidate/validators"

import api, { CitizenRegistrationForm } from "@/api"
import { toastError, toastSuccess } from "@/components/layout/AppToaster.vue"

const router = useRouter()

/** The information required to register a user */
const regForm = reactive(new CitizenRegistrationForm())

/** The validation rules for the form */
const rules = computed(() => ({
  firstName: { required },
  lastName: { required },
  email: { required, email },
  displayName: { },
  password: { required, length: minLength(8) },
  confirmPassword: { required, matchPassword: sameAs(regForm.password) }
}))

/** Initialize form validation */
const v$ = useVuelidate(rules, regForm, { $lazy: true })

/** Register the citizen */
const saveProfile = async () => {
  v$.value.$touch()
  if (v$.value.$error) return
  const registerResult = await api.citizen.register(regForm)
  if (typeof registerResult === "string") {
    toastError(registerResult, "registering")
  } else {
    toastSuccess("Registered Successfully")
    v$.value.$reset()
    await router.push("/citizen/registered")
  }
}
</script>
