<template>
  <div class="ai-terminal" :class="{ minimized: isMinimized }">
    <!-- Tab Bar -->
    <div class="term-tabbar">
      <div class="term-tabs">
        <button
          v-for="tab in tabs"
          :key="tab.key"
          class="term-tab"
          :class="{ active: activeTab === tab.key }"
          @click="activeTab = tab.key"
        >
          <span class="term-tab-dot" :class="tab.color"></span>
          {{ tab.label }}
        </button>
      </div>
      <div class="term-actions">
        <span class="term-status" :class="{ online: isConnected }">
          {{ isConnected ? "● ONLINE" : "○ OFFLINE" }}
        </span>
        <button class="term-btn" @click="clearTerminal" title="Clear">
          <i class="bi bi-trash3"></i>
        </button>
        <button
          class="term-btn"
          @click="$emit('toggle')"
          title="Thu nhỏ/Mở rộng"
        >
          <i
            class="bi"
            :class="isMinimized ? 'bi-chevron-up' : 'bi-chevron-down'"
          ></i>
        </button>
      </div>
    </div>

    <div v-show="!isMinimized" class="term-body">
      <!-- TAB: TERMINAL AI -->
      <div v-show="activeTab === 'terminal'" class="term-output" ref="outputEl">
        <div
          v-for="(line, i) in terminalLines"
          :key="i"
          class="term-line"
          :class="line.type"
        >
          <span class="term-time">{{ line.time }}</span>
          <span class="term-content" v-html="line.html"></span>
        </div>
        <div v-if="loading" class="term-line info">
          <span class="term-time">{{ now }}</span>
          <span class="term-content">
            <span class="blink">▋</span> Đang xử lý...
          </span>
        </div>
      </div>

      <!-- TAB: AUDIT LOG -->
      <div v-show="activeTab === 'audit'" class="term-output" ref="auditEl">
        <div v-if="auditLogs.length === 0" class="term-line muted">
          <span class="term-content">-- Chưa có nhật ký --</span>
        </div>
        <div v-for="log in auditLogs" :key="log.id" class="term-line audit">
          <span class="term-time">{{ fmtTime(log.thoiGian) }}</span>
          <span
            class="audit-action"
            :class="'audit-' + log.hanhDong.toLowerCase()"
            >{{ log.hanhDong }}</span
          >
          <span class="term-content">{{ log.moTa }}</span>
          <span class="audit-user">@{{ log.nguoiThucHien }}</span>
        </div>
      </div>

      <!-- TAB: CRON LOG -->
      <div v-show="activeTab === 'cron'" class="term-output">
        <div
          v-for="(line, i) in cronLogs"
          :key="i"
          class="term-line"
          :class="line.type"
        >
          <span class="term-time">{{ line.time }}</span>
          <span class="term-content">{{ line.text }}</span>
        </div>
        <div v-if="cronLogs.length === 0" class="term-line muted">
          <span class="term-content"
            >-- Worker chạy lúc 00:00:01 mỗi ngày --</span
          >
        </div>
      </div>

      <!-- Approval Box -->
      <div
        v-if="pendingAction"
        class="approval-box"
        :class="'risk-' + pendingAction.riskLevel.toLowerCase()"
      >
        <div class="approval-header">
          <span
            class="approval-risk-badge"
            :class="'badge-' + pendingAction.riskLevel.toLowerCase()"
          >
            {{ riskIcon(pendingAction.riskLevel) }}
            {{ pendingAction.riskLevel }} RISK
          </span>
          <span class="approval-title">AI muốn thực thi lệnh</span>
        </div>
        <div class="approval-body">
          <div class="approval-fn">
            <i class="bi bi-terminal me-2"></i>
            <strong>{{ pendingAction.functionName }}</strong>
          </div>
          <div class="approval-args">
            {{ fmtArgs(pendingAction.functionArgs) }}
          </div>
        </div>
        <div class="approval-btns">
          <button
            class="btn-accept"
            @click="confirmAction(true)"
            :disabled="loading"
          >
            <i class="bi bi-check-circle me-1"></i>Xác nhận (Y)
          </button>
          <button
            class="btn-reject"
            @click="confirmAction(false)"
            :disabled="loading"
          >
            <i class="bi bi-x-circle me-1"></i>Từ chối (N)
          </button>
        </div>
      </div>

      <!-- Input -->
      <div class="term-input-row">
        <span class="term-prompt">pos36-super$</span>
        <input
          ref="inputEl"
          v-model="inputText"
          class="term-input"
          :placeholder="loading ? 'Đang xử lý...' : 'Nhập lệnh hoặc câu hỏi...'"
          :disabled="loading || !!pendingAction"
          @keydown.enter="sendPrompt"
          @keydown.up="historyUp"
          @keydown.down="historyDown"
          @keydown.y.exact="handleKeyY"
          @keydown.n.exact="handleKeyN"
          autocomplete="off"
          spellcheck="false"
        />
        <button
          class="term-send"
          @click="sendPrompt"
          :disabled="loading || !!pendingAction"
        >
          <i class="bi bi-send"></i>
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, nextTick, onMounted, onUnmounted } from "vue";
import axios from "axios";

const props = defineProps({ isMinimized: Boolean });
const emit = defineEmits(["toggle"]);

const activeTab = ref("terminal");
const inputText = ref("");
const loading = ref(false);
const isConnected = ref(true);
const terminalLines = ref([]);
const auditLogs = ref([]);
const cronLogs = ref([]);
const pendingAction = ref(null);
const outputEl = ref(null);
const inputEl = ref(null);
const cmdHistory = ref([]);
const historyIdx = ref(-1);

const sessionId = `sa-${Date.now()}`;

const tabs = [
  { key: "terminal", label: "TERMINAL AI", color: "dot-green" },
  { key: "audit", label: "AUDIT LOG", color: "dot-yellow" },
  { key: "cron", label: "CRON LOG", color: "dot-blue" },
];

const now = computed(() => new Date().toLocaleTimeString("vi-VN"));
const fmtTime = (d) => (d ? new Date(d).toLocaleTimeString("vi-VN") : "");
const fmtArgs = (a) => {
  try {
    return JSON.stringify(JSON.parse(a), null, 2);
  } catch {
    return a;
  }
};
const riskIcon = (r) => ({ LOW: "🟢", MEDIUM: "🟡", HIGH: "🔴" })[r] ?? "⚪";

const pushLine = (text, type = "output", isHtml = false) => {
  terminalLines.value.push({
    time: new Date().toLocaleTimeString("vi-VN"),
    html: isHtml ? text : escHtml(text),
    type,
  });
  nextTick(() => {
    if (outputEl.value) outputEl.value.scrollTop = outputEl.value.scrollHeight;
  });
};

const escHtml = (t) =>
  String(t)
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;")
    .replace(/`([^`]+)`/g, "<code>$1</code>")
    .replace(/\*\*([^*]+)\*\*/g, "<strong>$1</strong>");

const sendPrompt = async () => {
  const prompt = inputText.value.trim();
  if (!prompt || loading.value || pendingAction.value) return;

  cmdHistory.value.unshift(prompt);
  historyIdx.value = -1;
  inputText.value = "";
  loading.value = true;

  pushLine(`$ ${prompt}`, "command");

  try {
    const res = await axios.post("/api/AIChat/chat", { prompt, sessionId });
    const data = res.data;

    if (data.requiresAction) {
      pendingAction.value = {
        functionName: data.functionName,
        functionArgs: data.functionArgs,
        riskLevel: data.riskLevel,
      };
      pushLine(
        `⚡ AI yêu cầu thực thi: <strong>${data.functionName}</strong> — Vui lòng xác nhận bên dưới.`,
        "warning",
        true,
      );
    } else {
      pushLine(data.text, "output");
    }
  } catch (e) {
    pushLine(`❌ Lỗi kết nối: ${e.message}`, "error");
    isConnected.value = false;
  } finally {
    loading.value = false;
    nextTick(() => inputEl.value?.focus());
  }
};

const confirmAction = async (confirmed) => {
  if (!pendingAction.value) return;
  loading.value = true;
  const action = { ...pendingAction.value };
  pendingAction.value = null;

  try {
    const res = await axios.post("/api/AIChat/confirm", {
      confirmed,
      functionName: action.functionName,
      functionArgs: action.functionArgs,
    });
    const data = res.data;
    const type = confirmed ? (data.success ? "success" : "error") : "muted";
    pushLine(data.message, type);

    // Refresh audit log
    if (activeTab.value === "audit" || confirmed) loadAuditLog();
  } catch (e) {
    pushLine(`❌ ${e.message}`, "error");
  } finally {
    loading.value = false;
    nextTick(() => inputEl.value?.focus());
  }
};

const handleKeyY = () => {
  if (pendingAction.value && !loading.value) confirmAction(true);
};
const handleKeyN = () => {
  if (pendingAction.value && !loading.value) confirmAction(false);
};

const historyUp = () => {
  if (historyIdx.value < cmdHistory.value.length - 1) {
    historyIdx.value++;
    inputText.value = cmdHistory.value[historyIdx.value];
  }
};
const historyDown = () => {
  if (historyIdx.value > 0) {
    historyIdx.value--;
    inputText.value = cmdHistory.value[historyIdx.value];
  } else {
    historyIdx.value = -1;
    inputText.value = "";
  }
};

const clearTerminal = () => {
  if (activeTab.value === "terminal") terminalLines.value = [];
  else if (activeTab.value === "audit") auditLogs.value = [];
  else cronLogs.value = [];
};

const loadAuditLog = async () => {
  try {
    const res = await axios.get("/api/CauHinh/nhat-ky", {
      params: { page: 1, pageSize: 30 },
    });
    auditLogs.value = res.data.items ?? res.data;
  } catch {
    auditLogs.value = [];
  }
};

// Simulate cron status
const addCronLine = (text, type = "info") =>
  cronLogs.value.push({
    time: new Date().toLocaleTimeString("vi-VN"),
    text,
    type,
  });

onMounted(() => {
  pushLine("POS36 Super Admin AI Terminal v2.0", "success");
  pushLine("Gemini 1.5 Flash — Function Calling Mode", "muted");
  pushLine("─────────────────────────────────────", "muted");
  pushLine(
    'Gõ lệnh bằng tiếng Việt. Ví dụ: "thống kê hệ thống", "khóa quán 5"',
    "muted",
  );
  loadAuditLog();
  addCronLine("SubscriptionWorker: đang chờ đến 00:00:01...", "info");
  inputEl.value?.focus();
});
</script>

<style scoped>
.ai-terminal {
  display: flex;
  flex-direction: column;
  background: #0d1117;
  color: #c9d1d9;
  font-family: "Cascadia Code", "Fira Code", "Courier New", Courier, monospace;
  font-size: 13px;
  border-top: 1px solid #30363d;
  height: 100%;
  overflow: hidden;
}

/* Tab bar */
.term-tabbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: #161b22;
  border-bottom: 1px solid #30363d;
  padding: 0 8px;
  flex-shrink: 0;
  height: 36px;
}
.term-tabs {
  display: flex;
  gap: 2px;
}
.term-tab {
  background: none;
  border: none;
  color: #8b949e;
  padding: 6px 14px;
  cursor: pointer;
  font-size: 11px;
  font-weight: 600;
  letter-spacing: 0.5px;
  border-bottom: 2px solid transparent;
  transition: 0.15s;
  display: flex;
  align-items: center;
  gap: 6px;
}
.term-tab:hover {
  color: #c9d1d9;
}
.term-tab.active {
  color: #f0f6fc;
  border-bottom-color: #f59e0b;
}
.term-tab-dot {
  width: 7px;
  height: 7px;
  border-radius: 50%;
}
.dot-green {
  background: #3fb950;
}
.dot-yellow {
  background: #d29922;
}
.dot-blue {
  background: #388bfd;
}

.term-actions {
  display: flex;
  align-items: center;
  gap: 8px;
}
.term-status {
  font-size: 10px;
  font-weight: 700;
  color: #3fb950;
  letter-spacing: 0.5px;
}
.term-status:not(.online) {
  color: #8b949e;
}
.term-btn {
  background: none;
  border: none;
  color: #8b949e;
  cursor: pointer;
  padding: 3px 6px;
  border-radius: 4px;
  font-size: 12px;
}
.term-btn:hover {
  background: #21262d;
  color: #c9d1d9;
}

/* Output */
.term-body {
  display: flex;
  flex-direction: column;
  flex: 1;
  overflow: hidden;
}
.term-output {
  flex: 1;
  overflow-y: auto;
  padding: 8px 12px;
  scrollbar-width: thin;
  scrollbar-color: #30363d transparent;
}
.term-output::-webkit-scrollbar {
  width: 4px;
}
.term-output::-webkit-scrollbar-track {
  background: transparent;
}
.term-output::-webkit-scrollbar-thumb {
  background: #30363d;
  border-radius: 4px;
}

.term-line {
  display: flex;
  gap: 10px;
  line-height: 1.6;
  margin-bottom: 1px;
}
.term-time {
  color: #484f58;
  font-size: 11px;
  white-space: nowrap;
  flex-shrink: 0;
  padding-top: 1px;
}
.term-content {
  flex: 1;
  word-break: break-word;
}
.term-line.command .term-content {
  color: #79c0ff;
}
.term-line.output .term-content {
  color: #c9d1d9;
}
.term-line.success .term-content {
  color: #3fb950;
}
.term-line.error .term-content {
  color: #f85149;
}
.term-line.warning .term-content {
  color: #d29922;
}
.term-line.info .term-content {
  color: #388bfd;
}
.term-line.muted .term-content {
  color: #484f58;
}

/* Audit log */
.term-line.audit {
  align-items: baseline;
  gap: 8px;
}
.audit-action {
  font-size: 10px;
  font-weight: 700;
  padding: 1px 6px;
  border-radius: 4px;
  white-space: nowrap;
  flex-shrink: 0;
}
.audit-aichat {
  background: rgba(56, 139, 253, 0.15);
  color: #388bfd;
}
.audit-aithucthi {
  background: rgba(63, 185, 80, 0.15);
  color: #3fb950;
}
.audit-aihuylenh {
  background: rgba(248, 81, 73, 0.15);
  color: #f85149;
}
.audit-tudongkhoa {
  background: rgba(210, 153, 34, 0.15);
  color: #d29922;
}
.audit-user {
  color: #484f58;
  font-size: 11px;
  white-space: nowrap;
}

/* Approval box */
.approval-box {
  margin: 8px 12px;
  border-radius: 8px;
  border: 1px solid;
  overflow: hidden;
  flex-shrink: 0;
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
  animation: pulseRed 1.5s infinite;
}
@keyframes pulseRed {
  0%,
  100% {
    box-shadow: 0 0 0 0 rgba(248, 81, 73, 0.3);
  }
  50% {
    box-shadow: 0 0 0 6px rgba(248, 81, 73, 0);
  }
}

.approval-header {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 8px 14px;
  background: rgba(255, 255, 255, 0.03);
  border-bottom: 1px solid #30363d;
}
.approval-risk-badge {
  font-size: 11px;
  font-weight: 800;
  letter-spacing: 0.5px;
}
.badge-low {
  color: #3fb950;
}
.badge-medium {
  color: #d29922;
}
.badge-high {
  color: #f85149;
}
.approval-title {
  color: #8b949e;
  font-size: 12px;
}

.approval-body {
  padding: 10px 14px;
}
.approval-fn {
  color: #f0f6fc;
  font-size: 13px;
  margin-bottom: 6px;
}
.approval-args {
  background: #161b22;
  border: 1px solid #30363d;
  border-radius: 6px;
  padding: 8px 12px;
  color: #8b949e;
  font-size: 11px;
  white-space: pre-wrap;
  max-height: 80px;
  overflow-y: auto;
}

.approval-btns {
  display: flex;
  gap: 8px;
  padding: 8px 14px 12px;
}
.btn-accept {
  flex: 1;
  background: rgba(63, 185, 80, 0.15);
  border: 1px solid #3fb950;
  color: #3fb950;
  padding: 7px;
  border-radius: 6px;
  font-size: 12px;
  font-weight: 700;
  cursor: pointer;
  font-family: inherit;
  transition: 0.2s;
}
.btn-accept:hover {
  background: rgba(63, 185, 80, 0.3);
}
.btn-reject {
  flex: 1;
  background: rgba(248, 81, 73, 0.1);
  border: 1px solid #f85149;
  color: #f85149;
  padding: 7px;
  border-radius: 6px;
  font-size: 12px;
  font-weight: 700;
  cursor: pointer;
  font-family: inherit;
  transition: 0.2s;
}
.btn-reject:hover {
  background: rgba(248, 81, 73, 0.25);
}
button:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

/* Input */
.term-input-row {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 12px;
  border-top: 1px solid #21262d;
  background: #0d1117;
  flex-shrink: 0;
}
.term-prompt {
  color: #f59e0b;
  font-weight: 700;
  white-space: nowrap;
  flex-shrink: 0;
}
.term-input {
  flex: 1;
  background: transparent;
  border: none;
  outline: none;
  color: #c9d1d9;
  font-family: inherit;
  font-size: 13px;
  caret-color: #f59e0b;
}
.term-input::placeholder {
  color: #484f58;
}
.term-input:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}
.term-send {
  background: none;
  border: none;
  color: #f59e0b;
  cursor: pointer;
  font-size: 14px;
  padding: 2px 4px;
}
.term-send:hover {
  color: #fbbf24;
}
.term-send:disabled {
  opacity: 0.3;
  cursor: not-allowed;
}

/* Blink cursor */
@keyframes blink {
  0%,
  100% {
    opacity: 1;
  }
  50% {
    opacity: 0;
  }
}
.blink {
  animation: blink 1s infinite;
}

code {
  background: #21262d;
  padding: 1px 5px;
  border-radius: 3px;
  color: #ff7b72;
  font-family: inherit;
  font-size: 0.9em;
}
</style>
