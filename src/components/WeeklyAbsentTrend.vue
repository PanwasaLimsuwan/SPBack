<template>
  <div id="weekly-absent-trend"></div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import Plotly from 'plotly.js';
import axios from 'axios';

const absentData = ref([]);

onMounted(async () => {
  await fetchAbsentData();
  drawChart(); // เรียกใช้ชื่อฟังก์ชันใหม่
});

const fetchAbsentData = async () => {
  try {
    const res = await axios.get('http://localhost:5000/api/GateEntry');
    absentData.value = extractAbsentDates(res.data);
  } catch (err) {
    console.error('❌ Error fetching absent data:', err);
  }
};

const extractAbsentDates = (data) => {
  return data.map(emp => {
    const absentDates = [];

    if (emp.status === 'status-missing' && emp.entryDateTime) {
      const fixedDate = emp.entryDateTime.replace(/-/g, '/');
      const dateObj = new Date(fixedDate);
      if (!isNaN(dateObj)) {
        absentDates.push(dateObj);
      }
    }

    return { ...emp, absentDates };
  });
};

const getWeekNumber = (date) => {
  const startDate = new Date(date.getFullYear(), 0, 1);
  const diff = date - startDate;
  const oneDay = 1000 * 60 * 60 * 24;
  return Math.ceil(diff / oneDay / 7);
};

const drawChart = () => {
  const weeklyCount = {};
  const weeksSet = new Set();

  absentData.value.forEach(emp => {
    emp.absentDates.forEach(date => {
      const week = getWeekNumber(date);
      weeksSet.add(week);
      if (!weeklyCount[week]) weeklyCount[week] = 0;
      weeklyCount[week]++;
    });
  });

  const sortedWeeks = Array.from(weeksSet).sort((a, b) => a - b);
  const weeksFormatted = sortedWeeks.map(w => `Week ${w}`);
  const counts = sortedWeeks.map(w => weeklyCount[w] || 0);

  const trace = {
    x: weeksFormatted,
    y: counts,
    type: 'bar',
    name: 'Total Absences',
    marker: {
      color: counts.map(c => c >= 10 ? '#f44336' : c >= 5 ? '#ffc107' : '#4caf50'),
    },
  };

  const layout = {
    title: 'Weekly Absent',
    barmode: 'group',
    height: 400,
    xaxis: { title: 'Week Number', dtick: 1 },
    yaxis: { title: 'Total Absences', dtick: 1 },
    paper_bgcolor: '#fff',
    plot_bgcolor: '#f9f9f9',
    legend: { orientation: 'h', x: 0.5, xanchor: 'center', y: 1.1 },
    margin: { l: 60, r: 20, t: 50, b: 60 },
  };

  Plotly.newPlot('weekly-absent-trend', [trace], layout);
};
</script>

<style scoped>
#weekly-absent-trend {
  width: 100%;
  height: 100%;
}
</style>
