<template>
  <div class="chart-container">
    <div id="worked-time-chart" class="scrollable-chart"></div>
  </div>
</template>

<script>
import Plotly from "plotly.js";
import axios from "axios";

export default {
  name: "WorkedTimeChart",
  data() {
    return {
      worktime: [],
      employeeInfo: [],
      combinedData: [],
    };
  },
  async mounted() {
    await this.fetchData();
    this.prepareData();
    this.drawChart();
  },
  methods: {
    async fetchData() {
      try {
        const [workRes, empRes] = await Promise.all([
          axios.get("http://localhost:5000/api/Worktime"),
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
      const worked = parseFloat(w.workedHours) || 0;

      if (!groupedByEmp[empID]) {
        groupedByEmp[empID] = { totalWorked: 0 };
      }

      groupedByEmp[empID].totalWorked += worked;
    });

    this.combinedData = Object.keys(groupedByEmp).map(empID => {
  const emp = this.employeeInfo.find(e => `${e.empID}` === `${empID}`);
  const name = emp ? `${emp.firstName} ${emp.lastName}` : `Emp ${empID}`;

  const total = groupedByEmp[empID].totalWorked;
  const percent = (total / 60) * 100;

  let color = "#4caf50"; // เขียว
  if (percent >= 90) color = "#f44336"; // แดง
  else if (percent >= 80) color = "#ffc107"; // เหลือง

  return { name, percent, color };
});

// ✅ เรียงลำดับสี และเปอร์เซ็นต์จากมาก → น้อย
this.combinedData.sort((a, b) => {
  const getRank = (color) => {
    if (color === "#f44336") return 1;   // แดง
    if (color === "#ffc107") return 2;   // เหลือง
    return 3;                            // เขียว
  };

  const rankA = getRank(a.color);
  const rankB = getRank(b.color);

  if (rankA !== rankB) {
    return rankA - rankB; // เรียงตามสี
  } else {
    return b.percent - a.percent; // ถ้าสีเท่ากัน เรียงตามเปอร์เซ็นต์
  }
});

},

    drawChart() {
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
        yaxis: { automargin: true, autorange: "reversed" }, // 👈 พลิกลำดับแกน Y
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

#worked-time-chart {
  width: 100%;
  height: 100%;
}

.scrollable-chart::-webkit-scrollbar-thumb {
  background-color: rgba(100, 100, 100, 0.2);
  border-radius: 4px;
}
</style>
