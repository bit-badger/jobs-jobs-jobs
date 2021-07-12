import { createRouter, createWebHistory, RouteLocationNormalized, RouteLocationNormalizedLoaded, RouteRecordRaw } from 'vue-router'
import Home from '../views/Home.vue'

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
    component: () => import(/* webpackChunkName: "privacy" */ '../views/PrivacyPolicy.vue')
  },
  {
    path: '/terms-of-service',
    name: 'TermsOfService',
    component: () => import(/* webpackChunkName: "terms" */ '../views/TermsOfService.vue')
  }
]

const router = createRouter({
  history: createWebHistory(process.env.BASE_URL),
  scrollBehavior (to: RouteLocationNormalized, from: RouteLocationNormalizedLoaded, savedPosition: any) {
    return savedPosition ?? { top: 0, left: 0 }
  },
  routes
})

export default router
