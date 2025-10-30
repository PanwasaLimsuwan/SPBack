<template>
  <button v-if="getToken()" class="btn-logout" @click="doLogout">Log out</button>
</template>

<script setup>
import { useRouter } from "vue-router";

// ฟังก์ชันสำหรับออกจากระบบ
function doLogout() {
  clearToken(); // ลบ token
  localStorage.removeItem("role"); // ลบ role
  router.push("/login"); // เปลี่ยนเส้นทางไปหน้าล็อกอิน
}

// ฟังก์ชันตรวจสอบว่า token มีอยู่ใน localStorage หรือไม่
function getToken() {
  return localStorage.getItem("token");
}

// ฟังก์ชันสำหรับลบ token
function clearToken() {
  localStorage.removeItem("token");
}

const router = useRouter();
</script>

<style scoped>
.btn-logout {
  padding: 10px 14px;
  border: none;
  border-radius: 8px;
  background-color: #e57373;
  color: white;
  font-weight: bold;
  cursor: pointer;
}

.btn-logout:hover {
  background-color: #ef5350;
}
</style>
