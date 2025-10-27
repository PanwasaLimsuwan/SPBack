<template>
  <div class="DashboardMFG">
    <!-- ✅ Header and Filters -->
    <header class="header">
      <div class="logo-title">
        <a href="http://localhost:8080/" class="logo">
          <img src="logo2.png" alt="Sony Logo" />
        </a>
        <h1>Real-time Monitoring Dashboard for Leader Allocation</h1>
        <h1 style="color: red;">For MFG</h1>
      </div>

      <!-- ✅ Filter Section -->
      <div class="filters">
        <select v-model="filters.division">
          <option value="ALL">Division: ALL</option>
          <option v-for="division in divisions" :key="division" :value="division">{{ division }}</option>
        </select>
        <select v-model="filters.department">
          <option value="ALL">Department: ALL</option>
          <option v-for="department in departments" :key="department" :value="department">{{ department }}</option>
        </select>
        <select v-model="filters.section">
          <option value="ALL">Section: ALL</option>
          <option v-for="section in sections" :key="section" :value="section">{{ section }}</option>
        </select>
        <select v-model="filters.biz">
          <option value="ALL">Biz: ALL</option>
          <option v-for="biz in bizs" :key="biz" :value="biz">{{ biz }}</option>
        </select>
        <select v-model="filters.process">
          <option value="ALL">Process: ALL</option>
          <option v-for="process in processes" :key="process" :value="process">{{ process }}</option>
        </select>
      </div>

      <button class="btn-customize" @click="panelOpen = !panelOpen" title="Customize Widgets">
        ⚙️ Customize
      </button>
    </header>

    <!-- ✅ Widgets and Draggable Zone -->
    <section class="stats">
      <StatusTabMFG :filters="filters" />
    </section>

    <draggable
      v-model="widgets"
      item-key="id"
      class="grid-two-col"
      ghost-class="drag-ghost"
      handle=".drag-handle"
      :animation="200"
    >
      <template #item="{ element }">
        <div class="card" :class="{ 'span-2': element?.span2 }" v-show="visibility[element.id] !== false">
          <div class="card-bar">
            <span class="drag-handle" title="Drag to move">⠿</span>
            <button class="icon-btn eye-toggle" @click.stop="visibility[element.id] = visibility[element.id] === false ? true : false">
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

          <!-- Render the component dynamically -->
          <component :is="element.comp" v-bind="element.binds()" v-on="element.on()" />
        </div>
      </template>
    </draggable>

    <!-- ✅ Customize Panel for Widget Visibility -->
    <aside class="customize-panel" :class="{ open: panelOpen }" @keydown.esc="panelOpen=false">
      <div class="cp-head">
        <h3>Show/Hide Widgets</h3>
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

      <p class="cp-hint">These settings are temporary and will not be saved permanently.</p>
    </aside>
    <div class="customize-backdrop" :class="{ show: panelOpen }" @click="panelOpen=false"></div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue';
import axios from 'axios';
import draggable from 'vuedraggable'; // ✅ Add draggable component
import StatusTabMFG from "../components/StatusTabMFG.vue"; // Status Tab Widget

const API_BASE = 'http://localhost:5000/api';
const employees = ref([]);
const selectedProcess = ref(null);  // Add reference for selectedProcess
const selectedSkill = ref(null);    // Add reference for selectedSkill

const filters = ref({
  division: 'ALL',
  department: 'ALL',
  section: 'ALL',
  biz: 'ALL',
  process: 'ALL',
});

const visibility = ref({}); // Stores widget visibility

// Fetch employees
onMounted(async () => {
  try {
    const response = await axios.get(`${API_BASE}/EmployeeInfo`);
    employees.value = response.data;
  } catch (error) {
    console.error("Error fetching employees", error);
  }
});

// Function to show all widgets
const showAll = () => {
  for (const id of Object.keys(visibility.value)) visibility.value[id] = true;
};

// Function to hide all widgets
const hideAll = () => {
  for (const id of Object.keys(visibility.value)) visibility.value[id] = false;
};

// Function to edit item in widget
const editItem = async (item, widgetId) => {
  try {
    const updatedData = { ...item, updatedAt: new Date() };
    const response = await axios.put(`http://localhost:5000/api/${widgetId}/edit`, updatedData);

    if (response.status === 200) {
      alert('Item updated successfully');
      fetchWidgetData(widgetId);
    }
  } catch (error) {
    console.error('Error during edit', error);
    alert('Error during editing item');
  }
};

// Function to delete item from widget
const deleteItem = async (itemId, widgetId) => {
  try {
    const response = await axios.delete(`http://localhost:5000/api/${widgetId}/delete/${itemId}`);
    if (response.status === 200) {
      alert('Item deleted successfully');
      fetchWidgetData(widgetId);
    }
  } catch (error) {
    console.error('Error during delete', error);
    alert('Error during deleting item');
  }
};

// Function to fetch widget data after editing/deleting
const fetchWidgetData = async (widgetId) => {
  try {
    const response = await axios.get(`http://localhost:5000/api/${widgetId}`);
    if (response.data) {
      updateWidgetData(widgetId, response.data);  // Update widget data
    }
  } catch (error) {
    console.error('Error fetching widget data', error);
  }
};

// Function to update widget data in state
const updateWidgetData = (widgetId, data) => {
  const widget = widgets.value.find(w => w.id === widgetId);
  if (widget) {
    widget.data = data; // Update widget data
  }
};

// Widgets definition
const widgets = ref([
  {
    id: 'recommendations',
    comp: EmployeeRecommendations,
    binds: () => ({
      selectedProcess: selectedProcess.value,
      selectedSkill: selectedSkill.value,
      filters: filters.value
    }),
    on: () => ({
      selectEmployee,
      'edit-item': editItem,
      'delete-item': deleteItem
    })
  },
]);

</script>

<style scoped>
/* Customize widget styles */
.card { background: #fff; padding: 16px; }
.card-bar { display: flex; justify-content: space-between; }
.icon-btn { background: transparent; padding: 8px; border-radius: 8px; cursor: pointer; }
</style>
