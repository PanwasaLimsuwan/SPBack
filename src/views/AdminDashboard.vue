<script setup>
import { ref, computed, onMounted, defineComponent, h } from 'vue';
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
import StatusTabMFG from "../components/StatusTabMFG.vue";

// กำหนด API
const API_BASE = 'http://localhost:5000/api';
const api = axios.create({ baseURL: API_BASE });

const employees = ref([]);
const selectedStatus = ref(null);
const selectedSkill = ref(null);
const selectedEmployee = ref(null);
const skills = ref([]);

const filters = ref({
  division: 'ALL',
  department: 'ALL',
  section: 'ALL',
  biz: 'ALL',
  process: 'ALL',
});

const visibility = ref({}); // สำหรับการแสดง/ซ่อน widget
const showModal = ref(false); // เปิดปิด Modal สำหรับเพิ่ม/แก้ไข widget
const isEdit = ref(false); // สำหรับการแยกระหว่าง Add กับ Edit widget
const widgetForm = ref({ name: '', description: '' }); // ข้อมูลฟอร์ม widget
const widgetToEdit = ref(null); // widget ที่จะทำการแก้ไข

const widgets = ref([
  { 
    id: 'insights', 
    name: 'Insights Widget', 
    description: 'Overview of insights', 
    isActive: true, 
    comp: InsightsWidget, 
    span2: true,
    binds: () => ({ filters: filters.value }) // Ensure binds returns an object
  },
  { 
    id: 'headcountStatus', 
    name: 'Headcount Status', 
    description: 'Employee headcount details', 
    isActive: true, 
    comp: HeadcountStatusMFG,
    binds: () => ({ filters: filters.value }) // Ensure binds returns an object
  },
  { 
    id: 'requiredBar', 
    name: 'Required Bar Chart', 
    description: 'Required bar chart widget', 
    isActive: true, 
    comp: RequiredBarChart,
    binds: () => ({ filters: filters.value }) // Ensure binds returns an object
  },
  // เพิ่ม widgets อื่นๆ ตามต้องการ
]);

// ฟังก์ชันเปิด/ปิด widget
const toggleWidgetVisibility = (widget) => {
  widget.isActive = !widget.isActive;
};

// ฟังก์ชันแก้ไข widget
const editWidget = (widget) => {
  isEdit.value = true;
  widgetForm.value = { ...widget };
  widgetToEdit.value = widget;
  showModal.value = true;
};

// ฟังก์ชันลบ widget
const deleteWidget = (widgetId) => {
  widgets.value = widgets.value.filter(widget => widget.id !== widgetId);
};

// ฟังก์ชันเพิ่ม/แก้ไข widget
const handleSubmit = () => {
  if (isEdit.value) {
    // แก้ไข widget
    Object.assign(widgetToEdit.value, widgetForm.value);
  } else {
    // เพิ่ม widget ใหม่
    const newWidget = { id: Date.now().toString(), ...widgetForm.value, isActive: true };
    widgets.value.push(newWidget);
  }
  closeModal();
};

// ฟังก์ชันปิด Modal
const closeModal = () => {
  showModal.value = false;
  isEdit.value = false;
  widgetForm.value = { name: '', description: '' };
};

// ฟังก์ชันสำหรับการตั้งค่าการแสดง/ซ่อน widget
const initVisibilityDefault = () => {
  const v = {};
  for (const w of widgets.value) v[w.id] = w.isActive; // กำหนดค่าตั้งต้นว่า widget ไหนจะแสดง
  visibility.value = v;
};

onMounted(() => {
  initVisibilityDefault();
});

// ฟังก์ชัน toggle visibility ของ widget ในการจัดเรียงใหม่
const showAll = () => {
  for (const id of Object.keys(visibility.value)) visibility.value[id] = true;
};

const hideAll = () => {
  for (const id of Object.keys(visibility.value)) visibility.value[id] = false;
};
</script>


<template>
  <div class="DashboardMFG">
    <header class="header">
      <div class="logo-title">
        <a href="http://localhost:8080/" class="logo">
          <img src="logo2.png" alt="Sony Logo" />
        </a>
        <h1>Real-time Monitoring Dashboard</h1>
        <h1 style="color: red;">For MFG</h1>
      </div>

      <button @click="showModal = true" class="btn btn-primary">Add New Widget</button>
    </header>

    <div class="widget-list mt-4">
      <div v-for="widget in widgets" :key="widget.id" class="widget-item bg-white p-4 rounded shadow-lg mb-4">
        <div class="flex justify-between items-center">
          <h2 class="text-lg font-semibold">{{ widget.name }}</h2>
          <div>
            <button @click="toggleWidgetVisibility(widget)" class="btn btn-sm btn-warning">
              {{ widget.isActive ? 'Deactivate' : 'Activate' }}
            </button>
            <button @click="editWidget(widget)" class="btn btn-sm btn-info">Edit</button>
            <button @click="deleteWidget(widget.id)" class="btn btn-sm btn-danger">Delete</button>
          </div>
        </div>
        <p class="mt-2">{{ widget.description }}</p>
        <p v-if="!widget.isActive" class="text-red-500">This widget is deactivated</p>
      </div>
    </div>

    <!-- Modal for adding/editing widgets -->
    <div v-if="showModal" class="modal">
      <div class="modal-content">
        <h2>{{ isEdit ? 'Edit Widget' : 'Add New Widget' }}</h2>
        <form @submit.prevent="handleSubmit">
          <div>
            <label for="widgetName">Widget Name</label>
            <input type="text" v-model="widgetForm.name" id="widgetName" required />
          </div>
          <div>
            <label for="widgetDescription">Description</label>
            <textarea v-model="widgetForm.description" id="widgetDescription" required></textarea>
          </div>
          <div>
            <button type="submit" class="btn btn-primary">{{ isEdit ? 'Save Changes' : 'Add Widget' }}</button>
            <button @click="closeModal" type="button" class="btn btn-secondary">Cancel</button>
          </div>
        </form>
      </div>
    </div>

    <!-- Draggable Widget Area -->
    <draggable v-model="widgets" item-key="id" class="grid-two-col" ghost-class="drag-ghost" handle=".drag-handle" :animation="200">
      <template #item="{ element }">
        <div class="card" :class="{ 'span-2': element?.span2 }" v-show="visibility[element.id] !== false">
          <div class="card-bar">
            <span class="drag-handle" title="Drag to move">⠿</span>
          </div>
          <component :is="element.comp" v-bind="element.binds()" v-on="element.on()" />
        </div>
      </template>
    </draggable>
  </div>
</template>

<style scoped>
.widget-item {
  display: flex;
  flex-direction: column;
}

.widget-item button {
  margin-left: 8px;
}

.modal {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  justify-content: center;
  align-items: center;
}

.modal-content {
  background-color: white;
  padding: 20px;
  border-radius: 8px;
  width: 400px;
}

.btn {
  padding: 8px 12px;
  margin-top: 10px;
  border-radius: 4px;
}

.btn-primary {
  background-color: #007bff;
  color: white;
}

.btn-info {
  background-color: #17a2b8;
  color: white;
}

.btn-danger {
  background-color: #dc3545;
  color: white;
}

.btn-warning {
  background-color: #ffc107;
  color: white;
}

.btn-secondary {
  background-color: #6c757d;
  color: white;
}
</style>
