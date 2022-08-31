import { useTitle } from "@vueuse/core"
import { InjectionKey } from "vue"
import { createStore, Store, useStore as baseUseStore } from "vuex"
import api, { Continent, LogOnSuccess } from "../api"
import * as Actions from "./actions"
import * as Mutations from "./mutations"

/** The state tracked by the application */
export interface State {
  /** The document's current title */
  pageTitle : string
  /** The currently logged-on user */
  user : LogOnSuccess | undefined
  /** The state of the log on process */
  logOnState : string
  /** All continents (use `ensureContinents` action) */
  continents : Continent[]
}

/** An injection key to identify this state with Vue */
export const key : InjectionKey<Store<State>> = Symbol("VueX Store")

/** Use this store in component `setup` functions */
export function useStore () : Store<State> {
  return baseUseStore(key)
}

/** The application name */
const appName = "Jobs, Jobs, Jobs"

export default createStore({
  state: () : State => {
    return {
      pageTitle: "",
      user: undefined,
      logOnState: "",
      continents: []
    }
  },
  mutations: {
    [Mutations.SetTitle]: (state, title : string) => {
      state.pageTitle = title === "" ? appName : `${title} | ${appName}`
      useTitle(state.pageTitle)
    },
    [Mutations.SetUser]: (state, user : LogOnSuccess) => { state.user = user },
    [Mutations.ClearUser]: (state) => { state.user = undefined },
    [Mutations.SetLogOnState]: (state, message : string) => { state.logOnState = message },
    [Mutations.SetContinents]: (state, continents : Continent[]) => { state.continents = continents }
  },
  actions: {
    [Actions.LogOn]: async ({ commit }, { form }) => {
      const logOnResult = await api.citizen.logOn(form)
      if (typeof logOnResult === "string") {
        commit(Mutations.SetLogOnState, logOnResult)
      } else {
        commit(Mutations.SetLogOnState, "")
        commit(Mutations.SetUser, logOnResult)
      }
    },
    [Actions.EnsureContinents]: async ({ state, commit }) => {
      if (state.continents.length > 0) return
      const theSeven = await api.continent.all()
      if (typeof theSeven === "string") {
        console.error(theSeven)
      } else {
        commit(Mutations.SetContinents, theSeven)
      }
    }
  },
  modules: {
  }
})

export * as Actions from "./actions"
export * as Mutations from "./mutations"
