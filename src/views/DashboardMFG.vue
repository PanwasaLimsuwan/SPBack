<template>
  <div class="DashboardMFG">
    <header class="header">
      <img src="logo.png" alt="Sony Logo" class="logo" />

      <!-- <div class="filters">
  <select v-model="selectedDivision">
    <option value="">Select Division</option>
    <option v-for="division in divisions" :key="division" :value="division">{{ division }}</option>
  </select>
  <select v-model="selectedDepartment">
    <option value="">Select Department</option>
    <option v-for="department in departments" :key="department" :value="department">{{ department }}</option>
  </select>
  <select v-model="selectedBiz">
    <option value="">Select Biz</option>
    <option v-for="biz in bizOptions" :key="biz" :value="biz">{{ biz }}</option>
  </select>
  <select v-model="selectedProcess">
    <option value="">Select Process</option>
    <option v-for="process in processes" :key="process" :value="process">{{ process }}</option>
  </select>
</div> -->


      <div class="filters">
        <select v-model="selectedDivision" @change="applyFilters">
          <option value="ALL">Division : ALL</option>
          <option value="ISM">ISM</option>
          <option value="DDM">DDM</option>
          <option value="LDM">LDM</option>
        </select>
        <select v-model="selectedDepartment" @change="applyFilters">
          <option value="ALL">Department : ALL</option>
          <option value="MF1">MF1</option>
          <option value="MF2">MF2</option>
        </select>
        <select v-model="selectedSection" @change="applyFilters">
          <option value="ALL">Section : ALL</option>
          <option value="ASSY Section">ASSY Section</option>
          <option value="TEST Section">TEST Section</option>
        </select>
        <select v-model="selectedBiz" @change="applyFilters">
          <option value="ALL">Biz : ALL</option>
          <option value="IS">IS</option>
          <option value="HTPS">HTPS</option>
          <option value="MOLED">MOLED</option>
        </select>
        <select v-model="selectedProcess" @change="applyFilters">
          <option value="ALL">Process : ALL</option>
          <option value="ASSY">ASSY</option>
          <option value="MOKU">MOKU</option>
          <option value="CSAT">CSAT</option>
          <option value="JUNB">JUNB</option>
        </select>
        <input
          type="text"
          placeholder="Search"
          v-model="searchQuery"
          @input="applyFilters"
        />
      </div>
    </header>

    <!-- สถิติ -->
    <section class="stats">
      <div class="stat-card" v-for="(card, index) in stats" :key="index">
        <div class="icon-container">
          <!-- ใช้ dot แทน icon -->
          <span
            v-if="card.dotColor"
            :style="{ backgroundColor: card.dotColor }"
            class="status-dot"
          ></span>
          <img v-else-if="card.icon" :src="card.icon" alt="Icon" class="icon" />
        </div>
        <div class="content">
          <!-- เปลี่ยนสีของตัวเลขเฉพาะช่องที่ label คือ 'ต้องการพนักงาน' -->
          <h3
            :style="{
              color: card.label === 'ต้องการพนักงาน' ? '#ff0000' : '#000',
            }"
          >
            {{ card.value }}
          </h3>
          <p>{{ card.label }}</p>
          <p v-if="card.subLabel" class="sub-label">{{ card.subLabel }}</p>
        </div>
      </div>
    </section>

    <!-- โซน Pie Chart และ Employee Table -->
    <section class="charts">
      <div class="chart">
        <button @click="resetFilters">
          <img src="refresh.png" alt="Refresh" class="refresh-icon" />
        </button>
        <!-- <PieChart :data="pieChartData" @filter="filterEmployeesByStatus" /> -->
        <!-- <PieChart
          :data="getPieChartData(filteredEmployees)"
          @filter="filterEmployeesByStatus"
        /> -->
        <PieChart
          :data="getPieChartData(filteredHeadcountEmployees)"
          @filter-headcount="filterEmployeesByStatus"
        />
      </div>
      <div class="table">
        <!-- <EmployeeTable
          :employees="
            filteredEmployees.length > 0 ? filteredEmployees : sortedEmployees
          "
        /> -->

        <!-- <EmployeeTable
  :employees="filteredEmployees.length > 0 ? filteredEmployees : employees"
  @selectEmployee="selectEmployee"
/> -->

        <EmployeeTable
          :employees="
            filteredHeadcountEmployees.length > 0
              ? filteredHeadcountEmployees
              : employees
          "
          @selectEmployee="selectEmployee"
        />
        <!-- <EmployeeTable :employees="sortedFilteredEmployees" /> -->
      </div>
    </section>

    <!-- โซน Bar Chart และ EmployeeRecommendations -->
    <section class="charts">
      <div class="chart">
        <!-- <BarChart :data="barChartData" /> -->
        <BarChart :data="barChartData" @filterSkill="filterEmployeesBySkill1" />
      </div>
      <div class="table">
        <EmployeeRecommendations
          :employees="recommendedEmployees"
          :skills="skills"
          @selectEmployee="selectEmployee"
        />
      </div>
    </section>

    <!-- โซน EmployeeSkillSection -->
    <section class="charts">
      <div class="chart">
        <button @click="resetFilters">
          <img src="refresh.png" alt="Refresh" class="refresh-icon" />
        </button>

        <fullySkilledPieChart
          :data="getFullySkilledData(filteredFullySkilledEmployees)"
          @filter-skills="filterEmployeesBySkill"
        />
      </div>

      <!-- ตารางพนักงานไม่ถูกฟิลเตอร์ -->
      <!-- <EmployeeSkillTable
          :employees="employees"
          :skills="skills"
          @selectEmployee="selectEmployee"
        /> -->
      <!-- <div class="table">
        <EmployeeSkillTable
          :employees="filteredEmployees"
          :skills="skills"
          @selectEmployee="selectEmployee"
        />
      </div> -->
      <div class="table">
        <!-- <EmployeeSkillTable
          :employees="filteredFullySkilled"
          :skills="skills"
          @selectEmployee="selectEmployee"
        /> -->
        <EmployeeSkillTable
          :employees="
            filteredFullySkilledEmployees.length > 0
              ? filteredFullySkilledEmployees
              : employees
          "
          :skills="skills"
          @selectEmployee="selectEmployee"
        />
      </div>
      <div class="table" ref="skillSection">
        <!-- แสดงข้อมูลพนักงานที่เลือก -->
        <EmployeeSkillSection
          v-if="selectedEmployee"
          :employee="selectedEmployee"
          :skills="skills"
        />
      </div>
    </section>

    <!-- โซน Pie Chart Skill Level และ EmployeeSkillTable -->
    <!-- <section class="charts">
      <div class="chart">
        <button @click="resetFilters">
          <img src="refresh.png" alt="Refresh" class="refresh-icon" />
        </button>
        <SkillLevelPieChart
          :data="skillDistribution"
          @filter="filterEmployeesBySkillLevel"
        />
      </div>
      <div class="table">
        <EmployeeSkillTable
          :employees="filteredEmployees"
          :skills="skills"
          @selectEmployee="selectEmployee"
        />
      </div>
    </section> -->

    <!-- โซน WorkedTimeChart และ MonthlyWorkedTimeOverload -->
    <section class="charts">
      <div class="chart">
        <WorkedTimeChart :data="employees" />
      </div>
      <div class="chart">
        <WeeklyOvertime :data="employees" />
      </div>
    </section>

    <!-- โซน WeeklyAbsentTrend และ ResourceAllocationSummary -->
    <section class="charts">
      <div class="chart">
        <MonthlyWorkedTimeOverload :data="employees" />
      </div>
      <div class="chart">
        <WeeklyAbsentTrend :data="employees" />
      </div>
      <!-- <div class="chart">
        <ResourceAllocationSummary :data="employees" />
      </div> -->
    </section>

    <!-- โซน ShiftRequireSupport และ ShiftcodeRequireSupport -->
    <!-- <section class="charts">
      <div class="chart">
        <ShiftcodeRequireSupport :data="shiftcodeData" />
        <ShiftcodeRequireSupport
          :data="shiftcodeData"
          @filterShift="updateShiftFilter"
        />
      </div>
      <div class="chart">
        <ShiftRequireSupport :data="shiftData" />
        <ShiftRequireSupport
        :data="shiftData"
        :selectedShiftcode="selectedShiftcode"
      />
        กราฟ ShiftRequireSupport ที่ใช้ filteredShiftData
        <ShiftRequireSupport :data="filteredShiftData" />
      </div>
    </section> -->

    <section class="charts">
      <!-- <div class="chart">
        <WeeklyOvertime :data="employees" />
      </div> -->
      <div class="chart">
        <HeadcountPlan :data="headcountData" />
      </div>
    </section>
  </div>
</template>

<script>
import PieChart from "./../components/PieChart.vue";
import BarChart from "./../components/BarChart.vue";
import EmployeeTable from "./../components/EmployeeTable.vue";
import EmployeeRecommendations from "./../components/EmployeeRecommendations.vue";
import EmployeeSkillTable from "./../components/EmployeeSkillTable.vue";
// import SkillLevelPieChart from "./../components/SkillLevelPieChart.vue";
import EmployeeSkillSection from "./../components/EmployeeSkillSection.vue";
import fullySkilledPieChart from "./../components/fullySkilledPieChart.vue";
import WorkedTimeChart from "./../components/WorkedTimeChart.vue";
import WeeklyAbsentTrend from "./../components/WeeklyAbsentTrend.vue";
import MonthlyWorkedTimeOverload from "./../components/MonthlyWorkedTimeOverload.vue";
// import ResourceAllocationSummary from "./../components/ResourceAllocationSummary.vue";
// import ShiftRequireSupport from "./../components/ShiftRequireSupport.vue";
// import ShiftcodeRequireSupport from "./../components/ShiftcodeRequireSupport.vue";
import WeeklyOvertime from "./../components/WeeklyOvertime.vue";
import HeadcountPlan from "./../components/HeadcountPlan.vue";

export default {
  components: {
    PieChart,
    BarChart,
    EmployeeTable,
    EmployeeRecommendations,
    EmployeeSkillTable,
    // SkillLevelPieChart,
    EmployeeSkillSection,
    fullySkilledPieChart,
    WorkedTimeChart,
    WeeklyAbsentTrend,
    MonthlyWorkedTimeOverload,
    // ResourceAllocationSummary,
    // ShiftRequireSupport,
    // ShiftcodeRequireSupport,
    WeeklyOvertime,
    HeadcountPlan,
  },
  data() {
    return {
      electedDivision: "ALL",
      selectedDepartment: "ALL",
      selectedSection: "ALL",
      selectedBiz: "ALL",
      selectedProcess: "ALL",
      searchQuery: "",
      stats: [
        {
          value: "7:15",
          label: "SHIFT : DAY",
          subLabel: "3/10/2024",
          icon: "clock.png",
        },
        { value: 150, label: "พนักงานทั้งหมด", subLabel: "คน" },
        {
          value: 85,
          label: "In Cleanroom",
          subLabel: "คน",
          dotColor: "#00cc66",
        },
        {
          value: 33,
          label: "Out Cleanroom",
          subLabel: "คน",
          dotColor: "#ffcc00",
        },
        { value: 0, label: "ขาดงาน", subLabel: "คน", dotColor: "#ff6666" },
        { value: 11, label: "ต้องการพนักงาน", subLabel: "คน" },
      ],

      pieChartData: {
        values: [5, 33, 85],
        labels: ["ขาดงาน", "Out Cleanroom", "In Cleanroom"],
        type: "pie",
      },
      employees: [
        {
          EmpID: "202035",
          Firstname: "Patchara",
          Lastname: "Khuneept",
          Datetime: "2023-09-01 07:15:00",
          GateNo: "D01",
          WorkGroup: "FTE",
          Division: "ISM",
          Department: "MF2",
          Section: "ASSY Section",
          Biz: "IS",
          Process: "ASSY",
          CourseGroup: "Injection",
          Status: "In Cleanroom",
          Material: 3,
          Operation: 3,
          "Machine:SAB#1": 3,
          "Machine:SAB#2": 3,
          "Machine:SAB#3": 3,
          Inspection: 3,
          WorkTime: 20, // Work Time
          absentDates: ["2024-03-07"], // ไม่เคยขาดงาน
        },
        {
          EmpID: "202034",
          Firstname: "Tanwaruth",
          Lastname: "Papapai",
          Datetime: "2023-09-01 07:12:00",
          GateNo: "D02",
          WorkGroup: "FTE",
          Division: "ISM",
          Department: "MF1",
          Section: "TEST Section",
          Biz: "HTPS",
          Process: "CSAT",
          CourseGroup: "OJT",
          Status: "Out Cleanroom",
          Material: 3,
          Operation: 2,
          "Machine:SAB#1": 3,
          "Machine:SAB#2": 1,
          "Machine:SAB#3": 3,
          Inspection: 3,
          WorkTime: 55, // Work Time
          absentDates: ["2024-03-01", "2024-03-07"], // เคยขาดงาน 2 วัน
        },
        {
          EmpID: "202033",
          Firstname: "Sudaporn",
          Lastname: "Sena",
          Datetime: "-",
          GateNo: "-",
          WorkGroup: "FTE",
          Division: "ISM",
          Department: "MF2",
          Section: "TEST Section",
          Biz: "MOLED",
          Process: "JUNB",
          CourseGroup: "OJT",
          Status: "ขาดงาน",
          Material: 3,
          Operation: 3,
          "Machine:SAB#1": 0,
          "Machine:SAB#2": 2,
          "Machine:SAB#3": 1,
          Inspection: 1,
          WorkTime: 70, // Work Time
          absentDates: ["2024-03-04", "2024-03-05", "2024-03-06"], // ขาดงาน 3 วัน
        },
      ],
      skills: [
        "Material",
        "Operation",
        "Machine:SAB#1",
        "Machine:SAB#2",
        "Machine:SAB#3",
        "Inspection",
      ],
      skillDistribution: {
        values: [10, 15, 25, 45],
        labels: ["Not Trained", "Basic", "Medium", "Expert"],
      },
      fullySkilled: {
        values: [80, 20],
        labels: ["Fully Skilled", "Need Training"],
      },
      // workedTimeData: [
      //   { firstname: "Patchara", lastname: "Khuneept", hours: 70 },
      //   { firstname: "Tanwaruth", lastname: "Papapai", hours: 55 },
      //   { firstname: "Sudaporn", lastname: "Sena", hours: 50 },
      // ],
      monthlyData: {
        months: ["ASSY", "MOKU", "CSAT", "JUNB"],
        hours: [10, 15, 5, 7],
      },
      weeklyData: {
        days: ["MON", "TUE", "WED", "THR", "FRI"],
        absences: [20, 10, 17, 14, 12],
      },
      resourceData: {
        processes: ["ASSY", "MOKU", "CSAT", "JUNB"],
        resources: [10, 5, 10, 7],
      },
      shiftcodeData: {
        processes: ["ASSY", "MOKU", "CSAT", "JUNB"],
        shiftA: [5, 10, 15, 5],
        shiftB: [8, 6, 5, 10],
        shiftC: [8, 10, 5, 8],
      },
      shiftData: {
        A: {
          processes: ["ASSY", "MOKU", "CSAT", "JUNB"],
          dayShift: [3, 7, 10, 5],
          nightShift: [2, 3, 5, 4],
        },
        B: {
          processes: ["ASSY", "MOKU", "CSAT", "JUNB"],
          dayShift: [4, 5, 6, 7],
          nightShift: [2, 3, 5, 4],
        },
        C: {
          processes: ["ASSY", "MOKU", "CSAT", "JUNB"],
          dayShift: [3, 7, 10, 5],
          nightShift: [2, 3, 5, 4],
        },
      },
      overtimeData: {
        processes: ["ASSY", "MOKU", "CSAT", "JUNB"],
        overtime: [10, 30, 20, 25],
      },
      headcountData: {
        processes: ["ASSY", "MOKU", "CSAT", "JUNB"],
        headcount: [175, 225, 150, 195],
        plan: [200, 225, 150, 200],
      },
      // ข้อมูลต้นฉบับ
      divisionData: {
        ALL: {
          labels: ["ISM", "DDM", "LDM"],
          values: [40, 30, 30],
        },
        ISM: {
          labels: ["MF1", "MF2"],
          values: [10, 20],
        },
        DDM: {
          labels: ["MF1", "MF2"],
          values: [25, 30],
        },
        LDM: {
          labels: ["MF1", "MF2"],
          values: [20, 35],
        },
      },

      bizData: {
        ALL: {
          labels: ["IS", "IS-BMS", "HTPS", "MOLED", "SMOLED"],
          values: [50, 30, 20, 25, 15],
        },
        ISBMS: {
          labels: ["ASSY", "MOKU", "CSAT", "JUNB"],
          values: [20, 15, 10, 5],
        },
        HTPS: {
          labels: ["ASSY", "MOKU", "CSAT", "JUNB"],
          values: [10, 23, 38, 12],
        },
        MOLED: {
          labels: ["ASSY", "MOKU", "CSAT", "JUNB"],
          values: [5, 10, 15, 20],
        },
        SMOLED: {
          labels: ["ASSY", "MOKU", "CSAT", "JUNB"],
          values: [8, 9, 8, 7],
        },
      },
      recommendedEmployees: [], // เพิ่มส่วนนี้
      filteredEmployees: [],
      filteredSkillEmployees: [],
      filteredFullySkilled: [],
      selectedEmployee: null, // พนักงานที่ถูกเลือก
      // selectedSkill: null, // สกิลที่ถูกเลือก
      // selectedShiftcode: "A",
      filteredShiftData: {},

      filteredEmployeesDivision: [],
      filteredEmployeesBiz: [],
      filteredEmployeesWorkGroup: [],
      // selectedSkill: null, // สกิลที่ถูกเลือก
      selectedDivision: "ALL",
      filteredDivisionData: {},
      filteredHeadcountEmployees: [], // สำหรับกราฟ Headcount
      filteredFullySkilledEmployees: [], // สำหรับกราฟ Fully Skilled
    };
  },
  methods: {
    applyFilters() {
      // กรองข้อมูลสำหรับกราฟ Headcount
      this.filteredHeadcountEmployees = this.filterEmployees(this.employees, {
        division: this.selectedDivision,
        department: this.selectedDepartment,
        section: this.selectedSection,
        biz: this.selectedBiz,
        process: this.selectedProcess,
        search: this.searchQuery,
      });

      this.filteredHeadcountEmployees = this.sortEmployeesByStatus(
        this.filteredHeadcountEmployees
      );

      // กรองข้อมูลสำหรับกราฟ Fully Skilled
      this.filteredFullySkilledEmployees = this.filterEmployees(
        this.employees,
        {
          division: this.selectedDivision,
          department: this.selectedDepartment,
          section: this.selectedSection,
          biz: this.selectedBiz,
          process: this.selectedProcess,
          search: this.searchQuery,
        }
      );

      // กรองข้อมูลสำหรับทั้งสองกราฟนี้ให้ถูกต้อง
      if (this.filteredHeadcountEmployees.length === 0) {
        this.filteredHeadcountEmployees = [...this.employees];
      }

      if (this.filteredFullySkilledEmployees.length === 0) {
        this.filteredFullySkilledEmployees = [...this.employees];
      }

      // กรองข้อมูลในตารางตาม searchQuery
      this.filteredEmployees = this.filterEmployees(this.employees, {
        division: this.selectedDivision,
        department: this.selectedDepartment,
        section: this.selectedSection,
        biz: this.selectedBiz,
        process: this.selectedProcess,
        search: this.searchQuery,
      });

      // คำนวณจำนวนพนักงานทั้งหมด, In Cleanroom, Out Cleanroom
      const totalEmployees = this.filteredHeadcountEmployees.length;
      const inCleanroom = this.filteredHeadcountEmployees.filter(
        (employee) => employee.Status === "In Cleanroom"
      ).length;
      const outCleanroom = this.filteredHeadcountEmployees.filter(
        (employee) => employee.Status === "Out Cleanroom"
      ).length;
      const missing = this.filteredHeadcountEmployees.filter(
        (employee) => employee.Status === "ขาดงาน"
      ).length;

      // อัพเดตค่าต่างๆ ใน stat
      this.stats = [
        { value: totalEmployees, label: "พนักงานทั้งหมด", subLabel: "คน" },
        {
          value: inCleanroom,
          label: "In Cleanroom",
          subLabel: "คน",
          dotColor: "#00cc66",
        },
        {
          value: outCleanroom,
          label: "Out Cleanroom",
          subLabel: "คน",
          dotColor: "#ffcc00",
        },
        {
          value: missing,
          label: "ขาดงาน",
          subLabel: "คน",
          dotColor: "#ff6666",
        },
        { value: 11, label: "ต้องการพนักงาน", subLabel: "คน" }, // คุณสามารถปรับค่าตัวนี้ตามเงื่อนไข
      ];

      // อัพเดตเวลาและวันที่จริง
      const currentTime = new Date();
      this.stats[0].value = currentTime.toLocaleTimeString(); // เวลาปัจจุบัน
      this.stats[0].subLabel = currentTime.toLocaleDateString("th-TH"); // วันที่ปัจจุบัน

      // กำหนดค่า shift โดยใช้เวลา
      this.updateShift(currentTime);
    },

    filterEmployees(employees, filters) {
      return employees.filter((employee) => {
        return Object.keys(filters).every((key) => {
          const filterValue = filters[key];
          if (filterValue === "ALL" || filterValue === "") return true;
          if (key === "search") {
            return `${employee.Firstname} ${employee.Lastname}`
              .toLowerCase()
              .includes(filterValue.toLowerCase());
          }
          return employee[key] === filterValue;
        });
      });
    },

    getPieChartData(employees) {
      const missing = employees.filter(
        (employee) => employee.Status === "ขาดงาน"
      ).length;
      const outCleanroom = employees.filter(
        (employee) => employee.Status === "Out Cleanroom"
      ).length;
      const inCleanroom = employees.filter(
        (employee) => employee.Status === "In Cleanroom"
      ).length;
      return {
        values: [missing, outCleanroom, inCleanroom],
        labels: ["ขาดงาน", "Out Cleanroom", "In Cleanroom"],
        type: "pie",
      };
    },
    getFullySkilledData(employees) {
      let fullySkilledCount = 0;
      let needTrainingCount = 0;

      employees.forEach((employee) => {
        const lowSkillCount = this.skills.reduce((count, skill) => {
          return employee[skill] === 0 || employee[skill] === 1
            ? count + 1
            : count;
        }, 0);

        if (lowSkillCount > 2) {
          needTrainingCount++;
        } else {
          fullySkilledCount++;
        }
      });

      return {
        values: [fullySkilledCount, needTrainingCount],
        labels: ["Fully Skilled", "Need Training"],
        type: "pie",
        marker: { colors: ["green", "red"] },
      };
    },

    // ฟังก์ชันอัพเดตค่า Shift
    updateShift(currentTime) {
      // ใช้เวลาปัจจุบันเพื่อกำหนด Shift
      const hours = currentTime.getHours();

      if (hours >= 7 && hours < 19) {
        this.stats[0].label = "SHIFT : DAY"; // ถ้าเวลาเป็น 7:00 - 18:59
      } else {
        this.stats[0].label = "SHIFT : NIGHT"; // ถ้าเวลาเป็น 19:00 - 6:59
      }
    },

    getSkillLevelDistributionData(employees) {
      const skillCounts = {
        "Not Trained": 0,
        Basic: 0,
        Medium: 0,
        Expert: 0,
      };

      // นับจำนวนพนักงานในแต่ละระดับทักษะ
      employees.forEach((employee) => {
        this.skills.forEach((skill) => {
          const skillLevel = employee[skill];
          if (skillLevel !== undefined) {
            if (skillLevel === 0) skillCounts["Not Trained"]++;
            if (skillLevel === 1) skillCounts["Basic"]++;
            if (skillLevel === 2) skillCounts["Medium"]++;
            if (skillLevel === 3) skillCounts["Expert"]++;
          }
        });
      });

      return {
        values: [
          skillCounts["Not Trained"],
          skillCounts["Basic"],
          skillCounts["Medium"],
          skillCounts["Expert"],
        ],
        labels: ["Not Trained", "Basic", "Medium", "Expert"],
      };
    },
    sortEmployeesByStatus(employees) {
      // ระบุลำดับการจัดเรียงสถานะ
      const statusOrder = ["ขาดงาน", "Out Cleanroom", "In Cleanroom"];

      // ใช้ sort เพื่อจัดเรียงตามลำดับที่กำหนด
      return employees.slice().sort((a, b) => {
        const statusA = statusOrder.indexOf(a.Status);
        const statusB = statusOrder.indexOf(b.Status);
        return statusA - statusB; // เรียงลำดับตาม index ใน statusOrder
      });
    },
    filterEmployeesByStatus(status) {
      console.log("Status Selected:", status); // ดูค่าที่ส่งมาจาก PieChart

      // กรองพนักงานที่มี Status ตรงกับที่เลือก
      this.filteredEmployees = this.employees.filter(
        (employee) => employee.Status === status
      );

      // กรองข้อมูลเฉพาะในตารางที่เกี่ยวข้อง
      // this.filteredSkillEmployees = [...this.filteredEmployees];  // สำหรับตารางที่เกี่ยวข้องกับ Skill
      // this.filteredFullySkilled = [...this.filteredEmployees]; // สำหรับ Fully Skilled Table

      console.log("Filtered Employees for PieChart:", this.filteredEmployees); // ดูพนักงานที่กรองได้
    },

    filterEmployeesBySkillLevel(selectedLevel) {
      const levelMapping = {
        "Not Trained": 0,
        Basic: 1,
        Medium: 2,
        Expert: 3,
      };

      const levelValue = levelMapping[selectedLevel];

      if (levelValue === undefined) {
        console.error("Invalid skill level selected.");
        return;
      }

      this.filteredSkillEmployees = this.employees
        .map((employee) => {
          const filteredSkills = this.skills.reduce((result, skill) => {
            result[skill] =
              employee[skill] === levelValue ? employee[skill] : null;
            return result;
          }, {});
          return { ...employee, ...filteredSkills };
        })
        .filter((employee) =>
          this.skills.some((skill) => employee[skill] === levelValue)
        );
    },
    filterEmployeesBySkill1(skill) {
      console.log("Filtering employees by skill:", skill); // Debug

      // Filter employees based on selected filters
      const filteredBySkill = this.filteredEmployees.filter(
        (employee) => employee[skill] >= 2
      ); // Employees with skillLevel >= 2

      filteredBySkill.sort((a, b) => {
        if (b[skill] === a[skill]) {
          const workTimeA = parseInt(a.WorkTime.split("/")[0], 10);
          const workTimeB = parseInt(b.WorkTime.split("/")[0], 10);
          return workTimeA - workTimeB;
        }
        return b[skill] - a[skill];
      });

      // Limit to top 5 employees
      this.recommendedEmployees = filteredBySkill.slice(0, 5);
      // this.recommendedEmployees = this.filteredEmployees;
    },

    filterEmployeesBySkill(label) {
      console.log("🔍 Fully Skilled Pie Chart Clicked - Label:", label);

      if (label === "Fully Skilled") {
        this.filteredSkillEmployees = this.filteredEmployees.filter(
          (employee) => this.skills.every((skill) => employee[skill] === 3)
        );
      } else if (label === "Need Training") {
        this.filteredSkillEmployees = this.filteredEmployees.filter(
          (employee) => {
            const lowSkillsCount = this.skills.reduce((count, skill) => {
              return employee[skill] === 0 || employee[skill] === 1
                ? count + 1
                : count;
            }, 0);
            return lowSkillsCount > 2;
          }
        );
      }

      console.log(
        "🟢 Updated filteredSkillEmployees:",
        this.filteredSkillEmployees
      );
    },

    updateDivisionFilter() {
      if (this.selectedDivision === "ALL") {
        this.filteredDivisionData = { ...this.divisionData }; // แสดงข้อมูลทั้งหมด
      } else {
        const index = this.divisionData.labels.indexOf(this.selectedDivision);
        this.filteredDivisionData = {
          labels: [this.divisionData.labels[index]],
          values: [this.divisionData.values[index]],
        };
      }
    },
    updateShiftFilter(shiftcode) {
      this.selectedShiftcode = shiftcode;
      this.filterShiftData();
    },
    filterShiftData() {
      this.filteredShiftData = this.shiftData[this.selectedShiftcode];
    },
    onBarClick(eventData) {
      const selectedSkill = eventData.points[0].x; // Skill ที่เลือกจากแกน X ของกราฟ
      this.$emit("filterSkill", selectedSkill); // ส่ง Skill กลับไปยัง App.vue
    },
    // resetFilters() {
    //   this.filteredSkillEmployees = this.employees; // รีเซ็ตข้อมูลกลับไปที่ทั้งหมด
    //   this.filteredFullySkilled = this.employees;
    //   this.filteredEmployees = [];
    //   this.recommendedEmployees = [];
    //   // this.selectedEmployee = null;
    //   console.log("Filters reset. Showing all employees.");
    // },
    resetFilters() {
      this.selectedDivision = "ALL";
      this.selectedDepartment = "ALL";
      this.selectedSection = "ALL";
      this.selectedBiz = "ALL";
      this.selectedProcess = "ALL";
      this.searchQuery = "";
      this.filteredEmployees = [...this.employees];
      this.filteredSkillEmployees = [...this.employees];
    },
    selectEmployee(employee) {
      this.selectedEmployee = employee; // ตั้งค่าพนักงานที่ถูกเลือก

      // ดึงหน้าจอไปยังส่วนที่แสดงผลข้อมูล
      this.$nextTick(() => {
        const section = this.$refs.skillSection;
        if (section) {
          section.scrollIntoView({ behavior: "smooth", block: "start" });
        }
      });
    },

    filterEmployeesByDivision(division) {
      this.filteredEmployeesDivision = this.sortEmployeesByStatus(
        this.employees.filter((employee) => employee.Division === division)
      );
    },
    filterEmployeesByBiz(biz) {
      this.filteredEmployeesBiz = this.sortEmployeesByStatus(
        this.employees.filter((employee) => employee.Biz === biz)
      );
    },
    filterEmployeesByWorkGroup(workgroup) {
      this.filteredEmployeesWorkGroup = this.sortEmployeesByStatus(
        this.employees.filter((employee) => employee.WorkGroup === workgroup)
      );
    },
  },
  computed: {
    // sortedEmployees() {
    //   return this.sortEmployeesByStatus([...this.employees]);
    // },
  },

  mounted() {
    // this.filteredEmployees = []; // เริ่มต้นให้ว่างเพื่อเรียกใช้ sortedEmployees
    // // this.filteredEmployees = this.employees; // เริ่มต้นแสดงพนักงานทั้งหมด
    // this.filteredSkillEmployees = this.employees;
    // this.filteredFullySkilled = this.employees;
    // this.filterShiftData();
    this.filteredDivisionData = { ...this.divisionData }; // ตั้งค่าข้อมูลเริ่มต้น

    this.filteredEmployeesDivision = this.sortEmployeesByStatus(this.employees);
    this.filteredEmployeesBiz = this.sortEmployeesByStatus(this.employees);
    this.filteredEmployeesWorkGroup = this.sortEmployeesByStatus(
      this.employees
    );
    this.filteredSkillEmployees = this.employees;
    this.filteredFullySkilled = this.employees;
    this.filteredEmployees = this.sortEmployeesByStatus(this.employees);
    this.filterShiftData();

    this.applyFilters();
  },

  watch: {
  selectedDivision() {
    this.drawChart();
  },
  selectedDepartment() {
    this.drawChart();
  },
  selectedBiz() {
    this.drawChart();
  },
  selectedProcess() {
    this.drawChart();
  },
  data: {
    deep: true,
    handler() {
      this.drawChart();
    },
  },
},

};
</script>

<style>
.dashboard {
  font-family: Arial, sans-serif;
  padding: 20px;
  background-color: #f9f9f9;
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  background-color: #fff;
  padding: 10px 20px;
  box-shadow: 0px 2px 4px rgba(0, 0, 0, 0.1);
}

.logo {
  width: 150px;
}

.filters select,
.filters input {
  margin-left: 10px;
  padding: 5px;
  border: 1px solid #ccc;
  border-radius: 4px;
}

.stats {
  display: flex;
  justify-content: space-between;
  margin-top: 20px;
  gap: 20px;
}

.stat-card {
  flex: 1;
  text-align: center;
  padding: 10px;
  background: #fff;
  border-radius: 8px;
  box-shadow: 0px 2px 4px rgba(0, 0, 0, 0.1);
}

.charts {
  display: flex;
  justify-content: space-between;
  margin-top: 20px;
  gap: 20px;
}

.chart {
  flex: 1;
  padding: 20px;
  background: #fff;
  border-radius: 8px;
  box-shadow: 0px 2px 4px rgba(0, 0, 0, 0.1);
}

.table {
  flex: 1;
  padding: 20px;
  background: #fff;
  border-radius: 8px;
  box-shadow: 0px 2px 4px rgba(0, 0, 0, 0.1);
}

button {
  /* background-color: #007bff; */
  color: white;
  float: right; /* ปุ่มอยู่ชิดขวา */
  padding: 10px 20px;
  border: none;
  border-radius: 5px;
  cursor: pointer;
  font-size: 16px;
  display: flex;
  align-items: center;
  gap: 8px;
}

button:hover {
  background-color: #0056b3;
}

.refresh-icon {
  width: 20px;
  height: 20px;
}

.stats {
  display: flex;
  justify-content: space-around;
  gap: 20px;
  margin-top: 20px;
}

.stat-card {
  display: flex;
  align-items: center;
  background-color: #f9f9f9;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  padding: 15px;
  width: 180px;
  transition: transform 0.2s;
  justify-content: space-between;
}

/* .stat-card:hover {
  transform: translateY(-5px);
} */

.icon-container {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 40px;
  height: 40px;
  margin-left: 20px;
}

.icon {
  width: 48px;
  height: 48px;
}

/* สไตล์สำหรับจุดสี */
.status-dot {
  width: 22px;
  height: 22px;
  border-radius: 50%;
}

.content {
  text-align: right;
}

h3 {
  font-size: 20px;
  font-weight: bold;
  margin: 0;
}

h3.red {
  color: #ff0000;
}

p {
  font-size: 14px;
  margin: 4px 0 0;
}

.sub-label {
  font-size: 16px;
  color: #888;
}
</style>
