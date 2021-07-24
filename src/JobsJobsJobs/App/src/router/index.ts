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
    component: () => import(/* webpackChurchName: "profedit" */ '../views/citizen/EditProfile.vue')
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
