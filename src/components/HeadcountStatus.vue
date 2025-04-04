<template>
  <!-- <button class="refresh-skill-btn" @click="resetSkillFilter" title="รีเซตฟิลเตอร์">
  <img src="refresh.png" alt="Refresh Icon" class="icon" />
  <span>Refresh</span>
</button> -->
  <div id="headcount-pie-chart">
  </div>
</template>

<script>
import axios from "axios";
import Plotly from "plotly.js";

export default {
  name: "HeadcountPieChart",
  data() {
    return {
      headcountData: [],
    };
  },
  async mounted() {
    await this.fetchHeadcountData();
    this.drawChart();
  },
  methods: {
    async fetchHeadcountData() {
      try {
        const response = await axios.get("http://localhost:5000/api/GateEntry");
        this.headcountData = response.data;
      } catch (error) {
        console.error("Error fetching headcount data:", error);
      }
    },
    drawChart() {
    if (!this.headcountData.length) return;

    const statusCounts = this.headcountData.reduce(
      (acc, entry) => {
        acc[entry.status] = (acc[entry.status] || 0) + 1;
        return acc;
      },
      { "status-in-cleanroom": 0, "status-out-cleanroom": 0, "status-missing": 0 }
    );

    const data = [
      {
        labels: ["In Cleanroom", "Out Cleanroom", "Missing"],
        values: [
          statusCounts["status-in-cleanroom"],
          statusCounts["status-out-cleanroom"],
          statusCounts["status-missing"],
        ],
        type: "pie",
        marker: {
          colors: ["#2ECC71", "#F39C12", "#E74C3C"],
        },
        textinfo: "label+percent",
        hoverinfo: "label+value+percent",
      },
    ];

    const layout = {
      title: "Headcount Status Distribution",
      height: 500,
      width: 500,
    };

    Plotly.newPlot("headcount-pie-chart", data, layout);

    const chart = document.getElementById("headcount-pie-chart");

    chart.on("plotly_click", (data) => {
      const clickedLabel = data.points[0].label;
      const labelToStatus = {
        "In Cleanroom": "status-in-cleanroom",
        "Out Cleanroom": "status-out-cleanroom",
        "Missing": "status-missing",
      };
      this.$emit("filter-status", labelToStatus[clickedLabel]);
    });
  },
  },
  watch: {
    headcountData: {
      handler() {
        this.drawChart();
      },
      deep: true,
    },
  },
};
</script>

<style scoped>
#headcount-pie-chart {
  width: 100%;
  max-width: 500px;
  margin: auto;
}

.refresh-skill-btn {
  display: flex;
  align-items: center;
  gap: 8px;
  background-color: tomato;
  color: white;
  border: none;
  border-radius: 25px;
  padding: 6px 14px;
  margin-bottom: 10px;
  cursor: pointer;
  font-weight: bold;
  font-size: 14px;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.2);
  transition: background-color 0.3s ease, transform 0.3s ease;
}

.refresh-skill-btn:hover {
  background-color: #0056b3;
  transform: scale(1.05);
}

.refresh-skill-btn .icon {
  width: 18px;
  height: 18px;
  filter: invert(1);
}
</style>
