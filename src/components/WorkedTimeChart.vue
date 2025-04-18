<template>
  <div class="chart-container">
    <div class="filter-bar">
      <label for="week-select">Select Week:</label>
      <select id="week-select" v-model="selectedWeekID" @change="onWeekChange">
        <option v-for="week in weekOptions" :key="week" :value="week">{{ week }}</option>
      </select>
    </div>
    <div id="worked-time-chart" class="scrollable-chart"></div>
  </div>
</template>

<script>
import Plotly from "plotly.js";
import axios from "axios";

export default {
  name: "WorkedTimeChart",
  props: {
    filters: Object,
  },
  data() {
    return {
      worktime: [],
      employeeInfo: [],
      combinedData: [],
      weekOptions: [],
      selectedWeekID: null,
    };
  },
  async mounted() {
    await this.fetchWeekOptions();
    this.selectedWeekID = this.weekOptions[this.weekOptions.length - 1]; // default สัปดาห์ล่าสุด
    await this.refreshChart();
  },
  watch: {
    filters: {
      handler() {
        this.fetchWeekOptions().then(() => {
          this.selectedWeekID = this.weekOptions[this.weekOptions.length - 1];
          this.refreshChart();
        });
      },
      deep: true,
    },
  },
  methods: {
    async fetchWeekOptions() {
      try {
        const response = await axios.get("http://localhost:5000/api/EICCControl", {
          params: {
            division: this.filters.division !== 'ALL' ? this.filters.division : undefined,
            department: this.filters.department !== 'ALL' ? this.filters.department : undefined,
            section: this.filters.section !== 'ALL' ? this.filters.section : undefined,
            biz: this.filters.biz !== 'ALL' ? this.filters.biz : undefined,
            process: this.filters.process !== 'ALL' ? this.filters.process : undefined,
            weekID: this.selectedWeekID
          },
        });
        const weeks = [...new Set(response.data.map(item => item.weekID))].sort((a, b) => a - b);
        this.weekOptions = weeks;
      } catch (error) {
        console.error("Error fetching weeks:", error);
      }
    },

    onWeekChange() {
      this.refreshChart();
    },

    async refreshChart() {
      await this.fetchData();
      this.prepareData();
      await this.$nextTick();
      this.drawChart();
    },

    async fetchData() {
      try {
        const [workRes, empRes] = await Promise.all([
          axios.get("http://localhost:5000/api/EICCControl", {
            params: {
              division: this.filters.division !== 'ALL' ? this.filters.division : undefined,
              department: this.filters.department !== 'ALL' ? this.filters.department : undefined,
              section: this.filters.section !== 'ALL' ? this.filters.section : undefined,
              biz: this.filters.biz !== 'ALL' ? this.filters.biz : undefined,
              process: this.filters.process !== 'ALL' ? this.filters.process : undefined,
              weekID: this.selectedWeekID,
            },
          }),
          axios.get("http://localhost:5000/api/EmployeeInfo"),
        ]);
        this.worktime = workRes.data;
        this.employeeInfo = empRes.data;
      } catch (error) {
        console.error("Error fetching data:", error);
      }
    },

    prepareData() {
      const groupedByEmp = {};

      this.worktime
        .filter(w => w.status === "Active")
        .forEach(w => {
          const empID = w.empID;
          const worked = parseFloat(w.totalHours) || 0;

          if (!groupedByEmp[empID]) {
            groupedByEmp[empID] = { totalWorked: 0 };
          }

          groupedByEmp[empID].totalWorked += worked;
        });

      const employeeMap = this.employeeInfo.reduce((map, e) => {
        map[e.empID] = e;
        return map;
      }, {});

      this.combinedData = Object.keys(groupedByEmp).map(empID => {
        const emp = employeeMap[empID];
        const name = emp ? `${emp.firstName} ${emp.lastName}` : `Emp ${empID}`;

        const total = groupedByEmp[empID].totalWorked;
        const percent = (total / 60) * 100;

        let color = "#4caf50"; // Green
        if (percent >= 100) color = "#f44336"; // Red
        else if (percent >= 80) color = "#ffc107"; // Yellow

        return { name, percent, color };
      });

      this.combinedData.sort((a, b) => {
        const getRank = (color) => {
          if (color === "#f44336") return 1;
          if (color === "#ffc107") return 2;
          return 3;
        };

        const rankA = getRank(a.color);
        const rankB = getRank(b.color);

        return rankA !== rankB ? rankA - rankB : b.percent - a.percent;
      });
    },

    drawChart() {
      if (!document.getElementById("worked-time-chart")) {
        console.error("Chart container not found!");
        return;
      }

      if (this.combinedData.length === 0) {
        Plotly.newPlot("worked-time-chart", [], {
          title: "No data to display",
          xaxis: { visible: false },
          yaxis: { visible: false },
          annotations: [{
            text: "No data to display",
            xref: "paper",
            yref: "paper",
            showarrow: false,
            font: { size: 16 },
          }],
        });
        return;
      }

      const chartData = [
        {
          x: this.combinedData.map(emp => emp.percent),
          y: this.combinedData.map(emp => emp.name),
          type: "bar",
          orientation: "h",
          marker: {
            color: this.combinedData.map(emp => emp.color),
          },
          text: this.combinedData.map(emp => `${emp.percent.toFixed(1)}%`),
          textposition: "auto",
        },
      ];

      const layout = {
        title: "Worked Time (60 hrs/week)",
        height: Math.max(400, this.combinedData.length * 40),
        margin: { l: 200, r: 20, t: 50, b: 50 },
        xaxis: { title: "Percentage", range: [0, 120] },
        yaxis: { automargin: true, autorange: "reversed" },
        plot_bgcolor: "#f9f9f9",
        paper_bgcolor: "#fff",
      };

      Plotly.newPlot("worked-time-chart", chartData, layout, { responsive: true });
    },
  },
};
</script>

<style scoped>
.chart-container {
  max-height: 400px;
  overflow-y: auto;
  padding: 15px;
  background: #f0f4f8;
  border-radius: 10px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
}

.filter-bar {
  margin-bottom: 10px;
  display: flex;
  align-items: center;
}

.filter-bar label {
  margin-right: 8px;
}

.filter-bar select {
  padding: 4px 8px;
  border-radius: 4px;
}

#worked-time-chart {
  width: 100%;
  height: 100%;
}

.scrollable-chart::-webkit-scrollbar-thumb {
  background-color: rgba(100, 100, 100, 0.2);
  border-radius: 4px;
}
</style>