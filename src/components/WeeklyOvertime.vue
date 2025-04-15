<template>
  <div id="worktime-chart-container">
    <div class="filter-bar">
      <label for="week-select">เลือกสัปดาห์:</label>
      <select id="week-select" v-model="selectedWeek" @change="updateChart">
        <option v-for="week in weekOptions" :key="week" :value="week">
          {{ 'Week ' + week }}
        </option>
      </select>
    </div>
    <div v-if="hasData" id="worktime-chart"></div>
    <div v-else class="no-data">No data to display</div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue';
import axios from 'axios';
import Plotly from 'plotly.js';

const props = defineProps({ filters: Object });
const emit = defineEmits(['filter']);

const workTimeData = ref([]);
const hasData = ref(true);
const weekOptions = ref([]);
const selectedWeek = ref(null);

// ✅ Map วันเป็น label + สี
const dayLabels = ['วันจันทร์', 'วันอังคาร', 'วันพุธ', 'วันพฤหัสบดี', 'วันศุกร์'];
const dayColors = ['#FFEB3B', '#E91E63', '#4CAF50', '#FF9800', '#2196F3'];

// ✅ Fetch ข้อมูลหลัก
const fetchWorkTimeData = async () => {
  try {
    const response = await axios.get('http://localhost:5000/api/EICCControl', {
      params: {
        division: props.filters.division !== 'ALL' ? props.filters.division : undefined,
        department: props.filters.department !== 'ALL' ? props.filters.department : undefined,
        section: props.filters.section !== 'ALL' ? props.filters.section : undefined,
        biz: props.filters.biz !== 'ALL' ? props.filters.biz : undefined,
        process: props.filters.process !== 'ALL' ? props.filters.process : undefined,
      },
    });

    workTimeData.value = response.data;

    const weeks = [...new Set(workTimeData.value.map(item => item.weekID))].sort((a, b) => a - b);
    weekOptions.value = weeks;
    selectedWeek.value = weeks[weeks.length - 1]; // Default: Week ล่าสุด

    updateChart();
  } catch (error) {
    console.error('Error fetching work time data:', error);
  }
};

// ✅ Update Chart
const updateChart = () => {
  if (!workTimeData.value.length) {
    hasData.value = false;
    Plotly.purge('worktime-chart');
    return;
  }

  // ✅ Filter ข้อมูลเฉพาะ week ที่เลือก
  const selectedData = workTimeData.value
    .filter(entry => entry.status === 'Active' && entry.weekID === selectedWeek.value)
    .sort((a, b) => b.controlID - a.controlID) // controlID จากล่าสุดไปเก่าสุด
    .slice(0, 5) // เอาแค่ 5 รายการ

  if (!selectedData.length) {
    hasData.value = false;
    Plotly.purge('worktime-chart');
    return;
  }

  hasData.value = true;

  const otHours = selectedData.map(entry => entry.totalOT || 0);

  const chartData = [
    {
      x: dayLabels,
      y: otHours,
      name: 'OT Hours',
      type: 'bar',
      marker: {
        color: dayColors,
      },
    },
  ];

  const layout = {
    title: `Weekly OverTime`,
    xaxis: { title: 'วันในสัปดาห์' },
    yaxis: { title: 'OT Hours', rangemode: 'tozero' },
    paper_bgcolor: '#fff',
    plot_bgcolor: '#f9f9f9',
    height: 400,
    margin: { l: 60, r: 20, t: 50, b: 60 },
  };

  Plotly.newPlot('worktime-chart', chartData, layout).then(() => {
    document.getElementById('worktime-chart').on('plotly_click', onBarClick);
  });
};

// ✅ Click event (optional)
const onBarClick = (eventData) => {
  if (eventData.points?.length) {
    emit('filter', selectedWeek.value);
  }
};

// ✅ Lifecycle
onMounted(fetchWorkTimeData);
watch(() => props.filters, fetchWorkTimeData, { deep: true });
</script>

<style scoped>
#worktime-chart-container {
  width: 100%;
  height: 100%;
  position: relative;
  min-height: 400px;
}

.filter-bar {
  margin-bottom: 10px;
  display: flex;
  align-items: center;
}

.filter-bar label {
  margin-right: 8px;
}

.filter-bar select {
  padding: 4px 8px;
  border-radius: 4px;
}

#worktime-chart {
  width: 100%;
  height: 100%;
}

.no-data {
  text-align: center;
  color: #999;
  font-size: 16px;
  padding: 150px 0;
}
</style>
