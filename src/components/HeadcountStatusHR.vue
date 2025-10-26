<script setup>
import { ref, onMounted, watch, nextTick } from 'vue';
import axios from 'axios';
import Plotly from 'plotly.js';

const props = defineProps({
  filters: Object
});

const emit = defineEmits(['filter-status']);
const headcountData = ref([]);
const isLoading = ref(false);

// ✅ ดึงข้อมูล headcount จาก API พร้อม filter
const fetchHeadcountData = async () => {
  isLoading.value = true;
  try {
    const response = await axios.get('http://localhost:5000/api/Attendance/ByDate', {
      params: {
        division: props.filters.division !== 'ALL' ? props.filters.division : undefined,
        department: props.filters.department !== 'ALL' ? props.filters.department : undefined,
        section: props.filters.section !== 'ALL' ? props.filters.section : undefined,
        biz: props.filters.biz !== 'ALL' ? props.filters.biz : undefined,
        process: props.filters.process !== 'ALL' ? props.filters.process : undefined,
      },
    });

    headcountData.value = response.data;  // ใช้ข้อมูลจาก Attendance
  } catch (error) {
    console.error('Error fetching headcount data:', error);
  } finally {
    isLoading.value = false;
    await nextTick(); // ✅ รอ DOM พร้อม
    drawChart();
  } // <-- ปิดปีกกาให้ถูกต้อง
};

// ✅ ฟังก์ชันวาดกราฟ Pie Chart
const drawChart = () => {
  if (!headcountData.value.length) return;

  const statusCounts = headcountData.value.reduce(
    (acc, entry) => {
      acc[entry.status] = (acc[entry.status] || 0) + 1;
      return acc;
    },
    { 'normal': 0, 'late': 0, 'Missing': 0 }
  );

  const data = [{
    labels: ['Normal', 'Late', 'Missing'],
    values: [
      statusCounts['normal'],
      statusCounts['late'],
      statusCounts['Missing'],
    ],
    type: 'pie',
    marker: {
      colors: ['#2ECC71', '#F39C12', '#E74C3C'],
    },
    textinfo: 'label+percent',
    hoverinfo: 'label+value+percent',
  }];

  const layout = {
    title: 'Headcount Status',
    height: 500,
    width: 500,
  };

  Plotly.newPlot('headcount-pie-chart', data, layout);

  const chart = document.getElementById('headcount-pie-chart');
  chart.on('plotly_click', (data) => {
    const clickedLabel = data.points[0].label;
    console.log('Clicked Label:', clickedLabel); // ตรวจสอบค่าที่คลิก
    const labelToStatus = {
      'Normal': 'normal',
      'Late': 'late',
      'Missing': 'Missing',
    };
    emit('filter-status', labelToStatus[clickedLabel]); // ส่งค่ากลับไปที่ตาราง
  });
};


// ✅ ดูค่าจาก filter ถ้ามีการเปลี่ยนแปลง -> reload ข้อมูลใหม่
watch(() => props.filters, async () => {
  await fetchHeadcountData();
}, { deep: true });

// ✅ เริ่มต้น mount component -> fetch data และวาด chart
onMounted(async () => {
  await fetchHeadcountData();
});
</script>

<template>
  <div class="headcount-pie-chart">
    <div v-if="isLoading" class="loading">Loading headcount data...</div>
    <div v-else>
      <div id="headcount-pie-chart"></div>
    </div>
  </div>
</template>

<style scoped>
#headcount-pie-chart {
  width: 100%;
  max-width: 500px;
  margin: auto;
}

.loading {
  text-align: center;
  font-weight: bold;
  color: #888;
  padding: 20px;
}

.refresh-skill-btn {
  display: flex;
  align-items: center;
  gap: 8px;
  background-color: blue;
  color: white;
  border: none;
  border-radius: 25px;
  padding: 6px 14px;
  margin-bottom: 10px;
  cursor: pointer;
  font-weight: bold;
  font-size: 14px;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.2);
  transition: background-color 0.3s ease, transform 0.3s ease;
}

.refresh-skill-btn:hover {
  background-color: #0056b3;
  transform: scale(1.05);
}

.refresh-skill-btn .icon {
  width: 18px;
  height: 18px;
  filter: invert(1);
}
</style>
