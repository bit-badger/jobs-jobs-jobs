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
      const newId = `new${this.nextIndex}`
      
      /** @type {HTMLTemplateElement} */
      const newSkillTemplate = document.getElementById("newSkill")
      /** @type {HTMLDivElement} */
      const newSkill = newSkillTemplate.content.firstElementChild.cloneNode(true)
      newSkill.setAttribute("id", `skillRow${newId}`)

      const cols = newSkill.children
      // Button column
      cols[0].querySelector("button").setAttribute("onclick", `jjj.profile.removeSkill('${newId}')`)
      // Skill column
      const skillField = cols[1].querySelector("input")
      skillField.setAttribute("id", `skillDesc${newId}`)
      skillField.setAttribute("name", `Skills[${this.nextIndex}].Description`)
      cols[1].querySelector("label").setAttribute("for", `skillDesc${newId}`)
      if (this.nextIndex > 0) cols[1].querySelector("div.form-text").remove()
      // Notes column
      const notesField = cols[2].querySelector("input")
      notesField.setAttribute("id", `skillNotes${newId}`)
      notesField.setAttribute("name", `Skills[${this.nextIndex}].Notes`)
      cols[2].querySelector("label").setAttribute("for", `skillNotes${newId}`)
      if (this.nextIndex > 0) cols[2].querySelector("div.form-text").remove()

      // Add the row
      const skills = document.querySelectorAll("div[id^=skillRow]")
      const sibling = skills.length > 0 ? skills[skills.length - 1] : newSkillTemplate
      sibling.insertAdjacentElement('afterend', newSkill)

      this.nextIndex++
    },

    /**
     * Remove a skill row from the profile form
     * @param {string} id The ID of the skill row to remove
     */
    removeSkill(id) {
      document.getElementById(`skillRow${id}`).remove()
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
