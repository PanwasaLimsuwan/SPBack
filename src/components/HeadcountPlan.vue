<template>
  <div id="headcount-chart"></div>
</template>

<script>
import axios from "axios";
import Plotly from "plotly.js";

export default {
  name: "HeadcountPlan",
  data() {
    return {
      headcountData: [],
    };
  },
  mounted() {
    this.fetchData();
  },
  methods: {
    async fetchData() {
      try {
        const response = await axios.get("http://localhost:5000/api/ManpowerPlan"); // เปลี่ยนเป็น API จริง
        this.headcountData = response.data;
        this.drawChart();
      } catch (error) {
        console.error("Error fetching data:", error);
      }
    },
    drawChart() {
      if (!this.headcountData.length) return;
      
      const dates = this.headcountData.map(item => item.date);
      const planned = this.headcountData.map(item => item.plannedHeadcount);
      const actual = this.headcountData.map(item => item.actualHeadcount);
      
      const chartData = [
        {
          x: dates,
          y: planned,
          type: "bar",
          name: "Planned Headcount",
          marker: { color: "red" },
        },
        {
          x: dates,
          y: actual,
          type: "bar",
          name: "Actual Headcount",
          marker: { color: "blue" },
        },
      ];

      const layout = {
        title: "Headcount vs Plan",
        barmode: "group",
        xaxis: { title: "Date", tickangle: -45 },
        yaxis: { title: "Headcount" },
        responsive: true,
      };

      Plotly.newPlot("headcount-chart", chartData, layout);
    },
  },
};
</script>
