<template>
  <!-- Teleport to body for window/fullpage modes -->
  <Teleport
    to="body"
    :disabled="layoutMode !== 'window' && layoutMode !== 'fullpage'"
  >
    <div
      class="ai-panel"
      :class="['mode-' + layoutMode, 'theme-' + theme]"
      :style="panelStyle"
      ref="panelEl"
    >
      <!-- TOOLBAR -->
      <div
        class="ai-toolbar"
        @mousedown="layoutMode === 'window' && startDrag($event)"
      >
        <!-- Layout mode buttons -->
        <div class="tb-group">
          <button
            v-for="m in MODES"
            :key="m.key"
            class="tb-icon"
            :class="{ active: layoutMode === m.key }"
            :title="m.label"
            @click="setMode(m.key)"
          >
            <i :class="'bi bi-' + m.icon"></i>
          </button>
        </div>

        <!-- Model selector -->
        <select class="tb-select" v-model="selectedModel" :disabled="loading">
          <option v-for="m in models" :key="m.id" :value="m.id">
            {{ m.displayName }}
          </option>
        </select>

        <!-- Mode toggle -->
        <button
          class="tb-mode-btn"
          :class="mode"
          @click="mode = mode === 'agent' ? 'chat' : 'agent'"
          :title="'Chế độ: ' + mode"
        >
          <i :class="mode === 'agent' ? 'bi bi-robot' : 'bi bi-chat-dots'"></i>
          {{ mode === "agent" ? "Agent" : "Chat" }}
        </button>

        <!-- Theme picker -->
        <div class="tb-group">
          <button
            v-for="(t, key) in THEMES"
            :key="key"
            class="theme-dot"
            :class="{ active: theme === key }"
            :style="{ background: t.accent }"
            :title="key"
            @click="theme = key"
          />
        </div>

        <!-- Clear / Close -->
        <div class="tb-group ms-auto">
          <span v-if="isConnected" class="status-dot online" title="Đã kết nối"
            >●</span
          >
          <span v-else class="status-dot offline" title="Mất kết nối">○</span>
          <button class="tb-icon" @click="clearMessages" title="Xóa lịch sử">
            <i class="bi bi-trash3"></i>
          </button>
          <button class="tb-icon" @click="$emit('close')" title="Đóng">
            <i class="bi bi-x-lg"></i>
          </button>
        </div>
      </div>

      <!-- CHAT AREA -->
      <div class="ai-messages" ref="msgEl">
        <!-- Welcome -->
        <div v-if="messages.length === 0" class="ai-welcome">
          <i class="bi bi-stars fs-2 mb-2" style="color: var(--accent)"></i>
          <p>POS36 AI Assistant</p>
          <small
            >Model: {{ selectedModel }} · Chế độ:
            {{ mode === "agent" ? "Agent (Tools)" : "Chat thuần" }}</small
          >
        </div>

        <div
          v-for="(msg, i) in messages"
          :key="i"
          class="msg-row"
          :class="msg.role"
        >
          <!-- User -->
          <template v-if="msg.role === 'user'">
            <div class="msg-spacer" />
            <div class="msg-bubble user">
              <div class="msg-text" v-text="msg.text" />
              <div class="msg-meta">{{ fmtTime(msg.time) }}</div>
            </div>
          </template>

          <!-- AI -->
          <template v-else-if="msg.role === 'ai'">
            <div class="msg-avatar"><i class="bi bi-robot"></i></div>
            <div class="msg-bubble ai" :class="{ loading: msg.loading }">
              <div
                class="msg-text typing-cursor"
                :class="{ typing: msg.typing }"
                v-html="renderMd(msg.text)"
              />
              <!-- Approval Box -->
              <div
                v-if="msg.approval && !msg.approvalDone"
                class="approval"
                :class="'risk-' + msg.approval.riskLevel.toLowerCase()"
              >
                <div class="appr-header">
                  <span class="risk-badge"
                    >{{ riskEmoji(msg.approval.riskLevel) }}
                    {{ msg.approval.riskLevel }}</span
                  >
                  <code>{{ msg.approval.functionName }}</code>
                </div>
                <textarea 
                  class="appr-args-input" 
                  v-model="msg.approval.functionArgsStr"
                  rows="4"
                ></textarea>
                <div class="appr-btns">
                  <button
                    class="btn-y"
                    @click="doConfirm(msg, true)"
                    :disabled="loading"
                  >
                    ✅ Xác nhận (Y)
                  </button>
                  <button
                    class="btn-n"
                    @click="doConfirm(msg, false)"
                    :disabled="loading"
                  >
                    ❌ Từ chối (N)
                  </button>
                </div>
              </div>
              <!-- Usage stats -->
              <div v-if="msg.usage && !msg.typing" class="msg-stats">
                <span><i class="bi bi-cpu me-1"></i>{{ msg.usage.model }}</span>
                <span>⏱ {{ msg.usage.elapsedMs }}ms</span>
                <span>📥 {{ msg.usage.promptTokens }}t</span>
                <span>📤 {{ msg.usage.responseTokens }}t</span>
                <span>Σ {{ msg.usage.totalTokens }}t</span>
              </div>
              <div class="msg-meta">{{ fmtTime(msg.time) }}</div>
            </div>
          </template>

          <!-- System -->
          <template v-else>
            <div
              class="msg-system"
              v-html="renderMd(msg.text)"
              :class="{ typing: msg.typing }"
            />
          </template>
        </div>
      </div>

      <!-- RESIZE HANDLE -->
      <div
        v-if="layoutMode === 'bottom'"
        class="resize-bar-h"
        @mousedown="startResizeH"
      />
      <div
        v-if="layoutMode === 'left'"
        class="resize-bar-v left-mode"
        @mousedown="startResizeV('left')"
      />
      <div
        v-if="layoutMode === 'right'"
        class="resize-bar-v right-mode"
        @mousedown="startResizeV('right')"
      />
      <div
        v-if="layoutMode === 'window'"
        class="resize-handle-win"
        @mousedown="startResizeWin"
      />

      <!-- INPUT -->
      <div class="ai-input-area">
        <textarea
          ref="inputEl"
          v-model="inputText"
          class="ai-input"
          :placeholder="
            loading
              ? 'Đang xử lý...'
              : 'Gõ lệnh bằng tiếng Việt... (Enter gửi, Shift+Enter xuống dòng)'
          "
          :disabled="loading"
          rows="1"
          @keydown.enter.exact.prevent="sendMsg"
          @input="autoResize"
        />
        <button
          class="ai-send"
          @click="sendMsg"
          :disabled="loading || !inputText.trim()"
        >
          <i class="bi bi-send-fill"></i>
        </button>
      </div>
    </div>
  </Teleport>
</template>

<script setup>
import { ref, computed, watch, nextTick, onMounted, onUnmounted } from "vue";
import { useAIChat } from "../composables/useAIChat.js";

const emit = defineEmits(["close", "modeChange"]);

const {
  messages,
  loading,
  models,
  selectedModel,
  mode,
  theme,
  lastUsage,
  isConnected,
  currentTheme,
  THEMES,
  fetchModels,
  sendMessage,
  confirmAction,
  clearMessages,
} = useAIChat();

const MODES = [
  { key: "bottom", icon: "window-split", label: "Dưới" },
  { key: "left", icon: "layout-sidebar", label: "Trái" },
  { key: "right", icon: "layout-sidebar-reverse", label: "Phải" },
  { key: "window", icon: "pip", label: "Cửa sổ" },
  { key: "fullpage", icon: "fullscreen", label: "Toàn màn" },
];

const layoutMode = ref("bottom");
const panelEl = ref(null);
const msgEl = ref(null);
const inputEl = ref(null);
const inputText = ref("");

// Window drag
const winPos = ref({ x: 80, y: 80 });
const winSize = ref({ w: 520, h: 480 });
let dragging = false,
  dragStart = { mx: 0, my: 0, px: 0, py: 0 };

// Resize state
let resizing = false,
  resizeStart = { y: 0, x: 0, h: 0, w: 0, dir: "" };
const bottomH = ref(300);
const sideW = ref(420);

const panelStyle = computed(() => {
  const t = currentTheme();
  const base = {
    "--bg": t.bg,
    "--surface": t.surface,
    "--text": t.text,
    "--muted": t.muted,
    "--accent": t.accent,
    "--border": t.border,
    "--user-color": t.user,
  };
  if (layoutMode.value === "window") {
    return {
      ...base,
      left: winPos.value.x + "px",
      top: winPos.value.y + "px",
      width: winSize.value.w + "px",
      height: winSize.value.h + "px",
    };
  }
  if (layoutMode.value === "bottom")
    return { ...base, height: bottomH.value + "px" };
  if (layoutMode.value === "left" || layoutMode.value === "right")
    return { ...base, width: sideW.value + "px" };
  return base;
});

const setMode = (m) => {
  layoutMode.value = m;
  emit("modeChange", m);
  localStorage.setItem("pos36_ai_mode", m);
};

// Drag window
const startDrag = (e) => {
  if (layoutMode.value !== "window") return;
  dragging = true;
  dragStart = {
    mx: e.clientX,
    my: e.clientY,
    px: winPos.value.x,
    py: winPos.value.y,
  };
  window.addEventListener("mousemove", onDrag);
  window.addEventListener("mouseup", stopDrag);
};
const onDrag = (e) => {
  if (!dragging) return;
  winPos.value = {
    x: dragStart.px + e.clientX - dragStart.mx,
    y: dragStart.py + e.clientY - dragStart.my,
  };
};
const stopDrag = () => {
  dragging = false;
  window.removeEventListener("mousemove", onDrag);
  window.removeEventListener("mouseup", stopDrag);
};

// Resize bottom
const startResizeH = (e) => {
  resizing = true;
  resizeStart = { y: e.clientY, h: bottomH.value };
  document.body.style.cursor = "ns-resize";
  document.body.style.userSelect = "none";
  window.addEventListener("mousemove", onResizeH);
  window.addEventListener("mouseup", stopResize);
};
const onResizeH = (e) => {
  if (resizing)
    bottomH.value = Math.max(
      160,
      Math.min(700, resizeStart.h - (e.clientY - resizeStart.y)),
    );
};

// Resize side (left or right)
const startResizeV = (dir) => {
  resizing = true;
  resizeStart = { x: event.clientX, w: sideW.value, dir };
  document.body.style.cursor = "ew-resize";
  document.body.style.userSelect = "none";
  window.addEventListener("mousemove", onResizeV);
  window.addEventListener("mouseup", stopResize);
};
const onResizeV = (e) => {
  if (!resizing) return;
  const delta =
    resizeStart.dir === "left"
      ? e.clientX - resizeStart.x
      : resizeStart.x - e.clientX;
  sideW.value = Math.max(280, Math.min(800, resizeStart.w + delta));
};

// Resize window
const startResizeWin = (e) => {
  e.stopPropagation();
  resizing = true;
  resizeStart = {
    x: e.clientX,
    y: e.clientY,
    w: winSize.value.w,
    h: winSize.value.h,
  };
  document.body.style.cursor = "nwse-resize";
  document.body.style.userSelect = "none";
  window.addEventListener("mousemove", onResizeWin);
  window.addEventListener("mouseup", stopResize);
};
const onResizeWin = (e) => {
  if (!resizing) return;
  winSize.value.w = Math.max(320, resizeStart.w + (e.clientX - resizeStart.x));
  winSize.value.h = Math.max(200, resizeStart.h + (e.clientY - resizeStart.y));
};

const stopResize = () => {
  resizing = false;
  document.body.style.cursor = "";
  document.body.style.userSelect = "";
  window.removeEventListener("mousemove", onResizeH);
  window.removeEventListener("mousemove", onResizeV);
  window.removeEventListener("mousemove", onResizeWin);
  window.removeEventListener("mouseup", stopResize);
};

// Send
const sendMsg = async () => {
  const p = inputText.value.trim();
  if (!p) return;
  inputText.value = "";
  await nextTick();
  if (inputEl.value) {
    inputEl.value.style.height = "auto";
  }
  await sendMessage(p);
  await nextTick();
  scrollDown();
};

const doConfirm = async (msg, confirmed) => {
  msg.approvalDone = true;
  await confirmAction(msg.approval, confirmed);
  await nextTick();
  scrollDown();
};

const scrollDown = () => {
  if (msgEl.value) msgEl.value.scrollTop = msgEl.value.scrollHeight;
};

watch(messages, () => nextTick(scrollDown), { deep: true });

// Textarea auto-resize
const autoResize = () => {
  if (!inputEl.value) return;
  inputEl.value.style.height = "auto";
  inputEl.value.style.height = Math.min(inputEl.value.scrollHeight, 120) + "px";
};

// Markdown render (basic)
const renderMd = (text) => {
  if (!text) return '<span class="cursor-blink">▋</span>';
  return text
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;")
    .replace(/\*\*(.+?)\*\*/g, "<strong>$1</strong>")
    .replace(/`([^`]+)`/g, "<code>$1</code>")
    .replace(/\n/g, "<br>");
};

const fmtTime = (d) =>
  d
    ? new Date(d).toLocaleTimeString("vi-VN", {
        hour: "2-digit",
        minute: "2-digit",
      })
    : "";
const fmtArgs = (a) => {
  try {
    return JSON.stringify(JSON.parse(a ?? "{}"), null, 2);
  } catch {
    return a;
  }
};
const riskEmoji = (r) => ({ HIGH: "🔴", MEDIUM: "🟡", LOW: "🟢" })[r] ?? "⚪";

onMounted(() => {
  const saved = localStorage.getItem("pos36_ai_mode");
  if (saved) {
    layoutMode.value = saved;
    emit("modeChange", saved);
  }
  fetchModels();
  nextTick(() => inputEl.value?.focus());
});
onUnmounted(() => {
  stopDrag();
  stopResize();
});
</script>

<style scoped>
.ai-panel {
  --bg: #0d1117;
  --surface: #161b22;
  --text: #c9d1d9;
  --muted: #8b949e;
  --accent: #f59e0b;
  --border: #30363d;
  --user-color: #1f6feb;
  display: flex;
  flex-direction: column;
  background: var(--bg);
  color: var(--text);
  font-family: "Inter", "Segoe UI", sans-serif;
  font-size: 13px;
  border: 1px solid var(--border);
  overflow: hidden;
  transition:
    background 0.3s,
    color 0.3s;
}

/* Layout modes */
.mode-bottom {
  border-top: 1px solid var(--border);
  border-left: none;
  border-right: none;
  border-bottom: none;
}
.mode-left {
  border-right: 1px solid var(--border);
  height: 100%;
  border-top: none;
  border-bottom: none;
  border-left: none;
}
.mode-right {
  border-left: 1px solid var(--border);
  height: 100%;
  border-top: none;
  border-bottom: none;
  border-right: none;
}
.mode-window {
  position: fixed;
  z-index: 9999;
  border-radius: 12px;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.5);
  min-width: 320px;
  min-height: 200px;
}
.mode-fullpage {
  position: fixed;
  inset: 0;
  z-index: 9990;
  border: none;
}

/* Toolbar */
.ai-toolbar {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 6px 10px;
  background: var(--surface);
  border-bottom: 1px solid var(--border);
  flex-shrink: 0;
  cursor: default;
  user-select: none;
  flex-wrap: wrap;
}
.mode-window .ai-toolbar {
  cursor: move;
  border-radius: 12px 12px 0 0;
}

.tb-group {
  display: flex;
  align-items: center;
  gap: 2px;
}
.tb-icon {
  background: none;
  border: none;
  color: var(--muted);
  padding: 4px 7px;
  border-radius: 6px;
  cursor: pointer;
  font-size: 13px;
  transition: 0.15s;
}
.tb-icon:hover,
.tb-icon.active {
  background: rgba(255, 255, 255, 0.08);
  color: var(--accent);
}

.tb-select {
  background: var(--bg);
  border: 1px solid var(--border);
  color: var(--text);
  padding: 3px 8px;
  border-radius: 6px;
  font-size: 12px;
  cursor: pointer;
  max-width: 160px;
  outline: none;
}
.tb-select:focus {
  border-color: var(--accent);
}

.tb-mode-btn {
  display: flex;
  align-items: center;
  gap: 5px;
  font-size: 11px;
  font-weight: 700;
  padding: 4px 10px;
  border-radius: 20px;
  border: 1px solid;
  cursor: pointer;
  transition: 0.2s;
}
.tb-mode-btn.agent {
  color: #3fb950;
  border-color: #3fb950;
  background: rgba(63, 185, 80, 0.1);
}
.tb-mode-btn.chat {
  color: #388bfd;
  border-color: #388bfd;
  background: rgba(56, 139, 253, 0.1);
}

.theme-dot {
  width: 14px;
  height: 14px;
  border-radius: 50%;
  border: 2px solid transparent;
  cursor: pointer;
  transition: 0.15s;
}
.theme-dot.active {
  border-color: var(--text);
  transform: scale(1.25);
}

.status-dot {
  font-size: 11px;
  font-weight: 700;
}
.status-dot.online {
  color: #3fb950;
}
.status-dot.offline {
  color: #f85149;
}

/* Messages */
.ai-messages {
  flex: 1;
  overflow-y: auto;
  padding: 12px;
  display: flex;
  flex-direction: column;
  gap: 10px;
  scrollbar-width: thin;
  scrollbar-color: var(--border) transparent;
}
.ai-welcome {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  opacity: 0.5;
  text-align: center;
  gap: 4px;
}
.ai-welcome p {
  font-size: 16px;
  font-weight: 700;
  margin: 0;
}
.ai-welcome small {
  font-size: 11px;
}

.msg-row {
  display: flex;
  gap: 8px;
  align-items: flex-start;
  width: 100%;
}
.msg-row.user {
  flex-direction: row-reverse;
}
.msg-row.system {
  justify-content: center;
}
.msg-spacer {
  flex: 1;
}

.msg-avatar {
  width: 28px;
  height: 28px;
  border-radius: 50%;
  background: var(--surface);
  border: 1px solid var(--border);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 14px;
  color: var(--accent);
  flex-shrink: 0;
}

.msg-bubble {
  max-width: 75%;
  display: flex;
  flex-direction: column;
  gap: 4px;
}
.msg-bubble.loading .msg-text {
  animation: pulse-loading 1.5s infinite;
  opacity: 0.6;
}
@keyframes pulse-loading {
  0%, 100% { opacity: 0.5; }
  50% { opacity: 1; }
}
.msg-bubble.user .msg-text {
  background: var(--user-color);
  color: #fff;
  padding: 8px 12px;
  border-radius: 16px 4px 16px 16px;
}
.msg-bubble.ai .msg-text {
  background: var(--surface);
  border: 1px solid var(--border);
  padding: 8px 12px;
  border-radius: 4px 16px 16px 16px;
  line-height: 1.6;
}
.msg-bubble.ai .msg-text code {
  background: rgba(255, 255, 255, 0.07);
  padding: 1px 5px;
  border-radius: 4px;
  color: #ff7b72;
  font-size: 0.9em;
}

.msg-system {
  background: rgba(255, 255, 255, 0.04);
  border: 1px solid var(--border);
  padding: 8px 16px;
  border-radius: 8px;
  font-size: 12px;
  color: var(--muted);
  max-width: 90%;
  text-align: center;
}

.msg-meta {
  font-size: 10px;
  color: var(--muted);
  padding: 0 4px;
}

.typing-cursor::after {
  content: "▋";
  animation: blink 1s infinite;
  opacity: 1;
}
.typing-cursor:not(.typing)::after {
  display: none;
}
@keyframes blink {
  0%,
  100% {
    opacity: 1;
  }
  50% {
    opacity: 0;
  }
}

/* Stats */
.msg-stats {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
  font-size: 10px;
  color: var(--muted);
  padding: 4px 4px 0;
}
.msg-stats span {
  display: flex;
  align-items: center;
}

/* Approval */
.approval {
  border: 1px solid;
  border-radius: 8px;
  overflow: hidden;
  margin-top: 6px;
}
.risk-low {
  border-color: #3fb950;
  background: rgba(63, 185, 80, 0.05);
}
.risk-medium {
  border-color: #d29922;
  background: rgba(210, 153, 34, 0.07);
}
.risk-high {
  border-color: #f85149;
  background: rgba(248, 81, 73, 0.07);
  animation: pulseR 1.5s infinite;
}
@keyframes pulseR {
  0%,
  100% {
    box-shadow: 0 0 0 0 rgba(248, 81, 73, 0.3);
  }
  50% {
    box-shadow: 0 0 0 5px rgba(248, 81, 73, 0);
  }
}

.appr-header {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 6px 10px;
  background: rgba(255, 255, 255, 0.03);
}
.risk-badge {
  font-size: 10px;
  font-weight: 800;
}
.appr-header code {
  font-size: 12px;
  color: var(--accent);
  background: rgba(255, 255, 255, 0.06);
  padding: 1px 6px;
  border-radius: 4px;
}
.appr-args-input {
  width: 100%;
  font-family: monospace;
  font-size: 11px;
  color: var(--text);
  background: rgba(0, 0, 0, 0.2);
  border: none;
  border-top: 1px solid var(--border);
  border-bottom: 1px solid var(--border);
  padding: 8px 10px;
  resize: vertical;
  outline: none;
  min-height: 80px;
}
.appr-args-input:focus {
  background: rgba(0, 0, 0, 0.3);
}
.appr-btns {
  display: flex;
  gap: 6px;
  padding: 6px 10px;
}
.btn-y {
  flex: 1;
  background: rgba(63, 185, 80, 0.15);
  border: 1px solid #3fb950;
  color: #3fb950;
  padding: 5px;
  border-radius: 6px;
  font-size: 11px;
  font-weight: 700;
  cursor: pointer;
}
.btn-n {
  flex: 1;
  background: rgba(248, 81, 73, 0.1);
  border: 1px solid #f85149;
  color: #f85149;
  padding: 5px;
  border-radius: 6px;
  font-size: 11px;
  font-weight: 700;
  cursor: pointer;
}
.btn-y:hover {
  background: rgba(63, 185, 80, 0.3);
}
.btn-n:hover {
  background: rgba(248, 81, 73, 0.25);
}
button:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

/* Loading dots */
.dots {
  display: inline-flex;
  gap: 4px;
  padding: 4px 0;
}
.dots span {
  width: 7px;
  height: 7px;
  border-radius: 50%;
  background: var(--muted);
  animation: bounce 0.9s infinite;
}
.dots span:nth-child(2) {
  animation-delay: 0.15s;
}
.dots span:nth-child(3) {
  animation-delay: 0.3s;
}
@keyframes bounce {
  0%,
  80%,
  100% {
    transform: translateY(0);
  }
  40% {
    transform: translateY(-6px);
  }
}

/* Resize bars */
.resize-bar-h {
  height: 5px;
  cursor: ns-resize;
  flex-shrink: 0;
  background: var(--border);
  transition: 0.15s;
}
.resize-bar-h:hover {
  background: var(--accent);
}
.resize-bar-v {
  width: 5px;
  cursor: ew-resize;
  flex-shrink: 0;
  background: var(--border);
  position: absolute;
  top: 0;
  bottom: 0;
  z-index: 10;
  transition: 0.15s;
}
.resize-bar-v.left-mode {
  right: 0;
}
.resize-bar-v.right-mode {
  left: 0;
}
.resize-bar-v:hover {
  background: var(--accent);
}

.resize-handle-win {
  position: absolute;
  right: 0;
  bottom: 0;
  width: 15px;
  height: 15px;
  cursor: nwse-resize;
  z-index: 20;
}
.resize-handle-win::after {
  content: "";
  position: absolute;
  right: 3px;
  bottom: 3px;
  width: 6px;
  height: 6px;
  border-right: 2px solid var(--muted);
  border-bottom: 2px solid var(--muted);
}

/* Input */
.ai-input-area {
  display: flex;
  align-items: flex-end;
  gap: 8px;
  padding: 8px 10px;
  border-top: 1px solid var(--border);
  background: var(--surface);
  flex-shrink: 0;
}
.ai-input {
  flex: 1;
  background: var(--bg);
  border: 1px solid var(--border);
  color: var(--text);
  border-radius: 10px;
  padding: 8px 12px;
  font-size: 13px;
  font-family: inherit;
  resize: none;
  outline: none;
  line-height: 1.5;
  max-height: 120px;
  overflow-y: auto;
  transition: border-color 0.2s;
}
.ai-input:focus {
  border-color: var(--accent);
}
.ai-input::placeholder {
  color: var(--muted);
}
.ai-send {
  background: var(--accent);
  border: none;
  color: #000;
  width: 36px;
  height: 36px;
  border-radius: 10px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 15px;
  flex-shrink: 0;
  transition: 0.2s;
}
.ai-send:hover:not(:disabled) {
  filter: brightness(1.15);
}
.ai-send:disabled {
  opacity: 0.35;
  cursor: not-allowed;
}

/* Scrollbar */
.ai-messages::-webkit-scrollbar {
  width: 4px;
}
.ai-messages::-webkit-scrollbar-track {
  background: transparent;
}
.ai-messages::-webkit-scrollbar-thumb {
  background: var(--border);
  border-radius: 4px;
}

/* ms-auto helper */
.ms-auto {
  margin-left: auto;
}
</style>
