<script setup>
import { ref, onMounted, computed } from 'vue';
import axios from 'axios';

const props = defineProps({
  filterStatus: String
});
const emit = defineEmits(['clear-status']);

const employees = ref([]);

const fetchEmployees = async () => {
  try {
    const response = await axios.get('http://localhost:5000/api/GateEntry');
    employees.value = response.data;
  } catch (error) {
    console.error('Error fetching gate entry data:', error);
  }
};

const filteredEmployees = computed(() => {
  if (!props.filterStatus) return employees.value;
  return employees.value.filter(e => e.status === props.filterStatus);
});

const sortedEmployees = computed(() => {
  const rank = {
    'status-missing': 0,
    'status-out-cleanroom': 1,
    'status-in-cleanroom': 2,
  };

  return [...filteredEmployees.value].sort((a, b) => {
    const rA = rank[a.status] ?? 99;
    const rB = rank[b.status] ?? 99;
    return rA - rB;
  });
});

const resetFilter = () => {
  emit('clear-status');
};

const getStatusClass = (status) => {
  return {
    'status-in-cleanroom': 'status-in-cleanroom',
    'status-out-cleanroom': 'status-out-cleanroom',
    'status-missing': 'status-missing',
  }[status] || '';
};

const getStatusLabel = (status) => {
  return {
    'status-in-cleanroom': 'In Cleanroom',
    'status-out-cleanroom': 'Out Cleanroom',
    'status-missing': 'Missing',
  }[status] || status;
};

onMounted(() => {
  fetchEmployees();
});
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
            <th class="datetime-column">Date-time</th>
            <th>Gate No</th>
            <th>Process</th>
            <th>CourseGroup</th>
            <th>WorkGroup</th>
            <th>Status</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="(employee, index) in  sortedEmployees" :key="index">
            <td>{{ employee.empID }}</td>
            <td>{{ employee.firstName }}</td>
            <td>{{ employee.lastName }}</td>
            <td class="datetime-column">{{ employee.entryDateTime || '-' }}</td>
            <td>{{ employee.gateNo || '-' }}</td>
            <td>{{ employee.process || '-' }}</td>
            <td>{{ employee.courseGroup || '-' }}</td>
            <td>{{ employee.workGroup || '-' }}</td>
            <td :class="getStatusClass(employee.status)">
              {{ getStatusLabel(employee.status) }}
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

.status-in-cleanroom {
  color: green;
  font-weight: bold;
}

.status-out-cleanroom {
  color: orange;
  font-weight: bold;
}

.status-missing {
  color: red;
  font-weight: bold;
}

.datetime-column {
  white-space: nowrap;
}
</style>