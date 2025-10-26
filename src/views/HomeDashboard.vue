<template>
  <div class="home">
    <!-- แถบหัว -->
    <header class="appbar" role="banner" aria-label="Top bar">
      <div class="brand">
        <img src="/logo2.png" alt="โลโก้บริษัท" class="logo" />
        <div class="brand-text">
          <!-- <h1 class="app-title">ศูนย์รวมแดชบอร์ดการปฏิบัติการ</h1>
          <p class="app-subtitle">กรุณาเลือกพื้นที่ทำงานที่ต้องการเข้าใช้งาน</p> -->
        </div>
      </div>

      <div class="actions">
        <div v-if="userName" class="user-box" aria-label="ผู้ใช้งานปัจจุบัน">
          <span class="user-initial">{{ userInitial }}</span>
          <span class="user-name" :title="userName">{{ userName }}</span>
        </div>
        <button class="btn-out" @click="onLogout" aria-label="ออกจากระบบ">ออกจากระบบ</button>
      </div>
    </header>

    <!-- เนื้อหาหลัก -->
    <main class="container" role="main">
      <section class="section-head">
        <h2 class="section-title">Select Dashboard</h2>
        <!-- <p class="section-desc">โปรดเลือกแดชบอร์ดที่คุณมีสิทธิ์เข้าถึง</p> -->
      </section>

      <div class="cards">
        <!-- MFG -->
        <router-link
          to="/dashboard-mfg"
          class="card mfg"
          aria-label="เข้าสู่แดชบอร์ดการผลิต (MFG)"
        >
          <div class="card-head">
            <div class="card-icon">🏭</div>
            <h3 class="card-title">การผลิต (MFG)</h3>
          </div>
          <p class="card-desc">
            สถานะการผลิตเรียลไทม์, Headcount, Overtime, Utilization และแนวโน้มการทำงาน
          </p>
          <!-- <div class="card-meta">
            <span class="meta-badge live">สด</span>
            <span class="meta-text">อัปเดตต่อเนื่อง</span>
          </div> -->
        </router-link>

        <!-- HR -->
        <router-link
          to="/dashboard-hr"
          class="card hr"
          aria-label="เข้าสู่แดชบอร์ดทรัพยากรบุคคล (HR)"
        >
          <div class="card-head">
            <div class="card-icon">👥</div>
            <h3 class="card-title">ทรัพยากรบุคคล (HR)</h3>
          </div>
          <p class="card-desc">
            ภาพรวมกำลังคน, สกิลเมทริกซ์, การย้ายเข้าออก, การฝึกอบรม และสถิติการขาดงาน
          </p>
          <!-- <div class="card-meta">
            <span class="meta-badge daily">รายวัน</span>
            <span class="meta-text">อัปเดตทุกวัน</span>
          </div> -->
        </router-link>
      </div>
    </main>

    <!-- ท้ายหน้า -->
    <!-- <footer class="footer" role="contentinfo">
      <span>© {{ year }} ระบบภายในองค์กร</span>
      <span class="dot">•</span>
      <span>ข้อมูลลับสำหรับงานภายใน</span>
    </footer> -->
  </div>
</template>

<script>
export default {
  name: "HomeDashboard",
  data() {
    return { year: new Date().getFullYear() };
  },
  computed: {
    userName() {
      // ถ้ามีการเก็บชื่อผู้ใช้ตอนล็อกอิน ให้ set ลง localStorage.userName
      return localStorage.getItem("userName") || "";
    },
    userInitial() {
      const n = this.userName?.trim();
      if (!n) return "U";
      const parts = n.split(" ");
      const first = parts[0]?.[0] || "";
      const last = parts[1]?.[0] || "";
      return (first + last || first).toUpperCase();
    },
  },
  methods: {
    onLogout() {
      localStorage.removeItem("token");
      localStorage.removeItem("userName");
      this.$router.replace({ name: "login" });
    },
  },
};
</script>

<style scoped>
/* ===== สี & โทน (สดใสแต่ยังดูองค์กร) ===== */
:root {
  --bg: #f6f7fb;
  --surface: #ffffff;
  --ink: #1f2430;
  --ink-sub: #626a78;
  --line: #e6e9f2;

  /* โทนสีหลักแบบมีสีสัน */
  --brand: #3b82f6;     /* ฟ้า */
  --brand-2: #7c3aed;   /* ม่วง */
  --accent-1: #10b981;  /* เขียว */
  --accent-2: #f59e0b;  /* เหลืองส้ม */
  --accent-3: #ef4444;  /* แดง */

  --shadow: 0 12px 30px rgba(31, 36, 48, 0.12);
  --radius: 16px;
}

/* พื้นหลังมีลวดลาย gradient เพื่อความมีชีวิตชีวา */
.home {
  min-height: 100vh;
  display: grid;
  grid-template-rows: auto 1fr auto;
  background:
    radial-gradient(1000px 500px at 85% -15%, rgba(124, 58, 237, 0.12), transparent 55%),
    radial-gradient(900px 450px at -15% -20%, rgba(59, 130, 246, 0.12), transparent 55%),
    var(--bg);
  color: var(--ink);
}

/* ===== App bar ===== */
.appbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  padding: 18px 28px;
  background: linear-gradient(90deg, rgba(59,130,246,.08), rgba(124,58,237,.08));
  backdrop-filter: blur(6px);
  border-bottom: 1px solid var(--line);
}

.brand {
  display: flex;
  align-items: center;
  gap: 14px;
}
.logo {
  width: 44px;
  height: 44px;
  object-fit: contain;
  filter: drop-shadow(0 2px 6px rgba(0,0,0,.08));
}
.brand-text .app-title {
  font-size: 1.18rem;
  font-weight: 800;
  margin: 0 0 2px;
  letter-spacing: .2px;
  background: linear-gradient(90deg, var(--brand), var(--brand-2));
  -webkit-background-clip: text;
  background-clip: text;
  color: transparent;
}
.brand-text .app-subtitle {
  margin: 0;
  font-size: 0.95rem;
  color: var(--ink-sub);
}

.actions {
  display: flex;
  align-items: center;
  gap: 12px;
}
.user-box {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 6px 10px;
  border: 1px solid var(--line);
  border-radius: 999px;
  background: #fff;
  box-shadow: 0 4px 14px rgba(0,0,0,.04);
}
.user-initial {
  width: 28px;
  height: 28px;
  border-radius: 50%;
  display: grid;
  place-items: center;
  font-weight: 800;
  background: linear-gradient(135deg, var(--brand), var(--brand-2));
  color: #fff;
  font-size: 0.85rem;
}
.user-name {
  max-width: 160px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
.btn-out {
  appearance: none;
  border: 1px solid var(--line);
  background: #fff;
  color: var(--ink);
  padding: 8px 14px;
  border-radius: 12px;
  font-weight: 700;
  cursor: pointer;
  transition: all .2s ease;
}
.btn-out:hover {
  border-color: #cfd3e1;
  transform: translateY(-1px);
}

/* ===== Main ===== */
.container {
  max-width: 1080px;
  margin: 44px auto 32px;
  padding: 0 20px;
}

.section-head { margin-bottom: 18px; }
.section-title {
  font-size: 1.28rem;
  margin: 0 0 6px;
  font-weight: 800;
}
.section-desc {
  margin: 0;
  color: var(--ink-sub);
  font-size: 0.98rem;
}

/* ===== Cards ===== */
.cards {
  display: grid;
  grid-template-columns: repeat(2, minmax(280px, 1fr));
  gap: 20px;
}

.card {
  display: block;
  border-radius: var(--radius);
  padding: 20px 18px 16px;
  text-decoration: none;
  color: inherit;
  box-shadow: var(--shadow);
  border: 1px solid var(--line);
  transition: transform .18s ease, box-shadow .18s ease, border-color .18s ease, filter .18s ease;
  outline: none;
  position: relative;
  overflow: hidden;
}

/* แถบสีเล็ก ๆ ด้านบนเพื่อบอกหมวด */
.card::before {
  content: "";
  position: absolute;
  top: -1px; left: 0; right: 0;
  height: 6px;
  opacity: .9;
}

.card:hover {
  transform: translateY(-3px);
  box-shadow: 0 16px 36px rgba(17, 24, 39, 0.16);
  filter: saturate(1.02);
}
.card:focus-visible {
  border-color: var(--brand);
  box-shadow: 0 0 0 3px rgba(59,130,246,.18), var(--shadow);
}

.card-head {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 8px;
}
.card-icon {
  width: 48px;
  height: 48px;
  border-radius: 14px;
  display: grid;
  place-items: center;
  font-size: 1.5rem;
  color: #fff;
  /* พื้นหลังไล่สีให้สดใส */
  background: linear-gradient(135deg, rgba(59,130,246,1), rgba(124,58,237,1));
  box-shadow: 0 6px 16px rgba(124,58,237,.25);
}
.card-title {
  font-size: 1.08rem;
  font-weight: 800;
  margin: 0;
}
.card-desc {
  margin: 8px 0 14px;
  color: var(--ink-sub);
  line-height: 1.5;
  font-size: .97rem;
}
.card-meta {
  display: flex;
  align-items: center;
  gap: 10px;
}
.meta-badge {
  display: inline-block;
  font-size: .8rem;
  font-weight: 800;
  color: #fff;
  padding: 5px 10px;
  border-radius: 999px;
  letter-spacing: .2px;
}
.meta-badge.live {
  background: linear-gradient(135deg, var(--accent-1), #34d399);
}
.meta-badge.daily {
  background: linear-gradient(135deg, #64748b, #94a3b8);
}
.meta-text {
  font-size: .9rem;
  color: var(--ink-sub);
}

/* สีพื้นหลังการ์ดแยกหมวด */
.card.mfg {
  background:
    radial-gradient(220px 160px at 85% -30%, rgba(16,185,129,.14), transparent 60%),
    radial-gradient(220px 160px at -10% -40%, rgba(59,130,246,.14), transparent 60%),
    var(--surface);
}
.card.mfg::before {
  background: linear-gradient(90deg, var(--accent-1), var(--brand));
}

.card.hr {
  background:
    radial-gradient(220px 160px at 85% -30%, rgba(245,158,11,.16), transparent 60%),
    radial-gradient(220px 160px at -10% -40%, rgba(124,58,237,.16), transparent 60%),
    var(--surface);
}
.card.hr::before {
  background: linear-gradient(90deg, var(--accent-2), var(--brand-2));
}

/* ===== Footer ===== */
.footer {
  display: flex;
  justify-content: center;
  gap: 10px;
  padding: 14px 18px;
  color: var(--ink-sub);
  font-size: .95rem;
  border-top: 1px solid var(--line);
  background: #fff;
}
.footer .dot { opacity: .6; }

/* ===== Responsive ===== */
@media (max-width: 920px) {
  .cards { grid-template-columns: 1fr; }
  .brand-text .app-title { font-size: 1.08rem; }
}
</style>
