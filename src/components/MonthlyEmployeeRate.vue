<template>
  <div id="resignation-chart"></div>
</template>

<script>
import axios from "axios";
import Plotly from "plotly.js";

export default {
  name: "ResignationChart",
  data() {
    return {
      resignationData: [],
    };
  },
  async mounted() {
    await this.fetchResignationData();
    this.renderChart();
  },
  methods: {
    async fetchResignationData() {
      try {
        const response = await axios.get("https://databasemanpowerdb.database.windows.net/api/HeadcountTransition");
        // คัดกรองเฉพาะพนักงานที่ลาออก (transType === "Resign")
        this.resignationData = response.data.filter(entry => entry.transType === "Resign");
      } catch (error) {
        console.error("Error fetching resignation data:", error);
      }
    },
    renderChart() {
      if (!this.resignationData.length) {
        console.error("No resignation data found.");
        return;
      }

      // 🔹 แปลงข้อมูลให้เป็นกลุ่มตามเดือน
      const resignationByMonth = this.aggregateResignationsByMonth();

      // 🔹 ดึงค่าที่ต้องการแสดงในกราฟ
      const months = Object.keys(resignationByMonth);
      const resignCounts = months.map(month => resignationByMonth[month]);

      // 🔹 สร้างข้อมูลสำหรับ Plotly
      const chartData = [
        {
          x: months,
          y: resignCounts,
          type: "bar",
          name: "Resignations",
          marker: { color: "red" },
        },
      ];

      // 🔹 ตั้งค่ากราฟ
      const layout = {
        title: "Employee Resignation Rate per Month",
        xaxis: { title: "Month" },
        yaxis: { title: "Number of Resignations" },
        paper_bgcolor: "rgba(0,0,0,0)",
        plot_bgcolor: "rgba(0,0,0,0)",
      };

      // 🔹 สร้างกราฟด้วย Plotly
      Plotly.newPlot("resignation-chart", chartData, layout);
    },
    aggregateResignationsByMonth() {
      const resignationsByMonth = {};

      this.resignationData.forEach(entry => {
        const month = entry.dateTime.substring(0, 7); // แปลงเป็นรูปแบบ "YYYY-MM"
        resignationsByMonth[month] = (resignationsByMonth[month] || 0) + 1;
      });

      return resignationsByMonth;
    },
  },
};
</script>

<style scoped>
#resignation-chart {
  width: 100%;
  height: 100%;
}
</style>
