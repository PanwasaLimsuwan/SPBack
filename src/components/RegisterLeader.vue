<script setup>
import { ref, onMounted } from 'vue';
import axios from 'axios';

const employees = ref([]);

// ฟังก์ชันดึงข้อมูลพนักงานจาก API
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

  const defaultPassword = "defaultpassword";  // หรือสามารถให้ admin ระบุรหัสผ่าน

  console.log(`Registering employee with empID: ${employee.empID}`);

  try {
    const response = await axios.post('http://localhost:5000/api/admin/register', {
      EmpID: employee.empID,
      FirstName: employee.firstName,
      LastName: employee.lastName,
      Email: employee.email,
      PasswordHash: defaultPassword,
      Role: "Leader"
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
  getSupervisors();
});
</script>
