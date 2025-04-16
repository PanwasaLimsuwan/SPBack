<template>
  <div class="headcount-container">
    <div class="chart-selector">
      <select v-model="selectedGroup" @change="drawChart">
        <option value="biz">Head Count by Biz</option>
        <option value="process">Head Count by Process</option> <!-- Changed to 'process' -->
      </select>
    </div>
    <div id="headcount-chart"></div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue';
import axios from 'axios';
import Plotly from 'plotly.js';

const emit = defineEmits(['filter']);
const selectedGroup = ref('biz');
const employees = ref([]);

onMounted(fetchData);

watch(selectedGroup, drawChart);

async function fetchData() {
  try {
    const res = await axios.get('http://localhost:5000/api/GateEntry');
    employees.value = res.data;
    drawChart();
  } catch (err) {
    console.error('❌ Error fetching data:', err);
  }
}

function drawChart() {
  const groupBy = selectedGroup.value;
  const counts = groupData(employees.value, groupBy);

  const chartData = [
    {
      values: Object.values(counts),
      labels: Object.keys(counts),
      type: 'pie',
      textinfo: 'label+percent',
    },
  ];

  const layout = {
    title: `Head Count by ${capitalize(groupBy)}`,
    height: 400,
    width: 400,
    paper_bgcolor: 'rgba(0,0,0,0)',
  };

  Plotly.newPlot('headcount-chart', chartData, layout).then(() => {
    document.getElementById('headcount-chart').on('plotly_click', (eventData) => {
      if (eventData.points?.length) {
        const selectedValue = eventData.points[0].label;
        emit('filter', { groupBy, value: selectedValue });
      }
    });
  });
}

function groupData(data, key) {
  return data.reduce((acc, emp) => {
    const k = emp[key] || 'undefined';
    acc[k] = (acc[k] || 0) + 1;
    return acc;
  }, {});
}

function capitalize(str) {
  return str.charAt(0).toUpperCase() + str.slice(1);
}
</script>

<style scoped>
.headcount-container {
  width: 100%;
}
.chart-selector {
  margin-bottom: 20px;
}
#headcount-chart {
  width: 100%;
  height: 100%;
}
</style>
