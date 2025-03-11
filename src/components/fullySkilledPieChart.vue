<template>
  <div id="fully-skilled-pie-chart"></div>
</template>

<script>
import Plotly from "plotly.js";

export default {
  name: "FullySkilledPieChart",
  props: {
    data: {
      type: Object,
      required: true,
    },
  },
  mounted() {
    this.drawChart();
  },
  methods: {
    drawChart() {
  const chartData = [
    {
      values: this.data.values,  // ใช้ข้อมูลที่คำนวณจาก getFullySkilledData
      labels: this.data.labels,  // ใช้ข้อมูลที่คำนวณจาก getFullySkilledData
      type: "pie",
      marker: {
        colors: this.data.marker ? this.data.marker.colors : ["gray"] // ตรวจสอบว่า data.marker มีค่าไหม
      }
    }
  ];

  const layout = {
    title: "Skill Competency Overview",
    height: 450,
    width: 450,
    paper_bgcolor: "rgba(0,0,0,0)", // พื้นหลังโปร่งใส
  };

  Plotly.newPlot("fully-skilled-pie-chart", chartData, layout).then(() => {
      document
        .getElementById("fully-skilled-pie-chart")
        .on("plotly_click", this.onSliceClick);
    });
  },

  onSliceClick(eventData) {
  if (eventData.points && eventData.points.length > 0) {
    const sliceLabel = eventData.points[0].label; // รับค่าที่คลิก
    this.$emit("filter-skills", sliceLabel); // ส่งข้อมูลกลับไปที่ parent component
  }
},

  },
  watch: {
    data: {
      deep: true,
      handler() {
        this.drawChart();
      },
    },
  },
};
</script>

<style scoped>
#fully-skilled-pie-chart {
  width: 100%;
  height: 100%;
}
</style>
