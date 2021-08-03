import {
  createRouter,
  createWebHistory,
  RouteLocationNormalized,
  RouteLocationNormalizedLoaded,
  RouteRecordName,
  RouteRecordRaw
} from 'vue-router'
import store from '@/store'
import Home from '@/views/Home.vue'
import LogOn from '@/views/citizen/LogOn.vue'

/** The URL to which the user should be pointed once they have authorized with NAS */
export const AFTER_LOG_ON_URL = 'jjj-after-log-on-url'

/**
 * Get a value from the query string
 *
 * @param route The current route
 * @param key The key of the query string value to obtain
 * @returns The string value, the first of many (if included multiple times), or `undefined` if not present
 */
export function queryValue (route: RouteLocationNormalizedLoaded, key : string) : string | undefined {
  const value = route.query[key]
  if (value) return Array.isArray(value) && value.length > 0 ? value[0]?.toString() : value.toString()
}

const routes: Array<RouteRecordRaw> = [
  {
    path: '/',
    name: 'Home',
    component: Home
  },
  {
    path: '/how-it-works',
    name: 'HowItWorks',
    component: () => import(/* webpackChunkName: "help" */ '../views/HowItWorks.vue')
  },
  {
    path: '/privacy-policy',
    name: 'PrivacyPolicy',
    component: () => import(/* webpackChunkName: "legal" */ '../views/PrivacyPolicy.vue')
  },
  {
    path: '/terms-of-service',
    name: 'TermsOfService',
    component: () => import(/* webpackChunkName: "legal" */ '../views/TermsOfService.vue')
  },
  // Citizen URLs
  {
    path: '/citizen/log-on',
    name: 'LogOn',
    component: LogOn
  },
  {
    path: '/citizen/authorized',
    name: 'CitizenAuthorized',
    component: () => import(/* webpackChunkName: "dashboard" */ '../views/citizen/Authorized.vue')
  },
  {
    path: '/citizen/dashboard',
    name: 'Dashboard',
    component: () => import(/* webpackChunkName: "dashboard" */ '../views/citizen/Dashboard.vue')
  },
  {
    path: '/citizen/profile',
    name: 'EditProfile',
    component: () => import(/* webpackChunkName: "profedit" */ '../views/citizen/EditProfile.vue')
  },
  {
    path: '/citizen/log-off',
    name: 'LogOff',
    component: () => import(/* webpackChunkName: "logoff" */ '../views/citizen/LogOff.vue')
  },
  // Profile URLs
  {
    path: '/profile/view/:id',
    name: 'ViewProfile',
    component: () => import(/* webpackChunkName: "profview" */ '../views/profile/ProfileView.vue')
  },
  {
    path: '/profile/search',
    name: 'SearchProfiles',
    component: () => import(/* webpackChunkName: "profview" */ '../views/profile/ProfileSearch.vue')
  },
  {
    path: '/profile/seeking',
    name: 'PublicSearchProfiles',
    component: () => import(/* webpackChunkName: "seeking" */ '../views/profile/Seeking.vue')
  },
  // "So Long" URLs
  {
    path: '/so-long/options',
    name: 'DeletionOptions',
    component: () => import(/* webpackChunkName: "so-long" */ '../views/so-long/DeletionOptions.vue')
  },
  {
    path: '/so-long/success',
    name: 'DeletionSuccess',
    component: () => import(/* webpackChunkName: "so-long" */ '../views/so-long/DeletionSuccess.vue')
  },
  // Success Story URLs
  {
    path: '/success-story/list',
    name: 'ListStories',
    component: () => import(/* webpackChunkName: "success" */ '../views/success-story/StoryList.vue')
  },
  {
    path: '/success-story/add',
    name: 'AddStory',
    component: () => import(/* webpackChunkName: "succedit" */ '../views/success-story/StoryAdd.vue')
  },
  {
    path: '/success-story/view/:id',
    name: 'ViewStory',
    component: () => import(/* webpackChunkName: "success" */ '../views/success-story/StoryView.vue')
  }
]
/** The routes that do not require logins */
const publicRoutes : Array<RouteRecordName> = [
  'Home', 'HowItWorks', 'PrivacyPolicy', 'TermsOfService', 'LogOn', 'CitizenAuthorized', 'PublicSearchProfiles',
  'DeletionSuccess'
]

const router = createRouter({
  history: createWebHistory(process.env.BASE_URL),
  // eslint-disable-next-line
  scrollBehavior (to : RouteLocationNormalized, from : RouteLocationNormalizedLoaded, savedPosition : any) {
    return savedPosition ?? { top: 0, left: 0 }
  },
  routes
})

// eslint-disable-next-line
router.beforeEach((to : RouteLocationNormalized, from : RouteLocationNormalized) =>{
  if (store.state.user === undefined && !publicRoutes.includes(to.name || '')) {
    window.localStorage.setItem(AFTER_LOG_ON_URL, to.fullPath)
    return '/citizen/log-on'
  }
})

export default router
