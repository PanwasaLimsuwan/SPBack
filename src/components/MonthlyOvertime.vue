<template>
  <div id="monthly-overtime"></div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import axios from 'axios';
import Plotly from 'plotly.js';

const workTimeData = ref([]);
const emit = defineEmits(['filter']);

onMounted(async () => {
  await fetchWorkTimeData();
  drawChart();
});

const fetchWorkTimeData = async () => {
  try {
    const response = await axios.get('http://localhost:5000/api/Worktime');
    workTimeData.value = response.data;
  } catch (error) {
    console.error('Error fetching work time data:', error);
  }
};

const drawChart = () => {
  if (!workTimeData.value.length) {
    console.error('No work time data found.');
    return;
  }

  const { sortedMonths, formattedMonths, workTimeByMonth } = aggregateWorkTimeByMonth();
  const otHours = sortedMonths.map(month => workTimeByMonth[month].otHours);

  const chartData = [
    {
      x: formattedMonths,
      y: otHours,
      name: 'OT Hours',
      type: 'bar',
      marker: {
        color: otHours.map(h => h >= 60 ? '#f44336' : h >= 30 ? '#ffc107' : '#4caf50'),
      },
    },
  ];

  const layout = {
    title: 'Monthly Overtime Hours',
    xaxis: { title: 'Month' },
    yaxis: { title: 'Total OT Hours', rangemode: 'tozero' },
    paper_bgcolor: '#fff',
    plot_bgcolor: '#f9f9f9',
    height: 400,
    margin: { l: 60, r: 20, t: 50, b: 60 },
  };

  Plotly.newPlot('monthly-overtime', chartData, layout).then(() => {
    document.getElementById('monthly-overtime').addEventListener('plotly_click', onBarClick);
  });
};

const aggregateWorkTimeByMonth = () => {
  const workTimeByMonth = {};

  workTimeData.value.forEach(entry => {
    if (entry.status !== 'Active') return;

    const date = new Date(entry.date);
    const monthKey = date.toISOString().substring(0, 7); // YYYY-MM

    if (!workTimeByMonth[monthKey]) {
      workTimeByMonth[monthKey] = { otHours: 0 };
    }

    workTimeByMonth[monthKey].otHours += entry.oT_Hours || 0;
  });

  const sortedMonths = Object.keys(workTimeByMonth).sort();
  const formattedMonths = sortedMonths.map(m =>
    new Date(m + '-01').toLocaleString('en-US', { month: 'short', year: 'numeric' })
  );

  return { sortedMonths, formattedMonths, workTimeByMonth };
};

const onBarClick = (eventData) => {
  if (eventData.points?.length) {
    const selectedMonth = eventData.points[0].x;
    console.log('Clicked on month:', selectedMonth);
    emit('filter', selectedMonth);
  }
};
</script>

<style scoped>
#monthly-overtime {
  width: 100%;
  height: 100%;
}
</style>
