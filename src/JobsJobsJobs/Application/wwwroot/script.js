/** Script for Jobs, Jobs, Jobs */
this.jjj = {
  /**
   * Play an audio file
   * @param {HTMLElement} elt The element which was clicked
   */
  playFile(elt) {
    elt.querySelector("audio").play()
  },

  /**
   * Hide the offcanvas menu if it displayed
   */
  hideMenu() {
    /** @type {HTMLElement} */
    const menu = document.querySelector(".jjj-mobile-menu")
    if (menu.style.display !== "none") bootstrap.Offcanvas.getOrCreateInstance(menu).hide()
  },

  /**
   * Show a message via an alert
   * @param {string} message The message to show
   */
  showAlert (message) {
    const [level, msg] = message.split("|||")

    /** @type {HTMLTemplateElement} */
    const alertTemplate = document.getElementById("alertTemplate")
    /** @type {HTMLDivElement} */
    const alert = alertTemplate.content.firstElementChild.cloneNode(true)
    alert.classList.add(`alert-${level === "error" ? "danger" : level}`)

    const prefix = level === "success" ? "" : `<strong>${level.toUpperCase()}: </strong>`
    alert.querySelector("p").innerHTML = `${prefix}${msg}`

    const alerts = document.getElementById("alerts")
    alerts.appendChild(alert)
    alerts.scrollIntoView()
  },

  /**
   * The time zone of the current browser
   * @type {string}
   */
  timeZone: undefined,
  
  /**
   * Derive the time zone from the current browser
   */
  deriveTimeZone () {
     try {
       this.timeZone = (new Intl.DateTimeFormat()).resolvedOptions().timeZone
     } catch (_) { }
  },

  /**
   * Set up the onClick event for the preview button
   * @param {string} editorId The ID of the editor to wire up
   */
  markdownOnLoad(editorId) {
    document.getElementById(`${editorId}PreviewButton`).addEventListener("click", () => { this.showPreview(editorId) })
  },

  /**
   * Show a preview of the Markdown in the given editor
   * @param {string} editorId The ID of the Markdown editor whose preview should be shown
   */
  async showPreview(editorId) {
    /** @type {HTMLButtonElement} */
    const editBtn = document.getElementById(`${editorId}EditButton`)
    /** @type {HTMLDivElement} */
    const editDiv = document.getElementById(`${editorId}Edit`)
    /** @type {HTMLTextAreaElement} */
    const editor = document.getElementById(editorId)
    /** @type {HTMLButtonElement} */
    const previewBtn = document.getElementById(`${editorId}PreviewButton`)
    /** @type {HTMLDivElement} */
    const previewDiv = document.getElementById(`${editorId}Preview`)

    editBtn.classList.remove("btn-primary")
    editBtn.classList.add("btn-outline-secondary")
    editBtn.addEventListener("click", () => { this.showEditor(editorId) })
    previewBtn.classList.remove("btn-outline-secondary")
    previewBtn.classList.add("btn-primary")
    previewBtn.removeEventListener("click", () => { this.showPreview(editorId) })

    const preview = await fetch("/api/markdown-preview", { method: "POST", body: editor.value })
    let text
    if (preview.ok) {
      text = await preview.text()
    } else {
      text = `<p class="text-danger"><strong> ERROR ${preview.status}</strong> &ndash; ${preview.statusText}`
    }
    previewDiv.innerHTML = text
    
    editDiv.classList.remove("jjj-shown")
    editDiv.classList.add("jjj-not-shown")
    previewDiv.classList.remove("jjj-not-shown")
    previewDiv.classList.add("jjj-shown")

  },

  /**
   * Show the Markdown editor (hides preview)
   * @param {string} editorId The ID of the Markdown editor to show
   */
  showEditor(editorId) {
    /** @type {HTMLButtonElement} */
    const editBtn = document.getElementById(`${editorId}EditButton`)
    /** @type {HTMLDivElement} */
    const editDiv = document.getElementById(`${editorId}Edit`)
    /** @type {HTMLTextAreaElement} */
    const editor = document.getElementById(editorId)
    /** @type {HTMLButtonElement} */
    const previewBtn = document.getElementById(`${editorId}PreviewButton`)
    /** @type {HTMLDivElement} */
    const previewDiv = document.getElementById(`${editorId}Preview`)

    previewBtn.classList.remove("btn-primary")
    previewBtn.classList.add("btn-outline-secondary")
    this.markdownOnLoad(editorId)
    editBtn.classList.remove("btn-outline-secondary")
    editBtn.classList.add("btn-primary")
    editBtn.removeEventListener("click", () => { this.showEditor(editorId) })

    previewDiv.classList.remove("jjj-shown")
    previewDiv.classList.add("jjj-not-shown")
    previewDiv.innerHTML = ""
    editDiv.classList.remove("jjj-not-shown")
    editDiv.classList.add("jjj-shown")
  },

  citizen: {

    /**
     * The next index for a newly-added contact
     * @type {number}
     */
    nextIndex: 0,

    /**
     * Add a contact to the account form
     */
    addContact() {
      const next = this.nextIndex
      
      /** @type {HTMLTemplateElement} */
      const newContactTemplate = document.getElementById("newContact")
      /** @type {HTMLDivElement} */
      const newContact = newContactTemplate.content.firstElementChild.cloneNode(true)
      newContact.setAttribute("id", `contactRow${next}`)

      const cols = newContact.children
      // Button column
      cols[0].querySelector("button").setAttribute("onclick", `jjj.citizen.removeContact(${next})`)
      // Contact Type column
      const typeField = cols[1].querySelector("select")
      typeField.setAttribute("id", `contactType${next}`)
      typeField.setAttribute("name", `Contacts[${this.nextIndex}].ContactType`)
      cols[1].querySelector("label").setAttribute("for", `contactType${next}`)
      // Name column
      const nameField = cols[2].querySelector("input")
      nameField.setAttribute("id", `contactName${next}`)
      nameField.setAttribute("name", `Contacts[${this.nextIndex}].Name`)
      cols[2].querySelector("label").setAttribute("for", `contactName${next}`)
      if (next > 0) cols[2].querySelector("div.form-text").remove()
      // Value column
      const valueField = cols[3].querySelector("input")
      valueField.setAttribute("id", `contactValue${next}`)
      valueField.setAttribute("name", `Contacts[${this.nextIndex}].Value`)
      cols[3].querySelector("label").setAttribute("for", `contactName${next}`)
      if (next > 0) cols[3].querySelector("div.form-text").remove()
      // Is Public column
      const isPublicField = cols[4].querySelector("input")
      isPublicField.setAttribute("id", `contactIsPublic${next}`)
      isPublicField.setAttribute("name", `Contacts[${this.nextIndex}].IsPublic`)
      cols[4].querySelector("label").setAttribute("for", `contactIsPublic${next}`)

      // Add the row
      const contacts = document.querySelectorAll("div[id^=contactRow]")
      const sibling = contacts.length > 0 ? contacts[contacts.length - 1] : newContactTemplate
      sibling.insertAdjacentElement('afterend', newContact)

      this.nextIndex++
    },

    /**
     * Remove a contact row from the profile form
     * @param {number} idx The index of the contact row to remove
     */
    removeContact(idx) {
      document.getElementById(`contactRow${idx}`).remove()
    },

    /**
     * Register a comparison validation between a password and a "confirm password" field
     * @param {string} pwId The ID for the password field
     * @param {string} confirmId The ID for the "confirm password" field
     * @param {boolean} isRequired Whether these fields are required
     */
    validatePasswords(pwId, confirmId, isRequired) {
      const pw = document.getElementById(pwId)
      const pwConfirm = document.getElementById(confirmId)
      pwConfirm.addEventListener("input", () => {
          if (!pw.validity.valid) {
              pwConfirm.setCustomValidity("")
          } else if ((!pwConfirm.validity.valueMissing || !isRequired) && pw.value !== pwConfirm.value) {
              pwConfirm.setCustomValidity("Confirmation password does not match")
          } else {
              pwConfirm.setCustomValidity("")
          }
      })
    }
  },

  /**
   * Script for listing pages
   */
  listing: {
    
    /**
     * Show or hide the success story prompt based on whether a job was filled here
     */
    toggleFromHere() {
      /** @type {HTMLInputElement} */
      const isFromHere = document.getElementById("FromHere")
      const display = isFromHere.checked ? "unset" : "none"
      document.getElementById("successRow").style.display = display
      document.getElementById("SuccessStoryEditRow").style.display = display
    }
  },

  /**
   * Script for profile pages
   */
  profile: {
    
    /**
     * The next index for a newly-added skill
     * @type {number}
     */
    nextIndex: 0,

    /**
     * Add a skill to the profile form
     */
    addSkill() {
      const next = this.nextIndex
      
      /** @type {HTMLTemplateElement} */
      const newSkillTemplate = document.getElementById("newSkill")
      /** @type {HTMLDivElement} */
      const newSkill = newSkillTemplate.content.firstElementChild.cloneNode(true)
      newSkill.setAttribute("id", `skillRow${next}`)

      const cols = newSkill.children
      // Button column
      cols[0].querySelector("button").setAttribute("onclick", `jjj.profile.removeSkill(${next})`)
      // Skill column
      const skillField = cols[1].querySelector("input")
      skillField.setAttribute("id", `skillDesc${next}`)
      skillField.setAttribute("name", `Skills[${this.nextIndex}].Description`)
      cols[1].querySelector("label").setAttribute("for", `skillDesc${next}`)
      if (this.nextIndex > 0) cols[1].querySelector("div.form-text").remove()
      // Notes column
      const notesField = cols[2].querySelector("input")
      notesField.setAttribute("id", `skillNotes${next}`)
      notesField.setAttribute("name", `Skills[${this.nextIndex}].Notes`)
      cols[2].querySelector("label").setAttribute("for", `skillNotes${next}`)
      if (this.nextIndex > 0) cols[2].querySelector("div.form-text").remove()

      // Add the row
      const skills = document.querySelectorAll("div[id^=skillRow]")
      const sibling = skills.length > 0 ? skills[skills.length - 1] : newSkillTemplate
      sibling.insertAdjacentElement('afterend', newSkill)

      this.nextIndex++
    },

    /**
     * Remove a skill row from the profile form
     * @param {number} idx The index of the skill row to remove
     */
    removeSkill(idx) {
      document.getElementById(`skillRow${idx}`).remove()
    }
  }
}

htmx.on("htmx:configRequest", function (evt) {
  // Send the user's current time zone so that we can display local time
  if (jjj.timeZone) {
    evt.detail.headers["X-Time-Zone"] = jjj.timeZone
  }
})

htmx.on("htmx:responseError", function (evt) {
  /** @type {XMLHttpRequest} */
  const xhr = evt.detail.xhr
  jjj.showAlert(`error|||${xhr.status}: ${xhr.statusText}`)
})

jjj.deriveTimeZone()
