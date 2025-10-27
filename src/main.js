// src/main.js
import { createApp } from 'vue';
import App from './App.vue';
import router from './router';
import Toast from 'vue-toastification';
import 'vue-toastification/dist/index.css';
import axios from 'axios';

// ตั้งค่า base URL สำหรับทุกคำขอของ Axios
axios.defaults.baseURL = 'http://localhost:5000';  // ตั้งค่า base URL ไปที่ Backend API ของคุณ

// ส่งออก axios เพื่อใช้งานในที่อื่นๆ
export { axios };

const app = createApp(App);
app.use(router);
app.use(Toast);
app.mount('#app');
