import { createApp } from "vue"
import App from "./App.vue"
import router from "./router"
import store, { key } from "./store"
import Icon from "./components/Icon.vue"

const app = createApp(App)
  .use(router)
  .use(store, key)

app.component("Icon", Icon)

app.mount("#app")
