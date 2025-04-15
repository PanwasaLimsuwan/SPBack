<template>
  <div id="monthly-absent-summary"></div>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue';
import axios from 'axios';
import Plotly from 'plotly.js';

// ✅ รับ props filters จาก parent
const props = defineProps({
  filters: Object
});

const absentSummary = ref([]);

// ✅ โหลดข้อมูลเมื่อ component mount
onMounted(async () => {
  await fetchAbsentData();
  drawMonthlyChart();
});

// ✅ ดูว่า filter เปลี่ยนไหม ถ้าเปลี่ยนให้ fetch ใหม่
watch(() => props.filters, async () => {
  await fetchAbsentData();
  drawMonthlyChart();
}, { deep: true });

// ✅ ดึงข้อมูล GateEntry → กรองเฉพาะ status-missing พร้อมส่ง filters
const fetchAbsentData = async () => {
  try {
    const res = await axios.get('http://localhost:5000/api/GateEntry', {
      params: {
        division: props.filters.division !== 'ALL' ? props.filters.division : undefined,
        department: props.filters.department !== 'ALL' ? props.filters.department : undefined,
        section: props.filters.section !== 'ALL' ? props.filters.section : undefined,
        biz: props.filters.biz !== 'ALL' ? props.filters.biz : undefined,
        process: props.filters.process !== 'ALL' ? props.filters.process : undefined,
      },
    });

    const allEmployees = res.data || [];

    // ✅ กรองเฉพาะ status-missing
    const dates = allEmployees
      .filter(emp => emp.status === 'status-missing' && emp.entryDateTime)
      .map(emp => new Date(emp.entryDateTime.replace(/-/g, '/')))
      .filter(date => !isNaN(date.getTime()));

    absentSummary.value = countByMonth(dates);
  } catch (err) {
    console.error('❌ Failed to fetch absent data:', err);
  }
};

// ✅ รวมจำนวนการขาดงานตามเดือน
const countByMonth = (dateList) => {
  const counts = {};

  dateList.forEach(date => {
    const key = `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}`;
    if (!counts[key]) counts[key] = 0;
    counts[key]++;
  });

  return Object.entries(counts).sort().map(([monthKey, count]) => ({
    label: formatMonth(monthKey),
    count
  }));
};

// ✅ แปลงรูปแบบเดือน
const formatMonth = (monthStr) => {
  const [year, month] = monthStr.split('-');
  const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
  return `${months[parseInt(month) - 1]} ${year}`;
};

// ✅ วาดกราฟ
const drawMonthlyChart = () => {
  if (!absentSummary.value.length) {
    Plotly.purge('monthly-absent-summary');
    return;
  }

  const x = absentSummary.value.map(item => item.label);
  const y = absentSummary.value.map(item => item.count);

  const trace = {
    x,
    y,
    type: 'bar',
    marker: {
      color: y.map(n => n >= 10 ? '#f44336' : n >= 5 ? '#ffc107' : '#4caf50'),
    },
  };

  const layout = {
    title: 'Monthly Absent',
    xaxis: { title: 'Month', tickangle: -45 },
    yaxis: { title: 'Number of Absences', dtick: 1 },
    height: 400,
    paper_bgcolor: '#fff',
    plot_bgcolor: '#f9f9f9',
    margin: { l: 60, r: 20, t: 50, b: 60 },
  };

  Plotly.newPlot('monthly-absent-summary', [trace], layout);
};
</script>

<style scoped>
#monthly-absent-summary {
  width: 100%;
  height: 100%;
}
</style>
