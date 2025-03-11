<template>
  <div id="weekly-absent-trend"></div>
</template>

<script>
import Plotly from "plotly.js";

export default {
  name: "WeeklyAbsentTrend",
  props: {
    data: {
      type: Array,
      required: true,
    },
  },
  mounted() {
    this.drawChart();
  },
  watch: {
    data: {
      deep: true,
      handler() {
        this.drawChart();
      },
    },
  },
  methods: {
    calculateProcessedData() {
      console.log("🔍 Data received:", this.data);

      if (!Array.isArray(this.data)) {
        console.error("🚨 Expected an array but received:", this.data);
        return { weeks: [], processes: {} };
      }

      let processes = {};
      let weeksSet = new Set();

      // ดึง process ที่ไม่ซ้ำจาก employee data
      let uniqueProcesses = [...new Set(this.data.map(emp => emp.Process))];
      uniqueProcesses.forEach(process => {
        processes[process] = {};
      });

      // ตรวจสอบการขาดงานของพนักงานแต่ละคน
      this.data.forEach(employee => {
        if (Array.isArray(employee.absentDates)) {
          employee.absentDates.forEach(absentDate => {
            let date = new Date(absentDate);
            if (isNaN(date.getTime())) {
              date = new Date(absentDate.replace(/-/g, "/")); // รองรับฟอร์แมตที่ผิด
            }

            if (!isNaN(date.getTime())) {
              let week = this.getWeekNumber(date);
              weeksSet.add(week);

              if (!processes[employee.Process][week]) {
                processes[employee.Process][week] = 0;
              }
              processes[employee.Process][week]++;
            }
          });
        }
      });

      let weeks = Array.from(weeksSet).sort((a, b) => a - b);
      console.log("📊 Processed data:", processes);

      return { weeks, processes };
    },

    drawChart() {
      const { weeks, processes } = this.calculateProcessedData();

      if (Object.keys(processes).length === 0 || weeks.length === 0) {
        console.warn("⚠️ No data available for plotting");
        return;
      }

      let traces = Object.keys(processes).map(process => {
        return {
          x: weeks,
          y: weeks.map(week => processes[process][week] || 0), // ถ้าไม่มีข้อมูลให้เป็น 0
          type: "bar",
          name: process,
        };
      });

      const maxAbsences = Math.max(...Object.values(processes).flat().map(weekData => Math.max(...Object.values(weekData)))) + 2;

      const layout = {
        title: "Weekly Absent Trend by Process",
        height: 400,
        barmode: "group",
        xaxis: { title: "Week Number", tickmode: "linear", dtick: 1 },
        yaxis: {
          title: "Absences",
          range: [0, maxAbsences > 0 ? maxAbsences : 5],
          dtick: 1,
        },
        legend: { orientation: "h", x: 0.5, xanchor: "center", y: 1.1 },
        responsive: true,
      };

      Plotly.newPlot("weekly-absent-trend", traces, layout);
    },

    // ฟังก์ชันคำนวณหมายเลขสัปดาห์ของปี
    getWeekNumber(date) {
      const startDate = new Date(date.getFullYear(), 0, 1);
      const diff = date - startDate;
      const oneDay = 1000 * 60 * 60 * 24;
      return Math.ceil(diff / oneDay / 7);
    },
  },
};
</script>

<style scoped>
#weekly-absent-trend {
  width: 100%;
  height: 100%;
}
</style>
