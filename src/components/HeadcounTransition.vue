<template>
  <div id="headcount-transition-chart"></div>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue';
import axios from 'axios';
import Plotly from 'plotly.js';

const transitions = ref([]);

// ดึงข้อมูลเมื่อโหลด component
onMounted(async () => {
  await fetchHeadcountData();
  drawChart();
});

// ดึงข้อมูลจาก API
const fetchHeadcountData = async () => {
  try {
    const response = await axios.get("http://localhost:5000/api/HeadcountTransition");
    transitions.value = response.data;
  } catch (error) {
    console.error("Error fetching headcount transition data:", error);
  }
};

// วาดกราฟเมื่อ transitions เปลี่ยนแปลง
watch(transitions, () => {
  drawChart();
}, { deep: true });

const drawChart = () => {
  if (!transitions.value.length) {
    console.error("No data to plot.");
    return;
  }

  const formattedData = transitions.value.reduce((acc, entry) => {
    const date = new Date(entry.dateTime);
    const monthYear = date.toLocaleString("en-US", { month: "short", year: "numeric" });

    if (!acc[monthYear]) {
      acc[monthYear] = { sign: 0, resign: 0 };
    }

    if (entry.transType === "Sign") {
      acc[monthYear].sign += 1;
    } else if (entry.transType === "Resign") {
      acc[monthYear].resign += 1;
    }

    return acc;
  }, {});

  const months = Object.keys(formattedData);
  const newEmployees = months.map(month => formattedData[month].sign);
  const resignedEmployees = months.map(month => -formattedData[month].resign);

  const chartData = [
    {
      x: months,
      y: newEmployees,
      name: "พนักงานใหม่",
      type: "bar",
      marker: { color: "blue" },
    },
    {
      x: months,
      y: resignedEmployees,
      name: "พนักงานที่ลาออก",
      type: "bar",
      marker: { color: "red" },
    },
  ];

  const layout = {
    title: "Headcount Transition",
    barmode: "relative",
    height: 500,
    xaxis: {
      title: "Months",
      tickangle: -45,
    },
    yaxis: {
      title: "Number of Employees",
    },
    legend: {
      x: 1,
      y: 1,
    },
    paper_bgcolor: "rgba(0,0,0,0)",
    responsive: true,
  };

  Plotly.newPlot("headcount-transition-chart", chartData, layout);
};
</script>

<style scoped>
#headcount-transition-chart {
  width: 100%;
  height: 100%;
}
</style>
