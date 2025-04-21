<template>
  <div id="worktime-chart-container">
    <div class="filter-bar">
      <label for="week-select">Select Week:</label>
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

// ✅ กำหนดสีตามชื่อ process
const colorMapping = {
  'ASSY': '#FF5733',    // สีแดง
  'MOKU': '#33FF57',    // สีเขียว
  'CSAT': '#3357FF',    // สีน้ำเงิน
  'JUNB': '#FFC300',    // สีเหลือง
  'BMS': '#8E44AD',     // สีม่วง
  'BURN': '#FF6347',    // สีมะเขือเทศ
  'CSAT3': '#F39C12',   // สีทอง
  'PCLN': '#1ABC9C',    // สีเขียวมิ้นท์
  'PA': '#D35400',      // สีส้ม
  'JIK': '#2980B9',     // สีฟ้า
  'MPK': '#2C3E50',     // สีน้ำเงินเข้ม
  'KEN': '#7F8C8D',     // สีเทาควันบุหรี่
  'PK': '#34495E',      // สีน้ำเงินกรมท่า
  'PCL': '#16A085',     // สีเขียวมรกต
  'ELU1': '#2ECC71',    // สีเขียวสด
  'AG': '#8E44AD',      // สีม่วง
  'HTH': '#F1C40F',     // สีทองเหลือง
  'TKA': '#9B59B6',     // สีม่วงอ่อน
  'EGC': '#F39C12',     // สีทอง
  'LBL': '#1F618D',     // สีน้ำเงินเข้ม
  'KOC': '#2E4053',     // สีเทาเข้ม
  'KSP': '#A569BD',     // สีม่วง
  'KDU': '#F4D03F',     // สีเหลือง
  'INF': '#7D3C98',     // สีม่วงเข้ม
  'PF': '#FF7F50',      // สีปะการัง
  'ELU2': '#F8C471',    // สีทองอ่อน
  'FC': '#85C1AE',      // สีเขียวฟ้า
};

// ✅ Fetch ข้อมูลหลัก
const fetchWorkTimeData = async () => {
  try {
    const response = await axios.get('https://deploymanpowerdb-f5a0h6fqaehdajck.southeastasia-01.azurewebsites.net/api/EICCControl', {
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
    .filter(entry => entry.status === 'Active' && entry.weekID === selectedWeek.value);

  if (!selectedData.length) {
    hasData.value = false;
    Plotly.purge('worktime-chart');
    return;
  }

  hasData.value = true;

  // ✅ Group ข้อมูลตาม process และรวม OT ของแต่ละ process
  const processMap = {};
  selectedData.forEach(entry => {
    const key = entry.process || 'ไม่ระบุ';
    processMap[key] = (processMap[key] || 0) + (entry.totalOT || 0);
  });

  const processes = Object.keys(processMap);
  const otHours = Object.values(processMap);

  // กำหนดสีตามชื่อ process จาก colorMapping
  const colors = processes.map(process => colorMapping[process] || '#4CAF50'); // ถ้าไม่มีการกำหนดสี จะใช้สีเขียว

  const chartData = [
    {
      x: processes,
      y: otHours,
      name: 'OT Hours',
      type: 'bar',
      marker: {
        color: colors, // ใช้สีจาก colors
      },
    },
  ];

  const layout = {
    title: `Weekly Overtime (Week ${selectedWeek.value})`,
    xaxis: { title: 'Process' },
    yaxis: { rangemode: 'tozero' },
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
