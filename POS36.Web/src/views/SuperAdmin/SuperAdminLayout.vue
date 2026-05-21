<template>
  <div class="super-admin-app" :class="isDark ? 'theme-dark' : 'theme-light'">
    <!-- Sidebar -->
    <aside class="sa-sidebar" :class="{ collapsed: sidebarCollapsed }">
      <div class="sa-logo">
        <h2 v-if="!sidebarCollapsed">POS36</h2>
        <span v-if="!sidebarCollapsed" class="sa-logo-sub">Super Admin</span>
        <h2 v-else>P</h2>
      </div>

      <nav class="sa-nav">
        <template v-for="item in menuItems" :key="item.path ?? item.label">
          <!-- Group item (dropdown) -->
          <div v-if="item.isGroup" class="sa-nav-group">
            <button
              class="sa-nav-item sa-nav-group-btn"
              :class="{ active: isGroupActive(item) }"
              @click="toggleGroup(item.label)"
              :title="sidebarCollapsed ? item.label : ''"
            >
              <i :class="'bi bi-' + item.icon"></i>
              <span v-if="!sidebarCollapsed">{{ item.label }}</span>
              <i v-if="!sidebarCollapsed"
                :class="expandedGroups[item.label] ? 'bi bi-chevron-down' : 'bi bi-chevron-right'"
                class="sa-group-arrow ms-auto"></i>
            </button>

            <!-- Dropdown children -->
            <transition name="dropdown">
              <div
                v-if="expandedGroups[item.label] && !sidebarCollapsed"
                class="sa-nav-children"
              >
                <button
                  v-for="child in item.children"
                  :key="child.tab"
                  class="sa-nav-child"
                  :class="{ active: isChildActive(child) }"
                  @click="navigateChild(child)"
                >
                  <i :class="'bi bi-' + child.icon"></i>
                  {{ child.label }}
                </button>
              </div>
            </transition>
          </div>

          <!-- Regular nav item -->
          <router-link
            v-else
            :to="'/super-admin/' + item.path"
            class="sa-nav-item"
            :class="{ active: currentRoute === item.path }"
            :title="sidebarCollapsed ? item.label : ''"
          >
            <i :class="'bi bi-' + item.icon"></i>
            <span v-if="!sidebarCollapsed">{{ item.label }}</span>
            <span v-if="item.badge && !sidebarCollapsed" class="sa-badge">{{ item.badge }}</span>
          </router-link>
        </template>
      </nav>

      <!-- AI Terminal Toggle in sidebar -->
      <div class="sa-ai-btn-wrap" v-if="!sidebarCollapsed">
        <button class="sa-ai-btn" @click="toggleTerminal" :class="{ active: terminalVisible }">
          <i class="bi bi-terminal-fill me-2"></i>
          AI Terminal
          <span class="ai-dot" :class="{ pulse: terminalVisible }"></span>
        </button>
      </div>
      <div class="sa-ai-btn-wrap" v-else>
        <button class="sa-ai-btn sa-ai-btn-icon" @click="toggleTerminal" :class="{ active: terminalVisible }" title="AI Terminal">
          <i class="bi bi-terminal-fill"></i>
        </button>
      </div>

      <div class="sa-sidebar-footer">
        <button class="sa-toggle-btn" @click="sidebarCollapsed = !sidebarCollapsed">
          <i :class="sidebarCollapsed ? 'bi bi-chevron-right' : 'bi bi-chevron-left'"></i>
        </button>
      </div>
    </aside>

    <!-- Main Content Area -->
    <main class="sa-main" :class="{ 'sidebar-collapsed': sidebarCollapsed }">
      <!-- Header -->
      <header class="sa-header">
        <div class="sa-header-left">
          <h4 class="fw-bold mb-0 sa-title">{{ currentTitle }}</h4>
          <span class="sa-subtitle small">{{ currentDate }}</span>
        </div>
        <div class="sa-header-right">
          <button class="sa-theme-btn" @click="toggleTheme" :title="isDark ? 'Chuyển sáng' : 'Chuyển tối'">
            <i :class="isDark ? 'bi bi-sun-fill' : 'bi bi-moon-stars-fill'"></i>
            <span v-if="!sidebarCollapsed">{{ isDark ? 'Sáng' : 'Tối' }}</span>
          </button>
          <span class="term-hint ms-3" v-if="!sidebarCollapsed">
            <kbd>Ctrl</kbd>+<kbd>`</kbd> AI
          </span>
          <span class="sa-admin-name ms-3">
            <i class="bi bi-shield-lock-fill text-warning me-1"></i>SuperAdmin
          </span>
          <button class="btn btn-sm btn-outline-danger ms-3" @click="logout">
            <i class="bi bi-box-arrow-right me-1"></i> Đăng xuất
          </button>
        </div>
      </header>

      <!-- Split Panel Container -->
      <div class="sa-split" ref="splitEl">
        <div v-if="terminalVisible && terminalMode === 'left'" class="sa-left-pane">
          <AITerminal @close="terminalVisible = false" @modeChange="onModeChange" />
        </div>

        <div class="sa-content-pane" v-show="terminalMode !== 'fullpage'" :style="topPaneStyle">
          <div class="sa-content"><router-view /></div>
        </div>

        <div v-if="terminalVisible && terminalMode === 'right'" class="sa-right-pane">
          <AITerminal @close="terminalVisible = false" @modeChange="onModeChange" />
        </div>

        <div v-if="terminalVisible && terminalMode === 'bottom'" class="sa-terminal-pane">
          <AITerminal @close="terminalVisible = false" @modeChange="onModeChange" />
        </div>

        <div v-if="terminalVisible && terminalMode === 'fullpage'" class="sa-content-pane" style="height:100%">
          <AITerminal @close="terminalVisible = false" @modeChange="onModeChange" />
        </div>

        <AITerminal v-if="terminalVisible && terminalMode === 'window'"
          @close="terminalVisible = false" @modeChange="onModeChange" />
      </div>
    </main>

    <!-- AI Navigation Confirmation Dialog -->
    <Teleport to="body">
      <div class="sa-nav-confirm-overlay" v-if="navConfirm" @click.self="denyNavigation">
        <div class="sa-nav-confirm-modal">
          <div class="sa-nav-confirm-icon">
            <i class="bi bi-robot"></i>
          </div>
          <h5 class="sa-nav-confirm-title">AI đã tạo báo cáo!</h5>
          <p class="sa-nav-confirm-desc">
            Báo cáo đã được tạo thành công. Bạn có muốn AI chuyển bạn sang
            <strong>{{ navConfirm.label }}</strong> ngay bây giờ không?
          </p>
          <div class="sa-nav-confirm-prompt" v-if="navConfirm.prompt">
            <i class="bi bi-chat-dots me-2"></i>
            <em>{{ navConfirm.prompt }}</em>
          </div>
          <div class="sa-nav-confirm-btns">
            <button class="sa-nav-btn-yes" @click="acceptNavigation">
              <i class="bi bi-arrow-right-circle-fill me-2"></i>Có, chuyển trang
            </button>
            <button class="sa-nav-btn-no" @click="denyNavigation">
              <i class="bi bi-x-circle me-2"></i>Không, ở lại đây
            </button>
          </div>
        </div>
      </div>
    </Teleport>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import AITerminal from '../../components/AITerminal.vue';
import { useAIChat } from '../../composables/useAIChat.js';

const route = useRoute();
const router = useRouter();
const sidebarCollapsed = ref(false);
const isDark = ref(true);

// AI Chat - watch for report navigation requests
const { pendingNavigation, clearPendingNavigation } = useAIChat();
const navConfirm = ref(null);

watch(pendingNavigation, (nav) => {
  if (nav) {
    navConfirm.value = nav;
    clearPendingNavigation();
  }
});

// ===== SPLIT PANEL =====
const splitEl = ref(null);
const terminalVisible = ref(false);
const terminalMode = ref('bottom');
const topPaneStyle = computed(() => {
  if (terminalMode.value === 'left' || terminalMode.value === 'right') return { flex: '1', overflow: 'auto' };
  return { flex: '1', overflow: 'auto' };
});

const onModeChange = (m) => {
  terminalMode.value = m;
  localStorage.setItem('pos36_ai_mode', m);
};

const toggleTerminal = () => {
  terminalVisible.value = !terminalVisible.value;
  localStorage.setItem('pos36_terminal_visible', terminalVisible.value ? '1' : '0');
};

const handleKeydown = (e) => {
  if (e.ctrlKey && e.key === '`') { e.preventDefault(); toggleTerminal(); }
};

// ===== THEME =====
const toggleTheme = () => {
  isDark.value = !isDark.value;
  localStorage.setItem('pos36_sa_theme', isDark.value ? 'dark' : 'light');
};

// ===== MENU WITH GROUP SUPPORT =====
const expandedGroups = ref({ 'Cấu hình': false });

const menuItems = ref([
  { path: '',              icon: 'speedometer2',       label: 'Dashboard' },
  { path: 'stores',        icon: 'shop',               label: 'Cửa hàng' },
  { path: 'subscriptions', icon: 'credit-card-2-front', label: 'Đơn đăng ký' },
  { path: 'plans',         icon: 'box-seam',           label: 'Gói dịch vụ' },
  { path: 'analytics',     icon: 'graph-up-arrow',     label: 'Thống kê' },
  { path: 'ai-report',     icon: 'robot',              label: 'Báo Cáo AI' },
  { path: 'notifications', icon: 'bell',               label: 'Thông báo' },
  {
    icon: 'gear', label: 'Cấu hình', isGroup: true,
    children: [
      { tab: 'general',  icon: 'sliders',      label: 'Cấu hình chung' },
      { tab: 'logs',     icon: 'journal-text', label: 'Nhật ký hệ thống' },
      { tab: 'payment',  icon: 'credit-card',  label: 'Thanh toán & Webhook' },
      { tab: 'email',    icon: 'envelope',     label: 'Email & SMTP' },
    ]
  },
]);

const toggleGroup = (label) => {
  expandedGroups.value[label] = !expandedGroups.value[label];
};

const isGroupActive = (item) =>
  item.children?.some(c => route.path.includes('config') && (route.query.tab === c.tab || (!route.query.tab && c.tab === 'general')));

const isChildActive = (child) =>
  route.path.includes('config') && (route.query.tab === child.tab || (!route.query.tab && child.tab === 'general'));

const navigateChild = (child) => {
  router.push({ path: '/super-admin/config', query: { tab: child.tab } });
};

// Auto-expand Cấu hình group when on config page
watch(() => route.path, (path) => {
  if (path.includes('config')) expandedGroups.value['Cấu hình'] = true;
}, { immediate: true });

const currentRoute = computed(() => {
  return route.path.replace('/super-admin/', '').replace('/super-admin', '') || '';
});

const currentTitle = computed(() => {
  // Check config sub-items
  if (currentRoute.value.startsWith('config')) {
    const tab = route.query.tab;
    const group = menuItems.value.find(m => m.isGroup && m.label === 'Cấu hình');
    const child = group?.children?.find(c => c.tab === (tab || 'general'));
    return child ? `Cấu hình — ${child.label}` : 'Cấu hình hệ thống';
  }
  const item = menuItems.value.find(m => !m.isGroup && m.path === currentRoute.value);
  return item ? item.label : 'Super Admin';
});

const currentDate = computed(() =>
  new Date().toLocaleDateString('vi-VN', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' })
);

const logout = () => { localStorage.clear(); window.location.href = '/login'; };

const acceptNavigation = () => {
  const nav = navConfirm.value;
  navConfirm.value = null;
  if (nav?.route) router.push(nav.route);
};
const denyNavigation = () => { navConfirm.value = null; };

onMounted(() => {
  const savedTheme = localStorage.getItem('pos36_sa_theme');
  isDark.value = savedTheme !== 'light';
  const savedTerm = localStorage.getItem('pos36_terminal_visible');
  if (savedTerm === '1') terminalVisible.value = true;
  const savedMode = localStorage.getItem('pos36_ai_mode');
  if (savedMode) terminalMode.value = savedMode;
  window.addEventListener('keydown', handleKeydown);
});

onUnmounted(() => { window.removeEventListener('keydown', handleKeydown); });
</script>

<style scoped>
@import url("https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800&display=swap");

/* ===== THEME VARIABLES ===== */
.theme-dark {
  --sa-bg: #0f1117; --sa-surface: #1a1c23; --sa-surface-2: #16181d;
  --sa-border: rgba(255,255,255,0.06); --sa-text: #e4e4e7;
  --sa-text-muted: #9ca3af; --sa-text-faint: #6b7280; --sa-accent: #f59e0b;
  --sa-nav-active-bg: rgba(245,158,11,0.15); --sa-nav-hover-bg: rgba(245,158,11,0.08);
  --sa-header-bg: rgba(22,24,29,0.85); --sa-shadow: rgba(0,0,0,0.4);
}
.theme-light {
  --sa-bg: #f4f6fb; --sa-surface: #fff; --sa-surface-2: #f8fafc;
  --sa-border: rgba(0,0,0,0.08); --sa-text: #1a1c23;
  --sa-text-muted: #4b5563; --sa-text-faint: #9ca3af; --sa-accent: #d97706;
  --sa-nav-active-bg: rgba(217,119,6,0.12); --sa-nav-hover-bg: rgba(217,119,6,0.06);
  --sa-header-bg: rgba(255,255,255,0.92); --sa-shadow: rgba(0,0,0,0.08);
}

.super-admin-app {
  display: flex; height: 100vh; overflow: hidden;
  font-family: 'Inter', sans-serif;
  background: var(--sa-bg); color: var(--sa-text);
  transition: background 0.3s, color 0.3s;
}

/* ===== SIDEBAR ===== */
.sa-sidebar {
  width: 260px; background: var(--sa-surface-2);
  border-right: 1px solid var(--sa-border);
  display: flex; flex-direction: column;
  transition: width 0.3s ease; flex-shrink: 0;
  position: relative; z-index: 100;
  box-shadow: 2px 0 12px var(--sa-shadow);
}
.sa-sidebar.collapsed { width: 70px; }

.sa-logo { padding: 24px 20px 20px; border-bottom: 1px solid var(--sa-border); }
.sa-logo h2 { color: var(--sa-accent); font-weight: 800; font-size: 1.5rem; margin: 0; }
.sa-logo-sub { font-size: .7rem; color: var(--sa-text-faint); text-transform: uppercase; letter-spacing: 2px; }

.sa-nav { flex: 1; padding: 12px 10px; overflow-y: auto; }

/* Regular nav item */
.sa-nav-item {
  display: flex; align-items: center; gap: 12px;
  padding: 11px 16px; border-radius: 10px;
  color: var(--sa-text-muted); text-decoration: none;
  font-weight: 500; font-size: .9rem; margin-bottom: 4px; transition: all .2s;
  width: 100%; border: none; background: none; cursor: pointer; text-align: left;
}
.sa-nav-item:hover { background: var(--sa-nav-hover-bg); color: var(--sa-accent); }
.sa-nav-item.active { background: var(--sa-nav-active-bg); color: var(--sa-accent); font-weight: 600; }
.sa-nav-item i { font-size: 1.15rem; width: 22px; text-align: center; flex-shrink: 0; }
.sa-badge { margin-left: auto; background: #ef4444; color: #fff; font-size: .7rem; padding: 2px 8px; border-radius: 99px; font-weight: 700; }

/* Group & dropdown */
.sa-nav-group { margin-bottom: 2px; }
.sa-nav-group-btn { margin-bottom: 0 !important; }
.sa-group-arrow { font-size: .75rem; transition: transform .25s; flex-shrink: 0; }

.sa-nav-children {
  padding: 4px 0 6px 14px;
  overflow: hidden;
}
.sa-nav-child {
  display: flex; align-items: center; gap: 10px;
  width: 100%; padding: 9px 14px; border-radius: 8px;
  background: none; border: none; border-left: 2px solid var(--sa-border);
  color: var(--sa-text-faint); font-size: .85rem; font-weight: 500;
  cursor: pointer; transition: all .15s; text-align: left;
  margin-bottom: 2px;
}
.sa-nav-child:hover { background: var(--sa-nav-hover-bg); color: var(--sa-text); border-left-color: var(--sa-text-faint); }
.sa-nav-child.active {
  background: var(--sa-nav-active-bg); color: var(--sa-accent);
  border-left-color: var(--sa-accent); font-weight: 700;
}
.sa-nav-child i { font-size: .95rem; width: 18px; text-align: center; flex-shrink: 0; }

/* Dropdown animation */
.dropdown-enter-active, .dropdown-leave-active { transition: all .25s ease; max-height: 300px; overflow: hidden; }
.dropdown-enter-from, .dropdown-leave-to { max-height: 0; opacity: 0; transform: translateY(-6px); }

/* AI Terminal Button */
.sa-ai-btn-wrap { padding: 8px 10px; }
.sa-ai-btn {
  width: 100%; display: flex; align-items: center;
  background: rgba(245,158,11,.08); border: 1px solid rgba(245,158,11,.25);
  color: var(--sa-accent); padding: 10px 16px; border-radius: 10px;
  font-size: .85rem; font-weight: 700; cursor: pointer;
  transition: all .2s; position: relative; gap: 2px;
}
.sa-ai-btn:hover, .sa-ai-btn.active { background: rgba(245,158,11,.18); border-color: var(--sa-accent); }
.sa-ai-btn-icon { justify-content: center; padding: 10px; }
.ai-dot {
  width: 7px; height: 7px; border-radius: 50%;
  background: #3fb950; margin-left: auto; flex-shrink: 0;
}
.ai-dot.pulse { animation: dotPulse 1.5s infinite; }
@keyframes dotPulse { 0%,100% { opacity:1; transform:scale(1); } 50% { opacity:.5; transform:scale(.7); } }

.sa-sidebar-footer { padding: 12px; border-top: 1px solid var(--sa-border); }
.sa-toggle-btn {
  width: 100%; background: rgba(127,127,127,.08); border: 1px solid var(--sa-border);
  color: var(--sa-text-faint); padding: 8px; border-radius: 8px; cursor: pointer; transition: .2s;
}
.sa-toggle-btn:hover { background: rgba(127,127,127,.15); color: var(--sa-text); }

/* ===== MAIN ===== */
.sa-main { flex: 1; display: flex; flex-direction: column; min-width: 0; overflow: hidden; }

.sa-header {
  display: flex; justify-content: space-between; align-items: center;
  padding: 16px 28px; background: var(--sa-header-bg);
  backdrop-filter: blur(12px); border-bottom: 1px solid var(--sa-border);
  flex-shrink: 0; z-index: 50; transition: background .3s;
}
.sa-header-right { display: flex; align-items: center; }
.sa-title { color: var(--sa-text) !important; }
.sa-subtitle { color: var(--sa-text-muted) !important; }
.sa-admin-name { font-weight: 600; font-size: .9rem; color: var(--sa-text-muted); }

.sa-theme-btn {
  display: flex; align-items: center; gap: 6px;
  background: var(--sa-nav-hover-bg); border: 1px solid var(--sa-border);
  color: var(--sa-text); padding: 6px 14px; border-radius: 20px;
  font-size: .82rem; font-weight: 600; cursor: pointer; transition: all .2s;
}
.sa-theme-btn:hover { background: var(--sa-nav-active-bg); color: var(--sa-accent); }

.term-hint { font-size: .72rem; color: var(--sa-text-faint); }
.term-hint kbd {
  background: var(--sa-surface-2); border: 1px solid var(--sa-border);
  border-radius: 4px; padding: 1px 5px; font-size: .7rem;
}

/* ===== SPLIT PANEL ===== */
.sa-split { flex: 1; display: flex; flex-direction: column; overflow: hidden; position: relative; }
.sa-content-pane {
  background: var(--sa-bg); display: flex; flex-direction: column; flex: 1; overflow-y: auto;
}
.sa-content { flex: 1; padding: 24px 28px; }
.sa-terminal-pane { flex-shrink: 0; display: flex; flex-direction: column; }

.sa-split:has(.sa-left-pane), .sa-split:has(.sa-right-pane) { flex-direction: row; }
.sa-left-pane { flex-shrink: 0; overflow: hidden; border-right: 1px solid var(--sa-border); }
.sa-right-pane { flex-shrink: 0; overflow: hidden; border-left: 1px solid var(--sa-border); }

/* ===== AI NAVIGATION CONFIRM DIALOG ===== */
.sa-nav-confirm-overlay {
  position: fixed; inset: 0; background: rgba(0,0,0,.7);
  backdrop-filter: blur(6px); z-index: 99999;
  display: flex; align-items: center; justify-content: center;
  animation: fadeInOverlay .2s ease;
}
@keyframes fadeInOverlay { from { opacity: 0; } to { opacity: 1; } }

.sa-nav-confirm-modal {
  background: var(--sa-surface); border: 1px solid var(--sa-border);
  border-radius: 20px; padding: 36px; max-width: 460px; width: 90%;
  text-align: center; box-shadow: 0 30px 80px rgba(0,0,0,.5);
  animation: popIn .25s cubic-bezier(.34,1.56,.64,1);
}
@keyframes popIn { from { transform: scale(.85); opacity: 0; } to { transform: scale(1); opacity: 1; } }

.sa-nav-confirm-icon {
  width: 70px; height: 70px; background: rgba(245,158,11,.12);
  border: 2px solid rgba(245,158,11,.3); border-radius: 50%;
  display: flex; align-items: center; justify-content: center;
  font-size: 1.8rem; color: var(--sa-accent); margin: 0 auto 20px;
  animation: robotBounce 2s ease infinite;
}
@keyframes robotBounce { 0%,100% { transform: translateY(0); } 50% { transform: translateY(-6px); } }

.sa-nav-confirm-title { color: var(--sa-text); font-weight: 800; margin-bottom: 10px; }
.sa-nav-confirm-desc { color: var(--sa-text-muted); font-size: .9rem; line-height: 1.6; margin-bottom: 16px; }
.sa-nav-confirm-desc strong { color: var(--sa-accent); }
.sa-nav-confirm-prompt {
  background: rgba(245,158,11,.07); border: 1px solid rgba(245,158,11,.2);
  border-radius: 10px; padding: 10px 14px; font-size: .82rem; color: var(--sa-text-muted);
  text-align: left; margin-bottom: 24px; display: flex; align-items: flex-start; gap: 6px;
}
.sa-nav-confirm-btns { display: flex; gap: 12px; justify-content: center; flex-wrap: wrap; }
.sa-nav-btn-yes {
  display: flex; align-items: center; background: var(--sa-accent); color: #000;
  border: none; padding: 12px 24px; border-radius: 12px;
  font-weight: 800; font-size: .9rem; cursor: pointer; transition: all .2s;
}
.sa-nav-btn-yes:hover { filter: brightness(1.1); transform: translateY(-2px); }
.sa-nav-btn-no {
  display: flex; align-items: center; background: rgba(127,127,127,.1);
  border: 1px solid var(--sa-border); color: var(--sa-text-muted);
  padding: 12px 24px; border-radius: 12px;
  font-weight: 600; font-size: .9rem; cursor: pointer; transition: all .2s;
}
.sa-nav-btn-no:hover { background: rgba(127,127,127,.2); color: var(--sa-text); }
</style>
