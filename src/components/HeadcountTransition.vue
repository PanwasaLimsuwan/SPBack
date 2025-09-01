<template>
  <div>
    <div v-if="loading" style="text-align: center; padding: 20px;">
      Loading data...
    </div>
    <div v-else id="headcount-transition-chart"></div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch, nextTick } from 'vue';
import axios from 'axios';
import Plotly from 'plotly.js';

// ✅ รับ props filters จาก parent
const props = defineProps({
  filters: Object
});

const transitions = ref([]);
const loading = ref(true); // ✅ loading state

// ✅ ดึงข้อมูลตอนโหลด component
onMounted(async () => {
  await fetchHeadcountData();
});

// ✅ watch filters ถ้าเปลี่ยนให้ fetch ข้อมูลใหม่
watch(() => props.filters, async () => {
  await fetchHeadcountData();
}, { deep: true });

// ✅ ดึงข้อมูลจาก API พร้อม filter
const fetchHeadcountData = async () => {
  try {
    loading.value = true;

    const response = await axios.get("https://https://databasemanpowerdb-new.database.windows.net/api/HeadcountTransition", {
      params: {
        division: props.filters.division !== 'ALL' ? props.filters.division : undefined,
        department: props.filters.department !== 'ALL' ? props.filters.department : undefined,
        section: props.filters.section !== 'ALL' ? props.filters.section : undefined,
        biz: props.filters.biz !== 'ALL' ? props.filters.biz : undefined,
        process: props.filters.process !== 'ALL' ? props.filters.process : undefined,
      },
    });
    transitions.value = response.data;
  } catch (error) {
    console.error("Error fetching headcount transition data:", error);
  } finally {
    loading.value = false;

    // ✅ รอ DOM ready ก่อนวาดกราฟ
    await nextTick();
    drawChart();
  }
};

// ✅ วาดกราฟเมื่อข้อมูลเปลี่ยน
watch(transitions, async () => {
  if (!loading.value) {
    await nextTick();
    drawChart();
  }
}, { deep: true });

const drawChart = () => {
  if (!transitions.value.length) {
    Plotly.purge("headcount-transition-chart");
    return;
  }

  const formattedData = transitions.value.reduce((acc, entry) => {
    const date = new Date(entry.dateTime);
    const monthYear = date.toLocaleString("en-US", { month: "short", year: "numeric" });

    if (!acc[monthYear]) {
      acc[monthYear] = { sign: 0, resign: 0 };
    }

    if (entry.transType === "Signed") {
      acc[monthYear].sign += 1;
    } else if (entry.transType === "Resigned") {
      acc[monthYear].resign += 1;
    }

    return acc;
  }, {});

  const months = Object.keys(formattedData).sort((a, b) => {
    const [aMonth, aYear] = a.split(' ');
    const [bMonth, bYear] = b.split(' ');
    return new Date(`${aMonth} 1, ${aYear}`) - new Date(`${bMonth} 1, ${bYear}`);
  });

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
