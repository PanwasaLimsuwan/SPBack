<script setup>
import { ref, onMounted, watch } from 'vue'
import axios from 'axios'
import Plotly from 'plotly.js'

// ตัวแปรที่ใช้เก็บข้อมูลกราฟ
const selectedChart = ref('process') // default = process
const trainingData = ref([])

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

// ตัวกรองที่ใช้ในคำขอ
const filters = ref({
  division: 'ALL',
  department: 'ALL',
  section: 'ALL',
  biz: 'ALL',
  process: 'ALL',
  search: '',
});

// ฟังก์ชันกรองข้อมูลเมื่อเปลี่ยนแปลงฟิลเตอร์
const fetchFilteredData = async () => {
  try {
    const response = await axios.get('http://localhost:5000/api/OJTandInspectionSkill', {
      params: {
        division: filters.value.division !== 'ALL' ? filters.value.division : undefined,
        department: filters.value.department !== 'ALL' ? filters.value.department : undefined,
        section: filters.value.section !== 'ALL' ? filters.value.section : undefined,
        biz: filters.value.biz !== 'ALL' ? filters.value.biz : undefined,
        process: filters.value.process !== 'ALL' ? filters.value.process : undefined,
        search: filters.value.search,
      },
    });

    // กรองข้อมูลเฉพาะคนที่ active <= 2
    trainingData.value = response.data.filter(item => item.active <= 2);
    drawChart();
  } catch (error) {
    console.error('Error fetching data:', error);
  }
};

// คำนวณกราฟจากข้อมูลที่ได้
const drawChart = () => {
  const groupBy = selectedChart.value;
  const grouped = {};
  const colors = [];

  trainingData.value.forEach(item => {
    const key = item[groupBy] || 'Unknown'; // ใช้ 'process' หรือ 'biz'
    grouped[key] = (grouped[key] || 0) + 1;

    // กำหนดสีตามชื่อ process
    const process = item.process || 'Unknown';
    const color = colorMapping[process] || '#BDC3C7'; // สีเทาเป็นค่าเริ่มต้น
    colors.push(color);
  });

  const xLabels = Object.keys(grouped);
  const yValues = Object.values(grouped);

  const chartData = [
    {
      x: xLabels,
      y: yValues,
      type: 'bar',
      marker: { color: colors },
      text: yValues.map(v => `${v} คน`),
      textposition: 'auto',
    },
  ];

  const layout = {
    title: `Training Employee by ${groupBy.charAt(0).toUpperCase() + groupBy.slice(1)}`,
    xaxis: { title: groupBy === 'process' ? 'Process' : 'Biz', tickangle: -30 },
    yaxis: {},
    height: 450,
    paper_bgcolor: 'rgba(0,0,0,0)',
    plot_bgcolor: '#fff',
    margin: { t: 50, l: 50, r: 30, b: 80 },
  };

  Plotly.newPlot('training-chart', chartData, layout);
};

// เรียกใช้ fetchFilteredData เมื่อฟิลเตอร์เปลี่ยน
watch(filters, fetchFilteredData, { deep: true });

// โหลดข้อมูลตอน onMounted
onMounted(fetchFilteredData);

</script>

<template>
  <div class="training-employee-container">
    <!-- ลบตัวเลือก filter ออกไป -->
    <div id="training-chart"></div>
  </div>
</template>

<style scoped>
.training-employee-container {
  width: 100%;
  height: 100%;
}
#training-chart {
  width: 100%;
  height: 100%;
}
</style>
