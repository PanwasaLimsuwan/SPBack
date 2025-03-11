<template>
  <div id="pie-chart"></div>
</template>

<script>
import Plotly from "plotly.js";

export default {
  name: "PieChart",
  props: {
    data: {
      type: Object,
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
    drawChart() {
      const chartData = [
        {
          values: this.data.values,
          labels: this.data.labels,
          type: "pie",
          marker: {
            colors: ["red", "yellow", "green"],
          },
        },
      ];

      console.log("Chart Data to Plotly:", chartData);

      const layout = {
        title: "Head Count",
        height: 450,
        width: 450,
        paper_bgcolor: "rgba(0,0,0,0)", // พื้นหลังด้านนอกเป็น transparent
      };

      Plotly.newPlot("pie-chart", chartData, layout).then(() => {
        document
          .getElementById("pie-chart")
          .on("plotly_click", this.onSliceClick);
      });
    },

    onSliceClick(eventData) {
      if (eventData.points && eventData.points.length > 0) {
        const sliceData = eventData.points[0];
        const label = sliceData.label || sliceData.text;
        if (label) {
          this.$emit("filter-headcount", label); // ส่งเฉพาะข้อมูลจาก PieChart นี้
        }
      }
    },
  },
};
</script>

<style scoped>
#pie-chart {
  width: 100%;
  height: 100%;
}
</style>
