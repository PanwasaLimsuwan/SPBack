<template>
  <div>
    <div id="required-bar-chart"></div>

    <div class="employee-recommendations">
  <h3>แนะนำพนักงาน</h3>

  <p>
    <strong>Process:</strong> {{ selectedProcess || '-' }} |
    <strong>Skill:</strong> {{ selectedSkill || '-' }}
  </p>

  <button class="refresh-skill-btn" @click="resetSkillFilter" title="รีเซตฟิลเตอร์">
    <img src="refresh.png" alt="Refresh Icon" class="icon" />
    <span>Refresh</span>
  </button>

  <table>
    <thead>
      <tr>
        <th>No.</th>
        <th>EmpID</th>
        <th>Firstname</th>
        <th>Lastname</th>
        <th>Work Time</th>
        <th>Skill</th>
        <th>Select</th>
      </tr>
    </thead>
    <tbody>
      <tr v-if="!selectedSkill && !selectedProcess">
        <td colspan="7" style="text-align: center; padding: 20px;">
          กรุณาเลือก Process และ Skill จากกราฟทางด้านซ้ายเพื่อใช้ในการแนะนำพนักงาน
        </td>
      </tr>

      <tr v-else-if="filteredEmployees.length === 0">
        <td colspan="7" style="text-align: center; padding: 20px;">
          ไม่พบพนักงานที่ตรงกับเงื่อนไข กรุณาเลือก Process และ Skill อื่น
        </td>
      </tr>

      <tr v-else v-for="(employee, index) in filteredEmployees" :key="index">
        <td>{{ index + 1 }}</td>
        <td>{{ employee.empID }}</td>
        <td>{{ employee.firstName }}</td>
        <td>{{ employee.lastName }}</td>
        <td>{{ employee.totalTime }}</td>
        <td>{{ selectedSkill }}</td>
        <td>
          <button @click="$emit('selectEmployee', employee)">
            <img src="skill.png" alt="Skill" class="skill-icon" />
          </button>
        </td>
      </tr>
    </tbody>
  </table>
</div>

  </div>
</template>

<script setup>
import { ref, onMounted, watch, nextTick } from 'vue';
import axios from 'axios';
import Plotly from 'plotly.js';

// ✅ รับ props filter จาก parent component
const props = defineProps({
  filters: Object
});

const emit = defineEmits(['selectEmployee']);

const rawData = ref([]);
const skills = ref([]);
const worktime = ref([]);
const selectedSkill = ref(null);
const selectedProcess = ref(null);
const filteredEmployees = ref([]);

// ✅ รวมข้อมูล process + skill group
const aggregate = (process, skill) => {
  return rawData.value
    .filter(item => item.process === process && item.skillGroup === skill)
    .reduce((sum, item) => sum + item.require, 0);
};

// ✅ วาดกราฟ
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

// ✅ Handle click บนกราฟ
const handleBarClick = (event) => {
  const skill = event.points[0].data.name;
  const process = event.points[0].x;

  selectedSkill.value = skill;
  selectedProcess.value = process;

  const filtered = skills.value
  .map(emp => {
    const empWorktimeList = worktime.value.filter(w => w.empID === emp.empID);
    const totalTime = empWorktimeList.reduce((sum, w) => sum + (w.totalHours ?? 0), 0);

    return {
      ...emp,
      empID: emp.empID,
      process: emp.process,
      totalTime: totalTime,
      firstName: emp.firstName,
      lastName: emp.lastName,
      skillLevel: emp[selectedSkill.value.toLowerCase()] ?? 0
    };
  })
  .filter(emp => {
    return emp.skillGroup === selectedSkill.value && emp.process === selectedProcess.value && emp.totalTime <= 60;
  })
  .sort((a, b) => {
    // ✅ เรียง skill level สูง -> ต่ำ
    if (b.skillLevel !== a.skillLevel) return b.skillLevel - a.skillLevel;
    // ✅ ถ้า skill level เท่ากัน เรียง totalHours เหลือมากก่อน
    return (60 - b.totalTime) - (60 - a.totalTime);
  });

filteredEmployees.value = filtered;

};

// ✅ Reset filter
const resetSkillFilter = () => {
  filteredEmployees.value = [];
  selectedSkill.value = null;
  selectedProcess.value = null;
};

// ✅ ดึงข้อมูล API ทั้ง 3
const fetchData = async () => {
  try {
    const [req, skillRes, worktimeRes] = await Promise.all([
      axios.get("https://deploymanpowerdb-f5a0h6fqaehdajck.southeastasia-01.azurewebsites.net/api/ManpowerReq", {
        params: {
          division: props.filters.division !== 'ALL' ? props.filters.division : undefined,
          department: props.filters.department !== 'ALL' ? props.filters.department : undefined,
          section: props.filters.section !== 'ALL' ? props.filters.section : undefined,
          biz: props.filters.biz !== 'ALL' ? props.filters.biz : undefined,
          process: props.filters.process !== 'ALL' ? props.filters.process : undefined,
        }
      }),
      axios.get("https://deploymanpowerdb-f5a0h6fqaehdajck.southeastasia-01.azurewebsites.net/api/Skill", {
        params: {
          division: props.filters.division !== 'ALL' ? props.filters.division : undefined,
          department: props.filters.department !== 'ALL' ? props.filters.department : undefined,
          section: props.filters.section !== 'ALL' ? props.filters.section : undefined,
          biz: props.filters.biz !== 'ALL' ? props.filters.biz : undefined,
          process: props.filters.process !== 'ALL' ? props.filters.process : undefined,
        }
      }),
      axios.get("https://deploymanpowerdb-f5a0h6fqaehdajck.southeastasia-01.azurewebsites.net/api/EICCControl", {
        params: {
          division: props.filters.division !== 'ALL' ? props.filters.division : undefined,
          department: props.filters.department !== 'ALL' ? props.filters.department : undefined,
          section: props.filters.section !== 'ALL' ? props.filters.section : undefined,
          biz: props.filters.biz !== 'ALL' ? props.filters.biz : undefined,
          process: props.filters.process !== 'ALL' ? props.filters.process : undefined,
        }
      }),
    ]);

    rawData.value = req.data;
    skills.value = skillRes.data;
    worktime.value = worktimeRes.data;

    await nextTick();
    drawChart();
  } catch (err) {
    console.error("Error fetching data:", err);
  }
};

// ✅ ดู filter ถ้าเปลี่ยน -> reload
watch(() => props.filters, async () => {
  await fetchData();
  resetSkillFilter();
}, { deep: true });

// ✅ เริ่มต้น component
onMounted(async () => {
  await fetchData();
});
</script>

<style scoped>
/* สไตล์เดิมของคุณ */
.skill-icon {
  width: 30px;
  height: 30px;
}

.refresh-skill-btn {
  display: flex;
  align-items: center;
  gap: 8px;
  background-color: #007BFF;
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

button {
  background-color: transparent;
  border: none;
  cursor: pointer;
  transition: transform 0.2s ease, box-shadow 0.2s ease;
  padding: 5px;
  border-radius: 50%;
}

button:hover {
  transform: scale(1.05);
  box-shadow: 0 0 5px rgba(0, 123, 255, 0.3);
}

.active-button {
  background-color: #007bff;
  border-radius: 50%;
  padding: 5px;
  box-shadow: 0 0 10px rgba(0, 123, 255, 0.5);
  transition: all 0.3s ease;
}

.active-button img {
  filter: brightness(100%) contrast(100%);
}

#required-bar-chart {
  width: 100%;
  height: 100%;
  margin-bottom: 30px;
}

.employee-recommendations table {
  width: 100%;
  border-collapse: collapse;
}

.employee-recommendations th,
.employee-recommendations td {
  padding: 10px;
  text-align: center;
  border: 1px solid #ccc;
}

.employee-recommendations th {
  background: #eee;
}

.employee-recommendations td:last-child {
  display: flex;
  justify-content: center;
  align-items: center;
  height: 100%;
}
</style>
