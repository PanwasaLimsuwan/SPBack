<template>
  <div id="overtime-chart"></div>
</template>

<script>
import Plotly from "plotly.js";

export default {
  name: "WeeklyOvertime",
  props: {
    data: {
      type: Array,
      required: true,
    },
  },
  mounted() {
    this.drawChart();
  },
  methods: {
    drawChart() {
      // แยกข้อมูลจาก employees ตาม Process
      const processData = this.aggregateDataByProcess(this.data);

      // สร้างข้อมูลกราฟตามสัปดาห์และ Process
      const chartData = this.createChartData(processData);

      const layout = {
        title: "Weekly Overtime (OT)",
        barmode: "group", // แสดงกราฟแบบกลุ่ม
        xaxis: { title: "Week", tickmode: "linear", dtick: 1 },
        yaxis: { title: "WorkTime (Hours)", range: [0, 100] },
        responsive: true,
        height: 400,
      };

      Plotly.newPlot("overtime-chart", chartData, layout);
    },

    // ฟังก์ชันเพื่อรวบรวมข้อมูล WorkTime ตาม Process
    aggregateDataByProcess(data) {
      const processData = {};

      data.forEach(employee => {
        const { Process, WorkTime, absentDates } = employee;
        const weeks = this.getWeeksFromDates(absentDates); // คำนวณสัปดาห์จากวันที่ขาดงาน

        if (!processData[Process]) {
          processData[Process] = {};
        }

        weeks.forEach(week => {
          if (!processData[Process][week]) {
            processData[Process][week] = 0;
          }
          processData[Process][week] += WorkTime; // คำนวณเวลาทำงานรวมตามสัปดาห์
        });
      });

      return processData;
    },

    // ฟังก์ชันที่จะดึงข้อมูลสัปดาห์จากวันที่ขาดงาน (สามารถปรับปรุงได้)
    getWeeksFromDates(absentDates) {
      return absentDates.map(date => {
        const dateObj = new Date(date);
        const week = this.getWeekNumber(dateObj); // แปลงวันที่เป็นหมายเลขสัปดาห์
        return week;
      });
    },

    // ฟังก์ชันเพื่อคำนวณหมายเลขสัปดาห์จากวันที่
    getWeekNumber(date) {
      const startDate = new Date(date.getFullYear(), 0, 1);
      const diff = date - startDate;
      const oneDay = 1000 * 60 * 60 * 24;
      return Math.ceil(diff / oneDay / 7);
    },

    // สร้างข้อมูลสำหรับกราฟจากข้อมูลที่ได้
    createChartData(processData) {
      const chartData = [];
      const weeks = this.getWeeksFromDates(this.data.flatMap(emp => emp.absentDates)); // ดึงสัปดาห์ทั้งหมด

      Object.keys(processData).forEach(process => {
        const overtimeData = weeks.map(week => {
          return processData[process][week] || 0; // ถ้าไม่มีข้อมูล ให้เป็น 0
        });

        chartData.push({
          x: weeks,
          y: overtimeData,
          type: "bar",
          name: process,
          marker: {
            color: this.getRandomColor(),
          },
        });
      });

      return chartData;
    },

    // ฟังก์ชันสุ่มสี
    getRandomColor() {
      const letters = '0123456789ABCDEF';
      let color = '#';
      for (let i = 0; i < 6; i++) {
        color += letters[Math.floor(Math.random() * 16)];
      }
      return color;
    },
  },
};
</script>

<style scoped>
#overtime-chart {
  width: 100%;
  height: 100%;
}
</style>
