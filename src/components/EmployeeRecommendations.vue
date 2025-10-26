<template>
  <div>
    <div id="required-bar-chart"></div>

    <div class="employee-recommendations">
      <h3>แนะนำพนักงาน</h3>

      <p>
        <strong>Process:</strong> {{ selectedProcess || "-" }} |
        <strong>Skill:</strong> {{ selectedSkill || "-" }}
      </p>

      <button
        class="refresh-skill-btn"
        @click="resetSkillFilter"
        title="รีเซตฟิลเตอร์"
      >
        <img src="refresh.png" alt="Refresh Icon" class="icon" />
        <span>Refresh</span>
      </button>

      <table>
        <thead>
          <tr>
            <th>No.</th>
            <th>EmpID</th>
            <th>Firstname</th>
            <th>Lastname</th>
            <th>Work Time</th>
            <th>Skill</th>
            <th>Select</th>
            <th>Remove</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="!selectedSkill && !selectedProcess">
            <td colspan="8" style="text-align: center; padding: 20px">
              กรุณาเลือก Process และ Skill จากกราฟทางด้านซ้ายเพื่อใช้ในการแนะนำพนักงาน
            </td>
          </tr>

          <tr v-else-if="filteredEmployees.length === 0">
            <td colspan="8" style="text-align: center; padding: 20px">
              ไม่พบพนักงานที่ตรงกับเงื่อนไข กรุณาเลือก Process และ Skill อื่น
            </td>
          </tr>

          <tr v-else v-for="(employee, index) in filteredEmployees" :key="index">
            <td>{{ index + 1 }}</td>
            <td>{{ employee.empID }}</td>
            <td>{{ employee.firstName }}</td>
            <td>{{ employee.lastName }}</td>
            <td>{{ employee.totalTime }}</td>

            <!-- คอลัมน์ Skill: ปุ่มจะ disable ถ้ามีงานค้าง -->
            <!-- <td>
              <div class="skill-badge">
                <span>{{ selectedSkill }}</span>
                <button
                  :disabled="isActive(employee.empID)"
                  @click="openSelectionForm(employee)"
                  :title="isActive(employee.empID) ? 'มีงานค้างอยู่ ต้องกด Remove ก่อน' : 'เลือกพนักงาน'"
                >
                  <img src="skill.png" alt="Skill" class="skill-icon" />
                </button>
              </div>
            </td> -->
            <td>
              <div class="skill-badge">
                <span>{{ selectedSkill }}</span>
                <button @click="$emit('selectEmployee', employee)">
                  <img src="skill.png" alt="Skill" class="skill-icon" />
                </button>
              </div>
            </td>

            <!-- ปุ่ม Select: disable ถ้ามีงานค้าง -->
            <td>
              <div class="select-btn">
                <button
                  :disabled="isActive(employee.empID)"
                  @click="openSelectionForm(employee)"
                  :title="isActive(employee.empID) ? 'มีงานค้างอยู่ ต้องกด Remove ก่อน' : 'เลือกพนักงาน'"
                >
                  <img src="select.png" alt="select" class="select-icon" />
                </button>
              </div>
            </td>

            <!-- ปุ่ม Remove: ปิดงาน (ใส่ EndAt ตอนนี้ + Completed) -->
            <td>
              <button
                class="remove-btn"
                @click="updateEndAt(employee.empID)"
                title="บันทึก EndAt"
              >
                <img src="remove.png" alt="remove" class="remove-icon" />
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Modal for ToProcess and ToBiz -->
    <div v-if="showModal" class="modal">
      <div class="modal-content">
        <h3>กรอกข้อมูล Assignment</h3>

        <label for="toProcess">ToProcess:</label>
        <input
          id="toProcess"
          v-model="selectedEmployee.toProcess"
          type="text"
          placeholder="กรอกข้อมูล ToProcess"
          class="input-field"
        />

        <label for="toBiz">ToBiz:</label>
        <input
          id="toBiz"
          v-model="selectedEmployee.toBiz"
          type="text"
          placeholder="กรอกข้อมูล ToBiz"
          class="input-field"
        />

        <div style="display:flex; gap:8px; margin-top:10px;">
          <button class="refresh-skill-btn" @click="saveAssignment">บันทึกข้อมูล</button>
          <button class="refresh-skill-btn" @click="closeModal" style="background:#6c757d;">ยกเลิก</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch, nextTick } from "vue";
import axios from "axios";
import Plotly from "plotly.js";

// รับ props filter จาก parent
const props = defineProps({ filters: Object });
const emit = defineEmits(["selectEmployee", "removeEmployee"]);

const rawData = ref([]);
const skills = ref([]);
const worktime = ref([]);
const selectedSkill = ref(null);
const selectedProcess = ref(null);
const filteredEmployees = ref([]);
const selectedEmployees = ref([]);        // ใช้ภายในหน้า
const activeAssignments = ref([]);        // รายการงานที่ยังเปิดอยู่ (Active/EndAt NULL)

// Modal state
const showModal = ref(false);
const selectedEmployee = ref({ empID: null, toProcess: "", toBiz: "" });

// helper: map คีย์ Biz/Process ให้ชัวร์
const getBiz = (obj) => obj.biz ?? obj.Biz ?? null;
const getProcess = (obj) => obj.process ?? obj.Process ?? null;

// เปิด modal (กันเปิดถ้ามีงานค้าง)
const openSelectionForm = (employee) => {
  if (isActive(employee.empID)) {
    alert("พนักงานคนนี้มีงานที่ยังไม่ปิดอยู่ กรุณากด Remove เพื่อปิดงานก่อน");
    return;
  }
  selectedEmployee.value = {
    ...employee,
    biz: getBiz(employee),
    process: getProcess(employee),
    toProcess: "",
    toBiz: "",
  };
  showModal.value = true;
};
const closeModal = () => { showModal.value = false; };

// รวมข้อมูลไว้วาดกราฟ
const aggregate = (process, skill) => {
  return rawData.value
    .filter((item) => item.process === process && item.skillGroup === skill)
    .reduce((sum, item) => sum + item.require, 0);
};

// วาดกราฟ
const drawChart = () => {
  if (!rawData.value.length) return;

  const processes = [...new Set(rawData.value.map((d) => d.process))];
  const skillGroups = [...new Set(rawData.value.map((d) => d.skillGroup))];

  const traces = skillGroups.map((skill) => ({
    x: processes,
    y: processes.map((p) => aggregate(p, skill)),
    name: skill,
    type: "bar",
    text: processes.map((p) => aggregate(p, skill)),
    textposition: "auto",
  }));

  const layout = {
    title: "Manpower Requirement by Process and Skill Group",
    barmode: "stack",
    height: 450,
    xaxis: { title: "Process" },
    yaxis: { title: "Required Employees" },
  };

  Plotly.newPlot("required-bar-chart", traces, layout).then(() => {
    const chart = document.getElementById("required-bar-chart");
    chart.on("plotly_click", handleBarClick);
  });
};

// เมื่อ click bar chart → filter พนักงาน
const handleBarClick = (event) => {
  const skill = event.points[0].data.name;
  const process = event.points[0].x;

  selectedSkill.value = skill;
  selectedProcess.value = process;

  const filtered = skills.value
    .map((emp) => {
      const empWorktimeList = worktime.value.filter((w) => w.empID === emp.empID);
      const totalTime = empWorktimeList.reduce((sum, w) => sum + (w.totalHours ?? 0), 0);

      return {
        ...emp,
        totalTime,
        skillLevel: emp[selectedSkill.value.toLowerCase()] ?? 0,
      };
    })
    .filter(
      (emp) =>
        emp.skillGroup === selectedSkill.value &&
        emp.process === selectedProcess.value &&
        emp.totalTime <= 60
    )
    .sort((a, b) => {
      if (b.skillLevel !== a.skillLevel) return b.skillLevel - a.skillLevel;
      return 60 - b.totalTime - (60 - a.totalTime);
    });

  filteredEmployees.value = filtered;
};

// Reset filter
const resetSkillFilter = () => {
  filteredEmployees.value = [];
  selectedSkill.value = null;
  selectedProcess.value = null;
  selectedEmployees.value = [];
};

// ตรวจว่ามีงานค้างไหม
const isActive = (empID) => activeAssignments.value.some((a) => a.empID === empID);

// บันทึก Assignment (Select)
const saveAssignment = async () => {
  if (!selectedEmployee.value.toProcess || !selectedEmployee.value.toBiz) {
    alert("กรุณากรอกข้อมูล ToProcess และ ToBiz ก่อน");
    return;
  }

  const payload = {
    EmpID: selectedEmployee.value.empID,
    FromBiz: getBiz(selectedEmployee.value),
    FromProcess: getProcess(selectedEmployee.value),
    ToProcess: selectedEmployee.value.toProcess,
    ToBiz: selectedEmployee.value.toBiz,
    SkillGroup: selectedSkill.value,
    StartAt: new Date().toISOString(),
    EndAt: null,            // เปิดงาน = ยังไม่จบ
    Status: "Active",
  };

  try {
    await axios.post("http://localhost:5000/api/Assignment", payload);

    // เก็บไว้ว่า empID นี้มีงานค้างแล้ว → disable ปุ่มทันที
    activeAssignments.value.push({ empID: selectedEmployee.value.empID });
    alert("บันทึกข้อมูล Assignment สำเร็จ");
    closeModal();
  } catch (error) {
    console.error("Error creating assignment:", error);
    alert("บันทึกไม่สำเร็จ:\n" + (error?.response?.data || error.message));
  }
};

// ปิดงาน (Remove) → อัปเดต EndAt + Status
const updateEndAt = async (empID) => {
  try {
    const endAt = new Date().toISOString();
    const status = "Completed";

    const response = await axios.put(
      `http://localhost:5000/api/Assignment/${empID}`,
      { EndAt: endAt, Status: status }
    );

    if (response.status === 200) {
      // เอาคนนี้ออกจากรายการ Active เพื่อให้กลับมาเลือกได้
      activeAssignments.value = activeAssignments.value.filter((a) => a.empID !== empID);
      alert("บันทึกข้อมูล EndAt สำเร็จ");
    } else {
      alert("ไม่สามารถบันทึก EndAt ได้: " + response.data);
    }
  } catch (error) {
    console.error("Error updating EndAt:", error);
    alert("ไม่สามารถบันทึก EndAt ได้");
  }
};

// ดึงข้อมูล API
const fetchData = async () => {
  try {
    const [req, skillRes, worktimeRes, activeRes] = await Promise.all([
      axios.get("http://localhost:5000/api/ManpowerReq", {
        params: {
          division: props.filters.division !== "ALL" ? props.filters.division : undefined,
          department: props.filters.department !== "ALL" ? props.filters.department : undefined,
          section: props.filters.section !== "ALL" ? props.filters.section : undefined,
          biz: props.filters.biz !== "ALL" ? props.filters.biz : undefined,
          process: props.filters.process !== "ALL" ? props.filters.process : undefined,
        },
      }),
      axios.get("http://localhost:5000/api/Skill", {
        params: {
          division: props.filters.division !== "ALL" ? props.filters.division : undefined,
          department: props.filters.department !== "ALL" ? props.filters.department : undefined,
          section: props.filters.section !== "ALL" ? props.filters.section : undefined,
          biz: props.filters.biz !== "ALL" ? props.filters.biz : undefined,
          process: props.filters.process !== "ALL" ? props.filters.process : undefined,
        },
      }),
      axios.get("http://localhost:5000/api/EICCControl", {
        params: {
          division: props.filters.division !== "ALL" ? props.filters.division : undefined,
          department: props.filters.department !== "ALL" ? props.filters.department : undefined,
          section: props.filters.section !== "ALL" ? props.filters.section : undefined,
          biz: props.filters.biz !== "ALL" ? props.filters.biz : undefined,
          process: props.filters.process !== "ALL" ? props.filters.process : undefined,
        },
      }),
      // โหลดงานที่ยัง Active เพื่อ disable ปุ่ม
      axios.get("http://localhost:5000/api/Assignment", {
        params: { status: "Active" },
      }),
    ]);

    rawData.value = req.data;
    skills.value = skillRes.data;
    worktime.value = worktimeRes.data;
    activeAssignments.value = activeRes.data ?? [];

    await nextTick();
    drawChart();
  } catch (err) {
    console.error("Error fetching data:", err);
  }
};

// reload เมื่อ filter เปลี่ยน
watch(
  () => props.filters,
  async () => {
    await fetchData();
    resetSkillFilter();
  },
  { deep: true }
);

onMounted(async () => {
  await fetchData();
});
</script>

<style scoped>
.skill-icon { width: 30px; height: 30px; }

.refresh-skill-btn {
  display: flex; align-items: center; gap: 8px;
  background-color: #007bff; color: white; border: none; border-radius: 25px;
  padding: 6px 14px; margin-bottom: 10px; cursor: pointer; font-weight: bold; font-size: 14px;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.2); transition: background-color 0.3s ease, transform 0.3s ease;
}
.refresh-skill-btn:hover { background-color: #0056b3; transform: scale(1.05); }
.refresh-skill-btn .icon { width: 18px; height: 18px; filter: invert(1); }

button { background-color: transparent; border: none; cursor: pointer; transition: transform 0.2s ease, box-shadow 0.2s ease; padding: 5px; border-radius: 50%; }
button:hover { transform: scale(1.05); box-shadow: 0 0 5px rgba(0, 123, 255, 0.3); }

#required-bar-chart { width: 100%; height: 100%; margin-bottom: 30px; }

.employee-recommendations table { width: 100%; border-collapse: collapse; }
.employee-recommendations th, .employee-recommendations td { padding: 10px; text-align: center; border: 1px solid #ccc; }
.employee-recommendations th { background: #eee; }
.employee-recommendations td:last-child { display: flex; justify-content: center; align-items: center; height: 100%; }

.select-btn, .remove-btn {
  display: inline-flex; align-items: center; justify-content: center;
  width: 36px; height: 36px; border-radius: 50%;
}
.select-btn:hover { transform: translateY(-1px); background: green; }
.remove-btn:hover { transform: translateY(-1px); background: red; }
.select-btn:active, .remove-btn:active { transform: translateY(0); }
.select-icon, .remove-icon { width: 18px; height: 18px; }

.skill-badge {
  display: inline-flex; align-items: center; gap: 6px;
  background: #f0f8ff; border: 1px solid #007bff; border-radius: 20px;
  padding: 4px 10px; font-size: 13px; color: #007bff; font-weight: bold;
}

.modal {
  position: fixed; inset: 0; background: rgba(0,0,0,.7);
  display: flex; justify-content: center; align-items: center; z-index: 1000;
}
.modal-content {
  background: white; padding: 30px; border-radius: 10px;
  box-shadow: 0 0 15px rgba(0,0,0,.2); max-width: 500px; width: 100%;
  animation: fadeIn .3s ease-out;
}
.input-field { width: 100%; padding: 10px; border: 1px solid #ccc; border-radius: 4px; font-size: 14px; margin: 10px 0; }

button[disabled] { opacity: .5; cursor: not-allowed; box-shadow: none; }
.select-btn:hover button[disabled] { background: transparent; }

@keyframes fadeIn { 0%{opacity:0; transform:scale(.9)} 100%{opacity:1; transform:scale(1)} }
</style>
