<script setup>
import { ref, computed, onMounted, watch, defineComponent, h } from 'vue';
import axios from 'axios';
import draggable from 'vuedraggable'; // ✅ เพิ่ม
import InsightsWidget from "../components/InsightsWidget.vue";

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

const API_BASE = 'http://localhost:5000/api';
const AUTH_TOKEN = localStorage.getItem('token'); // จาก /api/admin/login ที่คุณมีอยู่

const api = axios.create({ baseURL: API_BASE });
api.interceptors.request.use(cfg => {
  const t = localStorage.getItem('token'); // อ่านสดทุกครั้ง
  if (t) cfg.headers.Authorization = `Bearer ${t}`;
  return cfg;
});

const employees = ref([]);
const selectedStatus = ref(null);
const selectedSkill = ref(null);
const selectedEmployee = ref(null);
const skills = ref([]);

const selectedProcess = ref(null);

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
    const response = await axios.get('http://localhost:5000/api/EmployeeInfo');
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

// รวมตารางสกิล + รายละเอียดเป็น panel เดียวเพื่อให้ลากเป็นก้อน (ถ้าต้องการ)
// const SkillPanel = {
//   name: 'SkillPanel',
//   props: { selectedSkill: Object, employees: Object, selectedEmployee: Object, skills: Object, filters: Object },
//   components: { EmployeeSkillTable, EmployeeSkillSection },
//   emits: ['clear-skill','selectEmployee'],
//   template: `
//     <div class="skill-section-container">
//       <div class="skill-table">
//         <EmployeeSkillTable
//           :selectedSkillFilter="selectedSkill"
//           :employees="employees"
//           :selectedEmployee="selectedEmployee"
//           :filters="filters"
//           @clear-skill="$emit('clear-skill')"
//           @selectEmployee="$emit('selectEmployee', $event)"
//         />
//       </div>
//       <div class="skill-detail">
//         <EmployeeSkillSection
//           v-if="selectedEmployee"
//           :employee="selectedEmployee"
//           :skills="skills"
//           :filters="filters"
//         />
//       </div>
//     </div>
//   `
// };
const SkillsBlock = defineComponent({
  name: 'SkillsBlock',
  components: {
    FullySkilledPieChart: fullySkilledPieChart,
    EmployeeSkillTable,
    EmployeeSkillSection
  },
  props: {
    filters: Object,
    selectedSkill: Object,
    selectedEmployee: Object,
    employees: Object,
    skills: Object,
  },
  emits: ['filter-skills', 'clear-skill', 'selectEmployee'],
  render() {
    return h('div', { class: 'skills-block' }, [
      // ซ้าย: Pie
      h('div', { class: 'skills-left' }, [
        // ✅ ใช้ตัวคอมโพเนนต์ (object) ไม่ใช่สตริง
        h(fullySkilledPieChart, {
          // ถ้า child emit เป็น 'filter-skills' ให้ฟังด้วยคีย์นี้
          'onFilter-skills': (e) => this.$emit('filter-skills', e),
          filters: this.filters
        })
      ]),

      // ขวา: ตาราง + รายละเอียด
      h('div', { class: 'skills-right skill-section-container' }, [
        h('div', { class: 'skill-table' }, [
          h(EmployeeSkillTable, {
            selectedSkillFilter: this.selectedSkill,
            employees: this.employees,
            selectedEmployee: this.selectedEmployee,
            filters: this.filters,
            'onClear-skill': () => this.$emit('clear-skill'),
            // ถ้า child emit เป็น 'selectEmployee' (camelCase) ใช้ onSelectEmployee
            onSelectEmployee: (e) => this.$emit('selectEmployee', e)
          })
        ]),
        this.selectedEmployee
          ? h('div', { class: 'skill-detail' }, [
              h(EmployeeSkillSection, {
                employee: this.selectedEmployee,
                skills: this.skills,
                filters: this.filters
              })
            ])
          : null
      ])
    ]);
  }
});

// ลิสต์วิดเจ็ตเริ่มต้น (ลำดับเริ่มต้น)
const widgets = ref([
  // ⬇️ ใส่การ์ด Insights เข้าไปสักตำแหน่ง (บนสุดก็ได้)
  { id: 'insights', comp: InsightsWidget, span2: true,
    binds: () => ({ filters: filters.value }), on: () => ({}) },

  // { id: 'statusTab',       comp: StatusTabMFG,             binds: () => ({ filters: filters.value }), on: () => ({}) },
  { id: 'headcountStatus', comp: HeadcountStatusMFG,       binds: () => ({ filters: filters.value }), on: () => ({ 'filter-status': filterEmployeesByStatus }) },
  { id: 'headcountTable',  comp: EmployeeHeadcountMFG,     binds: () => ({ employees: filteredEmployees.value, filterStatus: selectedStatus.value, filters: filters.value }), on: () => ({ 'clear-status': () => (selectedStatus.value = null) }) },
  { id: 'requiredBar',     comp: RequiredBarChart,         binds: () => ({ filters: filters.value }), on: () => ({}) },
  { id: 'recommendations', comp: EmployeeRecommendations,   binds: () => ({ selectedProcess: selectedProcess.value, selectedSkill: selectedSkill.value, filters: filters.value }), on: () => ({ selectEmployee }) },
  // { id: 'fullySkilled',    comp: fullySkilledPieChart,     binds: () => ({ filters: filters.value }), on: () => ({ 'filter-skills': filterEmployeesBySkill }) },
  // { id: 'skillPanel',      comp: SkillPanel,               binds: () => ({ selectedSkill: selectedSkill.value, employees: filteredEmployees.value, selectedEmployee: selectedEmployee.value, skills: skills.value, filters: filters.value }), on: () => ({ 'clear-skill': () => (selectedSkill.value = null), selectEmployee }) },
  
  {
  id: 'skillsBlock',
  comp: SkillsBlock,
  // ถ้าอยากให้ยาวเต็มแถว คอมเมนต์ span2 เปิดไว้ แล้วเพิ่ม CSS ข้างล่าง
  span2: true,
  binds: () => ({
    filters: filters.value,
    selectedSkill: selectedSkill.value,
    selectedEmployee: selectedEmployee.value,
    employees: filteredEmployees.value,
    skills: skills.value
  }),
  on: () => ({
    'filter-skills': filterEmployeesBySkill,
    'clear-skill': () => (selectedSkill.value = null),
    selectEmployee
  })
},

  { id: 'workedTime',      comp: WorkedTimeChart,          binds: () => ({ filters: filters.value }), on: () => ({}) },
  { id: 'weeklyOvertime',  comp: WeeklyOvertime,           binds: () => ({ filters: filters.value }), on: () => ({}) },
  { id: 'monthlyOverload', comp: MonthlyWorkedTimeOverload,binds: () => ({ filters: filters.value }), on: () => ({}) },
  { id: 'weeklyAbsent',    comp: WeeklyAbsentTrend,        binds: () => ({ filters: filters.value }), on: () => ({}) },
  { id: 'headcountPlan',   comp: HeadcountPlan, span2: true,            binds: () => ({ filters: filters.value }), on: () => ({}) },
]);

// helper: ใช้ layout ที่โหลดจาก API มาจัดเรียง widgets
function applyLayoutFromIds(ids) {
  const map = new Map(widgets.value.map(w => [w.id, w]));
  const ordered = ids.map(id => map.get(id)).filter(Boolean);
  const rest = widgets.value.filter(w => !ids.includes(w.id));
  widgets.value = [...ordered, ...rest];
}

// โหลดลำดับจาก API (หลังจาก mount)
onMounted(async () => {
  try {
    const res = await api.get('/WidgetOrder', { params: { contextKey: 'mfg_dashboard' } });
    if (Array.isArray(res.data?.layout) && res.data.layout.length) {
      applyLayoutFromIds(res.data.layout);
    }
  } catch (e) {
    console.warn('Load widget order failed', e);
  }
});

// debounce save เมื่อ reorder
// let t = null;
// watch(widgets, () => {
//   clearTimeout(t);
//   t = setTimeout(async () => {
//     try {
//       const layout = widgets.value.map(w => w.id);
//       await api.put('/WidgetOrder', { contextKey: 'mfg_dashboard', layout });
//     } catch (e) {
//       console.error('Save widget order failed', e);
//     }
//   }, 400);
// }, { deep: true });
watch(widgets, () => {
  const v = { ...visibility.value };
  for (const w of widgets.value) {
    if (typeof v[w.id] !== 'boolean') v[w.id] = true; // default = แสดง
  }
  for (const k of Object.keys(v)) {
    if (!widgets.value.find(w => w.id === k)) delete v[k];
  }
  visibility.value = v;
}, { deep: true });


// ==== Visibility prefs (ซ่อน/แสดงวิดเจ็ต) ====
const panelOpen = ref(false);
const visibility = ref({}); // { [id]: true|false }

const niceNames = {
  insights: 'Insights',

  headcountStatus: 'Headcount Status',
  headcountTable: 'Employee Headcount',
  requiredBar: 'Required Bar Chart',
  recommendations: 'Employee Recommendations',
  skillsBlock: 'Skills & Skill Detail',
  workedTime: 'Worked Time',
  weeklyOvertime: 'Weekly Overtime',
  monthlyOverload: 'Monthly Worked Time Overload',
  weeklyAbsent: 'Weekly Absent Trend',
  headcountPlan: 'Headcount Plan',
};

// สร้าง default: ทุก widget แสดง
function initVisibilityDefault() {
  const v = {};
  for (const w of widgets.value) v[w.id] = true;
  visibility.value = v;
}

onMounted(() => {
  initVisibilityDefault();
});

// ปุ่มลัด
function showAll() {
  for (const id of Object.keys(visibility.value)) visibility.value[id] = true;
}
function hideAll() {
  for (const id of Object.keys(visibility.value)) visibility.value[id] = false;
}


</script>

<template>
  <div class="DashboardMFG">
    <!-- ✅ ฟิลเตอร์ยังอยู่ใน header -->
    <header class="header">
      <div class="logo-title">
        <a href="http://localhost:8080/" class="logo">
          <img src="logo2.png" alt="Sony Logo" />
        </a>
        <h1>Real time monitoring dashboard for leader allocation</h1>
        <h1 style="color: red;">For MFG</h1>
      </div>

      <!-- ✅ FILTERS: คงไว้ตามเดิม -->
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
      <button class="btn-customize" @click="panelOpen = !panelOpen" title="Customize widgets">
  ⚙️ Customize
</button>

    </header>

    <!-- ✅ StatusTabMFG อยู่นอก widgets ได้ -->
    <section class="stats">
      <StatusTabMFG :filters="filters" />
    </section>

    <!-- ✅ โซนลากสลับลำดับ (แทนทุก charts เดิม) -->
    <draggable
      v-model="widgets"
      item-key="id"
      class="grid-two-col"
      ghost-class="drag-ghost"
      handle=".drag-handle"
      :animation="200"
    >
      <!-- <template #item="{ element }">
        <div class="card"> -->
          <!-- <template #item="{ element }">
    <div class="card" :class="{ 'span-2': element?.span2 }" v-show="visibility[element.id] !== false">
          <div class="card-bar">
            <span class="drag-handle" title="ลากเพื่อย้าย">⠿</span>
          </div> -->
          <!-- <component :is="element.comp" v-bind="element.binds()" v-on="element.on()" /> -->
        <!-- <component
        v-if="element && element.comp"
        :is="element.comp"
        v-bind="element.binds()"
        v-on="element.on()"
      />
      <div v-else class="text-sm" style="color:#6b7280;">Component not found</div>
        </div>
      </template> -->
      <template #item="{ element }">
  <div class="card" :class="{ 'span-2': element?.span2 }" v-show="visibility[element.id] !== false">
    <div class="card-bar">
      <span class="drag-handle" title="ลากเพื่อย้าย">⠿</span>

      <!-- 👁 Toggle visibility per widget -->
      <button
        class="icon-btn eye-toggle"
        :aria-pressed="visibility[element.id] !== false"
        :title="visibility[element.id] === false ? 'Show widget' : 'Hide widget'"
        @click.stop="visibility[element.id] = visibility[element.id] === false ? true : false"
        @mousedown.stop
      >
        <!-- eye (แสดง) / eye-off (ซ่อน) -->
        <svg v-if="visibility[element.id] !== false" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2">
          <path d="M1 12s4-7 11-7 11 7 11 7-4 7-11 7-11-7-11-7Z"/>
          <circle cx="12" cy="12" r="3"/>
        </svg>
        <svg v-else xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2">
          <path d="M17.94 17.94A10.94 10.94 0 0 1 12 19c-7 0-11-7-11-7a21.77 21.77 0 0 1 5.18-5.94M9.9 4.24A10.94 10.94 0 0 1 12 4c7 0 11 7 11 7a21.7 21.7 0 0 1-3.23 4.31"/>
          <path d="M14.12 9.88a3 3 0 1 1-4.24 4.24M1 1l22 22"/>
        </svg>
      </button>
    </div>

    <component
      v-if="element && element.comp"
      :is="element.comp"
      v-bind="element.binds()"
      v-on="element.on()"
    />
    <div v-else class="text-sm" style="color:#6b7280;">Component not found</div>
  </div>
</template>
    </draggable>
    
    <aside class="customize-panel" :class="{ open: panelOpen }" @keydown.esc="panelOpen=false">
  <div class="cp-head">
    <h3>แสดง / ซ่อน วิดเจ็ต</h3>
    <button class="cp-close" @click="panelOpen=false">✕</button>
  </div>

  <div class="cp-actions">
    <button class="cp-btn" @click="showAll()">Show all</button>
    <button class="cp-btn" @click="hideAll()">Hide all</button>
  </div>

  <ul class="cp-list">
    <li v-for="w in widgets" :key="w.id">
      <label class="cp-row">
        <input type="checkbox" v-model="visibility[w.id]" />
        <span class="cp-name">{{ niceNames[w.id] ?? w.id }}</span>
      </label>
    </li>
  </ul>

  <p class="cp-hint">การตั้งค่านี้จะอยู่แค่ในหน้านี้ (ไม่บันทึกถาวร)</p>
</aside>
<div class="customize-backdrop" :class="{ show: panelOpen }" @click="panelOpen=false"></div>


    <!-- <section class="charts">
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
    <div class="skill-detail">
      <EmployeeSkillSection
        v-if="selectedEmployee"
        :employee="selectedEmployee"
        :skills="skills"
        :filters="filters"
      />
    </div>
  </div>
</section>
 -->

    <!-- ❌ ลบทุก <section class="charts"> ... </section> ที่ซ้ำกับ widgets ทิ้ง -->
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

.grid-two-col{
  display:grid;
  grid-template-columns:repeat(2, minmax(0,1fr));
  gap:20px;
  padding:20px;
}
@media(max-width:1024px){ .grid-two-col{ grid-template-columns:1fr; } }

.card{
  background:#fff;
  border-radius:8px;
  box-shadow:0 2px 4px rgba(0,0,0,.1);
  padding:16px;
  display:flex;
  flex-direction:column;
  min-height:120px;
}
.card-bar{
  display:flex;
  justify-content:flex-end;
  margin-bottom:8px;
}
.drag-handle{
  cursor:grab;
  user-select:none;
  padding:4px 8px;
  border-radius:6px;
  background:#f3f4f6;
}
.drag-handle:active{ cursor:grabbing; }
.drag-ghost{ opacity:.6; }

/* ---- Compact widgets inside draggable cards ---- */
.card .widget,
.card .panel,
.card .chart-container,
.card .content,
.card .box,
.card .wrap,
.card .table-wrapper {
  height: auto !important;
  min-height: 0 !important;
}

/* ถ้าปุ่ม Refresh วางแบบ absolute แล้วกันพื้นที่ไว้ ให้ย้ายไปวางทับได้โดยไม่ดันเนื้อหา */
.card [class*="refresh"],
.card .refresh,
.card .btn-refresh {
  position: absolute;
  right: 16px;
  bottom: 16px;
  /* อย่าตั้ง top:... เพราะจะกันพื้นที่ด้านบน */
}

/* กัน margin collapse และช่องว่างเกินจำเป็น */
.card > *:first-child { margin-top: 0 !important; }
.card h1, .card h2, .card h3 { margin-top: 0.25rem; }

/* ให้กริดวางการ์ดชิดบน ไม่ยืดความสูงการ์ดโดยไม่จำเป็น */
.grid-two-col { align-items: start; }

/* ให้การ์ดบางใบกิน 2 คอลัมน์ได้ ถ้าเปิด span2 */
.card.span-2 { grid-column: 1 / -1; }

/* Layout ภายใน SkillsBlock = 2 คอลัมน์ */
.skills-block {
  display: grid;
  grid-template-columns: 1fr 1fr; /* ซ้าย Pie | ขวา Table+Detail */
  gap: 20px;
}
@media (max-width: 1024px) {
  .skills-block { grid-template-columns: 1fr; }
}

/* Reset ความสูงที่กันไว้จากสไตล์เก่า */
.skills-left, .skills-right,
.skill-section-container, .skill-table, .skill-detail {
  min-height: 0 !important;
  height: auto !important;
}

/* ทำให้ปุ่ม refresh แบบ absolute ไม่ดัน content */
.card { position: relative; }
.card [class*="refresh"], .card .refresh, .card .btn-refresh {
  position: absolute; right: 16px; bottom: 16px;
}

/* Customize button */
.btn-customize{
  padding:8px 12px;
  border:1px solid #e5e7eb;
  background:#f9fafb;
  border-radius:8px;
  cursor:pointer;
  transition:transform .06s ease;
  margin-left:8px;
}
.btn-customize:active{ transform:scale(0.98); }

/* Slide-over panel */
.customize-panel{
  position:fixed; top:0; right:-360px;
  width:320px; max-width:90vw; height:100vh;
  background:#fff; box-shadow:-4px 0 16px rgba(0,0,0,.12);
  z-index:1200; padding:16px; display:flex; flex-direction:column;
  transition:right .22s ease;
}
.customize-panel.open{ right:0; }
.customize-backdrop{
  position:fixed; inset:0; background:rgba(0,0,0,.25);
  z-index:1190; display:none;
}
.customize-backdrop.show{ display:block; }

.cp-head{ display:flex; align-items:center; justify-content:space-between; margin-bottom:8px; }
.cp-close{ border:none; background:#f3f4f6; border-radius:8px; padding:6px 10px; cursor:pointer; }
.cp-actions{ display:flex; gap:8px; margin:8px 0 12px; }
.cp-btn{ padding:6px 10px; border:1px solid #e5e7eb; background:#f9fafb; border-radius:8px; cursor:pointer; }
.cp-list{ list-style:none; margin:0; padding:0; overflow:auto; }
.cp-row{ display:flex; align-items:center; gap:10px; padding:8px 4px; }
.cp-name{ font-size:14px; }
.cp-hint{ font-size:12px; color:#6b7280; margin-top:auto; }

.icon-btn{
  border:none;
  background:transparent;
  padding:6px;
  border-radius:8px;
  cursor:pointer;
  line-height:0;
}
.icon-btn:hover{ background:#f3f4f6; }
.eye-toggle{ margin-left:8px; }
.card-bar{ gap:8px; align-items:center; }

</style>