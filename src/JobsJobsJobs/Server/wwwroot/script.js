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
   * Show a message via toast
   * @param {string} message The message to show
   */
   showToast (message) {
    const [level, msg] = message.split("|||")
    
    let header
    if (level !== "success") {
      const heading = typ => `<span class="me-auto"><strong>${typ.toUpperCase()}</strong></span>`
      
      header = document.createElement("div")
      header.className = "toast-header"
      header.innerHTML = heading(level === "warning" ? level : "error")
      
      const close = document.createElement("button")
      close.type = "button"
      close.className = "btn-close"
      close.setAttribute("data-bs-dismiss", "toast")
      close.setAttribute("aria-label", "Close")
      header.appendChild(close)
    }

    const body = document.createElement("div")
    body.className = "toast-body"
    body.innerText = msg
    
    const toastEl = document.createElement("div")
    toastEl.className = `toast bg-${level === "error" ? "danger" : level} text-white`
    toastEl.setAttribute("role", "alert")
    toastEl.setAttribute("aria-live", "assertlive")
    toastEl.setAttribute("aria-atomic", "true")
    toastEl.addEventListener("hidden.bs.toast", e => e.target.remove())
    if (header) toastEl.appendChild(header)
    
    toastEl.appendChild(body)
    document.getElementById("toasts").appendChild(toastEl)
    new bootstrap.Toast(toastEl, { autohide: level === "success" }).show()
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

htmx.on("htmx:afterOnLoad", function (evt) {
  const hdrs = evt.detail.xhr.getAllResponseHeaders()
  // Show a message if there was one in the response
  if (hdrs.indexOf("x-toast") >= 0) {
    jjj.showToast(evt.detail.xhr.getResponseHeader("x-toast"))
  }
})

htmx.on("htmx:configRequest", function (evt) {
  // Send the user's current time zone so that we can display local time
  if (jjj.timeZone) {
    evt.detail.headers["X-Time-Zone"] = jjj.timeZone
  }
})

jjj.deriveTimeZone()
