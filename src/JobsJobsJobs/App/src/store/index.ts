import { InjectionKey } from 'vue'
import { createStore, Store, useStore as baseUseStore } from 'vuex'
import api, { Continent, LogOnSuccess } from '../api'

/** The state tracked by the application */
export interface State {
  /** The currently logged-on user */
  user: LogOnSuccess | undefined
  /** The state of the log on process */
  logOnState: string
  /** All continents (use `ensureContinents` action) */
  continents: Continent[]
}

/** An injection key to identify this state with Vue */
export const key : InjectionKey<Store<State>> = Symbol('VueX Store')

/** Use this store in component `setup` functions */
export function useStore () : Store<State> {
  return baseUseStore(key)
}

export default createStore({
  state: () : State => {
    return {
      user: undefined,
      logOnState: 'Logging you on with No Agenda Social...',
      continents: []
    }
  },
  mutations: {
    setUser (state, user : LogOnSuccess) {
      state.user = user
    },
    clearUser (state) {
      state.user = undefined
    },
    setLogOnState (state, message : string) {
      state.logOnState = message
    },
    setContinents (state, continents : Continent[]) {
      state.continents = continents
    }
  },
  actions: {
    async logOn ({ commit }, code: string) {
      const logOnResult = await api.citizen.logOn(code)
      if (typeof logOnResult === 'string') {
        commit('setLogOnState', logOnResult)
      } else {
        commit('setUser', logOnResult)
      }
    },
    async ensureContinents ({ state, commit }) {
      if (state.continents.length > 0) return
      const theSeven = await api.continent.all()
      if (typeof theSeven === 'string') {
        console.error(theSeven)
      } else {
        commit('setContinents', theSeven)
      }
    }
  },
  modules: {
  }
})
