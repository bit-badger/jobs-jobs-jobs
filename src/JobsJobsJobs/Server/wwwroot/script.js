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
  }
}
