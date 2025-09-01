<script setup>
import { ref, onMounted, watch } from 'vue';
import axios from 'axios';

const props = defineProps({
  filter: {
    type: Object,
    default: null, // เช่น { groupBy: 'biz', value: 'IS' }
  }
});

const resetFilter = () => {
  emit('clear-employee');
};

const employees = ref([]);
const filteredEmployees = ref([]);
const emit = defineEmits(['clear-employee']);

onMounted(async () => {
  try {
    const response = await axios.get("https://databasemanpowerdb.database.windows.net/api/GateEntry");
    employees.value = response.data;
    filteredEmployees.value = response.data;
  } catch (error) {
    console.error("Error fetching employee data:", error);
  }
});

watch(
  () => props.filter,
  (newFilter) => {
    if (!newFilter || !newFilter.groupBy || !newFilter.value) {
      filteredEmployees.value = employees.value;
    } else {
      filteredEmployees.value = employees.value.filter(emp => emp[newFilter.groupBy] === newFilter.value);
    }
  },
  { immediate: true }
);
</script>

<template>
  <div class="employee-table">
    <div v-if="isLoading" class="loading">Loading employee data...</div>
    <div v-else class="table-scroll">
      <h3>Head Count</h3>
      <button class="refresh-skill-btn" @click="resetFilter" title="รีเซตฟิลเตอร์">
        <img src="refresh.png" alt="Refresh Icon" class="icon" />
        <span>Refresh</span>
      </button>
      <table>
        <thead>
          <tr>
            <th>EmpID</th>
            <th>Firstname</th>
            <th>Lastname</th>
            <th>Section</th>
            <th>Position</th>
            <th>ShiftCode</th>
            <th>Biz</th>
            <th>Process</th> <!-- Changed Division to Process -->
          </tr>
        </thead>
        <tbody>
          <tr v-for="(employee, index) in filteredEmployees" :key="index">
            <td>{{ employee.empID }}</td>
            <td>{{ employee.firstName }}</td>
            <td>{{ employee.lastName }}</td>
            <td>{{ employee.section }}</td>
            <td>{{ employee.position }}</td>
            <td>{{ employee.shiftCode }}</td>
            <td>{{ employee.biz }}</td>
            <td>{{ employee.process }}</td> <!-- Updated to reflect 'process' field -->
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

.employee-table {
  margin-top: 20px;
}

.loading {
  text-align: center;
  font-weight: bold;
  color: #888;
  padding: 20px;
}

.table-scroll {
  max-height: 400px;
  overflow-y: auto;
  overflow-x: auto;
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
</style>
