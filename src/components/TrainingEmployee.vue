<script setup>
import { ref, onMounted, watch } from 'vue'
import axios from 'axios'
import Plotly from 'plotly.js'

// รับ filters จาก props
const props = defineProps({
  filters: Object
})

const trainingData = ref([])

// โหลดข้อมูลจาก API และกรองตามค่าที่ได้รับจาก props
onMounted(async () => {
  await fetchTrainingData()
})

// ฟังก์ชันดึงข้อมูลจาก API
const fetchTrainingData = async () => {
  try {
    const response = await axios.get('https://deploymanpowerdb-f5a0h6fqaehdajck.southeastasia-01.azurewebsites.net/api/OJTandInspectionSkill', {
      params: {
        division: props.filters.division !== 'ALL' ? props.filters.division : undefined,
        department: props.filters.department !== 'ALL' ? props.filters.department : undefined,
        section: props.filters.section !== 'ALL' ? props.filters.section : undefined,
        biz: props.filters.biz !== 'ALL' ? props.filters.biz : undefined,
        process: props.filters.process !== 'ALL' ? props.filters.process : undefined
      }
    })

    // กรองข้อมูลเฉพาะที่ active >= 2
    trainingData.value = response.data.filter(item => item.active >= 2)

    drawChart()
  } catch (error) {
    console.error('Error fetching training data:', error)
  }
}

// ฟังก์ชันวาดกราฟ
const drawChart = () => {
  const groupBy = 'process' // แสดงแค่ process
  const grouped = {}

  trainingData.value.forEach(item => {
    const key = item[groupBy] || 'Unknown'
    grouped[key] = (grouped[key] || 0) + 1
  })

  const xLabels = Object.keys(grouped)
  const yValues = Object.values(grouped)

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

  // ใช้สีตาม process
  const colors = xLabels.map(name => colorMapping[name] || '#4caf50');

  const chartData = [
    {
      x: xLabels,
      y: yValues,
      type: 'bar',
      marker: {
        color: colors,
      },
      text: yValues.map(v => `${v} คน`),
      textposition: 'auto',
    },
  ]

  const layout = {
    title: 'Training Employee',
    xaxis: {
      title: 'Process',
      tickangle: -30,
    },
    yaxis: {
      title: 'จำนวนผู้ผ่านการอบรม',
    },
    height: 450,
    paper_bgcolor: 'rgba(0,0,0,0)',
    plot_bgcolor: '#fff',
    margin: { t: 50, l: 50, r: 30, b: 80 },
  }

  Plotly.newPlot('training-chart', chartData, layout)
}

// Watch เพื่อรีเฟรชข้อมูลเมื่อ filters เปลี่ยน
watch(() => props.filters, async () => {
  await fetchTrainingData()
}, { deep: true })
</script>

<template>
  <div class="training-employee-container">
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
