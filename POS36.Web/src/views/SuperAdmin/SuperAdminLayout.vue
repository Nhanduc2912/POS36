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
        <router-link
          v-for="item in menuItems" :key="item.path"
          :to="'/super-admin/' + item.path"
          class="sa-nav-item" :class="{ active: currentRoute === item.path }"
        >
          <i :class="'bi bi-' + item.icon"></i>
          <span v-if="!sidebarCollapsed">{{ item.label }}</span>
          <span v-if="item.badge && !sidebarCollapsed" class="sa-badge">{{ item.badge }}</span>
        </router-link>
      </nav>

      <!-- AI Terminal Toggle in sidebar -->
      <div class="sa-ai-btn-wrap" v-if="!sidebarCollapsed">
        <button class="sa-ai-btn" @click="toggleTerminal"
          :class="{ active: terminalVisible }">
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
        <!-- Left panel AI (left mode) -->
        <div v-if="terminalVisible && terminalMode === 'left'" class="sa-left-pane">
          <AITerminal @close="terminalVisible = false" @modeChange="onModeChange" />
        </div>

        <!-- Top: Router View (hidden in fullpage mode) -->
        <div class="sa-content-pane" v-show="terminalMode !== 'fullpage'" :style="topPaneStyle">
          <div class="sa-content"><router-view /></div>
        </div>

        <!-- Resizer (bottom mode) -->
        <div v-if="terminalVisible && terminalMode === 'bottom'" class="sa-resizer"
          @mousedown="startResize" :class="{ resizing: isResizing }">
          <div class="resizer-handle"><span></span><span></span><span></span></div>
        </div>

        <!-- Bottom panel -->
        <div v-if="terminalVisible && terminalMode === 'bottom'" class="sa-terminal-pane"
          :style="{ height: terminalHeight + 'px' }">
          <AITerminal @close="terminalVisible = false" @modeChange="onModeChange" />
        </div>

        <!-- Fullpage mode -->
        <div v-if="terminalVisible && terminalMode === 'fullpage'" class="sa-content-pane" style="height:100%">
          <AITerminal @close="terminalVisible = false" @modeChange="onModeChange" />
        </div>

        <!-- Window mode: handled by Teleport inside AITerminal -->
        <AITerminal v-if="terminalVisible && terminalMode === 'window'"
          @close="terminalVisible = false" @modeChange="onModeChange" />
      </div>
    </main>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import AITerminal from '../../components/AITerminal.vue';

const route = useRoute();
const router = useRouter();
const sidebarCollapsed = ref(false);
const isDark = ref(true);

// ===== SPLIT PANEL =====
const splitEl = ref(null);
const terminalVisible = ref(false);
const terminalMode = ref('bottom'); // bottom | left | window | fullpage
const terminalHeight = ref(260);
const isResizing = ref(false);
const MIN_TERMINAL = 140;
const MAX_TERMINAL = 600;
const DEFAULT_TERMINAL = 260;

const topPaneStyle = computed(() => {
  if (terminalMode.value === 'left') return { flex: '1', overflow: 'auto' };
  if (!splitEl.value) return {};
  const total = splitEl.value.clientHeight;
  const h = terminalVisible.value && terminalMode.value === 'bottom'
    ? total - terminalHeight.value - 6 : total;
  return { height: h + 'px' };
});

// Keep old topHeight for backward compat
const topHeight = computed(() => topPaneStyle.value?.height?.replace('px',''));

const onModeChange = (m) => {
  terminalMode.value = m;
  localStorage.setItem('pos36_ai_mode', m);
};

const toggleTerminal = () => {
  terminalVisible.value = !terminalVisible.value;
  localStorage.setItem('pos36_terminal_visible', terminalVisible.value ? '1' : '0');
};

// Resize drag logic
let startY = 0;
let startH = 0;

const startResize = (e) => {
  isResizing.value = true;
  startY = e.clientY;
  startH = terminalHeight.value;
  document.addEventListener('mousemove', onResize);
  document.addEventListener('mouseup', stopResize);
  document.body.style.userSelect = 'none';
  document.body.style.cursor = 'ns-resize';
};

const onResize = (e) => {
  if (!isResizing.value) return;
  const delta = startY - e.clientY; // dragging up → terminal grows
  const newH = Math.min(MAX_TERMINAL, Math.max(MIN_TERMINAL, startH + delta));
  terminalHeight.value = newH;
  localStorage.setItem('pos36_terminal_h', String(newH));
};

const stopResize = () => {
  isResizing.value = false;
  document.removeEventListener('mousemove', onResize);
  document.removeEventListener('mouseup', stopResize);
  document.body.style.userSelect = '';
  document.body.style.cursor = '';
};

// Ctrl+` keyboard shortcut
const handleKeydown = (e) => {
  if (e.ctrlKey && e.key === '`') {
    e.preventDefault();
    toggleTerminal();
  }
};

// ===== THEME =====
const toggleTheme = () => {
  isDark.value = !isDark.value;
  localStorage.setItem('pos36_sa_theme', isDark.value ? 'dark' : 'light');
};

// ===== MENU =====
const menuItems = ref([
  { path: '', icon: 'speedometer2', label: 'Dashboard', badge: null },
  { path: 'stores', icon: 'shop', label: 'Cửa hàng', badge: null },
  { path: 'subscriptions', icon: 'credit-card-2-front', label: 'Đơn đăng ký', badge: null },
  { path: 'plans', icon: 'box-seam', label: 'Gói dịch vụ', badge: null },
  { path: 'analytics', icon: 'graph-up-arrow', label: 'Thống kê', badge: null },
  { path: 'notifications', icon: 'bell', label: 'Thông báo', badge: null },
  { path: 'config', icon: 'gear', label: 'Cấu hình', badge: null },
]);

const currentRoute = computed(() => {
  const path = route.path.replace('/super-admin/', '').replace('/super-admin', '');
  return path || '';
});

const currentTitle = computed(() => {
  const item = menuItems.value.find(m => m.path === currentRoute.value);
  return item ? item.label : 'Super Admin';
});

const currentDate = computed(() =>
  new Date().toLocaleDateString('vi-VN', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' })
);

const logout = () => { localStorage.clear(); window.location.href = '/login'; };

onMounted(() => {
  const savedTheme = localStorage.getItem('pos36_sa_theme');
  isDark.value = savedTheme !== 'light';
  const savedTerm = localStorage.getItem('pos36_terminal_visible');
  if (savedTerm === '1') terminalVisible.value = true;
  const savedH = localStorage.getItem('pos36_terminal_h');
  if (savedH) terminalHeight.value = parseInt(savedH) || DEFAULT_TERMINAL;
  window.addEventListener('keydown', handleKeydown);
});

onUnmounted(() => {
  window.removeEventListener('keydown', handleKeydown);
  document.removeEventListener('mousemove', onResize);
  document.removeEventListener('mouseup', stopResize);
});
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
.sa-nav-item {
  display: flex; align-items: center; gap: 12px;
  padding: 11px 16px; border-radius: 10px;
  color: var(--sa-text-muted); text-decoration: none;
  font-weight: 500; font-size: .9rem; margin-bottom: 4px; transition: all .2s;
}
.sa-nav-item:hover { background: var(--sa-nav-hover-bg); color: var(--sa-accent); }
.sa-nav-item.active { background: var(--sa-nav-active-bg); color: var(--sa-accent); font-weight: 600; }
.sa-nav-item i { font-size: 1.15rem; width: 22px; text-align: center; }

.sa-badge { margin-left: auto; background: #ef4444; color: #fff; font-size: .7rem; padding: 2px 8px; border-radius: 99px; font-weight: 700; }

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
.sa-main {
  flex: 1; display: flex; flex-direction: column;
  min-width: 0; overflow: hidden;
}

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
.sa-split {
  flex: 1; display: flex; flex-direction: column;
  overflow: hidden; position: relative;
}

.sa-content-pane { overflow-y: auto; flex-shrink: 0; }
.sa-content { padding: 24px 28px; }

/* Resizer */
.sa-resizer {
  height: 6px; background: var(--sa-border);
  cursor: ns-resize; flex-shrink: 0;
  display: flex; align-items: center; justify-content: center;
  transition: background .15s;
}
.sa-resizer:hover, .sa-resizer.resizing { background: var(--sa-accent); }
.resizer-handle { display: flex; gap: 3px; }
.resizer-handle span {
  width: 20px; height: 2px;
  background: currentColor; opacity: .4; border-radius: 1px;
}
.sa-resizer:hover .resizer-handle span { opacity: .8; }

.sa-terminal-pane { flex-shrink: 0; overflow: hidden; }

/* Left panel mode */
.sa-split:has(.sa-left-pane) { flex-direction: row; }
.sa-left-pane { flex-shrink: 0; overflow: hidden; border-right: 1px solid var(--sa-border); }
</style>
