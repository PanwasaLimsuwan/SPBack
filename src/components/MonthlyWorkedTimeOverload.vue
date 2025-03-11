<template>
  <div id="monthly-worked-time-overload"></div>
</template>

<script>
import Plotly from "plotly.js";

export default {
  name: "MonthlyWorkedTimeOverload",
  props: {
    data: {
      type: Array, // รับ employees ตรง ๆ ไม่ใช่ Object
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
  computed: {
    processedData() {
      // โครงสร้างของข้อมูล Process
      let processNames = ["ASSY", "MOKU", "CSAT", "JUNB"];
      let workedTimeOverload = [0, 0, 0, 0]; // เริ่มต้นที่ 0

      // คำนวณ WorkTime รวมของแต่ละ Process
      this.data.forEach(emp => {
        let index = processNames.indexOf(emp.Process);
        if (index !== -1) {
          workedTimeOverload[index] += emp.WorkTime || 0;
        }
      });

      return { processNames, workedTimeOverload };
    },
  },
  methods: {
    drawChart() {
      const { processNames, workedTimeOverload } = this.processedData;

      const chartData = [
        {
          x: processNames,
          y: workedTimeOverload,
          type: "bar",
          marker: {
            color: workedTimeOverload.map(hours => (hours > 60 ? "red" : "green")),
          },
        },
      ];

      const layout = {
        title: "Monthly Worked Time Overload",
        xaxis: {
          title: "Process",
        },
        yaxis: {
          title: "Worked Hours",
          range: [0, Math.max(...workedTimeOverload) + 10],
        },
        height: 400,
        responsive: true,
      };

      Plotly.newPlot("monthly-worked-time-overload", chartData, layout);
    },
  },
};
</script>

<style scoped>
#monthly-worked-time-overload {
  width: 100%;
  height: 100%;
}
</style>
