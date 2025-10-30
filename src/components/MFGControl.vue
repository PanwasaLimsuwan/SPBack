<template>
  <div class="admin-dashboard">
    <header class="header">
      <div class="logo-title">
        <a href="/dashboard" class="logo">
          <img src="logo2.png" alt="Admin Dashboard" />
        </a>
        <h1>Admin Dashboard</h1>
      </div>

      <div class="filters">
        <!-- Filters for various features -->
        <select v-model="filters.division">
          <option value="ALL">Division: All</option>
          <option v-for="division in divisions" :key="division" :value="division">{{ division }}</option>
        </select>

        <select v-model="filters.department">
          <option value="ALL">Department: All</option>
          <option v-for="department in departments" :key="department" :value="department">{{ department }}</option>
        </select>
        <!-- Add other filters as needed -->
      </div>

      <button class="btn-customize" @click="panelOpen = !panelOpen" title="Customize widgets">
        ⚙️ Customize
      </button>
    </header>

    <!-- Customization Panel -->
    <aside class="customize-panel" :class="{ open: panelOpen }" @keydown.esc="panelOpen=false">
      <div class="cp-head">
        <h3>Manage Widgets</h3>
        <button class="cp-close" @click="panelOpen=false">✕</button>
      </div>

      <div class="cp-actions">
        <button class="cp-btn" @click="showAll()">Show All</button>
        <button class="cp-btn" @click="hideAll()">Hide All</button>
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

    <!-- Widgets Section -->
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

<script setup>
import { ref, computed, onMounted } from 'vue';
import draggable from 'vuedraggable'; 
import axios from 'axios';
import HeadcountStatusMFG from "../components/HeadcountStatusMFG.vue";
import EmployeeRecommendations from "../components/EmployeeRecommendations.vue";
// Import other necessary components

const filters = ref({
  division: 'ALL',
  department: 'ALL',
  section: 'ALL',
});

const widgets = ref([
  { id: 'headcountStatus', comp: HeadcountStatusMFG, binds: () => ({ filters: filters.value }), on: () => ({}) },
  { id: 'recommendations', comp: EmployeeRecommendations, binds: () => ({ filters: filters.value }), on: () => ({}) },
  // Add more widgets here
]);

const visibility = ref({});
const panelOpen = ref(false);
const niceNames = {
  headcountStatus: 'Headcount Status',
  recommendations: 'Employee Recommendations',
  // Add other widget names here
};

function showAll() {
  for (const id of Object.keys(visibility.value)) visibility.value[id] = true;
}
function hideAll() {
  for (const id of Object.keys(visibility.value)) visibility.value[id] = false;
}

onMounted(() => {
  const v = {};
  for (const w of widgets.value) v[w.id] = true;
  visibility.value = v;
});
</script>

<style scoped>
/* Styles for header, filters, widgets, and customize panel */
.header {
  background-color: #fff;
  padding: 10px;
}

.filters select {
  margin-left: 10px;
}

.card {
  background: #fff;
  border-radius: 8px;
  box-shadow: 0px 2px 4px rgba(0, 0, 0, 0.1);
  padding: 16px;
}

.customize-panel {
  position: fixed;
  right: 0;
  top: 0;
  background: #fff;
  width: 320px;
  height: 100vh;
  z-index: 1000;
  box-shadow: -4px 0 16px rgba(0, 0, 0, 0.1);
  padding: 16px;
}

.customize-backdrop {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.25);
  z-index: 999;
}

.card-bar {
  display: flex;
  justify-content: flex-end;
}

.card .drag-handle {
  cursor: grab;
  padding: 8px;
}

.grid-two-col {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 20px;
}

.grid-two-col .card {
  min-height: 120px;
}
</style>
