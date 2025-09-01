<template>
  <div id="weekly-absent-by-person">
    <div class="filter-bar">
      <label for="week-select">Select Week:</label>
      <select id="week-select" v-model="selectedWeek" @change="drawChart">
        <option v-for="week in weekOptions" :key="week" :value="week">
          {{ 'Week ' + week }}
        </option>
      </select>
    </div>
    <div v-if="hasData" id="absent-person-chart"></div>
    <div v-else class="no-data">ไม่พบข้อมูล</div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch, nextTick } from 'vue';
import axios from 'axios';
import Plotly from 'plotly.js';

const props = defineProps({
  filters: {
    type: Object,
    default: () => ({}),
  },
});

const absentData = ref([]);
const selectedWeek = ref(null);
const weekOptions = ref([]);
const hasData = ref(true);

// Utility: คืนค่าเลขสัปดาห์
const getWeekNumber = (date) => {
  const start = new Date(date.getFullYear(), 0, 1);
  const diff = date - start + (start.getTimezoneOffset() - date.getTimezoneOffset()) * 60000;
  return Math.ceil((diff / 86400000 + start.getDay() + 1) / 7);
};

const fetchAbsentData = async () => {
  try {
    const res = await axios.get('https://databasemanpowerdb.database.windows.net/api/Attendance', {
      params: {
        division: props.filters.division !== 'ALL' ? props.filters.division : undefined,
        department: props.filters.department !== 'ALL' ? props.filters.department : undefined,
        section: props.filters.section !== 'ALL' ? props.filters.section : undefined,
        biz: props.filters.biz !== 'ALL' ? props.filters.biz : undefined,
        process: props.filters.process !== 'ALL' ? props.filters.process : undefined,
      },
    });

    const filtered = res.data
      .filter(item => item.status === 'late' || item.status === 'Missing')
      .map(item => {
        const date = item.date ? new Date(item.date) : null;
        return {
          ...item,
          dateObj: date,
          weekNumber: date ? getWeekNumber(date) : null,
        };
      })
      .filter(item => item.weekNumber !== null);

    absentData.value = filtered;

    const weeks = [...new Set(filtered.map(i => i.weekNumber))].sort((a, b) => a - b);
    weekOptions.value = weeks;
    selectedWeek.value = weeks[weeks.length - 1];

    await nextTick();
    drawChart();
  } catch (err) {
    console.error('❌ Error fetching data:', err);
  }
};

const drawChart = () => {
  const dataForWeek = absentData.value.filter(i => i.weekNumber === selectedWeek.value);
  if (!dataForWeek.length) {
    hasData.value = false;
    Plotly.purge('absent-person-chart');
    return;
  }

  const grouped = {};
  dataForWeek.forEach(item => {
    const process = item.process || 'ไม่ระบุ';
    if (!grouped[process]) grouped[process] = 0;
    grouped[process]++;
  });

  const names = Object.keys(grouped);
  const counts = names.map(name => grouped[name]);

  if (counts.every(c => c === 0)) {
    hasData.value = false;
    Plotly.purge('absent-person-chart');
    return;
  }

  hasData.value = true;

  // กำหนดสีตามชื่อ process
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
  'ELU2': '#F8C471',    // สีทองอ่อน
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

// นำไปใช้ในการกำหนดสีในแผนภูมิ
const colors = names.map(name => colorMapping[name] || '#4caf50'); // ถ้าไม่มีการกำหนดสี จะใช้สีเขียว

  const trace = {
    x: names,
    y: counts,
    type: 'bar',
    marker: {
      color: colors,
    },
  };

  const layout = {
    title: `Weekly Absent Trend (Week ${selectedWeek.value})`,
    height: 400,
    xaxis: { title: 'Process', tickangle: -45 },
    // yaxis: { title: 'จำนวนขาด/สาย' },
    margin: { l: 60, r: 20, t: 50, b: 100 },
    paper_bgcolor: '#fff',
    plot_bgcolor: '#f9f9f9',
  };

  Plotly.newPlot('absent-person-chart', [trace], layout);
};

onMounted(fetchAbsentData);
watch(() => props.filters, fetchAbsentData, { deep: true });
</script>

<style scoped>
#weekly-absent-by-person {
  width: 100%;
  height: 100%;
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
.no-data {
  text-align: center;
  padding: 100px 0;
  color: #999;
}
</style>
