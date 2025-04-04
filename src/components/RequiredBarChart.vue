<template>
  <div>
    <div id="required-bar-chart"></div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import axios from 'axios';
import Plotly from 'plotly.js';

// State
const chartData = ref([]);

// ฟังก์ชันรวมข้อมูลตาม process และ skillGroup
const aggregate = (process, skill) => {
  return chartData.value
    .filter(item => item.process === process && item.skillGroup === skill)
    .reduce((sum, item) => sum + item.require, 0);
};

// ฟังก์ชันวาดกราฟ
const drawChart = () => {
  const processes = [...new Set(chartData.value.map(d => d.process))];
  const skills = [...new Set(chartData.value.map(d => d.skillGroup))];

  const traces = skills.map(skill => ({
    x: processes,
    y: processes.map(p => aggregate(p, skill)),
    name: skill,
    type: 'bar',
    text: processes.map(p => aggregate(p, skill)),
    textposition: 'auto'
  }));

  const layout = {
    title: 'Manpower Requirement by Process and Skill Group',
    barmode: 'stack',
    height: 450,
    xaxis: { title: 'Process' },
    yaxis: { title: 'Required Employees' }
  };

  Plotly.newPlot('required-bar-chart', traces, layout);
};

// ดึงข้อมูลเมื่อ component โหลด
onMounted(async () => {
  try {
    const res = await axios.get('http://localhost:5000/api/ManpowerReq');
    chartData.value = res.data;
    drawChart();
  } catch (err) {
    console.error('Error fetching data:', err);
  }
});
</script>

<style scoped>
#required-bar-chart {
  width: 100%;
  height: 100%;
}
</style>
