<template>
  <article>
    <h3 class="pb-3">Account Profile</h3>
    <p>
      This information is visible to all fellow logged-on citizens. For publicly-visible employment profiles and job
      listings, the &ldquo;Display Name&rdquo; fields and any public contacts will be displayed.
    </p>
    <load-data :load="retrieveData">
      <form class="row g-3" novalidate>
        <div class="col-6 col-xl-4">
          <div class="form-floating has-validation">
            <input type="text" id="firstName" :class="{ 'form-control': true, 'is-invalid': v$.firstName.$error }"
                   v-model="v$.firstName.$model" placeholder="First Name">
            <div class="invalid-feedback">Please enter your first name</div>
            <label class="jjj-required" for="firstName">First Name</label>
          </div>
        </div>
        <div class="col-6 col-xl-4">
          <div class="form-floating">
            <input type="text" id="lastName" :class="{ 'form-control': true, 'is-invalid': v$.lastName.$error }"
                   v-model="v$.lastName.$model" placeholder="Last Name">
            <div class="invalid-feedback">Please enter your last name</div>
            <label class="jjj-required" for="firstName">Last Name</label>
          </div>
        </div>
        <div class="col-6 col-xl-4">
          <div class="form-floating">
            <input type="text" id="displayName" class="form-control" v-model="v$.displayName.$model"
                   placeholder="Display Name">
            <label for="displayName">Display Name</label>
            <div class="form-text"><em>Optional; overrides first/last for display</em></div>
          </div>
        </div>
        <div class="col-6 col-xl-4">
          <div class="form-floating">
            <input type="password" id="newPassword"
                   :class="{ 'form-control': true, 'is-invalid': v$.newPassword.$error }"
                   v-model="v$.newPassword.$model" placeholder="Password">
            <div class="invalid-feedback">Password must be at least 8 characters long</div>
            <label for="newPassword">New Password</label>
          </div>
          <div class="form-text">Leave blank to keep your current password</div>
        </div>
        <div class="col-6 col-xl-4">
          <div class="form-floating">
            <input type="password" id="newPasswordConfirm"
                   :class="{ 'form-control': true, 'is-invalid': v$.newPasswordConfirm.$error }"
                   v-model="v$.newPasswordConfirm.$model" placeholder="Confirm Password">
            <div class="invalid-feedback">The passwords do not match</div>
            <label for="newPasswordConfirm">Confirm New Password</label>
          </div>
          <div class="form-text">Leave blank to keep your current password</div>
        </div>
        <div class="col-12">
          <hr>
          <h4 class="pb-2">
            Ways to Be Contacted &nbsp;
            <button class="btn btn-sm btn-outline-primary rounded-pill" @click.prevent="addContact">
              Add a Contact Method
            </button>
          </h4>
        </div>
        <contact-edit v-for="(contact, idx) in accountForm.contacts" :key="contact.id"
                      v-model="accountForm.contacts[idx]" @remove="removeContact(contact.id)"
                      @input="v$.contacts.$touch" />
        <div class="col-12">
          <p v-if="v$.$error" class="text-danger">Please correct the errors above</p>
          <button class="btn btn-primary" @click.prevent="saveAccount(false)">
            <icon :icon="mdiContentSaveOutline" />&nbsp; Save
          </button>
        </div>
      </form>
    </load-data>
    <hr>
    <p class="text-muted fst-italic">
      (If you want to delete your profile, or your entire account,
      <router-link to="/so-long/options">see your deletion options here</router-link>.)
    </p>
    <maybe-save :saveAction="() => saveAccount(true)" :validator="v$" />
  </article>
</template>

<script setup lang="ts">
import { computed, reactive } from "vue"
import { mdiContentSaveOutline } from "@mdi/js"
import useVuelidate from "@vuelidate/core"
import { minLength, required, sameAs } from "@vuelidate/validators"

import api, { AccountProfileForm, Citizen, LogOnSuccess } from "@/api"
import { toastError, toastSuccess } from "@/components/layout/AppToaster.vue"
import { useStore } from "@/store"

import LoadData from "@/components/LoadData.vue"
import MaybeSave from "@/components/MaybeSave.vue"
import ContactEdit from "@/components/citizen/ContactEdit.vue"

const store = useStore()

/** The currently logged-on user */
const user = store.state.user as LogOnSuccess

/** The information available to update */
const accountForm = reactive(new AccountProfileForm())

/** The validation rules for the form */
const rules = computed(() => ({
  firstName: { required },
  lastName: { required },
  displayName: { },
  newPassword: { length: minLength(8) },
  newPasswordConfirm: { matchPassword: sameAs(accountForm.newPassword) },
  contacts: { }
}))

/** Initialize form validation */
const v$ = useVuelidate(rules, accountForm, { $lazy: true })

/** The ID for new contacts */
let newContactId = 0

/** Add a contact to the profile */
const addContact = () => {
  accountForm.contacts.push({
    id: `new${newContactId++}`,
    contactType: "Website",
    name: undefined,
    value: "",
    isPublic: false
  })
  v$.value.contacts.$touch()
}

/** Remove the given contact from the profile */
const removeContact = (contactId : string) => {
  accountForm.contacts = accountForm.contacts.filter(c => c.id !== contactId)
  v$.value.contacts.$touch()
}

/** Retrieve the account profile */
const retrieveData = async (errors : string[]) => {
  const citizenResult = await api.citizen.retrieve(user.citizenId, user)
  if (typeof citizenResult === "string") {
    errors.push(citizenResult)
  } else if (typeof citizenResult === "undefined") {
    errors.push("Citizen not found")
  } else {
    // Update the empty form with appropriate values
    const c = citizenResult as Citizen
    accountForm.firstName = c.firstName
    accountForm.lastName = c.lastName
    accountForm.displayName = c.displayName
    accountForm.contacts = c.otherContacts
  }
}

/** Save the account profile */
const saveAccount = async (isNavigating: boolean) => {
  v$.value.$touch()
  if (v$.value.$error) return
  // Remove any blank contacts before submitting
  accountForm.contacts = accountForm.contacts.filter(c => !((c.name?.trim() ?? "") === "" && c.value.trim() === ""))
  const saveResult = await api.citizen.save(accountForm, user)
  if (typeof saveResult === "string") {
    toastError(saveResult, "saving profile")
  } else {
    toastSuccess("Account Profile Saved Successfully")
    if (!isNavigating) {
      v$.value.$reset()
      const errors: string[] = []
      await retrieveData(errors)
      if (errors.length > 0) {
        toastError(errors[0], "retrieving updated profile")
      }
    }
  }
}
</script>

<style scoped>

</style>
