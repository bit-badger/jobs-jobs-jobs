import {
  createRouter,
  createWebHistory,
  RouteLocationNormalized,
  RouteLocationNormalizedLoaded,
  RouteRecordRaw
} from "vue-router"
import store, { Mutations } from "@/store"
import Home from "@/views/Home.vue"

/** The URL to which the user should be pointed once they have authorized with Mastodon */
export const AFTER_LOG_ON_URL = "jjj-after-log-on-url"

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
    path: "/",
    name: "Home",
    component: Home,
    meta: { title: "Welcome!" }
  },
  {
    path: "/how-it-works",
    name: "HowItWorks",
    component: () => import(/* webpackChunkName: "help" */ "../views/HowItWorks.vue"),
    meta: { auth: false, title: "How It Works" }
  },
  {
    path: "/privacy-policy",
    name: "PrivacyPolicy",
    component: () => import(/* webpackChunkName: "legal" */ "../views/PrivacyPolicy.vue"),
    meta: { auth: false, title: "Privacy Policy" }
  },
  {
    path: "/terms-of-service",
    name: "TermsOfService",
    component: () => import(/* webpackChunkName: "legal" */ "../views/TermsOfService.vue"),
    meta: { auth: false, title: "Terms of Service" }
  },
  // Citizen URLs
  {
    path: "/citizen/account",
    name: "AccountProfile",
    component: () => import(/* webpackChunkName: "account" */ "../views/citizen/AccountProfile.vue"),
    meta: { auth: true, title: "Account Profile" }
  },
  // Job Listing URLs
  {
    path: "/help-wanted",
    name: "HelpWanted",
    component: () => import(/* webpackChunkName: "joblist" */ "../views/listing/HelpWanted.vue"),
    meta: { auth: true, title: "Help Wanted" }
  },
  {
    path: "/listing/:id/edit",
    name: "EditListing",
    component: () => import(/* webpackChunkName: "jobedit" */ "../views/listing/ListingEdit.vue"),
    meta: { auth: true, title: "Edit Job Listing" }
  },
  {
    path: "/listing/:id/expire",
    name: "ExpireListing",
    component: () => import(/* webpackChunkName: "jobedit" */ "../views/listing/ListingExpire.vue"),
    meta: { auth: true, title: "Expire Job Listing" }
  },
  {
    path: "/listing/:id/view",
    name: "ViewListing",
    component: () => import(/* webpackChunkName: "joblist" */ "../views/listing/ListingView.vue"),
    meta: { auth: true, title: "Loading Job Listing..." }
  },
  {
    path: "/listings/mine",
    name: "MyListings",
    component: () => import(/* webpackChunkName: "joblist" */ "../views/listing/MyListings.vue"),
    meta: { auth: true, title: "My Job Listings" }
  },
  // Profile URLs
  {
    path: "/profile/seeking",
    name: "PublicSearchProfiles",
    component: () => import(/* webpackChunkName: "seeking" */ "../views/profile/Seeking.vue"),
    meta: { auth: false, title: "People Seeking Work" }
  },
  // "So Long" URLs
  {
    path: "/so-long/options",
    name: "DeletionOptions",
    component: () => import(/* webpackChunkName: "so-long" */ "../views/so-long/DeletionOptions.vue"),
    meta: { auth: true, title: "Account Deletion Options" }
  },
  {
    path: "/so-long/success",
    name: "DeletionSuccess",
    component: () => import(/* webpackChunkName: "so-long" */ "../views/so-long/DeletionSuccess.vue"),
    meta: { auth: false, title: "Account Deletion Success" }
  },
  // Success Story URLs
  {
    path: "/success-story/list",
    name: "ListStories",
    component: () => import(/* webpackChunkName: "success" */ "../views/success-story/StoryList.vue"),
    meta: { auth: false, title: "Success Stories" }
  },
  {
    path: "/success-story/:id/edit",
    name: "EditStory",
    component: () => import(/* webpackChunkName: "succedit" */ "../views/success-story/StoryEdit.vue"),
    meta: { auth: false, title: "Edit Success Story" }
  },
  {
    path: "/success-story/:id/view",
    name: "ViewStory",
    component: () => import(/* webpackChunkName: "success" */ "../views/success-story/StoryView.vue"),
    meta: { auth: false, title: "Success Story" }
  }
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
router.beforeEach((to : RouteLocationNormalized, from : RouteLocationNormalized) => {
  if (store.state.user === undefined && to.meta.auth) {
    window.localStorage.setItem(AFTER_LOG_ON_URL, to.fullPath)
    return "/citizen/log-on"
  }
  store.commit(Mutations.SetTitle, to.meta.title ?? "")
})

export default router
