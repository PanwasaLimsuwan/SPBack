<template>
  <div class="dashboard-container">
    <h1 class="header">Manage Leader <Logout /></h1>

    <!-- ตารางแสดงข้อมูลพนักงาน -->
    <table v-if="employees.length > 0" class="employee-table">
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
            <!-- ปุ่ม Register, Edit, Delete -->
            <button 
              class="btn-register" 
              :disabled="employee.isRegistered" 
              @click="registerLeader(employee)">
              {{ employee.isRegistered ? 'Registered' : 'Register' }}
            </button>
            <button class="btn-edit" @click="openEditForm(employee)">Edit</button>
            <button class="btn-delete" @click="deleteEmployee(employee.empID)">Delete</button>
          </td>
        </tr>
      </tbody>
    </table>

    <div v-else>
      <p class="no-employees">No employees found.</p>
    </div>

    <!-- ฟอร์มแก้ไขข้อมูล (Pop-up) -->
    <div v-if="isEditFormVisible" class="edit-form-overlay">
      <div class="edit-form-container">
        <h3>Edit Employee</h3>
        <form @submit.prevent="editEmployee">
          <label>EmpID:</label>
          <input type="text" v-model="editData.empID" readonly />
          
          <label>First Name:</label>
          <input type="text" v-model="editData.firstName" required />
          
          <label>Last Name:</label>
          <input type="text" v-model="editData.lastName" required />
          
          <label>Email:</label>
          <input type="email" v-model="editData.email" required />

          <button type="submit">Save Changes</button>
          <button type="button" @click="closeEditForm">Cancel</button>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import axios from 'axios';
import Logout from "../views/Logout";

const employees = ref([]);
const isEditFormVisible = ref(false);
const editData = ref({
  empID: '',
  firstName: '',
  lastName: '',
  email: '',
  role: 'Leader',
});

// ฟังก์ชันดึงข้อมูลพนักงานจาก API
const getSupervisors = async () => {
  try {
    const response = await axios.get('http://localhost:5000/api/EmployeeInfo/get-supervisors');
    if (response.data && response.data.length > 0) {
      // ตรวจสอบว่าพนักงานแต่ละคนลงทะเบียนแล้วหรือยัง
      employees.value = response.data.map(employee => ({
        ...employee,
        isRegistered: false // เริ่มต้นให้ไม่มีการลงทะเบียน
      }));
      
      // ตรวจสอบการลงทะเบียนใน API
      for (const employee of employees.value) {
        const isRegisteredResponse = await checkIfRegistered(employee.empID);
        employee.isRegistered = isRegisteredResponse.isRegistered;
      }
    } else {
      console.error("No supervisors found or invalid data");
    }
  } catch (error) {
    console.error('Error fetching supervisors', error);
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

// ฟังก์ชันตรวจสอบว่าพนักงานได้ลงทะเบียนหรือยัง
const checkIfRegistered = async (empID) => {
  try {
    const response = await axios.get(`http://localhost:5000/api/admin/check-registration-leader?empID=${empID}`);
    return response.data;
  } catch (error) {
    console.error('Error checking registration:', error);
    return { isRegistered: false };
  }
};


// เปิดฟอร์มแก้ไข
const openEditForm = (employee) => {
  editData.value = { ...employee }; // คัดลอกข้อมูลพนักงานที่ต้องการแก้ไข
  if (!editData.value.empID) {
    alert("Invalid employee data!");
    return;
  }
  isEditFormVisible.value = true;
};


// ปิดฟอร์มแก้ไข
const closeEditForm = () => {
  isEditFormVisible.value = false;
};

// ฟังก์ชันการลงทะเบียนพนักงาน
const registerLeader = async (employee) => {
  if (!employee || !employee.empID) {
    console.error("empID is undefined or invalid!");
    alert("Invalid employee data.");
    return;
  }

  if (!employee.firstName || !employee.lastName || !employee.email) {
    console.error("Incomplete employee data!");
    alert("Please complete the employee data.");
    return;
  }

  // const defaultPassword = generateRandomPassword(12);
  // console.log(`Generated Password: ${defaultPassword}`);

  const defaultPassword = generateRandomPassword(12);
  try {
    const response = await axios.post('http://localhost:5000/api/admin/register-leader', {
      EmpID: employee.empID,
      FirstName: employee.firstName,
      LastName: employee.lastName,
      Email: employee.email,
      PasswordHash: defaultPassword,
      Role: "Leader"
    });
    if (response.status === 200) {
      alert('Employee registered successfully.');
      // getSupervisors();  // รีเฟรชข้อมูลพนักงานหลังจากการลงทะเบียน
      getRegisteredLeaders();  // ดึงข้อมูลจาก Admin มาแสดง
    }
  } catch (error) {
    console.error("Error during registration:", error);
    alert('Error during registration.');
  }

  // try {
  //   const response = await axios.post('http://localhost:5000/api/admin/register-leader', {
  //     EmpID: employee.empID,
  //     FirstName: employee.firstName,
  //     LastName: employee.lastName,
  //     Email: employee.email,
  //     PasswordHash: defaultPassword,
  //     Role: "Leader"
  //   });

  //   if (response.status === 200) {
  //     alert('Employee registered successfully.');
  //     getSupervisors();  // รีเฟรชข้อมูลพนักงานหลังจากการลงทะเบียน
  //   }
  // } catch (error) {
  //   console.error("Error during registration:", error);
  //   alert('Error during registration.');
  // }
};

const getRegisteredLeaders = async () => {
  try {
    // ดึงข้อมูลจาก Admin หลังการลงทะเบียน
    const response = await axios.get('http://localhost:5000/api/admin/get-admins'); // ใช้ API ที่ดึงข้อมูลจาก Admin
    if (response.data && response.data.length > 0) {
      employees.value = response.data;
    } else {
      console.error("No registered leaders found or invalid data");
    }
  } catch (error) {
    console.error('Error fetching registered leaders from Admin', error);
  }
};


// ฟังก์ชันการแก้ไขพนักงาน
const editEmployee = async (employee) => {
  if (!editData.value.empID) {
    console.error("empID is undefined or invalid!");
    alert("Invalid EmpID.");
    return;
  }

  const updatedData = {
    EmpID: editData.value.empID,  // ส่ง empID ไปด้วยใน body
  };

  // ตรวจสอบการเปลี่ยนแปลงของฟิลด์ firstName
  if (editData.value.firstName !== employee.firstName) {
    updatedData.FirstName = editData.value.firstName;
  }

  // ตรวจสอบการเปลี่ยนแปลงของฟิลด์ lastName
  if (editData.value.lastName !== employee.lastName) {
    updatedData.LastName = editData.value.lastName;
  }

  // ตรวจสอบการเปลี่ยนแปลงของฟิลด์ email
  if (editData.value.email !== employee.email) {
    updatedData.Email = editData.value.email;
  }

  // ตรวจสอบว่ามีการเปลี่ยนแปลงใด ๆ หรือไม่
  if (Object.keys(updatedData).length === 1) {  // เช็คเฉพาะการเปลี่ยนแปลง (empID จะอยู่ใน updatedData เสมอ)
    alert("No changes detected.");
    return;
  }

  console.log("Sending data with empID:", updatedData.EmpID);
  console.log("Updated data:", updatedData);

  try {
    const response = await axios.put(
      `http://localhost:5000/api/admin/edit/${updatedData.EmpID}`,  // ใช้ URL ปกติไม่ต้องใส่ empID ใน URL
      updatedData,  // ส่งข้อมูลทั้งหมดรวมทั้ง empID ใน body
      {
        headers: {
          'Content-Type': 'application/json',
        },
      }
    );
    if (response.status === 200) {
      alert("Employee updated successfully.");
      // getSupervisors();  // รีเฟรชข้อมูลพนักงานหลังจากการแก้ไข
      getRegisteredLeaders();  // รีเฟรชข้อมูลจาก Admin หลังการแก้ไข
      closeEditForm();  // ปิดฟอร์มแก้ไข
    }
  } catch (error) {
    console.error("Error during edit:", error);
    alert("Error during edit. " + (error.response?.data || error.message));  // แสดงข้อความจากเซิร์ฟเวอร์
  }
};

// const editEmployee = async (employee) => {
//   if (!editData.value.empID) {
//     console.error("empID is undefined or invalid!");
//     alert("Invalid EmpID.");
//     return;
//   }

//   const updatedData = {
//     EmpID: editData.value.empID,  // ส่ง empID ไปด้วยใน body
//     FirstName: editData.value.firstName,
//     LastName: editData.value.lastName,
//     Email: editData.value.email,
//   };

//   try {
//     // เริ่มต้นการอัปเดตทั้งสองตาราง
//     const responseEmployee = await axios.put(`http://localhost:5000/api/EmployeeInfo/edit/${updatedData.EmpID}`, updatedData);  // อัปเดตใน EmployeeInfo
//     if (responseEmployee.status === 200) {
//       const responseAdmin = await axios.put(`http://localhost:5000/api/admin/edit/${updatedData.EmpID}`, updatedData);  // อัปเดตใน Admin
//       if (responseAdmin.status === 200) {
//         alert("Employee information updated successfully.");
//         getSupervisors();  // รีเฟรชข้อมูลพนักงานหลังการอัปเดต
//         closeEditForm();  // ปิดฟอร์มแก้ไข
//       }
//     }
//   } catch (error) {
//     console.error("Error during edit:", error);
//     alert("Error during edit.");
//   }
// };

// ฟังก์ชันการลบพนักงาน
const deleteEmployee = async (empID) => {
  if (!empID) {
    console.error("Invalid EmpID!");
    return;
  }

  try {
    const response = await axios.delete(`http://localhost:5000/api/admin/delete/${empID}`);
    if (response.status === 200) {
      alert('Employee deleted successfully.');
      getSupervisors();  // รีเฟรชข้อมูลพนักงานหลังจากการลบ
    }
  } catch (error) {
    console.error("Error during delete:", error);
    alert('Error during delete.');
  }
};

onMounted(() => {
  getSupervisors();
});
</script>

<style scoped>
.dashboard-container {
  padding: 20px;
  font-family: 'Arial', sans-serif;
}

.header {
  text-align: center;
  color: #333;
  margin-bottom: 20px;
}

.employee-table {
  width: 100%;
  border-collapse: collapse;
  margin-top: 20px;
}

.employee-table th, .employee-table td {
  padding: 12px;
  text-align: left;
  border: 1px solid #ddd;
}

.employee-table th {
  background-color: #f4f4f4;
}

.employee-table tr:hover {
  background-color: #f1f1f1;
}

button {
  margin: 5px;
  padding: 8px 16px;
  cursor: pointer;
  border: none;
  border-radius: 4px;
  transition: background-color 0.3s ease;
}

.btn-register {
  background-color: #4CAF50;
  color: white;
}

.btn-register:hover {
  background-color: #45a049;
}

.btn-register:disabled {
  background-color: #dcdcdc;
  cursor: not-allowed;
}

.btn-edit {
  background-color: #f9a825;
  color: white;
}

.btn-edit:hover {
  background-color: #f57f17;
}

.btn-delete {
  background-color: #e53935;
  color: white;
}

.btn-delete:hover {
  background-color: #d32f2f;
}

.no-employees {
  text-align: center;
  color: #666;
  font-size: 18px;
}

/* ฟอร์ม Pop-up */
.edit-form-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(0, 0, 0, 0.5);
  display: flex;
  justify-content: center;
  align-items: center;
}

.edit-form-container {
  background-color: white;
  padding: 20px;
  border-radius: 8px;
  width: 300px;
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
}

.edit-form-container h3 {
  text-align: center;
  margin-bottom: 20px;
}

.edit-form-container label {
  display: block;
  margin-bottom: 5px;
}

.edit-form-container input,
.edit-form-container select {
  width: 100%;
  padding: 8px;
  margin-bottom: 10px;
  border-radius: 4px;
  border: 1px solid #ddd;
}

.edit-form-container button {
  padding: 10px;
  width: 100%;
  border: none;
  border-radius: 4px;
  background-color: #4CAF50;
  color: white;
  cursor: pointer;
}

.edit-form-container button[type="button"] {
  background-color: #f44336;
  margin-top: 5px;
}
</style>
