<template>
  <div id="resource-allocation-summary"></div>
</template>

<script>
import Plotly from "plotly.js";

export default {
  name: "ResourceAllocationSummary",
  props: {
    data: {
      type: Array, // คาดว่าข้อมูลจะเป็น Array ของการขาดงานที่ประกอบไปด้วยแต่ละ process และเดือน
      required: true,
    },
  },
  mounted() {
    this.drawChart();
  },
  methods: {
    drawChart() {
      if (!Array.isArray(this.data) || this.data.length === 0) {
        console.error("Data is invalid or empty");
        return;
      }

      // สมมุติว่า data ของคุณมีโครงสร้างแบบนี้:
      // data = [
      //   { month: "January", process: "Process 1", absence: 10 },
      //   { month: "January", process: "Process 2", absence: 5 },
      //   { month: "February", process: "Process 1", absence: 8 },
      //   { month: "February", process: "Process 2", absence: 3 },
      //   ...
      // ]

      // เตรียมข้อมูลกราฟ
      const months = [...new Set(this.data.map(item => item.month))]; // หามีเดือนที่แตกต่างกันทั้งหมด
      const processes = [...new Set(this.data.map(item => item.process))]; // หาชื่อ process ที่แตกต่าง

      // สร้างข้อมูลกราฟ (สำหรับแต่ละ process ในแต่ละเดือน)
      const chartData = processes.map(process => {
        return {
          x: months,
          y: months.map(month => {
            const item = this.data.find(
              (dataItem) => dataItem.month === month && dataItem.process === process
            );
            return item ? item.absence : 0; // ถ้าไม่มีข้อมูลให้คืนค่า 0
          }),
          type: "bar", // หรือ "scatter" หากต้องการใช้แบบเส้น
          name: process, // ชื่อของ Process
        };
      });

      const layout = {
        title: "การสรุปข้อมูลการขาดงานตาม Process",
        barmode: "group", // กำหนดให้แสดง bar แต่ละ process ในแต่ละเดือน
        xaxis: { title: "เดือน", automargin: true },
        yaxis: {
          title: "จำนวนการขาดงาน",
          range: [0, 25], // กำหนดช่วงแกน y (สามารถปรับตามข้อมูลจริงได้)
        },
        responsive: true,
      };

      Plotly.newPlot("resource-allocation-summary", chartData, layout);
    },
  },
};
</script>

<style scoped>
#resource-allocation-summary {
  width: 100%;
  height: 100%;
}
</style>
