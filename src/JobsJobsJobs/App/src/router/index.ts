import { createRouter, createWebHistory, RouteLocationNormalized, RouteLocationNormalizedLoaded, RouteRecordRaw } from 'vue-router'
import Home from '../views/Home.vue'

const routes: Array<RouteRecordRaw> = [
  { path: '/', component: Home },
  { path: '/how-it-works', component: () => import(/* webpackChunkName: "help" */ '../views/HowItWorks.vue') },
  { path: '/privacy-policy', component: () => import(/* webpackChunkName: "legal" */ '../views/PrivacyPolicy.vue') },
  { path: '/terms-of-service', component: () => import(/* webpackChunkName: "legal" */ '../views/TermsOfService.vue') },
  // Citizen URLs
  {
    path: '/citizen/authorized',
    component: () => import(/* webpackChunkName: "logon" */ '../views/citizen/Authorized.vue')
  },
  {
    path: '/citizen/dashboard',
    component: () => import(/* webpackChunkName: "logon" */ '../views/citizen/Dashboard.vue')
  },
  {
    path: '/citizen/profile',
    component: () => import(/* webpackChunkName: "profedit" */ '../views/citizen/EditProfile.vue')
  },
  {
    path: '/citizen/log-off',
    component: () => import(/* webpackChunkName: "logoff" */ '../views/citizen/LogOff.vue')
  },
  // Profile URLs
  {
    path: '/profile/view/:id',
    component: () => import(/* webpackChunkName: "profview" */ '../views/profile/ProfileView.vue')
  },
  {
    path: '/profile/search',
    component: () => import(/* webpackChunkName: "profview" */ '../views/profile/ProfileSearch.vue')
  },
  {
    path: '/profile/seeking',
    component: () => import(/* webpackChunkName: "seeking" */ '../views/profile/Seeking.vue')
  },
  // "So Long" URLs
  {
    path: '/so-long/options',
    component: () => import(/* webpackChunkName: "so-long" */ '../views/so-long/DeletionOptions.vue')
  },
  {
    path: '/so-long/success',
    component: () => import(/* webpackChunkName: "so-long" */ '../views/so-long/DeletionSuccess.vue')
  },
  // Success Story URLs
  {
    path: '/success-story/list',
    component: () => import(/* webpackChunkName: "succview" */ '../views/success-story/StoryList.vue')
  },
  {
    path: '/success-story/add',
    component: () => import(/* webpackChunkName: "succedit" */ '../views/success-story/StoryAdd.vue')
  }
]

const router = createRouter({
  history: createWebHistory(process.env.BASE_URL),
  // eslint-disable-next-line
  scrollBehavior (to: RouteLocationNormalized, from: RouteLocationNormalizedLoaded, savedPosition: any) {
    return savedPosition ?? { top: 0, left: 0 }
  },
  routes
})

export default router
