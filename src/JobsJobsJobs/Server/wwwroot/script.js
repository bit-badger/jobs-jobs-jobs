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
   **/
   timeZone: undefined,
  
   /**
    * Derive the time zone from the current browser
    */
   deriveTimeZone () {
     try {
       this.timeZone = (new Intl.DateTimeFormat()).resolvedOptions().timeZone
     } catch (_) { }
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
