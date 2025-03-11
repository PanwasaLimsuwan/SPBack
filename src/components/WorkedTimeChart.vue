<template>
  <div id="worked-time-chart"></div>
</template>

<script>
import Plotly from "plotly.js";

export default {
  name: "WorkedTimeChart",
  props: {
    data: Array, // รับข้อมูลพนักงานเข้ามา
  },
  mounted() {
    this.drawChart();
  },
  methods: {
    // คำนวณเปอร์เซ็นต์การทำงานและสี
    calculateWorkedHours(workTime) {
      if (!workTime) {
        return { workedHours: 0, percentage: 0, color: "#00ff00" }; // ค่าเริ่มต้นเมื่อไม่มีข้อมูลที่ถูกต้อง
      }

      const totalWorkTime = 60; // 60 ชั่วโมงเป็น 100%
      let percentage = (workTime / totalWorkTime) * 100; // คำนวณเปอร์เซ็นต์
      let color = "#00ff00"; // สีเริ่มต้น (สีเขียว)

      // กำหนดสีตามเปอร์เซ็นต์ที่คำนวณ
      if (percentage >= 90) {
        color = "#ff0000"; // สีแดงสำหรับ 90% ขึ้นไป
      } else if (percentage >= 80) {
        color = "#ffcc00"; // สีเหลืองสำหรับ 80%-89%
      }

      return { workedHours: workTime, percentage, color };
    },

    drawChart() {
      // ตรวจสอบว่ามีข้อมูลหรือไม่
      if (!this.data || this.data.length === 0) {
        console.error("No data provided for WorkedTimeChart.");
        return;
      }

      const chartData = [
        {
          x: this.data.map(emp => {
            const { percentage } = this.calculateWorkedHours(emp.WorkTime);
            return percentage; // ใช้เปอร์เซ็นต์ในการแสดงในกราฟ
          }),
          y: this.data.map(emp => `${emp.Firstname} ${emp.Lastname}`), // ชื่อพนักงาน
          type: "bar",
          orientation: "h", // แกน Y เป็นชื่อพนักงาน
          marker: {
            color: this.data.map(emp => {
              const { color } = this.calculateWorkedHours(emp.WorkTime); // ใช้สีจากการคำนวณ
              return color; // สีที่คำนวณจากเปอร์เซ็นต์
            }),
          },
        },
      ];

      const layout = {
        title: "Worked Time (60hrs./week)",
        height: 300,
        margin: { l: 200, r: 20, t: 50, b: 50 }, // ปรับขนาดแกน Y ให้กว้างขึ้น
        xaxis: { title: "Percentage", automargin: true, range: [0, 100] }, // กำหนดให้สูงสุดที่ 100%
        yaxis: { automargin: true },
        responsive: true,
      };

      Plotly.newPlot("worked-time-chart", chartData, layout);
    },
  },
};
</script>

<style scoped>
#worked-time-chart {
  width: 100%;
  height: 100%;
}
</style>
