<template>
  <div class="employee-table">
    <h3>Head Count</h3>
    <table>
      <thead>
        <tr>
          <th>No.</th>
          <th>EmpID</th>
          <th>Firstname</th>
          <th>Lastname</th>
          <th class="datetime-column">Date-time</th>
          <th>Gate No</th>
          <th>Division</th>
          <th>Department</th>
          <th>Biz</th>
          <th>Process</th>
          <th>CourseGroup</th>
          <th>WorkGroup</th>
          <th>Status</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="(employee, index) in employees" :key="index">
          <td>{{ index + 1 }}</td>
          <td>{{ employee.EmpID }}</td>
          <td>{{ employee.Firstname }}</td>
          <td>{{ employee.Lastname }}</td>
          <td class="datetime-column">{{ employee.Datetime || "-" }}</td>
          <td>{{ employee.GateNo }}</td>
          <td>{{ employee.Division }}</td>
          <td>{{ employee.Department }}</td>
          <td>{{ employee.Biz }}</td>
          <td>{{ employee.Process }}</td>
          <td>{{ employee.CourseGroup }}</td>
          <td>{{ employee.WorkGroup }}</td>
          <td :class="getStatusClass(employee.Status)">
            {{ employee.Status }}
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<script>
import axios from "axios";

export default {
  name: "EmployeeTable",
  data() {
    return {
      employees: [], // เก็บข้อมูลพนักงานที่ดึงจาก API
    };
  },
  methods: {
    async fetchEmployees() {
      try {
        const response = await axios.get("https://https://databasemanpowerdb-new.database.windows.net/EmployeeInfo"); // ดึงข้อมูลพนักงานจาก API
        this.employees = response.data; // กำหนดข้อมูลพนักงาน
      } catch (error) {
        console.error("Error fetching employee data:", error);
      }
    },
    getStatusClass(status) {
      return {
        "status-missing": status === "ขาดงาน",
        "status-out-cleanroom": status === "Out Cleanroom",
        "status-in-cleanroom": status === "In Cleanroom",
      };
    },

    getDivisionClass(division) {
      return {
        "division-ism": division === "ISM",
        "division-ddm": division === "DDM",
        "division-ldm": division === "LDM",
        "division-default": !division,
      };
    },
  },
  mounted() {
    this.fetchEmployees(); // เรียกใช้งานเมื่อคอมโพเนนต์ถูกโหลด
  },
};
</script>

<style scoped>
.employee-table {
  margin-top: 20px;
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
}

.status-out-cleanroom {
  color: orange;
}

.status-missing {
  color: red;
}

.datetime-column {
  white-space: nowrap;
}
</style>
