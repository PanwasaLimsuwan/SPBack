<template>
  <div id="monthly-overload-chart"></div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import Plotly from 'plotly.js';

const months = ref([]);
const overloads = ref([]);

const fetchWorkTimeData = async () => {
  try {
    const response = await fetch('http://localhost:5000/api/Worktime');
    const data = await response.json();

    const monthlyData = {};

    data.forEach(entry => {
      if (entry.status !== 'Active') return;

      const dateObj = new Date(entry.date);
      const monthKey = `${dateObj.getFullYear()}-${(dateObj.getMonth() + 1).toString().padStart(2, '0')}`;
      const empID = entry.empID;

      const worked = parseFloat(entry.workedHours) || 0;
      const ot = parseFloat(entry.oT_Hours ?? entry.OT_Hours ?? entry.ot_Hours) || 0;
      const total = worked + ot;

      if (!monthlyData[monthKey]) monthlyData[monthKey] = {};
      if (!monthlyData[monthKey][empID]) monthlyData[monthKey][empID] = 0;

      monthlyData[monthKey][empID] += total;
    });

    const monthlyOverload = {};
    for (const month in monthlyData) {
      let overloadSum = 0;
      for (const empID in monthlyData[month]) {
        const total = monthlyData[month][empID];
        const overload = total > 60 ? total - 60 : 0;
        overloadSum += overload;
      }
      monthlyOverload[month] = overloadSum;
    }

    months.value = Object.keys(monthlyOverload);
    overloads.value = Object.values(monthlyOverload);

    drawChart();
  } catch (error) {
    console.error('Error fetching data:', error);
  }
};

const drawChart = () => {
  const trace = {
    x: months.value,
    y: overloads.value,
    type: 'bar',
    name: 'Monthly Overload',
    marker: { color: 'crimson' },
  };

  const layout = {
    title: 'Monthly Overload (Worktime + OT - 60 hrs)',
    xaxis: { title: 'Month' },
    yaxis: { title: 'Total Overload Hours' },
    margin: { l: 60, r: 30, t: 50, b: 60 },
    plot_bgcolor: '#f9f9f9',
    paper_bgcolor: '#fff',
  };

  Plotly.newPlot('monthly-overload-chart', [trace], layout, { responsive: true });
};

onMounted(fetchWorkTimeData);
</script>

<style scoped>
#monthly-overload-chart {
  width: 100%;
  height: 100%;
}
</style>
