<template>
  <div>
    <div id="required-bar-chart"></div>

    <div v-if="filteredEmployees.length" class="employee-recommendations">
      <h3>แนะนำพนักงาน</h3>

      <p>
        <strong>Process:</strong> {{ selectedProcess }} |
        <strong>Skill:</strong> {{ selectedSkill }}
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
          <tr v-for="(employee, index) in filteredEmployees" :key="index">
            <td>{{ index + 1 }}</td>
            <td>{{ employee.empID }}</td>
            <td>{{ employee.firstName }}</td>
            <td>{{ employee.lastName }}</td>
            <td>{{ employee.workTime }}</td>
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
import { ref, onMounted } from 'vue';
import axios from 'axios';
import Plotly from 'plotly.js';

const emit = defineEmits(['selectEmployee']);

const rawData = ref([]);
const skills = ref([]);
const worktime = ref([]);
const selectedSkill = ref(null);
const selectedProcess = ref(null);
const filteredEmployees = ref([]);

const aggregate = (process, skill) => {
  return rawData.value
    .filter(item => item.process === process && item.skillGroup === skill)
    .reduce((sum, item) => sum + item.require, 0);
};

const drawChart = () => {
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

const handleBarClick = (event) => {
  const skill = event.points[0].data.name;
  const process = event.points[0].x;

  selectedSkill.value = skill;
  selectedProcess.value = process;

  const filtered = skills.value.filter(emp => {
    const empWorktime = worktime.value.find(w => parseInt(w.empID) === emp.empID);
    const totalTime = empWorktime ? empWorktime.workedHours + empWorktime.oT_Hours + empWorktime.overloadHours : 0;
    return emp.skillGroup === skill && totalTime <= 60;
  }).map(emp => {
    const empWorktime = worktime.value.find(w => parseInt(w.empID) === emp.empID);
    return {
      ...emp,
      workTime: empWorktime ? (empWorktime.workedHours + empWorktime.oT_Hours + empWorktime.overloadHours) : 0
    };
  });

  filteredEmployees.value = filtered;
};

const resetSkillFilter = () => {
  filteredEmployees.value = [];
  selectedSkill.value = null;
  selectedProcess.value = null;
};

onMounted(async () => {
  try {
    const [req, skillRes, worktimeRes] = await Promise.all([
      axios.get("http://localhost:5000/api/ManpowerReq"),
      axios.get("http://localhost:5000/api/Skill"),
      axios.get("http://localhost:5000/api/Worktime")
    ]);
    rawData.value = req.data;
    skills.value = skillRes.data;
    worktime.value = worktimeRes.data;
    drawChart();
  } catch (err) {
    console.error("Error fetching data:", err);
  }
});
</script>

<style scoped>
.skill-icon {
  width: 30px;
  height: 30px;
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
