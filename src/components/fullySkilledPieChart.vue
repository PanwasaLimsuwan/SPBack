<script setup>
import { ref, onMounted, watch, nextTick } from 'vue';
import Plotly from 'plotly.js';

const emit = defineEmits(['filter-skills']);

// ✅ รับ props filters
const props = defineProps({
  filters: Object
});

const months = ref([]);
const fullySkilledCounts = ref([]);
const needTrainingCounts = ref([]);

// ✅ ฟังก์ชัน fetch data พร้อม filter
const fetchSkillData = async () => {
  try {
    const response = await fetch(`https://https://databasemanpowerdb-new.database.windows.net/api/Skill?division=${props.filters.division !== 'ALL' ? props.filters.division : ''}&department=${props.filters.department !== 'ALL' ? props.filters.department : ''}&section=${props.filters.section !== 'ALL' ? props.filters.section : ''}&biz=${props.filters.biz !== 'ALL' ? props.filters.biz : ''}&process=${props.filters.process !== 'ALL' ? props.filters.process : ''}`);
    const data = await response.json();

    const skillCategories = {
      'Fully Skilled': 0,
      'Need Training': 0,
    };

    data.forEach((employee) => {
      const isFullySkilled =
        employee.material === 3 &&
        employee.operation === 3 &&
        employee.machineSAB1 === 3 &&
        employee.machineSAB2 === 3 &&
        employee.machineSAB3 === 3 &&
        employee.inspection === 3;

      if (isFullySkilled) {
        skillCategories['Fully Skilled']++;
      } else {
        skillCategories['Need Training']++;
      }
    });

    months.value = Object.keys(skillCategories);
    fullySkilledCounts.value = [skillCategories['Fully Skilled']];
    needTrainingCounts.value = [skillCategories['Need Training']];

    const trace = {
      labels: ['Fully Skilled', 'Need Training'],
      values: [skillCategories['Fully Skilled'], skillCategories['Need Training']],
      type: 'pie',
      name: 'Skill Level',
      marker: {
        colors: ['rgb(0, 204, 0)', 'rgb(255, 99, 132)'],
      },
      textinfo: 'label+percent',
      hoverinfo: 'label+value+percent',
    };

    const layout = {
      title: 'Skill Level Distribution: Fully Skilled vs Need Training',
      height: 450,
      width: 450,
      showlegend: true,
    };

    await nextTick();
    Plotly.newPlot('fully-skilled-pie-chart', [trace], layout);

    const chart = document.getElementById('fully-skilled-pie-chart');
    chart.on('plotly_click', (data) => {
      const clickedLabel = data.points[0].label;
      if (clickedLabel === 'Fully Skilled') {
        emit('filter-skills', 3); // Fully Skilled = ทุกสกิลระดับ 3
      } else if (clickedLabel === 'Need Training') {
        emit('filter-skills', 0); // Need Training = มีสกิลต่ำกว่า 3
      }
    });

  } catch (error) {
    console.error('Error fetching skill data:', error);
  }
};

// ✅ ดู filter ถ้าเปลี่ยน -> reload
watch(() => props.filters, async () => {
  await fetchSkillData();
}, { deep: true });

// ✅ เริ่มต้น component
onMounted(() => {
  fetchSkillData();
});
</script>

<template>
  <div id="fully-skilled-pie-chart"></div>
</template>

<style scoped>
#fully-skilled-pie-chart {
  width: 100%;
  height: 100%;
}

.refresh-skill-btn {
  display: flex;
  align-items: center;
  gap: 8px;
  background-color: tomato;
  color: white;
  border: none;
  border-radius: 25px;
  padding: 6px 14px;
  margin-bottom: 10px;
  cursor: pointer;
  font-weight: bold;
  font-size: 14px;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.2);
  transition: background-color 0.3s ease, transform 0.3s ease;
}

.refresh-skill-btn:hover {
  background-color: #0056b3;
  transform: scale(1.05);
}

.refresh-skill-btn .icon {
  width: 18px;
  height: 18px;
  filter: invert(1);
}
</style>
