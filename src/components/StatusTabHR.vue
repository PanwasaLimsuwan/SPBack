<template>
    <div class="stat-card" v-for="(card, index) in stats" :key="index">
      <div class="icon-container">
        <span v-if="card.dotColor" :style="{ backgroundColor: card.dotColor }" class="status-dot"></span>
        <img v-else-if="card.icon" :src="card.icon" alt="Icon" class="icon" />
      </div>
      <div class="content">
        <h3>
          {{ card.value }}
        </h3>
        <p>{{ card.label }}</p>
        <p v-if="card.subLabel" class="sub-label">{{ card.subLabel }}</p>
        <!-- <div v-if="showBell && card.icon === 'clock.png'" class="bell-alert">
          <img src="/bell.png" alt="Notification" class="bell-icon" />
          <span class="bell-text">ถึงเวลาเปลี่ยนกะ!</span>
        </div> -->
      </div>
    </div>
  </template>
  
  <script>
  import axios from 'axios';
  import { useToast } from 'vue-toastification';
  
  export default {
    props: {
      filters: Object // ✅ รับ props filter เข้ามา
    },
    data() {
      return {
        employees: [],
        showBell: false,
        hasAlerted: false,
        toast: null,
        stats: []
      };
    },
    watch: {
      filters: {
        handler() {
          this.refreshAll(); // ✅ ถ้ามีการเปลี่ยน filter -> refresh ข้อมูล
        },
        deep: true
      }
    },
    methods: {
      async fetchGateEntryData() {
        try {
          const response = await axios.get('https://deploymanpowerdb-f5a0h6fqaehdajck.southeastasia-01.azurewebsites.net/api/Attendance/ByDate', {
            params: {
              division: this.filters.division !== 'ALL' ? this.filters.division : undefined,
              department: this.filters.department !== 'ALL' ? this.filters.department : undefined,
              section: this.filters.section !== 'ALL' ? this.filters.section : undefined,
              biz: this.filters.biz !== 'ALL' ? this.filters.biz : undefined,
              process: this.filters.process !== 'ALL' ? this.filters.process : undefined,
            }
          });
          this.employees = response.data;
          this.updateStats();
        } catch (error) {
          console.error("Error fetching data from API:", error);
        }
      },
  
      updateStats() {
    const totalEmployees = this.employees.length;
    const inCleanroom = this.employees.filter(e => e.status === "normal").length;
    const outCleanroom = this.employees.filter(e => e.status === "late").length;
    const missing = this.employees.filter(e => e.status === "Missing").length;
    const need = missing;
  
    const currentTime = new Date();
  
    this.stats = [
      {
        value: currentTime.toLocaleTimeString(),
        label: "SHIFT : ?",
        subLabel: currentTime.toLocaleDateString('th-TH'),
        icon: "clock.png"
      },
      { value: totalEmployees, label: "พนักงานทั้งหมด", subLabel: "คน" },
      { value: inCleanroom, label: "Normal", subLabel: "คน", dotColor: "#00cc66" },
      { value: outCleanroom, label: "Late", subLabel: "คน", dotColor: "#ffcc00" },
      { value: missing, label: "Missing", subLabel: "คน", dotColor: "#ff6666" },
    ];
  
    this.updateShift(currentTime);
  },
  
      updateShift(currentTime) {
        const hours = currentTime.getHours();
        const shiftLabel = (hours >= 7 && hours < 19) ? "SHIFT : DAY" : "SHIFT : NIGHT";
        this.stats[0].label = shiftLabel;
      },
  
      startClock() {
        setInterval(() => {
          const now = new Date();
          if (this.stats.length > 0) {
            this.stats[0].value = now.toLocaleTimeString();
            this.stats[0].subLabel = now.toLocaleDateString('th-TH');
            this.updateShift(now);
          }
        }, 1000);
      },
  
      checkShiftAlert() {
        const now = new Date();
        const hour = now.getHours();
        const minute = now.getMinutes();
  
        const inMorningShift = hour === 7 && minute >= 15 && minute <= 45;
        const inNightShift = hour === 19 && minute >= 15 && minute <= 45;
  
        const isShiftTime = inMorningShift || inNightShift;
  
        this.showBell = isShiftTime;
  
        if (isShiftTime && !this.hasAlerted) {
          this.hasAlerted = true;
  
          this.toast.warning("ถึงเวลาเปลี่ยนกะ กรุณาแจ้งพนักงาน!", {
            timeout: 5000,
            position: 'top-right',
          });
        } else if (!isShiftTime) {
          this.hasAlerted = false;
        }
      },
  
      async refreshAll() {
        await this.fetchGateEntryData();
        this.checkShiftAlert();
      },
    },
  
    mounted() {
      this.toast = useToast();
      this.refreshAll();
      this.startClock();
  
      setInterval(() => {
        this.checkShiftAlert();
        this.refreshAll();
      }, 10000); // ⏱️ 10 วินาทีพอ ไม่ต้องถี่ 1 วิครับ
    }
  };
  </script>
  
  <style scoped>
  /* (style เดิมของคุณ ไม่มีเปลี่ยน) */
  .bell-alert {
    display: flex;
    align-items: center;
    background-color: #fffbe6;
    border: 1px solid #ffcc00;
    border-radius: 8px;
    padding: 10px;
    margin: 10px auto;
    width: fit-content;
    box-shadow: 0 0 10px rgba(255, 204, 0, 0.4);
    animation: pulse 1s infinite;
  }
  
  .bell-icon {
    width: 30px;
    height: 30px;
    margin-right: 10px;
  }
  
  .bell-text {
    font-weight: bold;
    color: #ff9900;
    font-size: 16px;
  }
  
  @keyframes pulse {
    0% { transform: scale(1); }
    50% { transform: scale(1.1); }
    100% { transform: scale(1); }
  }
  
  .stat-card {
    flex: 1;
    text-align: center;
    padding: 10px;
    background: #fff;
    border-radius: 8px;
    box-shadow: 0px 2px 4px rgba(0, 0, 0, 0.1);
    display: flex;
    align-items: center;
    background-color: #f9f9f9;
    border-radius: 12px;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
    padding: 15px;
    width: 180px;
    transition: transform 0.2s;
    justify-content: space-between;
  }
  
  .icon-container {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 40px;
    height: 40px;
    margin-left: 20px;
  }
  
  .status-dot {
    width: 22px;
    height: 22px;
    border-radius: 50%;
  }
  
  .content {
    text-align: right;
  }
  
  h3 {
    font-size: 20px;
    font-weight: bold;
    margin: 0;
  }
  
  p {
    font-size: 14px;
    margin: 4px 0 0;
  }
  
  .sub-label {
    font-size: 16px;
    color: #888;
  }
  
  .status-in-cleanroom { color: green; }
  .status-out-cleanroom { color: orange; }
  .status-missing { color: red; }
  .stat-get-off { color: blue; }
  </style>
  