<template>
  <div id="monthly-overload-chart"></div>
</template>

<script setup>
import { ref, onMounted, watch, nextTick } from 'vue';
import Plotly from 'plotly.js';
import axios from 'axios';

// ✅ รับ props filter
const props = defineProps({
  filters: {
    type: Object,
    default: () => ({}),
  },
});

const months = ref([]);
const overloads = ref([]);

const fetchWorkTimeData = async () => {
  try {
    const response = await axios.get('https://databasemanpowerdb.database.windows.net/api/EICCControl', {
      params: {
        division: props.filters.division !== 'ALL' ? props.filters.division : undefined,
        department: props.filters.department !== 'ALL' ? props.filters.department : undefined,
        section: props.filters.section !== 'ALL' ? props.filters.section : undefined,
        biz: props.filters.biz !== 'ALL' ? props.filters.biz : undefined,
        process: props.filters.process !== 'ALL' ? props.filters.process : undefined,
      },
    });

    const data = response.data;

    const monthlyData = {};

    data.forEach(entry => {
      if (entry.status !== 'Active') return;

      const monthKey = entry.monthYear;

      const empID = entry.empID;
      const worked = parseFloat(entry.totalHours) || 0;
      const ot = parseFloat(entry.totalOT) || 0;
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
        const overload = total > 240 ? total - 240 : 0; // ✅ เช็คเกิน 240
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
    title: 'Monthly Overload (Over 240 Hours)',
    xaxis: { title: 'Month' },
    // yaxis: { title: 'Total Overload Hours' },
    margin: { l: 60, r: 30, t: 50, b: 60 },
    plot_bgcolor: '#f9f9f9',
    paper_bgcolor: '#fff',
  };

  Plotly.newPlot('monthly-overload-chart', [trace], layout, { responsive: true });
};

// ✅ Lifecycle
onMounted(fetchWorkTimeData);

// ✅ watch filter → reload data + update chart
watch(() => props.filters, async () => {
  await fetchWorkTimeData();
  await nextTick();
  drawChart();
}, { deep: true });
</script>

<style scoped>
#monthly-overload-chart {
  width: 100%;
  height: 100%;
}
</style>
