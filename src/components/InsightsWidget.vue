<script setup>
import { ref, watch } from 'vue';
import axios from 'axios';

const props = defineProps({
  filters: Object
});

const api = axios.create({ baseURL: 'http://localhost:5000/api' });

const loading = ref(false);
const selected = ref('skill-gaps'); // ค่าเริ่มต้น
const params = ref({
  year: new Date().getFullYear(),
  month: new Date().getMonth()+1,
  weeks: 4
});

const rows = ref([]);
const title = ref('');

const presets = [
  { key: 'skill-gaps', label: 'Skill gaps by process' },
  { key: 'ot-hotspots', label: 'OT hotspots (last N weeks)' },
  { key: 'headcount-vs-plan', label: 'Headcount vs plan variance' },
  { key: 'absence-streaks', label: 'Consecutive absence (>=2 days)' },
  { key: 'eicc-risk', label: 'EICC risk (hours)' },
];

async function run() {
  loading.value = true;
  try {
    const body = {
      key: selected.value,
      year: params.value.year,
      month: params.value.month,
      weeks: params.value.weeks,
      division: props.filters?.division ?? 'ALL',
      department: props.filters?.department ?? 'ALL',
      section: props.filters?.section ?? 'ALL',
      biz: props.filters?.biz ?? 'ALL',
      process: props.filters?.process ?? 'ALL'
    };
    const res = await api.post('/Analytics/run', body);
    title.value = res.data.title;
    rows.value = Array.isArray(res.data.data) ? res.data.data : [];
  } catch (error) {
    console.error('API Error:', error.response ? error.response.data : error.message);
    title.value = "Error: Unable to fetch data";
  } finally {
    loading.value = false;
  }
}

watch(() => ({...props.filters}), () => run(), { deep:true, immediate:true });
</script>

<template>
  <div class="insights">
    <div class="insights-bar">
      <label>
        Insight:
        <select v-model="selected">
          <option v-for="p in presets" :key="p.key" :value="p.key">{{ p.label }}</option>
        </select>
      </label>

      <label v-if="selected==='ot-hotspots'">
        Weeks: <input type="number" v-model.number="params.weeks" min="1" max="12" />
      </label>
      <label v-if="['headcount-vs-plan','absence-streaks','eicc-risk'].includes(selected)">
        Year: <input type="number" v-model.number="params.year" min="2020" max="2100" />
      </label>
      <label v-if="['absence-streaks','eicc-risk'].includes(selected)">
        Month: <input type="number" v-model.number="params.month" min="1" max="12" />
      </label>

      <button @click="run" :disabled="loading">{{ loading ? 'Running...' : 'Run' }}</button>
    </div>

    <h3 style="margin:8px 0 12px;">{{ title }}</h3>

    <div v-if="rows.length===0 && !loading" class="muted">No data</div>

    <div class="table-wrapper" v-else>
      <table class="table-basic">
        <thead>
          <tr>
            <th v-for="col in Object.keys(rows[0] || {})" :key="col">{{ col }}</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="(r, i) in rows" :key="i">
            <td v-for="(v, k) in r" :key="k">{{ v }}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<style scoped>
.insights { display:flex; flex-direction:column; gap:10px; }
.insights-bar { display:flex; gap:12px; align-items:center; flex-wrap:wrap; }
.table-wrapper { overflow:auto; }
.table-basic { width:100%; border-collapse:collapse; }
.table-basic th, .table-basic td { border:1px solid #e5e7eb; padding:8px 10px; text-align:left; }
.muted { color:#6b7280; font-size:14px; }
</style>
