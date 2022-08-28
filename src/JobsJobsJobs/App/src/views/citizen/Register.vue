<template>
  <article>
    <h3 class="pb-3">Register</h3>
    <form class="row g-3">
      <div class="col-6">
        <div class="form-floating">
          <input class="form-control" type="text" id="firstName" v-model="v$.firstName.$model" placeholder="First Name">
          <div id="firstNameFeedback" class="invalid-feedback">Please enter your first name</div>
          <label class="jjj-required" for="firstName">First Name</label>
        </div>
      </div>
      <div class="col-6">
        <div class="form-floating">
          <input class="form-control" type="text" id="lastName" v-model="v$.lastName.$model" placeholder="Last Name">
          <div id="lastNameFeedback" class="invalid-feedback">Please enter your last name</div>
          <label class="jjj-required" for="firstName">Last Name</label>
        </div>
      </div>
      <div class="col-6">
        <div class="form-floating">
          <input class="form-control" type="text" id="displayName" v-model="v$.displayName.$model"
                 placeholder="Display Name">
          <label for="displayName">Display Name</label>
          <div class="form-text"><em>Optional; overrides "FirstName LastName"</em></div>
        </div>
      </div>
      <div class="col-6">
        <div class="form-floating">
          <input class="form-control" type="email" id="email" v-model="v$.email.$model" placeholder="E-mail Address">
          <div id="emailFeedback" class="invalid-feedback">Please enter a valid e-mail address</div>
          <label class="jjj-required" for="email">E-mail Address</label>
        </div>
      </div>
      <div class="col-6">
        <div class="form-floating">
          <input class="form-control" type="password" id="password" v-model="v$.password.$model" placeholder="Password"
                 minlength="8">
          <div id="passwordFeedback" class="invalid-feedback">Please enter a password at least 8 characters long</div>
          <label class="jjj-required" for="password">Password</label>
        </div>
      </div>
      <div class="col-6">
        <div class="form-floating">
          <input class="form-control" type="password" id="confirmPassword" v-model="v$.confirmPassword.$model"
                 placeholder="Confirm Password">
          <div id="confirmPasswordFeedback" class="invalid-feedback">The passwords do not match</div>
          <label class="jjj-required" for="confirmPassword">Confirm Password</label>
        </div>
      </div>
    </form>
  </article>
</template>

<script setup lang="ts">
import { computed, ref, reactive } from "vue"
import api, { Citizen, CitizenRegistrationForm, LogOnSuccess, Profile } from "@/api"
import useVuelidate from "@vuelidate/core"
import { email, minLength, required, sameAs } from "@vuelidate/validators"
import { toastError, toastSuccess } from "@/components/layout/AppToaster.vue"
import { useStore } from "@/store"

const store = useStore()

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

</script>

<style scoped>

</style>
