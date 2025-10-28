<template>
  <div>
    <h1>Admin Dashboard</h1>
    
    <!-- ฟอร์มการลงทะเบียน Admin -->
    <!-- <form @submit.prevent="registerAdmin">
      <h2>Register Admin</h2>
      <input v-model="admin.firstName" placeholder="First Name" required />
      <input v-model="admin.lastName" placeholder="Last Name" required />
      <input v-model="admin.email" type="email" placeholder="Email" required />
      <input v-model="admin.password" type="password" placeholder="Password" required />
      <button type="submit">Register Admin</button>
    </form> -->

    <!-- ตารางแสดงข้อมูลพนักงาน -->
    <table v-if="employees.length > 0">
      <thead>
        <tr>
          <th>EmpID</th>
          <th>First Name</th>
          <th>Last Name</th>
          <th>Email</th>
          <th>Action</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="employee in employees" :key="employee.empID">
          <td>{{ employee.empID }}</td>
          <td>{{ employee.firstName }}</td>
          <td>{{ employee.lastName }}</td>
          <td>{{ employee.email }}</td>
          <td>
          <button @click="registerAdmin(employee)">Register</button>
            <button @click="editEmployee(employee)">Edit</button>
            <button @click="deleteEmployee(employee.empID)">Delete</button>
          </td>
        </tr>
      </tbody>
    </table>
    <div v-else>
      <p>No employees found.</p>
    </div>
  </div>
</template>


<script setup>
import { ref, onMounted } from 'vue';
import axios from 'axios';

const employees = ref([]);

const getTechnicians = async () => {
  try {
    const response = await axios.get('http://localhost:5000/api/EmployeeInfo/get-technicians');
    console.log('Technicians data:', response.data);

    if (response.data && response.data.length > 0) {
      employees.value = response.data;
    } else {
      console.error("No technicians found or invalid data");
    }
  } catch (error) {
    console.error('Error fetching technicians', error);
  }
};

const generateRandomPassword = (length = 8) => {
  const characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()_+";
  let password = "";
  for (let i = 0; i < length; i++) {
    const randomIndex = Math.floor(Math.random() * characters.length);
    password += characters[randomIndex];
  }
  return password;
};

const registerAdmin = async (employee) => {
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

//   const defaultPassword = "defaultpassword";  // หรือสามารถให้ admin ระบุรหัสผ่าน

//   console.log(`Registering employee with empID: ${employee.empID}`);

const defaultPassword = generateRandomPassword(12);  // ความยาวรหัสผ่านเป็น 12 ตัวอักษร
  console.log(`Generated Password: ${defaultPassword}`);

  try {
    const response = await axios.post('http://localhost:5000/api/admin/register-admin', {
      EmpID: employee.empID,
      FirstName: employee.firstName,
      LastName: employee.lastName,
      Email: employee.email,
      PasswordHash: defaultPassword,
      Role: "Admin"
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

// ฟังก์ชันการแก้ไขพนักงาน
const editEmployee = async (employee) => {
  const updatedData = {
    EmpID: employee.empID,
    FirstName: employee.firstName,
    LastName: employee.lastName,
    Email: employee.email,
    Role: "Leader"  // หรือ role ที่ต้องการแก้ไข
  };

  try {
    const response = await axios.put(`http://localhost:5000/api/admin/edit/${employee.empID}`, updatedData);
    console.log("Edit Response:", response.data);
    if (response.status === 200) {
      alert('Employee updated successfully.');
      getSupervisors();  // รีเฟรชข้อมูลพนักงานหลังจากการแก้ไข
    }
  } catch (error) {
    console.error("Error during edit:", error);
    alert('Error during edit.');
  }
};

// ฟังก์ชันการลบพนักงาน
const deleteEmployee = async (empID) => {
  if (!empID) {
    console.error("Invalid EmpID!");
    return;
  }

  try {
    const response = await axios.delete(`http://localhost:5000/api/admin/delete/${empID}`);
    console.log("Delete Response:", response.data);
    if (response.status === 200) {
      alert('Employee deleted successfully.');
      getSupervisors();  // รีเฟรชข้อมูลพนักงานหลังจากการลบ
    }
  } catch (error) {
    console.error("Error during delete:", error);
    alert('Error during delete.');
  }
};

// เรียกฟังก์ชันเมื่อ component ถูก mount
onMounted(() => {
//   getSupervisors();
  getTechnicians();
});
</script>
