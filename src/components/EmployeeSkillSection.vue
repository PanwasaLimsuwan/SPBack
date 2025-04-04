<template>
  <div v-if="employee" class="employee-section">
    <h3>{{ employee.firstName }} {{ employee.lastName }}</h3>
    <div v-for="s in skillMap" :key="s.key" class="skill-row">
      <p>{{ s.label }}: {{ getLevelLabel(employee[s.key]) }}</p>
      <div class="skill-bar" :style="barStyle(employee[s.key])"></div>
    </div>
  </div>
</template>

<script>
export default {
  name: "EmployeeSkillSection",
  props: {
    employee: { type: Object, required: true },
  },
  data() {
    return {
      skillMap: [
        { key: "material", label: "Material" },
        { key: "operation", label: "Operation" },
        { key: "machineSAB1", label: "Machine:SAB#1" },
        { key: "machineSAB2", label: "Machine:SAB#2" },
        { key: "machineSAB3", label: "Machine:SAB#3" },
        { key: "inspection", label: "Inspection" },
      ],
    };
  },
  methods: {
    barStyle(level) {
      const percent = (level / 3) * 100;
      const colorMap = ["gray", "red", "yellow", "green"];
      const color = colorMap[level] || "gray";
      return {
        background: `linear-gradient(to right, ${color} ${percent}%, #ddd ${percent}%)`,
      };
    },
    getLevelLabel(level) {
      return ["Not Trained", "Basic", "Medium", "Expert"][level] ?? "Unknown";
    },
  },
};
</script>

<style scoped>
.employee-section {
  display: flex;
  flex-direction: column;
  flex: 1;
  width: 100%;
  background-color: #fff;
  padding: 20px;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
}

.skill-row {
  display: flex;
  flex-direction: column;
  margin-bottom: 16px;
}

.skill-bar {
  height: 10px;
  border-radius: 6px;
  background-color: gray;
  transition: width 0.3s ease;
}

h3 {
  margin-bottom: 15px;
}
</style>
