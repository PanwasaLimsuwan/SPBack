<template>
  <div class="chart-container">
    <div class="filter-bar">
      <!-- ตัวเลือกเดือน -->
      <label for="month-select">Select Month:</label>
      <select id="month-select" v-model="selectedMonth" @change="fetchAbsentData">
        <option v-for="(month, index) in months" :key="index" :value="index + 1">
          {{ month }}
        </option>
      </select>
    </div>

    <!-- แสดงกราฟ -->
    <div id="monthly-absent-summary" class="scrollable-chart"></div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue';
import axios from 'axios';
import Plotly from 'plotly.js';

// ✅ กำหนดค่าตัวกรองเดือน
const selectedMonth = ref(null);  // เดือนที่เลือก
const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];

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

// ✅ ฟังก์ชันดึงข้อมูลและกรองตามเดือนที่เลือก
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

    const allEmployees = res.data || [];

    // ✅ กรองเฉพาะ status-missing
    const dates = allEmployees
      .filter(emp => (emp.status === 'late' || emp.status === 'Missing') && (emp.date || emp.entryDateTime))
      .map(emp => ({
        name: `${emp.firstName} ${emp.lastName}`,  // Employee name
        date: new Date(emp.date),  // Date of absence
      }))
      .filter(({ date }) => !isNaN(date.getTime()));

    // ✅ กรองข้อมูลตามเดือนที่เลือก
    if (selectedMonth.value) {
      absentSummary.value = countByEmployeeAndMonth(dates).filter(item => new Date(item.month).getMonth() + 1 === selectedMonth.value);
    } else {
      absentSummary.value = countByEmployeeAndMonth(dates);  // ถ้าไม่เลือกเดือนให้แสดงทั้งหมด
    }

    drawMonthlyChart();
  } catch (err) {
    console.error('❌ Failed to fetch absent data:', err);
  }
};

// ✅ รวมจำนวนการขาดงานตามพนักงานและเดือน
const countByEmployeeAndMonth = (dateList) => {
  const counts = {};

  dateList.forEach(({ name, date }) => {
    const key = `${name}-${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}`;
    if (!counts[key]) counts[key] = 0;
    counts[key]++;
  });

  return Object.entries(counts).map(([key, count]) => {
    const [name, year, month] = key.split('-');
    return {
      name,
      month: formatMonth(`${year}-${month}`),
      count
    };
  });
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

  const y = absentSummary.value.map(item => `${item.name}`);  // พนักงานและเดือน
  const x = absentSummary.value.map(item => item.count);  // จำนวนการขาดงาน

  const trace = {
    x,
    y,
    type: 'bar',
    orientation: 'h', // แนวนอน
    marker: {
      color: '#f44336',  // สีแดงสำหรับทุกบาร์
    },
  };

  const layout = {
    title: 'Monthly Absent',
    xaxis: {
      title: 'จำนวนวันที่ขาดงาน',
      dtick: 1, // กำหนดให้มีระยะห่าง
      automargin: true
    },
    yaxis: {
      // title: 'Employee and Month',
      automargin: true
    },
    height: Math.max(400, absentSummary.value.length * 20),  // ปรับความสูงกราฟ
    paper_bgcolor: '#fff',
    plot_bgcolor: '#f9f9f9',
    margin: { l: 100, r: 20, t: 50, b: 60 },
  };

  Plotly.newPlot('monthly-absent-summary', [trace], layout);
};
</script>

<style scoped>
#monthly-absent-summary {
  width: 100%;
  height: 100%;
}

.chart-container {
  max-height: 400px;
  overflow-y: auto;
  padding: 15px;
  background: #f0f4f8;
  border-radius: 10px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
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

.scrollable-chart::-webkit-scrollbar-thumb {
  background-color: rgba(100, 100, 100, 0.2);
  border-radius: 4px;
}
</style>
