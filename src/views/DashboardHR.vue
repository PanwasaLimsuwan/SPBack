<script setup>
import { ref, computed, onMounted, watch } from "vue";
import axios from "axios";

import HeadcountStatusHR from "../components/HeadcountStatusHR.vue";
import EmployeeSkillTable from "../components/EmployeeSkillTable.vue";
import EmployeeSkillSection from "../components/EmployeeSkillSection.vue";
import fullySkilledPieChart from "../components/fullySkilledPieChart.vue";
import HeadcountTransition from "../components/HeadcountTransition.vue";
import MonthlyOvertime from "../components/MonthlyOvertime.vue";
import MonthlyAbsentTrend from "./../components/MonthlyAbsentTrend.vue";
import HeadcountEmployee from "./../components/HeadcountEmployee.vue";
import EmployeeHeadcount from "./../components/EmployeeHeadcount.vue";
import TrainingEmployee from "./../components/TrainingEmployee.vue";
import EmployeeHeadcountHR from "@/components/EmployeeHeadcountHR.vue";
import StatusTabHR from "@/components/StatusTabHR.vue";
import Logout from "../views/Logout";

// ✅ ตัวแปรหลัก
const employees = ref([]);
const filteredEmployees = ref([]);
const selectedStatus = ref(null);
const selectedSkill = ref(null);
const selectedEmployee = ref(null);
const selectedFilter = ref(null);
const skills = ref([]);

// ✅ Filter options
const filters = ref({
  division: "ALL",
  department: "ALL",
  section: "ALL",
  biz: "ALL",
  process: "ALL",
  // search: "",
});

// ✅ Dynamic dropdown options
const divisions = computed(() => [
  ...new Set(employees.value.map((e) => e.division).filter(Boolean)),
]);
const departments = computed(() => [
  ...new Set(employees.value.map((e) => e.department).filter(Boolean)),
]);
const sections = computed(() => [
  ...new Set(employees.value.map((e) => e.section).filter(Boolean)),
]);
const bizs = computed(() => [
  ...new Set(employees.value.map((e) => e.biz).filter(Boolean)),
]);
const processes = computed(() => [
  ...new Set(employees.value.map((e) => e.process).filter(Boolean)),
]);

// ✅ Fetch data
onMounted(async () => {
  try {
    const response = await axios.get(
      "http://localhost:5000/api/EmployeeInfo"
    );
    employees.value = response.data;
  } catch (error) {
    console.error("Error fetching employees:", error);
  }
});

// ✅ Filter function
const filteredEmployeesComputed = computed(() => {
  return employees.value.filter((emp) => {
    const matchDivision =
      filters.value.division === "ALL" ||
      emp.division === filters.value.division;
    const matchDepartment =
      filters.value.department === "ALL" ||
      emp.department === filters.value.department;
    const matchSection =
      filters.value.section === "ALL" || emp.section === filters.value.section;
    const matchBiz =
      filters.value.biz === "ALL" || emp.biz === filters.value.biz;
    const matchProcess =
      filters.value.process === "ALL" || emp.process === filters.value.process;
    // const matchSearch =
    //   filters.value.search === "" ||
    //   emp.firstName
    //     ?.toLowerCase()
    //     .includes(filters.value.search.toLowerCase()) ||
    //   emp.lastName?.toLowerCase().includes(filters.value.search.toLowerCase());

    return (
      matchDivision &&
      matchDepartment &&
      matchSection &&
      matchBiz &&
      matchProcess 
      // matchSearch
    );
  });
});

// ✅ ใช้ computed filter
filteredEmployees.value = filteredEmployeesComputed.value;

// ✅ Watch filter changes
watch(
  filters,
  () => {
    filteredEmployees.value = filteredEmployeesComputed.value;
  },
  { deep: true }
);

// ✅ Event handlers
const filterEmployeesByStatus = (status) => {
  selectedStatus.value = status;
};

const filterEmployeesBySkill = (skill) => {
  selectedSkill.value = skill;
};

const selectEmployee = (employee) => {
  selectedEmployee.value = employee;
};
</script>

<template>
  <div class="DashboardMFG">
    <header class="header">
      <div class="logo-title">
        <a href="http://localhost:8080/" class="logo">
          <!-- <a href="https://realtimemotitoringsystem.netlify.app/" class="logo"> -->
          <img src="logo2.png" alt="Sony Logo" />
        </a>
        <h1>Real time monitoring dashboard for leader allocation</h1>
        <h1 style="color: red">For HR</h1>
        <Logout />
      </div>
      <div class="filters">
        <select v-model="filters.division">
          <option value="ALL">Division : ALL</option>
          <option
            v-for="division in divisions"
            :key="division"
            :value="division"
          >
            {{ division }}
          </option>
        </select>
        <select v-model="filters.department">
          <option value="ALL">Department : ALL</option>
          <option
            v-for="department in departments"
            :key="department"
            :value="department"
          >
            {{ department }}
          </option>
        </select>
        <select v-model="filters.section">
          <option value="ALL">Section : ALL</option>
          <option v-for="section in sections" :key="section" :value="section">
            {{ section }}
          </option>
        </select>
        <select v-model="filters.biz">
          <option value="ALL">Biz : ALL</option>
          <option v-for="biz in bizs" :key="biz" :value="biz">{{ biz }}</option>
        </select>
        <select v-model="filters.process">
          <option value="ALL">Process : ALL</option>
          <option v-for="process in processes" :key="process" :value="process">
            {{ process }}
          </option>
        </select>
      </div>
    </header>

    <section class="stats">
      <StatusTabHR :filters="filters" />
    </section>

    <section class="charts">
      <div class="chart">
        <HeadcountStatusHR
          :filters="filters"
          @filter-status="filterEmployeesByStatus"
        />
      </div>
      <div class="table">
        <EmployeeHeadcountHR
          :employees="filteredEmployees"
          :filterStatus="selectedStatus"
          :filters="filters"
          @clear-status="selectedStatus = null"
        />
      </div>
    </section>

    <section class="charts">
      <div class="chart">
        <fullySkilledPieChart
          :filters="filters"
          @filter-skills="filterEmployeesBySkill"
        />
      </div>
      <div class="skill-section-container">
        <div class="skill-table">
          <EmployeeSkillTable
            :selectedSkillFilter="selectedSkill"
            :employees="filteredEmployees"
            :selectedEmployee="selectedEmployee"
            :filters="filters"
            @clear-skill="selectedSkill = null"
            @selectEmployee="selectEmployee"
          />
        </div>
        <div class="skill-detail" ref="skillSection">
          <EmployeeSkillSection
            v-if="selectedEmployee"
            :employee="selectedEmployee"
            :skills="skills"
            :filters="filters"
          />
        </div>
      </div>
    </section>

    <!-- โซน Headcount -->
    <section class="charts">
      <div class="chart">
        <!-- <HeadcountEmployee /> -->
        <HeadcountEmployee @filter="(filter) => (selectedFilter = filter)" />
      </div>
      <div class="table">
        <!-- <EmployeeHeadcount :employees="filteredEmployees" /> -->
        <!-- <EmployeeHeadcount :filter="selectedFilter" /> -->
        <EmployeeHeadcount
          :filter="selectedFilter"
          @clear-employee="selectedFilter = null"
        />
      </div>
    </section>

    <section class="charts">
      <div class="chart">
        <HeadcountTransition :filters="filters" />
      </div>
      <div class="chart">
        <TrainingEmployee :filters="filters" />
      </div>
    </section>

    <section class="charts">
      <div class="chart">
        <MonthlyAbsentTrend :filters="filters" />
      </div>
      <div class="chart">
        <MonthlyOvertime :filters="filters" />
      </div>
    </section>
  </div>
</template>

<style>
.dashboard {
  font-family: Arial, sans-serif;
  padding: 20px;
  background-color: #f9f9f9;
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  background-color: #fff;
  padding: 10px 20px;
  box-shadow: 0px 2px 4px rgba(0, 0, 0, 0.1);
}

.logo {
  width: 150px;
}

.filters select,
.filters input {
  margin-left: 10px;
  padding: 5px;
  border: 1px solid #ccc;
  border-radius: 4px;
}

.stats {
  display: flex;
  justify-content: space-between;
  margin-top: 20px;
  gap: 20px;
}

.charts {
  display: flex;
  justify-content: space-between;
  margin-top: 20px;
  gap: 20px;
}

.chart {
  flex: 1;
  padding: 20px;
  background: #fff;
  border-radius: 8px;
  box-shadow: 0px 2px 4px rgba(0, 0, 0, 0.1);
}

.table {
  flex: 1;
  padding: 20px;
  background: #fff;
  border-radius: 8px;
  box-shadow: 0px 2px 4px rgba(0, 0, 0, 0.1);
}

button {
  /* background-color: #007bff; */
  color: white;
  float: right; /* ปุ่มอยู่ชิดขวา */
  padding: 10px 20px;
  border: none;
  border-radius: 5px;
  cursor: pointer;
  font-size: 16px;
  display: flex;
  align-items: center;
  gap: 8px;
}

button:hover {
  background-color: #0056b3;
}

.refresh-icon {
  width: 20px;
  height: 20px;
}

.stats {
  display: flex;
  justify-content: space-around;
  gap: 20px;
  margin-top: 20px;
}

.stat-card {
  display: flex;
  align-items: center;
  background-color: #f9f9f9;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  padding: 15px;
  width: 180px;
  transition: transform 0.2s;
  justify-content: space-between;
}

/* .stat-card:hover {
  transform: translateY(-5px);
} */

.icon-container {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 40px;
  height: 40px;
  margin-left: 20px;
}

.icon {
  width: 48px;
  height: 48px;
}

/* สไตล์สำหรับจุดสี */
.status-dot {
  width: 22px;
  height: 22px;
  border-radius: 50%;
}

.content {
  text-align: right;
}

h3 {
  font-size: 20px;
  font-weight: bold;
  margin: 0;
}

h3.red {
  color: #ff0000;
}

p {
  font-size: 14px;
  margin: 4px 0 0;
}

.sub-label {
  font-size: 16px;
  color: #888;
}

.skill-section-container {
  display: flex;
  flex-direction: row;
  gap: 20px;
  align-items: flex-start;
}

.skill-table,
.skill-detail {
  flex: 1;
}
</style>
