<template>
  <div class="auth-page">
    <div class="card">
      <img src="/logo2.png" alt="Logo" class="logo" />
      <h2 v-if="isLogin">Sign in</h2>
      <h2 v-if="!isLogin">Sign up</h2>

      <!-- Register Form -->
      <form @submit.prevent="onSubmit">
        <div class="form-group">
          <label>Username</label>
          <input
            v-model.trim="username"
            type="text"
            placeholder="Enter username"
            autocomplete="username"
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
          <label>Employee ID</label>
          <input
            v-model.trim="empID"
            type="text"
            placeholder="Enter employee ID"
            :disabled="loading"
          />
        </div>

        <div class="form-group">
          <label>Role</label>
          <select v-model="role" :disabled="loading">
            <option value="admin">Admin</option>
            <option value="leader">Leader</option>
            <option value="employee">Employee</option>
          </select>
        </div>

        <div class="options">
          <label><input type="checkbox" v-model="showPassword" /> Show password</label>
          <label><input type="checkbox" v-model="remember" /> Remember me</label>
        </div>

        <button class="btn" type="submit" :disabled="loading">
          <span v-if="!loading">{{ isLogin ? 'Login' : 'Sign up' }}</span>
          <span v-else>Signing in...</span>
        </button>

        <p v-if="error" class="error">{{ error }}</p>
      </form>

      <p class="hint">
        <span v-if="isLogin">Don't have an account?</span>
        <span v-if="!isLogin">Already have an account?</span>
        <a href="#" @click.prevent="toggleAuthMode">{{ isLogin ? 'Sign up' : 'Login' }}</a>
      </p>
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

// login function (mock หรือ api จริง)
async function doLogin({ username, password }) {
  const { data } = await axios.post(`${API_BASE_URL}/api/Admin/login`, {
    Username: username,
    Password: password,  // ส่งรหัสผ่านธรรมดา
  });

  if (!data?.token) throw new Error("ไม่พบโทเคนจากระบบ");
  setToken(data.token);
  return data;
}

async function doRegister({ username, password, email, empID, role }) {
  const { data } = await axios.post(`${API_BASE_URL}/api/Admin/register`, {
    Username: username,
    Password: password,
    Email: email,  // ส่งอีเมล
    EmpID: empID,  // ส่งรหัสพนักงาน
    Role: role, // ส่ง role
  });
  if (!data?.token) throw new Error("ไม่พบโทเคนจากระบบ");
  setToken(data.token);
  return data;
}

// ====== State ======
const route = useRoute();
const router = useRouter();

const username = ref("");
const password = ref("");
const email = ref("");
const empID = ref("");
const role = ref("employee");  // Default role is 'employee'
const remember = ref(true);
const showPassword = ref(false);
const loading = ref(false);
const error = ref("");
const isLogin = ref(true);  // This controls whether it's Login or Register

// Toggle between Login and Register
function toggleAuthMode() {
  isLogin.value = !isLogin.value;
}

// ====== Methods ======
async function onSubmit() {
  error.value = "";
  if (!username.value || !password.value || !email.value || !empID.value || !role.value) {
    error.value = "กรุณากรอกข้อมูลทั้งหมด";
    return;
  }

  try {
    loading.value = true;
    let token;

    if (isLogin.value) {
      const { Token: loginToken } = await doLogin({
        username: username.value,
        password: password.value
      });
      token = loginToken;
    } else {
      const { Token: registerToken } = await doRegister({
        username: username.value,
        password: password.value,
        email: email.value,
        empID: empID.value,
        role: role.value
      });
      token = registerToken;
    }

    setToken(token);
    router.push("/dashboard");
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
