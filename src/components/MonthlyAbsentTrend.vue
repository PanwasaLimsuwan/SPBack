<template>
  <div id="monthly-absent-summary"></div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import axios from 'axios';
import Plotly from 'plotly.js';

const absentSummary = ref([]);

onMounted(async () => {
  await fetchAbsentData();
  drawMonthlyChart();
});

// ดึงข้อมูล GateEntry → กรองเฉพาะ status-missing
const fetchAbsentData = async () => {
  try {
    const res = await axios.get('http://localhost:5000/api/GateEntry');
    const allEmployees = res.data || [];

    // คัดเฉพาะพนักงานที่มี status-missing
    const dates = allEmployees
      .filter(emp => emp.status === 'status-missing' && emp.entryDateTime)
      .map(emp => new Date(emp.entryDateTime.replace(/-/g, '/')))
      .filter(date => !isNaN(date.getTime())); // กรองค่าที่ parse ไม่ได้

    absentSummary.value = countByMonth(dates);
  } catch (err) {
    console.error('❌ Failed to fetch absent data:', err);
  }
};

// รวมจำนวนการขาดงานตามเดือน
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

// แปลงรูปแบบเดือน เช่น "2023-11" → "Nov 2023"
const formatMonth = (monthStr) => {
  const [year, month] = monthStr.split('-');
  const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
  return `${months[parseInt(month) - 1]} ${year}`;
};

// วาดกราฟ
const drawMonthlyChart = () => {
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
