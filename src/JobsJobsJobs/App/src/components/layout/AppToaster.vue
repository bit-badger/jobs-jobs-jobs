<template lang="pug">
div(aria-live='polite' aria-atomic='true' id='toastHost')
  .toast-container.position-absolute.p-3.bottom-0.start-50.translate-middle-x(id='toasts')
</template>

<script lang="ts">
import { defineComponent } from 'vue'
import { Toast } from 'bootstrap'

/** Remove a toast once it's hidden */
const removeToast = (event : Event) => (event.target as HTMLDivElement).remove()

/** Create a toast, add it to the DOM, and show it */
const createToast = (level : 'success' | 'warning' | 'danger', message : string, process : string | undefined) => {
  let header : HTMLDivElement | undefined
  if (level !== 'success') {
    // Create a heading, optionally including the process that generated the message
    const heading = (typ : string) : string => {
      const proc = process ? ` (${process})` : ''
      return `<span class="me-auto"><strong>${typ.toUpperCase()}</strong>${proc}</span>`
    }
    header = document.createElement('div')
    header.className = 'toast-header'
    header.innerHTML = heading(level === 'warning' ? level : 'error')
    // Include a close button, as these will not auto-close
    const close = document.createElement('button')
    close.type = 'button'
    close.className = 'btn-close'
    close.setAttribute('data-bs-dismiss', 'toast')
    close.setAttribute('aria-label', 'Close')
    header.appendChild(close)
  }
  const body = document.createElement('div')
  body.className = 'toast-body'
  body.innerHTML = message

  const toastEl = document.createElement('div')
  toastEl.className = `toast bg-${level} text-white`
  toastEl.setAttribute('role', 'alert')
  toastEl.setAttribute('aria-live', 'assertlive')
  toastEl.setAttribute('aria-atomic', 'true')
  toastEl.addEventListener('hidden.bs.toast', removeToast)
  if (header) toastEl.appendChild(header)
  toastEl.appendChild(body)

  ;(document.getElementById('toasts') as HTMLDivElement).appendChild(toastEl)
  new Toast(toastEl, { autohide: level === 'success' }).show()
}

/**
 * Generate a success toast
 *
 * @param message The message to be displayed
 */
export function toastSuccess (message : string) : void {
  createToast('success', message, undefined)
}

/**
 * Generate a warning toast
 *
 * @param message The message to be displayed
 * @param process The process which generated the warning (optional)
 */
export function toastWarning (message : string, process : string | undefined) : void {
  createToast('warning', message, process)
}

/**
 * Generate an error toast
 *
 * @param message The message to be displayed
 * @param process The process which generated the error (optional)
 */
export function toastError (message : string, process : string | undefined) : void {
  createToast('danger', message, process)
}

export default defineComponent({
  name: 'AppToaster'
})
</script>

<style lang="sass" scoped>
#toastHost
  position: sticky
  bottom: 0
</style>
