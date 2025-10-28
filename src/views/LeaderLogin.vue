<template>
  <div class="auth-page">
    <div class="card">
      <img src="/logo2.png" alt="Logo" class="logo" />
      <h2 v-if="isLogin">Sign in</h2>

      <!-- Login Form -->
      <form @submit.prevent="onSubmit">
        <div class="form-group">
          <label>Email</label>
          <input
            v-model.trim="email"
            type="email"
            placeholder="Enter email"
            autocomplete="email"
            :disabled="loading"
          />
        </div>

        <div class="form-group">
          <label>Password</label>
          <input
            v-model.trim="password"
            :type="showPassword ? 'text' : 'password'"
            placeholder="Enter password"
            autocomplete="current-password"
            :disabled="loading"
          />
        </div>

        <div class="options">
          <label><input type="checkbox" v-model="showPassword" /> Show password</label>
          <!-- <label><input type="checkbox" v-model="remember" /> Remember me</label> -->
        </div>

        <button class="btn" type="submit" :disabled="loading">
          <span v-if="!loading">Login</span>
          <span v-else>Signing in...</span>
        </button>

        <p v-if="error" class="error">{{ error }}</p>
      </form>

      <!-- <p class="hint">
        <span v-if="isLogin">Don't have an account?</span>
        <a href="#" @click.prevent="toggleAuthMode">Sign up</a>
      </p> -->
    </div>
  </div>
</template>

<script setup>
import { ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import axios from "axios";

// ====== Config & Helpers ======
const API_BASE_URL = "http://localhost:5000";
const USE_MOCK = false;

function setToken(token) {
  localStorage.setItem("token", token);
}

function clearToken() {
  localStorage.removeItem("token");
}

function getToken() {
  return localStorage.getItem("token");
}

// login function
async function doLogin({ email, password }) {
  const { data } = await axios.post(`${API_BASE_URL}/api/Admin/login`, {
    Email: email,
    Password: password,
  });
  if (!data?.token) throw new Error("ไม่พบโทเคนจากระบบ");
  setToken(data.token);
  // เก็บ role ไว้ใช้ต่อ
  if (data.role) localStorage.setItem("role", data.role);
  return data; // { token, role, ... }
}

// ====== State ======
const route = useRoute();
const router = useRouter();

const email = ref("");  // ใช้แค่ Email สำหรับการ Login
const password = ref("");
const remember = ref(true);
const showPassword = ref(false);
const loading = ref(false);
const error = ref("");
const isLogin = ref(true);  // ใช้เฉพาะ Login

// Toggle between Login and Register (จะใช้แค่ Login ตอนนี้)
function toggleAuthMode() {
  isLogin.value = !isLogin.value;
}

// ====== Methods ======
async function onSubmit() {
  error.value = "";
  if (!email.value || !password.value) {
    error.value = "กรุณากรอกข้อมูลทั้งหมด";
    return;
  }
  try {
    loading.value = true;
    const { token, role } = await doLogin({
      email: email.value,
      password: password.value,
    });

    setToken(token);

    // ✅ ใช้ role จาก response (ไม่ใช่ตัวแปรลอย ๆ)
    if (role === "Admin") {
      router.push("/dashboard-admin");
    } else {
      router.push("/dashboard");
    }
  } catch (e) {
    error.value = e?.response?.data?.message || e?.message || "เข้าสู่ระบบไม่สำเร็จ";
  } finally {
    loading.value = false;
  }
}
</script>


<style scoped>
.auth-page {
  min-height: 100vh;
  display: grid;
  place-items: center;
  background: #f6f7fb;
  padding: 20px;
}

.card {
  width: 100%;
  max-width: 420px;
  background: #fff;
  border-radius: 16px;
  box-shadow: 0 12px 40px rgba(0, 0, 0, 0.08);
  padding: 28px 24px;
}

.logo {
  width: 90px;
  display: block;
  margin: 0 auto 8px;
}

h2 {
  text-align: center;
  margin-bottom: 18px;
}

.form-group {
  margin-bottom: 12px;
}

label {
  display: block;
  margin-bottom: 6px;
  font-weight: 600;
}

input {
  width: 100%;
  padding: 10px 12px;
  border: 1px solid #d9dce3;
  border-radius: 8px;
  background: #fafafa;
  outline: none;
}

input:focus {
  border-color: #6c8cff;
  background: #fff;
}

.options {
  display: flex;
  justify-content: space-between;
  font-size: 0.92rem;
  margin: 8px 0 12px;
}

.btn {
  width: 100%;
  padding: 10px 14px;
  border: none;
  border-radius: 8px;
  background: linear-gradient(135deg, #1e88e5, #42a5f5);
  color: #fff;
  font-weight: 600;
  cursor: pointer;
}

.btn:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

.error {
  margin-top: 10px;
  color: #d93025;
}

.hint {
  margin-top: 16px;
  font-size: 0.85rem;
  color: #777;
  text-align: center;
}
</style>
