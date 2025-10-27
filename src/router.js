// src/router.js
import { createRouter, createWebHistory } from 'vue-router';
import HomeDashboard from './views/HomeDashboard.vue';
import DashboardMFG from './views/DashboardMFG.vue';
import DashboardHR from './views/DashboardHR.vue';
import AdminDashboard from './views/AdminDashboard.vue';
import CamerasList from './components/CamerasList.vue';
import Login from './views/Login.vue';

const routes = [
  { path: "/", name: "login", component: Login, meta: { guestOnly: true } },
  { path: '/dashboard', component: HomeDashboard },
  { path: '/dashboard-mfg', component: DashboardMFG },
  { path: '/dashboard-hr', component: DashboardHR },
  { path: '/dashboard-admin' , component: AdminDashboard },
  // { path: '/cameras' , component: CamerasList },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

export default router;
