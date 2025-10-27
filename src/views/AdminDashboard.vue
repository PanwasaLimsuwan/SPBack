<template>
  <div>
    <h1>Admin Dashboard</h1>
    <table>
      <thead>
        <tr>
          <th>EmpID</th>
          <th>First Name</th>
          <th>Last Name</th>
          <th>Email</th>
          <th>Action</th>
        </tr>
      </thead>
      <tbody v-if="employees.length > 0">
        <tr v-for="employee in employees" :key="employee.empID">
          <td>{{ employee.empID }}</td>
          <td>{{ employee.firstName }}</td>
          <td>{{ employee.lastName }}</td>
          <td>{{ employee.email }}</td>
          <td>
            <button @click="registerEmployee(employee)">Register</button>
          </td>
        </tr>
      </tbody>
      <tbody v-else>
        <tr>
          <td colspan="5">ไม่พบข้อมูล</td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import axios from 'axios';

const employees = ref([]);

// ฟังก์ชันดึงข้อมูลพนักงานจาก API
// const getEmployees = async () => {
//   try {
//     const response = await axios.get('http://localhost:5000/api/EmployeeInfo');
//     console.log('Employee data:', response.data);  // ดูข้อมูลที่ได้จาก API

//     // แปลงข้อมูลจาก Proxy เป็น plain object
//     employees.value = JSON.parse(JSON.stringify(response.data));

//     // กรองข้อมูลเพื่อแสดงเฉพาะพนักงานที่มี Position เป็น "Supervisor"
//     const Supervisor = employees.value.filter(employee => employee.Position === 'Supervisor');
//     console.log('Supervisor:', Supervisor);  // ดูข้อมูลพนักงานที่เป็น Supervisor

//     // นำข้อมูล Supervisor มาใช้ในการแสดงผล
//     employees.value = Supervisor;

//   } catch (error) {
//     console.error('Error fetching employees', error);
//   }
// };

const getSupervisors = async () => {
  try {
    const response = await axios.get('http://localhost:5000/api/EmployeeInfo/get-supervisors');
    console.log('Supervisors data:', response.data);

    if (response.data && response.data.length > 0) {
      employees.value = response.data;
    } else {
      console.error("No supervisors found or invalid data");
    }
  } catch (error) {
    console.error('Error fetching supervisors', error);
  }
};


// ฟังก์ชันการลงทะเบียนพนักงาน
const registerEmployee = async (employee) => {
  if (!employee || !employee.empID) {
    console.error("empID is undefined or invalid!");
    alert("Invalid employee data.");
    return;
  }

  // ตรวจสอบว่า firstName, lastName และ email มีค่าหรือไม่
  if (!employee.firstName || !employee.lastName || !employee.email) {
    console.error("Incomplete employee data!");
    alert("Please complete the employee data.");
    return;
  }

  console.log(`Registering employee with empID: ${employee.empID}`);

  try {
    const response = await axios.post('http://localhost:5000/api/admin/register', {
      EmpID: employee.empID,  // ใช้ empID ที่ถูกต้อง
      FirstName: employee.firstName,  // ใช้ firstName ที่ถูกต้อง
      LastName: employee.lastName,    // ใช้ lastName ที่ถูกต้อง
      Email: employee.email,          // ใช้ email ที่ถูกต้อง
      PasswordHash: "defaultpassword", // รหัสผ่านเริ่มต้น
      Role: "Leader"               // หรือ role ที่ต้องการ
    });

    console.log("Registration Response:", response.data);
    if (response.status === 200) {
      alert('Employee registered successfully.');
      getSupervisors();  // รีเฟรชข้อมูลพนักงานหลังจากการลงทะเบียน
    }
  } catch (error) {
    console.error("Error during registration:", error);
    alert('Error during registration.');
  }
};

// เรียกฟังก์ชันเมื่อ component ถูก mount
onMounted(() => {
//   getEmployees();
getSupervisors();
});
</script>

<style scoped>
/* สามารถเพิ่มสไตล์ต่างๆ ได้ */
</style>
