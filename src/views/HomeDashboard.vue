<template>
  <div class="home">
    <!-- แถบหัว (App Bar) --><header class="appbar" role="banner" aria-label="แถบนำทางและข้อมูลผู้ใช้งาน">
      <div class="brand">
        <!-- Logo --><img src="/logo2.png" alt="โลโก้บริษัท" class="logo" />
        <div class="brand-text">
          <h1 class="app-title">ศูนย์รวมแดชบอร์ดการปฏิบัติการ</h1>
          <p class="app-subtitle">กรุณาเลือกพื้นที่ทำงานที่ต้องการเข้าใช้งาน</p>
        </div>
      </div>

      <div class="actions">
        <!-- กล่องแสดงชื่อผู้ใช้งานและปุ่ม Logout --><div v-if="username" class="user-box" aria-label="ผู้ใช้งานปัจจุบัน">
          <span class="user-initial">{{ userInitial }}</span>
          <span class="user-name" :title="username">{{ username }}</span>
        </div>
        <!-- ใช้ Logout Global Component --><Logout />
      </div>
    </header>

    <!-- เนื้อหาหลัก --><main class="container" role="main">
      <section class="section-head">
        <h2 class="section-title">Select Dashboard</h2>
        <p class="section-desc">โปรดเลือกแดชบอร์ดที่คุณมีสิทธิ์เข้าถึงเพื่อดูภาพรวมการดำเนินงาน</p>
      </section>

      <div class="cards">
        <!-- MFG (การผลิต) Card --><router-link
          to="/dashboard-mfg"
          class="card card-mfg"
          aria-label="เข้าสู่แดชบอร์ดการผลิต (MFG)"
        >
          <div class="card-head">
            <div class="card-icon">
              <!-- SVG Icon for Factory --><svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="lucide lucide-factory"><path d="M2 20a2 2 0 0 0 2 2h16a2 2 0 0 0 2-2V8l-2 2-2-2-2 2-2-2-2 2-2-2-2 2-2-2v12z"/><path d="M8 10V6"/><path d="M12 10V6"/><path d="M16 10V6"/></svg>
            </div>
            <h3 class="card-title">การผลิต (Manufacturing)</h3>
          </div>
          <p class="card-desc">
            สถานะการผลิตเรียลไทม์, Headcount, Overtime, Utilization และแนวโน้มการทำงาน
          </p>
          <div class="card-arrow">
            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="lucide lucide-arrow-right"><path d="M5 12h14"/><path d="m12 5 7 7-7 7"/></svg>
          </div>
        </router-link>

        <!-- HR (ทรัพยากรบุคคล) Card --><router-link
          to="/dashboard-hr"
          class="card card-hr"
          aria-label="เข้าสู่แดชบอร์ดทรัพยากรบุคคล (HR)"
        >
          <div class="card-head">
            <div class="card-icon">
              <!-- SVG Icon for Users --><svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="lucide lucide-users-round"><path d="M18.36 17.55a4 4 0 0 0-5.83 0"/><path d="M15 11a3 3 0 1 0 0-6 3 3 0 0 0 0 6z"/><path d="M22 20a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2v-2a4 4 0 0 1 4-4h12a4 4 0 0 1 4 4v2z"/></svg>
            </div>
            <h3 class="card-title">ทรัพยากรบุคคล (Human Resources)</h3>
          </div>
          <p class="card-desc">
            ภาพรวมกำลังคน, สกิลเมทริกซ์, การย้ายเข้าออก, การฝึกอบรม และสถิติการขาดงาน
          </p>
          <div class="card-arrow">
            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="lucide lucide-arrow-right"><path d="M5 12h14"/><path d="m12 5 7 7-7 7"/></svg>
          </div>
        </router-link>
      </div>
    </main>

    <!-- Footer --><footer class="app-footer">
      <p>&copy; {{ year }} Operation Dashboard Hub. All rights reserved.</p>
    </footer>
  </div>
</template>

<script>
import Logout from '../views/Logout';

export default {
  name: "HomeDashboard",
  data() {
    return { year: new Date().getFullYear() };
  },
  computed: {
    username() {
      return localStorage.getItem("username") || "Guest User";
    },
    userInitial() {
      const n = this.username?.trim();
      if (!n) return "U";
      const parts = n.split(/\s+/);
      const first = parts[0]?.[0] || "";
      const last = parts.length > 1 ? parts[parts.length - 1]?.[0] || "" : "";
      return (first + last || first).toUpperCase();
    },
  },
  components: {
    Logout,
  },
};
</script>

<style scoped>
/*
|=======================================================================
| สไตล์รวมและตัวแปร (ใช้สีสันสดใสขึ้น)
|=======================================================================
*/
:root {
  --color-bg-start: #e0f2fe; /* ฟ้าอ่อนมาก */
  --color-bg-end: #dcfce7;   /* เขียวอ่อนมาก */
  --color-surface: #ffffff;
  --color-ink: #1f2937; /* สีดำเทาเข้ม */
  --color-ink-sub: #6b7280; /* สีเทาอ่อนลง */
  --color-border: #e5e7eb;

  /* สี Brand ที่มีชีวิตชีวา */
  --color-mfg-prime: #3b82f6; /* ฟ้าสดใส */
  --color-mfg-gradient-start: #2563eb;
  --color-mfg-gradient-end: #3b82f6;

  --color-hr-prime: #8b5cf6; /* ม่วงสดใส */
  --color-hr-gradient-start: #7c3aed;
  --color-hr-gradient-end: #8b5cf6;

  --shadow-sm: 0 1px 3px 0 rgba(0,0,0,0.1), 0 1px 2px -1px rgba(0,0,0,0.1);
  --shadow-md: 0 4px 6px -1px rgba(0,0,0,0.1), 0 2px 4px -2px rgba(0,0,0,0.1);
  --shadow-lg: 0 10px 15px -3px rgba(0,0,0,0.1), 0 4px 6px -4px rgba(0,0,0,0.1);
  --shadow-xl: 0 20px 25px -5px rgba(0,0,0,0.1), 0 8px 10px -6px rgba(0,0,0,0.1);

  --radius: 16px; /* ขอบโค้งมนมากขึ้น */
}

/* พื้นหลังมี Gradient */
.home {
  min-height: 100vh;
  display: grid;
  grid-template-rows: auto 1fr auto;
  background: linear-gradient(135deg, var(--color-bg-start), var(--color-bg-end));
  color: var(--color-ink);
  font-family: 'Poppins', sans-serif; /* ใช้ Font Poppins ที่ดูทันสมัย */
}

/* ===== App bar (มี Gradient ด้านหลัง) ===== */
.appbar {
  position: sticky;
  top: 0;
  z-index: 10;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  padding: 16px 32px; /* เพิ่ม Padding */
  background: linear-gradient(90deg, rgba(255, 255, 255, 0.95), rgba(248, 250, 252, 0.95)); /* Gradient อ่อนๆ */
  backdrop-filter: blur(10px);
  border-bottom: 1px solid var(--color-border);
  box-shadow: var(--shadow-md);
}

.brand {
  display: flex;
  align-items: center;
  gap: 16px; /* เพิ่มระยะห่าง */
}
.logo {
  width: 48px; /* ขยายโลโก้ขึ้นเล็กน้อย */
  height: 48px;
  object-fit: contain;
  filter: drop-shadow(0 4px 8px rgba(0,0,0,0.1)); /* เพิ่มเงาให้โลโก้ */
}
.app-title {
  font-size: 1.4rem; /* ขยายขนาด Title */
  font-weight: 700; /* ใช้ Font Weight ที่ชัดเจน */
  margin: 0;
  color: var(--color-ink);
}
.app-subtitle {
  font-size: 0.9rem;
  color: var(--color-ink-sub);
  margin: 0;
}

.actions {
  display: flex;
  align-items: center;
  gap: 18px; /* เพิ่มระยะห่าง */
}

/* กล่องผู้ใช้งาน (User Box) */
.user-box {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 8px 16px; /* เพิ่ม Padding */
  border-radius: 9999px;
  background-color: var(--color-surface);
  border: 1px solid var(--color-border);
  box-shadow: var(--shadow-sm); /* เพิ่มเงาเล็กน้อย */
}
.user-initial {
  width: 36px; /* ขยายขนาด Initial */
  height: 36px;
  border-radius: 50%;
  background-image: linear-gradient(45deg, var(--color-hr-gradient-start), var(--color-hr-gradient-end)); /* Gradient */
  color: #fff;
  display: grid;
  place-items: center;
  font-weight: 700;
  font-size: 1rem;
  flex-shrink: 0;
  box-shadow: var(--shadow-sm);
}
.user-name {
  font-weight: 600;
  font-size: 0.95rem;
  color: var(--color-ink);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  max-width: 150px; /* เพิ่มความกว้างสูงสุด */
}

/* ปุ่ม Logout (ใช้ :deep เพื่อเข้าถึง) */
:deep(.btn-out) {
  appearance: none;
  border: none;
  background: var(--color-ink); /* เปลี่ยนเป็นสี Ink */
  color: #fff;
  padding: 10px 20px; /* เพิ่มขนาด */
  border-radius: 10px;
  font-weight: 600;
  cursor: pointer;
  transition: all .2s ease;
  box-shadow: var(--shadow-md);
}
:deep(.btn-out:hover) {
  background: #334155; /* สีเข้มขึ้นเมื่อ Hover */
  transform: translateY(-2px);
  box-shadow: var(--shadow-lg);
}

/* ===== Main Content Layout ===== */
.container {
  max-width: 1120px; /* ขยายความกว้างสูงสุด */
  margin: 60px auto 80px; /* เพิ่ม Margin */
  padding: 0 32px;
}

.section-head { margin-bottom: 40px; } /* เพิ่มระยะห่าง */
.section-title {
  font-size: 2.2rem; /* ขยายขนาด Title */
  margin: 0 0 6px;
  font-weight: 800;
  color: var(--color-ink);
  text-align: center; /* จัดกลาง */
}
.section-desc {
    font-size: 1.1rem;
    color: var(--color-ink-sub);
    margin: 0;
    text-align: center; /* จัดกลาง */
    max-width: 700px;
    margin-left: auto;
    margin-right: auto;
}

/* ===== Cards Container & Layout ===== */
.cards {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(350px, 1fr)); /* ปรับขนาดขั้นต่ำของการ์ด */
  gap: 32px; /* เพิ่มระยะห่างระหว่างการ์ด */
  grid-auto-rows: minmax(200px, auto); /* กำหนดความสูงต่ำสุด */
}

/* การ์ด (Base Style) */
.card {
  display: flex;
  flex-direction: column;
  border-radius: var(--radius);
  padding: 28px; /* เพิ่ม Padding */
  text-decoration: none;
  color: var(--color-ink);
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  box-shadow: var(--shadow-lg); /* ใช้เงาที่ใหญ่ขึ้น */
  transition: all .3s ease;
  outline: none;
  position: relative;
  overflow: hidden;
  will-change: transform, box-shadow; /* Optimize for animation */
}

/* Hover Effect ที่ปรับปรุงแล้ว */
.card:hover {
  transform: translateY(-8px); /* ยกขึ้นสูงขึ้น */
  box-shadow: var(--shadow-xl); /* เงาเข้มขึ้น */
  border-color: transparent; /* ซ่อน border เดิม */
}

.card-head {
  display: flex;
  align-items: center;
  gap: 18px; /* เพิ่มระยะห่าง */
  margin-bottom: 16px;
}

.card-icon {
  width: 64px; /* ขยาย Icon */
  height: 64px;
  border-radius: 50%;
  display: grid;
  place-items: center;
  font-size: 2rem; /* ขยายขนาด Icon */
  color: #fff;
  box-shadow: var(--shadow-md);
  flex-shrink: 0;
}

.card-title {
  font-size: 1.5rem; /* ขยายขนาด Title */
  font-weight: 700;
  margin: 0;
}

.card-desc {
  margin: 0;
  color: var(--color-ink-sub);
  line-height: 1.6;
  font-size: 1rem;
  flex-grow: 1;
}

.card-arrow {
    position: absolute;
    bottom: 24px;
    right: 28px;
    font-size: 2rem;
    color: var(--color-ink-sub);
    opacity: 0.4;
    transition: opacity .3s ease, transform .3s ease;
}

.card:hover .card-arrow {
    opacity: 1;
    transform: translateX(8px);
    color: var(--color-ink);
}


/* ===== สีเฉพาะสำหรับ Card แต่ละประเภท ===== */

/* MFG Card */
.card-mfg .card-icon {
  background-image: linear-gradient(45deg, var(--color-mfg-gradient-start), var(--color-mfg-gradient-end));
  box-shadow: 0 6px 16px rgba(59, 130, 246, 0.3);
}
.card-mfg:hover {
    background: linear-gradient(135deg, var(--color-surface) 60%, var(--color-mfg-light) 100%); /* มี Gradient ที่อ่อนกว่าเมื่อ Hover */
    border: 1px solid var(--color-mfg-prime);
    color: var(--color-ink);
}
.card-mfg:hover .card-title {
    color: var(--color-mfg-prime);
}
.card-mfg:hover .card-arrow {
    color: var(--color-mfg-prime);
}

/* HR Card */
.card-hr .card-icon {
  background-image: linear-gradient(45deg, var(--color-hr-gradient-start), var(--color-hr-gradient-end));
  box-shadow: 0 6px 16px rgba(139, 92, 246, 0.3);
}
.card-hr:hover {
    background: linear-gradient(135deg, var(--color-surface) 60%, var(--color-hr-light) 100%);
    border: 1px solid var(--color-hr-prime);
    color: var(--color-ink);
}
.card-hr:hover .card-title {
    color: var(--color-hr-prime);
}
.card-hr:hover .card-arrow {
    color: var(--color-hr-prime);
}


/* ===== Footer ===== */
.app-footer {
    padding: 20px 32px;
    text-align: center;
    font-size: 0.9rem;
    color: var(--color-ink-sub);
    border-top: 1px solid var(--color-border);
    background-color: var(--color-surface);
    box-shadow: var(--shadow-md);
}

/* ===== Responsive ===== */
@media (max-width: 768px) {
  .appbar {
    flex-direction: column;
    align-items: flex-start;
    gap: 12px;
    padding: 12px 20px;
  }
  .brand-text {
    flex-grow: 1; /* ให้ยืดพื้นที่ */
  }
  .actions {
    width: 100%;
    justify-content: space-between;
    padding-top: 10px;
    border-top: 1px solid var(--color-border);
  }
  .user-box {
    order: 2; /* ย้ายไปอยู่หลังปุ่ม Logout */
  }

  .container {
    padding: 0 20px;
    margin-top: 40px;
    margin-bottom: 60px;
  }
  .section-title {
    font-size: 1.8rem;
  }
  .section-desc {
    font-size: 0.95rem;
  }
  .cards {
    grid-template-columns: 1fr;
    gap: 24px;
  }
  .card {
    padding: 20px;
  }
  .card-title {
    font-size: 1.3rem;
  }
  .card-icon {
      width: 56px;
      height: 56px;
      font-size: 1.8rem;
  }
  .card-arrow {
      bottom: 20px;
      right: 20px;
      font-size: 1.8rem;
  }
}

@media (max-width: 480px) {
    .app-title { font-size: 1.1rem; }
    .app-subtitle { display: none; }
    .user-name { display: none; }
    .logo { width: 40px; height: 40px; }
    :deep(.btn-out) { padding: 8px 15px; }
    .section-title { font-size: 1.5rem; }
    .section-desc { font-size: 0.85rem; }
}
</style>
