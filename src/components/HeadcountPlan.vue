<template>
  <div id="headcount-chart"></div>
</template>

<script setup>
import { ref, onMounted, watch, nextTick } from 'vue';
import axios from 'axios';
import Plotly from 'plotly.js';

// ✅ รับ props filters
const props = defineProps({
  filters: {
    type: Object,
    default: () => ({}),
  },
});

const headcountData = ref([]);

const fetchData = async () => {
  try {
    const response = await axios.get("https://databasemanpowerdb.database.windows.net/api/ManpowerPlan", {
      params: {
        division: props.filters.division !== 'ALL' ? props.filters.division : undefined,
        department: props.filters.department !== 'ALL' ? props.filters.department : undefined,
        section: props.filters.section !== 'ALL' ? props.filters.section : undefined,
        biz: props.filters.biz !== 'ALL' ? props.filters.biz : undefined,
        process: props.filters.process !== 'ALL' ? props.filters.process : undefined,
      },
    });

    headcountData.value = response.data;
    await nextTick();
    drawChart();
  } catch (error) {
    console.error("Error fetching data:", error);
  }
};

const drawChart = () => {
  if (!headcountData.value.length) return;

  const dates = headcountData.value.map(item => item.date);
  const planned = headcountData.value.map(item => item.plannedHeadcount);
  const actual = headcountData.value.map(item => item.actualHeadcount);

  const chartData = [
    {
      x: dates,
      y: planned,
      type: "bar",
      name: "Planned Headcount",
      marker: { color: "red" },
    },
    {
      x: dates,
      y: actual,
      type: "bar",
      name: "Actual Headcount",
      marker: { color: "blue" },
    },
  ];

  const layout = {
    title: "Headcount vs Plan",
    barmode: "group",
    xaxis: { title: "Date", tickangle: -45 },
    yaxis: { dtick: 1000},
    paper_bgcolor: "#fff",
    plot_bgcolor: "#f9f9f9",
    margin: { l: 60, r: 20, t: 50, b: 60 },
    responsive: true,
  };

  Plotly.newPlot("headcount-chart", chartData, layout);
};

// ✅ lifecycle
onMounted(fetchData);

// ✅ watch filter เปลี่ยน → re-fetch ข้อมูลใหม่
watch(() => props.filters, async () => {
  await fetchData();
}, { deep: true });
</script>

<style scoped>
#headcount-chart {
  width: 100%;
  height: 100%;
}
</style>
