<template>
  <div>
    <h2>Forgot Password</h2>
    <form @submit.prevent="requestPasswordReset">
      <div>
        <label>Email:</label>
        <input v-model="email" type="email" placeholder="Enter your email" required />
      </div>
      <button type="submit" :disabled="loading">Send Reset Link</button>
    </form>
    <p v-if="error">{{ error }}</p>
  </div>
</template>

<script setup>
import { ref } from 'vue';
import axios from 'axios';

const email = ref('');
const error = ref('');
const loading = ref(false);

const requestPasswordReset = async () => {
  loading.value = true;
  try {
    await axios.post('http://localhost:5000/api/admin/forgot-password', { Email: email.value });
    alert('Password reset email sent');
  } catch (err) {
    error.value = err.response.data || 'An error occurred';
  } finally {
    loading.value = false;
  }
};
</script>
