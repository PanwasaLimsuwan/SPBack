<template>
  <div>
    <div v-if="!workTimeData.length" class="no-data">
      No data to display.
    </div>
    <div id="monthly-overtime" v-else></div>
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

const workTimeData = ref([]);
const emit = defineEmits(['filter']);

// ✅ onMounted ดึงข้อมูลทันที
onMounted(async () => {
  console.log('🟢 Initial filters:', props.filters);
  await fetchWorkTimeData();
});

// ✅ watch filters ถ้าเปลี่ยนให้ fetch ข้อมูลใหม่
watch(() => props.filters, async () => {
  console.log('🟢 Filters changed:', props.filters);
  await fetchWorkTimeData();
}, { deep: true });

// ✅ fetch worktime data พร้อม filters
const fetchWorkTimeData = async () => {
  try {
    const response = await axios.get('https://databasemanpowerdb.database.windows.net/api/EICCControl/MonthlySummary', {
      params: {
        division: props.filters.division !== 'ALL' ? props.filters.division : undefined,
        department: props.filters.department !== 'ALL' ? props.filters.department : undefined,
        section: props.filters.section !== 'ALL' ? props.filters.section : undefined,
        biz: props.filters.biz !== 'ALL' ? props.filters.biz : undefined,
        process: props.filters.process !== 'ALL' ? props.filters.process : undefined,
      },
    });
    workTimeData.value = response.data;
    console.log('✅ Worktime data loaded:', workTimeData.value);

    if (workTimeData.value.length > 0) {
      drawChart();
    } else {
      console.warn('⚠️ No data after filtering');
      Plotly.purge('monthly-overtime');
    }
  } catch (error) {
    console.error('❌ Error fetching work time data:', error);
  }
};

// ✅ process worktime data by month (ใช้ monthYear ตรง ๆ)
const aggregateWorkTimeByMonth = () => {
  const workTimeByMonth = {};

  workTimeData.value.forEach(entry => {
    const monthKey = entry.month;

    if (!workTimeByMonth[monthKey]) {
      workTimeByMonth[monthKey] = { otHours: 0 };
    }

    workTimeByMonth[monthKey].otHours += entry.totalOT || 0;
  });

  const sortedMonths = Object.keys(workTimeByMonth).sort();
  const formattedMonths = sortedMonths.map(m =>
    new Date(m + '-01').toLocaleString('en-US', { month: 'short', year: 'numeric' })
  );

  return { sortedMonths, formattedMonths, workTimeByMonth };
};

// ✅ draw chart
const drawChart = async () => {
  if (!workTimeData.value.length) {
    console.warn('⚠️ drawChart: No work time data found.');
    Plotly.purge('monthly-overtime');
    return;
  }

  await nextTick(); // ✅ รอ DOM

  const { sortedMonths, formattedMonths, workTimeByMonth } = aggregateWorkTimeByMonth();
  const otHours = sortedMonths.map(month => workTimeByMonth[month].otHours);

  console.log('📊 Chart Data:', { sortedMonths, formattedMonths, otHours });

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
    title: 'Monthly Overtime',
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

// ✅ click event emit filter
const onBarClick = (eventData) => {
  if (eventData.points?.length) {
    const selectedMonth = eventData.points[0].x;
    console.log('🟢 Clicked on month:', selectedMonth);
    emit('filter', selectedMonth);
  }
};
</script>

<style scoped>
#monthly-overtime {
  width: 100%;
  height: 100%;
}
.no-data {
  text-align: center;
  padding: 20px;
  color: #888;
  font-weight: bold;
}
</style>
