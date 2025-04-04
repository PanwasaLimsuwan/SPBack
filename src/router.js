// src/router.js
import { createRouter, createWebHistory } from 'vue-router';
import HomeDashboard from './views/HomeDashboard.vue';
import DashboardMFG from './views/DashboardMFG.vue';
import DashboardHR from './views/DashboardHR.vue';

const routes = [
  { path: '/', component: HomeDashboard },
  { path: '/dashboard-mfg', component: DashboardMFG },
  { path: '/dashboard-hr', component: DashboardHR },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

export default router;
