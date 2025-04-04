<script setup>
import { ref } from 'vue';
import HeadcountStatus from "../components/HeadcountStatus.vue";
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
import EmployeeGateEntry from "../components/EmployeeGateEntry.vue";
import StatusTab from "@/components/StatusTab.vue";

const employees = ref([]);
const filteredEmployees = ref([]);
const selectedStatus = ref(null);
const selectedSkill = ref(null);
const selectedEmployee = ref(null);
const skills = ref([]);

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
      <a href="http://localhost:8080/" class="logo">
        <img src="logo2.png" alt="Sony Logo" class="logo" />
      </a>
      <div class="filters">
        <select>
          <option value="ALL">Division : ALL</option>
          <option value="ISM">ISM</option>
          <option value="DDM">DDM</option>
          <option value="LDM">LDM</option>
        </select>
        <select>
          <option value="ALL">Department : ALL</option>
          <option value="MF1">MF1</option>
          <option value="MF2">MF2</option>
        </select>
        <select>
          <option value="ALL">Section : ALL</option>
          <option value="ASSY Section">ASSY Section</option>
          <option value="TEST Section">TEST Section</option>
        </select>
        <select>
          <option value="ALL">Biz : ALL</option>
          <option value="IS">IS</option>
          <option value="HTPS">HTPS</option>
          <option value="MOLED">MOLED</option>
        </select>
        <select>
          <option value="ALL">Process : ALL</option>
          <option value="ASSY">ASSY</option>
          <option value="MOKU">MOKU</option>
          <option value="CSAT">CSAT</option>
          <option value="JUNB">JUNB</option>
        </select>
        <input type="text" placeholder="Search" />
      </div>
    </header>

    <section class="stats">
      <StatusTab />
    </section>

    <section class="charts">
      <div class="chart">
        <HeadcountStatus @filter-status="filterEmployeesByStatus" />
      </div>
      <div class="table">
        <EmployeeGateEntry :filterStatus="selectedStatus" @clear-status="selectedStatus = null" />
      </div>
    </section>

    <section class="charts">
      <div class="chart">
        <RequiredBarChart />
      </div>
      <div class="table">
        <EmployeeRecommendations :employees="filteredEmployees" @selectEmployee="selectEmployee" />
      </div>
    </section>

    <section class="charts">
      <div class="chart">
        <fullySkilledPieChart @filter-skills="filterEmployeesBySkill" />
      </div>
      <div class="skill-section-container">
        <div class="skill-table">
          <EmployeeSkillTable
            :selectedSkillFilter="selectedSkill"
            @clear-skill="selectedSkill = null"
            :employees="filteredEmployees"
            :selectedEmployee="selectedEmployee"
            @selectEmployee="selectEmployee"
          />
        </div>
        <div class="skill-detail" ref="skillSection">
          <EmployeeSkillSection v-if="selectedEmployee" :employee="selectedEmployee" :skills="skills" />
        </div>
      </div>
    </section>

    <section class="charts">
      <div class="chart">
        <WorkedTimeChart />
      </div>
      <div class="chart">
        <WeeklyOvertime />
      </div>
    </section>

    <section class="charts">
      <div class="chart">
        <MonthlyWorkedTimeOverload />
      </div>
      <div class="chart">
        <WeeklyAbsentTrend />
      </div>
    </section>

    <section class="charts">
      <div class="chart">
        <HeadcountPlan />
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
