<script setup>
import { ref, onMounted, watch } from 'vue'
import axios from 'axios'
import Plotly from 'plotly.js'

const selectedChart = ref('process') // default = process
const trainingData = ref([])

// โหลดข้อมูลตอน onMounted
onMounted(async () => {
  try {
    const response = await axios.get('http://localhost:5000/api/OJTandInspectionSkill')
    // กรองเฉพาะคนที่ active >= 2 (ผ่านการอบรม)
    trainingData.value = response.data.filter(item => item.active >= 2)
    drawChart()
  } catch (error) {
    console.error('Error fetching training data:', error)
  }
})

// วาดกราฟ
const drawChart = () => {
  const groupBy = selectedChart.value
  const grouped = {}

  trainingData.value.forEach(item => {
    const key = item[groupBy] || 'Unknown' // ใช้ 'process' หรือ 'biz'
    grouped[key] = (grouped[key] || 0) + 1
  })

  const xLabels = Object.keys(grouped)
  const yValues = Object.values(grouped)

  const chartData = [
    {
      x: xLabels,
      y: yValues,
      type: 'bar',
      marker: {
        color: '#4caf50',
      },
      text: yValues.map(v => `${v} คน`),
      textposition: 'auto',
    },
  ]

  const layout = {
    title: `Training Employee by ${groupBy.charAt(0).toUpperCase() + groupBy.slice(1)}`,
    xaxis: {
      title: groupBy === 'process' ? 'Process' : 'Biz',
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

// เพิ่ม watch เพื่อดูการเปลี่ยนแปลงของ selectedChart
watch(selectedChart, () => {
  drawChart()
})
</script>

<template>
  <div class="training-employee-container">
    <div class="chart-selector">
      <select v-model="selectedChart">
        <option value="process">Training Employee by Process</option>
        <option value="biz">Training Employee by Biz</option>
      </select>
    </div>
    <div id="training-chart"></div>
  </div>
</template>

<style scoped>
.training-employee-container {
  width: 100%;
  height: 100%;
}
.chart-selector {
  margin-bottom: 20px;
}
#training-chart {
  width: 100%;
  height: 100%;
}
</style>
