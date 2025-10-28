// src/router.js
import { createRouter, createWebHistory } from 'vue-router';
import HomeDashboard from './views/HomeDashboard.vue';
import DashboardMFG from './views/DashboardMFG.vue';
import DashboardHR from './views/DashboardHR.vue';
import AdminDashboard from './views/AdminDashboard.vue';
import CamerasList from './components/CamerasList.vue';
import LeaderLogin from './views/LeaderLogin.vue';
import RegisterLeader from './components/RegisterLeader'
import RegisterAdmin from './components/RegisterAdmin'

const routes = [
  { path: "/", name: "Leaderlogin", component: LeaderLogin, meta: { guestOnly: true } },
  { path: '/dashboard', component: HomeDashboard },
  { path: '/dashboard-mfg', component: DashboardMFG },
  { path: '/dashboard-hr', component: DashboardHR },
  { path: '/dashboard-admin' , component: AdminDashboard },
  { path: '/register-leader' , component: RegisterLeader },
  { path: '/register-admin' , component: RegisterAdmin }
  // { path: '/cameras' , component: CamerasList },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

export default router;
