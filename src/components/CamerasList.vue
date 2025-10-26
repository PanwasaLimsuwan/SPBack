<!-- src/components/CamerasList.vue -->
<template>
  <div class="p-4 max-w-5xl mx-auto">
    <div class="flex items-center justify-between gap-2 mb-4">
      <h1 class="text-xl font-semibold">Cameras</h1>
      <div class="flex items-center gap-2">
        <input
          v-model="q"
          type="text"
          placeholder="Search location/description…"
          class="border rounded px-3 py-2 w-64"
        />
        <button
          @click="fetchCameras"
          :disabled="loading"
          class="px-3 py-2 rounded bg-black text-white disabled:opacity-60"
        >
          {{ loading ? 'Loading…' : 'Refresh' }}
        </button>
      </div>
    </div>

    <div v-if="error" class="bg-red-50 text-red-700 border border-red-200 p-3 rounded mb-3">
      {{ error }}
    </div>

    <div v-if="!loading && filtered.length === 0" class="text-gray-500">
      ไม่พบข้อมูลกล้อง
    </div>

    <div class="overflow-auto rounded border" v-if="filtered.length">
      <table class="min-w-full bg-white">
        <thead class="bg-gray-50 border-b">
          <tr>
            <th class="text-left p-3">ID</th>
            <th class="text-left p-3">Location</th>
            <th class="text-left p-3">Description</th>
            <th class="text-right p-3">Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="c in filtered" :key="c.cameraID" class="border-b hover:bg-gray-50">
            <td class="p-3">{{ c.cameraID }}</td>
            <td class="p-3">{{ c.location }}</td>
            <td class="p-3">{{ c.description }}</td>
            <td class="p-3 text-right">
              <button @click="viewDetail(c.cameraID)" class="px-2 py-1 border rounded">
                View
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Detail -->
    <div v-if="detail" class="mt-6 border rounded p-4 bg-gray-50">
      <div class="flex items-center justify-between">
        <h2 class="font-semibold">Camera #{{ detail.cameraID }}</h2>
        <button @click="detail = null" class="text-sm underline">Close</button>
      </div>
      <div class="mt-2">
        <div><span class="font-medium">Location:</span> {{ detail.location }}</div>
        <div><span class="font-medium">Description:</span> {{ detail.description }}</div>
      </div>
    </div>
  </div>
</template>

<script setup>
import axios from "axios";
import { ref, computed, onMounted } from "vue";

/* ===== Vue CLI env (webpack) =====
   ตั้งในไฟล์ .env.development:
   VUE_APP_API_BASE=https://realtimeapi-14101.azurewebsites.net
*/
const API_BASE =
  (process.env && process.env.VUE_APP_API_BASE) ||
  "https://realtimeapi-14101.azurewebsites.net";

const api = axios.create({
  baseURL: String(API_BASE).replace(/\/+$/, ""), // ตัด '/' ท้าย
  timeout: 15000,
});

const loading = ref(false);
const error = ref("");
const list = ref([]);
const q = ref("");
const detail = ref(null);

const fetchCameras = async () => {
  loading.value = true;
  error.value = "";
  detail.value = null;
  try {
    const res = await api.get("/api/cameras");
    list.value = Array.isArray(res.data) ? res.data : [];
  } catch (err) {
    error.value = err?.response?.data ?? err?.message ?? "โหลดข้อมูลไม่สำเร็จ";
    console.error(err);
  } finally {
    loading.value = false;
  }
};

const viewDetail = async (id) => {
  loading.value = true;
  error.value = "";
  try {
    const res = await api.get(`/api/cameras/${encodeURIComponent(id)}`);
    detail.value = res.data;
  } catch (err) {
    error.value = err?.response?.data ?? err?.message ?? "โหลดรายละเอียดไม่สำเร็จ";
    console.error(err);
  } finally {
    loading.value = false;
  }
};

const filtered = computed(() => {
  const s = q.value.trim().toLowerCase();
  if (!s) return list.value;
  return list.value.filter((x) =>
    [x.location ?? "", x.description ?? "", x.cameraID ?? ""].some((v) =>
      String(v).toLowerCase().includes(s)
    )
  );
});

onMounted(fetchCameras);
</script>

<style scoped>
table { border-collapse: collapse; width: 100%; }
th, td { border-bottom: 1px solid #e5e7eb; }
.text-gray-500 { color: #6b7280; }
.bg-gray-50 { background: #f9fafb; }
.border { border: 1px solid #e5e7eb; }
.rounded { border-radius: 0.5rem; }
.p-3 { padding: 0.75rem; }
.p-4 { padding: 1rem; }
.px-2 { padding-left: 0.5rem; padding-right: 0.5rem; }
.px-3 { padding-left: 0.75rem; padding-right: 0.75rem; }
.py-1 { padding-top: 0.25rem; padding-bottom: 0.25rem; }
.py-2 { padding-top: 0.5rem; padding-bottom: 0.5rem; }
.mt-2 { margin-top: 0.5rem; }
.mt-6 { margin-top: 1.5rem; }
.mb-3 { margin-bottom: 0.75rem; }
.mb-4 { margin-bottom: 1rem; }
.max-w-5xl { max-width: 64rem; }
.mx-auto { margin-left: auto; margin-right: auto; }
.flex { display: flex; }
.items-center { align-items: center; }
.justify-between { justify-content: space-between; }
.gap-2 { gap: 0.5rem; }
.text-right { text-align: right; }
.text-white { color: #fff; }
.text-xl { font-size: 1.25rem; line-height: 1.75rem; }
.font-semibold { font-weight: 600; }
.bg-black { background: #111827; }
.disabled\:opacity-60:disabled { opacity: 0.6; }
.border-b { border-bottom: 1px solid #e5e7eb; }
.hover\:bg-gray-50:hover { background: #f9fafb; }
.w-64 { width: 16rem; }
</style>
