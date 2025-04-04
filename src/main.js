// import { createApp } from 'vue'
// import App from './App.vue'

// createApp(App).mount('#app')

// src/main.js
import { createApp } from 'vue';
import App from './App.vue';
import router from './router';
// import 'vue-toastification/dist/index.css';
import Toast from 'vue-toastification'
import 'vue-toastification/dist/index.css'

const app = createApp(App);
app.use(router);
app.use(Toast)
app.mount('#app');
