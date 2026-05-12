<template>
  <div class="super-admin-app">
    <!-- Sidebar -->
    <aside class="sa-sidebar" :class="{ collapsed: sidebarCollapsed }">
      <div class="sa-logo">
        <h2 v-if="!sidebarCollapsed">POS36</h2>
        <span v-if="!sidebarCollapsed" class="sa-logo-sub">Super Admin</span>
        <h2 v-else>P</h2>
      </div>

      <nav class="sa-nav">
        <router-link
          v-for="item in menuItems"
          :key="item.path"
          :to="'/super-admin/' + item.path"
          class="sa-nav-item"
          :class="{ active: currentRoute === item.path }"
        >
          <i :class="'bi bi-' + item.icon"></i>
          <span v-if="!sidebarCollapsed">{{ item.label }}</span>
          <span v-if="item.badge && !sidebarCollapsed" class="sa-badge">{{ item.badge }}</span>
        </router-link>
      </nav>

      <div class="sa-sidebar-footer">
        <button class="sa-toggle-btn" @click="sidebarCollapsed = !sidebarCollapsed">
          <i :class="sidebarCollapsed ? 'bi bi-chevron-right' : 'bi bi-chevron-left'"></i>
        </button>
      </div>
    </aside>

    <!-- Main Content -->
    <main class="sa-main">
      <header class="sa-header">
        <div class="sa-header-left">
          <h4 class="fw-bold mb-0">{{ currentTitle }}</h4>
          <span class="text-muted small">{{ currentDate }}</span>
        </div>
        <div class="sa-header-right">
          <span class="sa-admin-name">
            <i class="bi bi-shield-lock-fill text-warning me-1"></i>
            SuperAdmin
          </span>
          <button class="btn btn-sm btn-outline-danger ms-3" @click="logout">
            <i class="bi bi-box-arrow-right me-1"></i> Đăng xuất
          </button>
        </div>
      </header>

      <div class="sa-content">
        <router-view />
      </div>
    </main>
  </div>
</template>

<script setup>
import { ref, computed } from "vue";
import { useRoute, useRouter } from "vue-router";

const route = useRoute();
const router = useRouter();
const sidebarCollapsed = ref(false);

const menuItems = ref([
  { path: "", icon: "speedometer2", label: "Dashboard", badge: null },
  { path: "stores", icon: "shop", label: "Cửa hàng", badge: null },
  { path: "subscriptions", icon: "credit-card-2-front", label: "Đơn đăng ký", badge: null },
  { path: "plans", icon: "box-seam", label: "Gói dịch vụ", badge: null },
  { path: "analytics", icon: "graph-up-arrow", label: "Thống kê", badge: null },
  { path: "notifications", icon: "bell", label: "Thông báo", badge: null },
]);

const currentRoute = computed(() => {
  const path = route.path.replace("/super-admin/", "").replace("/super-admin", "");
  return path || "";
});

const currentTitle = computed(() => {
  const item = menuItems.value.find((m) => m.path === currentRoute.value);
  return item ? item.label : "Super Admin";
});

const currentDate = computed(() => {
  return new Date().toLocaleDateString("vi-VN", {
    weekday: "long", year: "numeric", month: "long", day: "numeric",
  });
});

const logout = () => {
  localStorage.clear();
  window.location.href = "/login";
};
</script>

<style scoped>
@import url("https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800&display=swap");

.super-admin-app {
  display: flex;
  min-height: 100vh;
  font-family: "Inter", sans-serif;
  background: #0f1117;
  color: #e4e4e7;
}

/* Sidebar */
.sa-sidebar {
  width: 260px;
  background: linear-gradient(180deg, #16181d 0%, #1a1c23 100%);
  border-right: 1px solid rgba(255, 255, 255, 0.06);
  display: flex;
  flex-direction: column;
  transition: width 0.3s ease;
  position: fixed;
  top: 0;
  left: 0;
  bottom: 0;
  z-index: 100;
}
.sa-sidebar.collapsed {
  width: 70px;
}
.sa-logo {
  padding: 24px 20px 20px;
  border-bottom: 1px solid rgba(255, 255, 255, 0.06);
}
.sa-logo h2 {
  color: #f59e0b;
  font-weight: 800;
  font-size: 1.5rem;
  margin: 0;
  letter-spacing: -0.5px;
}
.sa-logo-sub {
  font-size: 0.7rem;
  color: #6b7280;
  text-transform: uppercase;
  letter-spacing: 2px;
  font-weight: 600;
}

.sa-nav {
  flex: 1;
  padding: 12px 10px;
  overflow-y: auto;
}
.sa-nav-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 11px 16px;
  border-radius: 10px;
  color: #9ca3af;
  text-decoration: none;
  font-weight: 500;
  font-size: 0.9rem;
  margin-bottom: 4px;
  transition: all 0.2s;
}
.sa-nav-item:hover {
  background: rgba(245, 158, 11, 0.08);
  color: #f59e0b;
}
.sa-nav-item.active {
  background: rgba(245, 158, 11, 0.15);
  color: #f59e0b;
  font-weight: 600;
}
.sa-nav-item i {
  font-size: 1.15rem;
  width: 22px;
  text-align: center;
}
.sa-badge {
  margin-left: auto;
  background: #ef4444;
  color: white;
  font-size: 0.7rem;
  padding: 2px 8px;
  border-radius: 99px;
  font-weight: 700;
}

.sa-sidebar-footer {
  padding: 12px;
  border-top: 1px solid rgba(255, 255, 255, 0.06);
}
.sa-toggle-btn {
  width: 100%;
  background: rgba(255, 255, 255, 0.04);
  border: 1px solid rgba(255, 255, 255, 0.08);
  color: #6b7280;
  padding: 8px;
  border-radius: 8px;
  cursor: pointer;
  transition: 0.2s;
}
.sa-toggle-btn:hover {
  background: rgba(255, 255, 255, 0.08);
  color: #e4e4e7;
}

/* Main */
.sa-main {
  flex: 1;
  margin-left: 260px;
  transition: margin-left 0.3s ease;
  display: flex;
  flex-direction: column;
  min-height: 100vh;
}
.sa-sidebar.collapsed ~ .sa-main {
  margin-left: 70px;
}

.sa-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 28px;
  background: rgba(22, 24, 29, 0.8);
  backdrop-filter: blur(12px);
  border-bottom: 1px solid rgba(255, 255, 255, 0.06);
  position: sticky;
  top: 0;
  z-index: 50;
}
.sa-header-right {
  display: flex;
  align-items: center;
}
.sa-admin-name {
  font-weight: 600;
  font-size: 0.9rem;
  color: #d1d5db;
}

.sa-content {
  flex: 1;
  padding: 24px 28px;
}
</style>
