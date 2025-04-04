<template>
    <div class="home">
      <div class="dashboard-links">
        <router-link to="/dashboard-mfg" class="dashboard-card mfg">
          <h2>🏭 Dashboard MFG</h2>
        </router-link>
        <router-link to="/dashboard-hr" class="dashboard-card hr">
          <h2>👥 Dashboard HR</h2>
        </router-link>
      </div>
    </div>
  </template>
  
  <script>
  export default {
    name: "HomeDashboard",
  };
  </script>
  
  <style scoped>
  .home {
    text-align: center;
    margin-top: 100px;
    font-family: 'Segoe UI', sans-serif;
  }
  
  .dashboard-links {
    display: flex;
    justify-content: center;
    gap: 40px;
    margin-top: 40px;
  }
  
  .dashboard-card {
    padding: 30px 40px;
    border-radius: 16px;
    text-decoration: none;
    color: white;
    width: 250px;
    transition: all 0.3s ease;
    box-shadow: 0 8px 20px rgba(0,0,0,0.2);
  }
  
  .dashboard-card:hover {
    transform: scale(1.05);
    box-shadow: 0 10px 25px rgba(0,0,0,0.3);
  }
  
  .dashboard-card.mfg {
    background: linear-gradient(135deg, #1e88e5, #42a5f5);
  }
  
  .dashboard-card.hr {
    background: linear-gradient(135deg, #8e24aa, #ba68c8);
  }
  </style>
  