<template>
  <div id="weekly-absent-trend"></div>
</template>

<script setup>
import { ref, onMounted, watch, nextTick } from 'vue';
import Plotly from 'plotly.js';
import axios from 'axios';

// ✅ รับ props filters
const props = defineProps({
  filters: {
    type: Object,
    default: () => ({}),
  },
});

const absentData = ref([]);

// ฟังก์ชันดึงข้อมูลจาก backend
const fetchAbsentData = async () => {
  try {
    const res = await axios.get('http://localhost:5000/api/Attendance', {
      params: {
        division: props.filters.division !== 'ALL' ? props.filters.division : undefined,
        department: props.filters.department !== 'ALL' ? props.filters.department : undefined,
        section: props.filters.section !== 'ALL' ? props.filters.section : undefined,
        biz: props.filters.biz !== 'ALL' ? props.filters.biz : undefined,
        process: props.filters.process !== 'ALL' ? props.filters.process : undefined,
      },
    });

    // ตรวจสอบข้อมูลที่ได้รับจาก backend
    console.log(res.data);
    absentData.value = extractAbsentDates(res.data);
    await nextTick();
    drawChart();
  } catch (err) {
    console.error('❌ Error fetching absent data:', err);
  }
};

// ฟังก์ชันแยกข้อมูลการขาดงานออกมา
const extractAbsentDates = (data) => {
  return data.map(emp => {
    const absentDates = [];

    if (emp.status === 'late' || emp.status === 'normal') {  // เช็คการขาดงานหรือสถานะปกติ
      const fixedDate = emp.date.replace(/-/g, '/');  // แปลงวันที่ที่ได้รับจาก API
      const dateObj = new Date(fixedDate);
      if (!isNaN(dateObj)) {
        absentDates.push(dateObj);
      }
    }

    return { ...emp, absentDates };
  });
};

// ฟังก์ชันคำนวณหมายเลขสัปดาห์จากวันที่
const getWeekNumber = (date) => {
  const startDate = new Date(date.getFullYear(), 0, 1);
  const diff = date - startDate;
  const oneDay = 1000 * 60 * 60 * 24;
  return Math.ceil(diff / oneDay / 7);
};

// ฟังก์ชันสร้างกราฟ
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

  // ตรวจสอบข้อมูลใน weeklyCount และ weeksSet
  console.log("Weekly Count:", weeklyCount);

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
    title: 'Weekly Absent Trend',
    barmode: 'group',
    height: 400,
    xaxis: { title: 'Week Number', dtick: 1 },
    yaxis: { title: 'Total Absences', dtick: 1 },
    paper_bgcolor: '#fff',
    plot_bgcolor: '#f9f9f9',
    legend: { orientation: 'h', x: 0.5, xanchor: 'center', y: 1.1 },
    margin: { l: 60, r: 20, t: 50, b: 60 },
  };

  // ตรวจสอบว่า div มี ID ที่ถูกต้อง
  Plotly.newPlot('weekly-absent-trend', [trace], layout);
};

// ✅ Lifecycle
onMounted(fetchAbsentData);

// ✅ Watch filters → refetch data + update chart
watch(() => props.filters, async () => {
  await fetchAbsentData();
}, { deep: true });
</script>

<style scoped>
#weekly-absent-trend {
  width: 100%;
  height: 100%;
}
</style>
