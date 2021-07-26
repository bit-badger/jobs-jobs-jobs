import { createApp } from 'vue'
import vuetify from './plugins/vuetify'
import App from './App.vue'
import router from './router'
import store, { key } from './store'
import PageTitle from './components/PageTitle.vue'

const app = createApp(App)
  .use(router)
  .use(store, key)
  .use(vuetify)

app.component('PageTitle', PageTitle)

app.mount('#app')
