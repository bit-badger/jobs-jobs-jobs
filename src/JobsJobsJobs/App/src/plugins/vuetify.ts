import '@mdi/font/css/materialdesignicons.css'
import 'vuetify/lib/styles/main.sass'
import { createVuetify } from 'vuetify'
import * as components from 'vuetify/lib/components'
import * as directives from 'vuetify/lib/directives'

const jjjTheme = {
  dark: false,
  colors: {
    background: '#ffffff',
    surface: '#ffffff',
    primary: '#007bff',
    'primary-darken-1': '#3700b3',
    secondary: '#f7f7f7',
    'secondary-darken-1': '#018786',
    error: '#b00020',
    info: '#2196f3',
    success: '#4caf50',
    warning: '#fb8c00'
  },
  variables: {
    'border-color': '#000000',
    'border-opacity': 0.12,
    'high-emphasis-opacity': 0.87,
    'medium-emphasis-opacity': 0.60,
    'disabled-opacity': 0.38,
    'activated-opacity': 0.12,
    'hover-opacity': 0.04,
    'focus-opacity': 0.12,
    'selected-opacity': 0.08,
    'dragged-opacity': 0.08,
    'pressed-opacity': 0.16,
    'kbd-background-color': '#212529',
    'kbd-color': '#FFFFFF',
    'code-background-color': '#C2C2C2'
  }
}

export default createVuetify({
  components,
  directives,
  theme: {
    defaultTheme: 'jjjTheme',
    themes: {
      jjjTheme
    }
  }
})
