import { InjectionKey } from 'vue'
import { createStore, Store, useStore as baseUseStore } from 'vuex'
import api, { LogOnSuccess } from '../api'

/** The state tracked by the application */
export interface State {
  user: LogOnSuccess | undefined
  logOnState: string
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
      logOnState: 'Logging you on with No Agenda Social...'
    }
  },
  mutations: {
    setUser (state, user: LogOnSuccess) {
      state.user = user
    },
    setLogOnState (state, message) {
      state.logOnState = message
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
    }
  },
  modules: {
  }
})
