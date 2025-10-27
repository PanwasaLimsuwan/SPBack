// src/services/AuthService.js

import axios from 'axios';

const API_BASE_URL = 'http://localhost:5000';

// ฟังก์ชันการลงทะเบียนที่ Admin จะเป็นคนลงทะเบียนให้
export async function register({ empID, firstName, lastName, email, role }) {
  try {
    const { data } = await axios.post(`${API_BASE_URL}/api/Admin/register`, {
      EmpID: empID,         // ส่ง EmpID
      FirstName: firstName, // ส่ง FirstName
      LastName: lastName,   // ส่ง LastName
      Email: email,         // ส่ง Email
      Role: role,           // ส่ง Role
    });

    // ตรวจสอบว่าได้ token หรือไม่
    if (data?.token) {
      return data.token; // ส่งคืน token
    } else {
      throw new Error("ไม่พบ token จากระบบ");
    }
  } catch (error) {
    // ถ้าเกิดข้อผิดพลาด ให้แสดงข้อความที่ได้จาก API หรือข้อความผิดพลาดทั่วไป
    throw new Error(error.response?.data?.message || 'Registration failed');
  }
}

// ฟังก์ชันการเข้าสู่ระบบ (ใช้ Email แทน Username)
export async function login({ email, password }) {
  try {
    const { data } = await axios.post(`${API_BASE_URL}/api/Admin/login`, {
      Email: email,    // ใช้ Email แทน Username
      Password: password,
    });

    if (data?.token) {
      return data.token; // ส่งคืน token
    } else {
      throw new Error("ไม่พบ token จากระบบ");
    }
  } catch (error) {
    throw new Error(error.response?.data?.message || 'Login failed');
  }
}
