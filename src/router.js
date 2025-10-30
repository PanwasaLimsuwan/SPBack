// src/router.js
import { createRouter, createWebHistory } from 'vue-router';
import HomeDashboard from './views/HomeDashboard.vue';
import DashboardMFG from './views/DashboardMFG.vue';
import DashboardHR from './views/DashboardHR.vue';
import AdminDashboard from './views/AdminDashboard.vue';
import CamerasList from './components/CamerasList.vue';
import Login from './views/Login.vue';
import RegisterLeader from './components/RegisterLeader'
import RegisterAdmin from './components/RegisterAdmin'
import MFGControl from './components/MFGControl'

const routes = [
  { path: "/login", name: "Login", component: Login, meta: { guestOnly: true } },
  { path: '/dashboard', component: HomeDashboard },
  { path: '/dashboard-mfg', component: DashboardMFG },
  { path: '/dashboard-hr', component: DashboardHR },
  { path: '/dashboard-admin' , component: AdminDashboard },
  { path: '/register-leader' , component: RegisterLeader },
  { path: '/register-admin' , component: RegisterAdmin },
  { path: '/MFGControl' , component: MFGControl },
  // { path: 'MFG'}
  // { path: '/cameras' , component: CamerasList },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

router.beforeEach((to, from, next) => {
  if (to.meta.requiresAuth && !localStorage.getItem('token')) {
    next('/login'); // ถ้าไม่มี token, ไปหน้า login
  } else {
    next();
  }
});

export default router;
