<script setup>
import { ref, onMounted, computed, watch } from 'vue';
import axios from 'axios';

const props = defineProps({
  selectedEmployee: Object,
  selectedSkillFilter: Number
});

const emit = defineEmits(['selectEmployee']);

const employees = ref([]);
const selectedSkillLevel = ref(null);

const skills = [
  "material",
  "operation",
  "machineSAB1",
  "machineSAB2",
  "machineSAB3",
  "inspection",
];

const skillLevels = [
  { label: "Not Trained (0)", value: 0 },
  { label: "Basic (1)", value: 1 },
  { label: "Medium (2)", value: 2 },
  { label: "Expert (3)", value: 3 },
];

const filteredEmployees = computed(() => {
  if (props.selectedSkillFilter == null) {
    return employees.value;
  } else if (props.selectedSkillFilter === 3) {
    // Fully Skilled = ทุก skill ต้องเป็นระดับ 3
    return employees.value.filter(emp =>
      skills.every(skill => emp[skill] === 3)
    );
  } else if (props.selectedSkillFilter === 0) {
    // Need Training = มี skill ที่เป็น level 0 หรือ 1 มากกว่า 2 ตัว
    return employees.value.filter(emp => {
      const midLevelCount = skills.reduce((count, skill) => {
        const level = emp[skill];
        return level === 0 || level === 1 ? count + 1 : count;
      }, 0);
      return midLevelCount > 2;
    });
  } else {
    // กรณีเลือก filter อื่น ๆ เช่น 1 หรือ 2 แบบทั่วไป
    return employees.value.filter(emp =>
      skills.some(skill => emp[skill] === props.selectedSkillFilter)
    );
  }
});


const fetchEmployeeSkills = async () => {
  try {
    const response = await axios.get("http://localhost:5000/api/Skill");
    employees.value = response.data;
  } catch (error) {
    console.error("Error fetching employee skills:", error);
  }
};

const selectEmployee = (employee) => {
  emit('selectEmployee', employee);
};

const getSkillLevel = (level) => {
  return ["Not Trained", "Basic", "Medium", "Expert"][level] ?? "Unknown";
};

const getSkillLevelClass = (level) => {
  return level >= 0 && level <= 3 ? `level-${level}` : "level-unknown";
};

const filterBySkillLevel = (level) => {
  selectedSkillLevel.value = selectedSkillLevel.value === level ? null : level;
};

const resetFilter = () => {
  selectedSkillLevel.value = null;
  emit('clear-skill');
  // filteredEmployees.value = employees.value;
};

onMounted(() => {
  fetchEmployeeSkills();
});
</script>

<template>
  <div class="employee-skill-table">
    <h3>Employee Skill</h3>

    <div class="legend">
      <span
        v-for="level in skillLevels"
        :key="level.value"
        class="legend-item"
        @click="filterBySkillLevel(level.value)"
        :class="{ activeLegend: level.value === selectedSkillLevel }"
      >
        <span :class="'legend-dot level-' + level.value"></span>
        {{ level.label }}
      </span>

      <button class="refresh-skill-btn" @click="resetFilter" title="รีเซตฟิลเตอร์">
        <img src="refresh.png" alt="Refresh Icon" class="icon" />
        <span>Refresh</span>
      </button>
    </div>

    <div class="table-scroll">
      <table>
        <thead>
          <tr>
            <th>EmpID</th>
            <th>Firstname</th>
            <th>Lastname</th>
            <th v-for="skill in skills" :key="skill">{{ skill }}</th>
            <th>Select</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="employee in filteredEmployees" :key="employee.empID">
            <td>{{ employee.empID }}</td>
            <td>{{ employee.firstName }}</td>
            <td>{{ employee.lastName }}</td>
            <td
              v-for="skill in skills"
              :key="skill"
              :class="selectedSkillLevel === null || employee[skill] === selectedSkillLevel
                       ? getSkillLevelClass(employee[skill])
                       : ''"
            >
              <span v-if="selectedSkillLevel === null || employee[skill] === selectedSkillLevel">
                {{ getSkillLevel(employee[skill]) }}
              </span>
            </td>
            <td>
              <button
                :class="{ 'active-button': employee.empID === selectedEmployee?.empID }"
                @click="selectEmployee(employee)"
                :title="employee.empID === selectedEmployee?.empID ? 'กำลังดูข้อมูล ' + employee.firstName + ' ' + employee.lastName : 'ดูข้อมูลสกิล'"
              >
                <img src="skill.png" alt="Skill" class="skill-icon" />
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>


<style scoped>
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

.employee-skill-table {
  margin-top: 20px;
}

.table-scroll {
  max-height: 400px; /* หรือกำหนดตามที่เหมาะสม เช่น 60vh */
  overflow-y: auto;
  overflow-x: auto; /* เผื่อมีแนวนอนด้วย */
}

table {
  width: 100%;
  border-collapse: collapse;
}

th,
td {
  padding: 10px;
  text-align: center;
  border: 1px solid #ddd;
}

th {
  background-color: #f4f4f4;
}

.legend {
  margin-top: 10px;
  display: flex;
  justify-content: space-around;
}

.legend-dot {
  width: 15px;
  height: 15px;
  display: inline-block;
  border-radius: 50%;
  margin-right: 5px;
}

.level-0 {
  background: #ddd;
}
.level-1 {
  background: red;
}
.level-2 {
  background: yellow;
}
.level-3 {
  background: green;
}
.level-unknown {
  background: gray;
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

/* เมื่อเลือกพนักงาน */
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

.skill-icon {
  width: 30px;
  height: 30px;
}

.legend-item {
  cursor: pointer;
  padding: 5px;
  border-radius: 5px;
  transition: background-color 0.2s;
}

.legend-item:hover {
  background-color: #f0f0f0;
}

.activeLegend {
  background-color: #007bff;
  color: white;
}

</style>
