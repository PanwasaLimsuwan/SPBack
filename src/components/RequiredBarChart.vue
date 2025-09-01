<template>
  <div>
    <div id="required-bar-chart"></div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch, nextTick } from 'vue';
import axios from 'axios';
import Plotly from 'plotly.js';

const props = defineProps({
  filters: Object
});

const emit = defineEmits(['barClick']);

const rawData = ref([]);

// รวมข้อมูล process + skill group
const aggregate = (process, skill) => {
  return rawData.value
    .filter(item => item.process === process && item.skillGroup === skill)
    .reduce((sum, item) => sum + item.require, 0);
};

// วาดกราฟ
const drawChart = () => {
  if (!rawData.value.length) return;

  const processes = [...new Set(rawData.value.map(d => d.process))];
  const skillGroups = [...new Set(rawData.value.map(d => d.skillGroup))];

  const traces = skillGroups.map(skill => ({
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

  Plotly.newPlot('required-bar-chart', traces, layout).then(() => {
    const chart = document.getElementById('required-bar-chart');
    chart.on('plotly_click', handleBarClick);
  });
};

// Handle click บนกราฟ
const handleBarClick = (event) => {
  const skill = event.points[0].data.name;
  const process = event.points[0].x;

  emit('barClick', { skill, process });
};

// ดึงข้อมูล
const fetchData = async () => {
  try {
    const res = await axios.get("https://databasemanpowerdb.database.windows.net/api/ManpowerReq", {
      params: {
        division: props.filters.division !== 'ALL' ? props.filters.division : undefined,
        department: props.filters.department !== 'ALL' ? props.filters.department : undefined,
        section: props.filters.section !== 'ALL' ? props.filters.section : undefined,
        biz: props.filters.biz !== 'ALL' ? props.filters.biz : undefined,
        process: props.filters.process !== 'ALL' ? props.filters.process : undefined,
      }
    });
    rawData.value = res.data;
    await nextTick();
    drawChart();
  } catch (err) {
    console.error("Error fetching data:", err);
  }
};

// ดู filter ถ้าเปลี่ยน reload
watch(() => props.filters, async () => {
  await fetchData();
}, { deep: true });

// เริ่มต้น component
onMounted(async () => {
  await fetchData();
});
</script>

<style scoped>
#required-bar-chart {
  width: 100%;
  height: 100%;
}
</style>
