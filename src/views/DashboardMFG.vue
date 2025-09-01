<script setup>
import { ref, computed, onMounted } from 'vue';
import axios from 'axios';

import HeadcountStatusMFG from "../components/HeadcountStatusMFG.vue";
import RequiredBarChart from "../components/RequiredBarChart.vue";
import EmployeeRecommendations from "../components/EmployeeRecommendations.vue";
import EmployeeSkillTable from "../components/EmployeeSkillTable.vue";
import EmployeeSkillSection from "../components/EmployeeSkillSection.vue";
import fullySkilledPieChart from "../components/fullySkilledPieChart.vue";
import WorkedTimeChart from "../components/WorkedTimeChart.vue";
import WeeklyAbsentTrend from "../components/WeeklyAbsentTrend.vue";
import MonthlyWorkedTimeOverload from "../components/MonthlyWorkedTimeOverload.vue";
import WeeklyOvertime from "../components/WeeklyOvertime.vue";
import HeadcountPlan from "../components/HeadcountPlan.vue";
import EmployeeHeadcountMFG from "../components/EmployeeHeadcountMFG.vue";
// import StatusTabMFG from "@/components/StatusTabMFG.vue";
import StatusTabMFG from "../components/StatusTabMFG.vue";

const employees = ref([]);
const selectedStatus = ref(null);
const selectedSkill = ref(null);
const selectedEmployee = ref(null);
const skills = ref([]);

// ✅ Filters
const filters = ref({
  division: 'ALL',
  department: 'ALL',
  section: 'ALL',
  biz: 'ALL',
  process: 'ALL',
  // search: '',
});

// ✅ Dynamic options
const divisions = computed(() => [...new Set(employees.value.map(e => e.division).filter(Boolean))]);
const departments = computed(() => [...new Set(employees.value.map(e => e.department).filter(Boolean))]);
const sections = computed(() => [...new Set(employees.value.map(e => e.section).filter(Boolean))]);
const bizs = computed(() => [...new Set(employees.value.map(e => e.biz).filter(Boolean))]);
const processes = computed(() => [...new Set(employees.value.map(e => e.process).filter(Boolean))]);

// ✅ Fetch Employee Data
onMounted(async () => {
  try {
    const response = await axios.get('https://databasemanpowerdb.database.windows.net/api/EmployeeInfo');
    employees.value = response.data;
  } catch (error) {
    console.error('Error fetching employees:', error);
  }
});

// ✅ Filter function
const filteredEmployees = computed(() => {
  return employees.value.filter(emp => {
    const matchDivision = filters.value.division === 'ALL' || emp.division === filters.value.division;
    const matchDepartment = filters.value.department === 'ALL' || emp.department === filters.value.department;
    const matchSection = filters.value.section === 'ALL' || emp.section === filters.value.section;
    const matchBiz = filters.value.biz === 'ALL' || emp.biz === filters.value.biz;
    const matchProcess = filters.value.process === 'ALL' || emp.process === filters.value.process;
    // const matchSearch = filters.value.search === '' || emp.firstName?.toLowerCase().includes(filters.value.search.toLowerCase()) || emp.lastName?.toLowerCase().includes(filters.value.search.toLowerCase());

    return matchDivision && matchDepartment && matchSection && matchBiz && matchProcess;
  });
});

// ✅ Event functions
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
  <h1 style="color: red;">For MFG</h1>
</div>
      <div class="filters">
        <select v-model="filters.division">
          <option value="ALL">Division : ALL</option>
          <option v-for="division in divisions" :key="division" :value="division">{{ division }}</option>
        </select>
        <select v-model="filters.department">
          <option value="ALL">Department : ALL</option>
          <option v-for="department in departments" :key="department" :value="department">{{ department }}</option>
        </select>
        <select v-model="filters.section">
          <option value="ALL">Section : ALL</option>
          <option v-for="section in sections" :key="section" :value="section">{{ section }}</option>
        </select>
        <select v-model="filters.biz">
          <option value="ALL">Biz : ALL</option>
          <option v-for="biz in bizs" :key="biz" :value="biz">{{ biz }}</option>
        </select>
        <select v-model="filters.process">
          <option value="ALL">Process : ALL</option>
          <option v-for="process in processes" :key="process" :value="process">{{ process }}</option>
        </select>
      </div>
    </header>

    <section class="stats">
      <StatusTabMFG :filters="filters" />
    </section>

    <section class="charts">
      <div class="chart">
        <HeadcountStatusMFG :filters="filters" @filter-status="filterEmployeesByStatus" />
      </div>
      <div class="table">
        <EmployeeHeadcountMFG
          :employees="filteredEmployees"
          :filterStatus="selectedStatus"
          :filters="filters"
          @clear-status="selectedStatus = null"
        />
      </div>
    </section>

    <section class="charts">
      <div class="chart">
        <RequiredBarChart :filters="filters" />
      </div>
      <div class="table">
        <EmployeeRecommendations
  :selectedProcess="selectedProcess"
  :selectedSkill="selectedSkill"
  :filters="filters"
  
  @selectEmployee="selectEmployee"
/>

      </div>
    </section>

    <section class="charts">
      <div class="chart">
        <fullySkilledPieChart :filters="filters" @filter-skills="filterEmployeesBySkill" />
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

    <section class="charts">
      <div class="chart">
        <WorkedTimeChart :filters="filters" />
      </div>
      <div class="chart">
        <WeeklyOvertime :filters="filters" />
      </div>
    </section>

    <section class="charts">
      <div class="chart">
        <MonthlyWorkedTimeOverload :filters="filters" />
      </div>
      <div class="chart">
        <WeeklyAbsentTrend :filters="filters" />
      </div>
    </section>

    <section class="charts">
      <div class="chart">
        <HeadcountPlan :filters="filters" />
      </div>
    </section>
  </div>
</template>

<style>
.header {
  position: sticky;
  top: 0;
  z-index: 1000;
  background-color: #fff;
  display: flex;
  flex-wrap: wrap;
  justify-content: space-between;
  align-items: center;
  padding: 15px 20px;
  box-shadow: 0px 2px 8px rgba(0, 0, 0, 0.1);
  border-bottom: 1px solid #e0e0e0;
  border-radius: 0 0 8px 8px;
}

.logo-title {
  display: flex;
  align-items: center;
  gap: 16px;
}
.logo-title .logo img {
  width: 120px;
  height: auto;
}
.logo-title h1 {
  margin: 0;
  font-size: 1.5rem;
  font-weight: 600;
  color: #333;
}

.logo {
  width: 150px;
}

.filters-wrapper {
  flex: 1;
  overflow-x: auto;
}

.filters {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
  padding: 10px 20px;
  width: 100%;
  box-sizing: border-box;
  justify-content: center;
}

.filters select,
.filters input {
  padding: 8px 12px;
  border: 1px solid #ccc;
  border-radius: 6px;
  background-color: #f9f9f9;
  transition: border-color 0.3s, background-color 0.3s;
  min-width: 160px;
}

.filters select:focus,
.filters input:focus {
  border-color: #007bff;
  background-color: #fff;
  outline: none;
}

.filters input {
  flex: 1;
}

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
