<template>
  <div id="worktime-chart"></div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import axios from 'axios';
import Plotly from 'plotly.js';

const workTimeData = ref([]);

onMounted(async () => {
  await fetchWorkTimeData();
  updateChart();
});

const emit = defineEmits(['filter']);

const fetchWorkTimeData = async () => {
  try {
    const response = await axios.get('http://localhost:5000/api/Worktime');
    workTimeData.value = response.data;
  } catch (error) {
    console.error('Error fetching work time data:', error);
  }
};

const updateChart = () => {
  if (!workTimeData.value.length) {
    console.error('No work time data found.');
    return;
  }

  const { sortedWeeks, weeksFormatted, workTimeByWeek } = aggregateWorkTimeByWeek();
  const otHours = sortedWeeks.map(week => workTimeByWeek[week].otHours);

  const chartData = [
    {
      x: weeksFormatted,
      y: otHours,
      name: 'OT Hours',
      type: 'bar',
      marker: {
        color: otHours.map(h => h >= 20 ? '#f44336' : h >= 10 ? '#ffc107' : '#4caf50'),
      },
    },
  ];

  const layout = {
    title: 'Weekly OT Hours (Only OT)',
    xaxis: { title: 'Week' },
    yaxis: { title: 'Total OT Hours', rangemode: 'tozero' },
    paper_bgcolor: '#fff',
    plot_bgcolor: '#f9f9f9',
    height: 400,
    margin: { l: 60, r: 20, t: 50, b: 60 },
  };

  Plotly.newPlot('worktime-chart', chartData, layout).then(() => {
    document.getElementById('worktime-chart').addEventListener('plotly_click', onBarClick);
  });
};

const aggregateWorkTimeByWeek = () => {
  const workTimeByWeek = {};

  workTimeData.value.forEach(entry => {
    if (entry.status !== 'Active') return;

    const date = new Date(entry.date);
    const weekNumber = getWeekNumber(date);
    const weekKey = `${date.getFullYear()}-W${weekNumber}`;

    if (!workTimeByWeek[weekKey]) {
      workTimeByWeek[weekKey] = { otHours: 0 };
    }

    workTimeByWeek[weekKey].otHours += entry.oT_Hours || 0;
  });

  const sortedWeeks = Object.keys(workTimeByWeek).sort();
  const weeksFormatted = sortedWeeks.map(week => {
    const [year, weekNum] = week.split('-W');
    return `Week ${weekNum}, ${year}`;
  });

  return { sortedWeeks, weeksFormatted, workTimeByWeek };
};

const getWeekNumber = (date) => {
  const startDate = new Date(date.getFullYear(), 0, 1);
  const diff = date - startDate;
  const oneDay = 1000 * 60 * 60 * 24;
  const dayOfYear = Math.floor(diff / oneDay);
  return Math.ceil((dayOfYear + startDate.getDay() + 1) / 7);
};

const onBarClick = (eventData) => {
  if (eventData.points?.length) {
    const selectedWeek = eventData.points[0].x;
    console.log('Clicked on week:', selectedWeek);
    emit('filter', selectedWeek);
  }
};
</script>

<style scoped>
#worktime-chart {
  width: 100%;
  height: 100%;
}
</style>
