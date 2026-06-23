<template>
  <div class="checkout-root">

    <!-- ===== LOADING ===== -->
    <div v-if="uiState === 'loading'" class="fullscreen-center">
      <div class="spinner-ring"></div>
      <p class="mt-4 text-muted">Đang tải thông tin thanh toán...</p>
    </div>

    <!-- ===== ERROR ===== -->
    <div v-else-if="uiState === 'error'" class="fullscreen-center">
      <div class="status-icon">⚠️</div>
      <h2 class="mt-3">{{ errorMsg }}</h2>
      <button class="btn-action btn-cancel mt-4" @click="closePage">Đóng tab</button>
    </div>

    <!-- ===== SUCCESS ===== -->
    <transition name="pop">
      <div v-if="uiState === 'success'" class="overlay-screen success-screen">
        <div class="overlay-card">
          <svg class="checkmark-svg" viewBox="0 0 52 52">
            <circle class="ck-circle" cx="26" cy="26" r="25" fill="none"/>
            <path class="ck-path" fill="none" d="M14.1 27.2l7.1 7.2 16.7-16.8"/>
          </svg>
          <h2 class="ov-title">Thanh toán thành công! 🎉</h2>
          <p class="ov-sub">Gói <strong>{{ detail.tenGoi }}</strong> đã được kích hoạt tự động.</p>
          <p class="ov-note">Tab này sẽ tự đóng sau <strong>{{ closeCountdown }}</strong> giây...</p>
          <button class="btn-action btn-success" @click="closePage">Đóng ngay</button>
        </div>
      </div>
    </transition>

    <!-- ===== EXPIRED / CANCELLED ===== -->
    <transition name="pop">
      <div v-if="uiState === 'expired'" class="overlay-screen expired-screen">
        <div class="overlay-card">
          <div class="expired-icon">⏰</div>
          <h2 class="ov-title">Đơn hàng đã hết hạn</h2>
          <p class="ov-sub">Mã QR hết hiệu lực sau 10 phút không thanh toán.</p>
          <button class="btn-action btn-cancel" @click="closePage">Đóng tab</button>
        </div>
      </div>
    </transition>

    <!-- ===== MAIN CHECKOUT ===== -->
    <div v-if="uiState === 'active' && detail" class="checkout-wrapper">
      <!-- HEADER -->
      <header class="co-header">
        <div class="co-brand">
          <span class="co-brand-icon">🏪</span>
          <span class="co-brand-name">POS36</span>
        </div>
        <div class="co-header-center">
          <h1 class="co-title">Thanh toán mua gói</h1>
          <span class="co-plan-badge">{{ detail.tenGoi }}</span>
        </div>
        <div class="co-header-right">
          <span class="co-limit-note">Giới hạn mã: <strong>{{ soLanConLai }}</strong> lần còn lại hôm nay</span>
        </div>
      </header>

      <!-- TIMER BAR -->
      <div class="timer-bar" :class="{ 'timer-danger': remainingSecs < 120, 'timer-warning': remainingSecs < 300 && remainingSecs >= 120 }">
        <i class="bi bi-hourglass-split me-2"></i>
        <span class="timer-label">Mã QR hết hạn sau:</span>
        <span class="timer-value">{{ timerDisplay }}</span>
        <div class="timer-progress">
          <div class="timer-fill" :style="{ width: timerPercent + '%' }"></div>
        </div>
      </div>

      <!-- MAIN GRID -->
      <div class="co-grid">

        <!-- LEFT: QR CODE -->
        <div class="co-left">
          <div class="qr-card">
            <div class="qr-card-header">
              <span>Quét mã để thanh toán</span>
              <span class="qr-vietqr-badge">VietQR</span>
            </div>
            <div class="qr-image-wrap">
              <img :src="qrUrl" class="qr-image" alt="Mã QR chuyển khoản" />
            </div>
            <div class="qr-amount-display">
              <span class="qr-amount-label">Số tiền cần chuyển</span>
              <span class="qr-amount-value">{{ formatVND(detail.soTienThanhToan) }}</span>
            </div>
          </div>
        </div>

        <!-- RIGHT: INFO + ACTIONS -->
        <div class="co-right">

          <!-- Bank info card -->
          <div class="info-card">
            <div class="info-section-title">
              <i class="bi bi-bank2 me-2"></i>Tài khoản thụ hưởng
            </div>
            <div class="info-row">
              <span class="info-label">Ngân hàng</span>
              <span class="info-value">{{ detail.bankCode }}</span>
            </div>
            <div class="info-row copyable" @click="copyText(detail.bankAccountNo, 'Số tài khoản')">
              <span class="info-label">Số tài khoản</span>
              <span class="info-value mono">{{ detail.bankAccountNo }}<i class="bi bi-clipboard ms-2 copy-icon"></i></span>
            </div>
            <div class="info-row">
              <span class="info-label">Chủ tài khoản</span>
              <span class="info-value upper">{{ detail.bankAccountName }}</span>
            </div>

            <div class="info-divider"></div>

            <div class="info-section-title">
              <i class="bi bi-receipt me-2"></i>Thông tin chuyển khoản
            </div>
            <div class="info-row">
              <span class="info-label">Số tiền</span>
              <span class="info-value amount">{{ formatVND(detail.soTienThanhToan) }}</span>
            </div>
            <div class="info-row copyable highlight-row" @click="copyText(detail.maGiaoDich, 'Nội dung chuyển khoản')">
              <span class="info-label">Nội dung CK</span>
              <span class="info-value mono accent">{{ detail.maGiaoDich }}<i class="bi bi-clipboard ms-2 copy-icon"></i></span>
            </div>
            <div class="info-row">
              <span class="info-label">Gói đăng ký</span>
              <span class="info-value">{{ detail.tenGoi }}</span>
            </div>
          </div>

          <!-- Action area -->
          <div class="action-area">
            <button class="btn-action btn-check" @click="checkPaymentManual" :disabled="checking || remainingSecs === 0">
              <span v-if="checking" class="spinner-sm"></span>
              <i v-else class="bi bi-search me-1"></i>
              {{ checking ? 'Đang kiểm tra...' : 'Kiểm tra thanh toán' }}
            </button>
            <button class="btn-action btn-cancel" @click="confirmCancel">
              <i class="bi bi-x-circle me-1"></i>Hủy đơn hàng
            </button>
          </div>

          <!-- Auto-check notice -->
          <div class="auto-poll-notice">
            <span class="pulse-dot"></span>
            Tự động kiểm tra mỗi 5 giây...
          </div>

          <!-- Check result message -->
          <transition name="slide-up">
            <div v-if="checkMsg" class="check-result" :class="checkMsgType">
              {{ checkMsg }}
            </div>
          </transition>

        </div>
      </div>

      <!-- Warning -->
      <div class="co-warning">
        <i class="bi bi-exclamation-triangle-fill me-2"></i>
        Nhập <strong>chính xác</strong> nội dung chuyển khoản <code>{{ detail.maGiaoDich }}</code>. Sai nội dung hệ thống không thể tự động kích hoạt.
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue';
import { useRoute } from 'vue-router';
import axios from 'axios';

const route = useRoute();
const orderId = parseInt(route.params.id);

// ===== STATE =====
const uiState      = ref('loading'); // loading | active | success | expired | error
const errorMsg     = ref('');
const detail       = ref(null);
const checking     = ref(false);
const checkMsg     = ref('');
const checkMsgType = ref('info');
const remainingSecs = ref(600);
const soLanConLai  = ref(3);
const closeCountdown = ref(5);

// ===== TIMERS =====
let countdownTimer = null;
let pollTimer      = null;
let closeTimer     = null;

// ===== COMPUTED =====
const timerDisplay = computed(() => {
  const m = Math.floor(remainingSecs.value / 60).toString().padStart(2, '0');
  const s = (remainingSecs.value % 60).toString().padStart(2, '0');
  return `${m}:${s}`;
});

const timerPercent = computed(() => (remainingSecs.value / 600) * 100);

const qrUrl = computed(() => {
  if (!detail.value || !detail.value.configured) return '';
  const d = detail.value;
  return `https://img.vietqr.io/image/${d.bankCode}-${d.bankAccountNo}-compact2.png`
    + `?amount=${d.soTienThanhToan}`
    + `&addInfo=${encodeURIComponent(d.maGiaoDich)}`
    + `&accountName=${encodeURIComponent(d.bankAccountName)}`;
});

// ===== FORMAT =====
const formatVND = (n) => n ? Number(n).toLocaleString('vi-VN') + 'đ' : '0đ';

// ===== COPY =====
const copyText = async (text, label) => {
  try {
    await navigator.clipboard.writeText(text);
    showCheck(`✅ Đã sao chép ${label}: ${text}`, 'success');
  } catch {
    showCheck(`Sao chép thất bại. Hãy copy thủ công: ${text}`, 'error');
  }
};

const showCheck = (msg, type = 'info') => {
  checkMsg.value = msg;
  checkMsgType.value = type;
  setTimeout(() => { checkMsg.value = ''; }, 4000);
};

// ===== LOAD DETAIL =====
const loadDetail = async () => {
  try {
    const res = await axios.get(`/api/Subscription/checkout-detail/${orderId}`);
    detail.value = res.data;

    if (res.data.trangThai === 'DaThanhToan') {
      uiState.value = 'success';
      startCloseCountdown();
      return;
    }
    if (res.data.trangThai === 'DaHuy') {
      uiState.value = 'expired';
      return;
    }

    remainingSecs.value = Math.max(0, res.data.giayConLai);
    uiState.value = 'active';
    startCountdown();
    startAutoPoll();
  } catch (e) {
    errorMsg.value = 'Không tìm thấy đơn hàng hoặc đơn không thuộc về bạn.';
    uiState.value = 'error';
  }
};

// ===== COUNTDOWN =====
const startCountdown = () => {
  countdownTimer = setInterval(() => {
    if (remainingSecs.value > 0) {
      remainingSecs.value--;
    } else {
      clearInterval(countdownTimer);
      clearInterval(pollTimer);
      uiState.value = 'expired';
    }
  }, 1000);
};

// ===== AUTO POLL =====
const startAutoPoll = () => {
  pollTimer = setInterval(async () => {
    if (uiState.value !== 'active') { clearInterval(pollTimer); return; }
    try {
      const res = await axios.get(`/api/Subscription/check-payment/${orderId}`);
      if (res.data.trangThai === 'DaThanhToan') {
        clearInterval(pollTimer);
        clearInterval(countdownTimer);
        uiState.value = 'success';
        startCloseCountdown();
      } else if (res.data.trangThai === 'DaHuy') {
        clearInterval(pollTimer);
        clearInterval(countdownTimer);
        uiState.value = 'expired';
      } else if (res.data.giayConLai !== undefined) {
        // Sync server time
        remainingSecs.value = Math.max(0, res.data.giayConLai);
      }
    } catch { /* silent */ }
  }, 5000);
};

// ===== MANUAL CHECK =====
const checkPaymentManual = async () => {
  if (checking.value) return;
  checking.value = true;
  try {
    const res = await axios.get(`/api/Subscription/check-payment/${orderId}`);
    if (res.data.trangThai === 'DaThanhToan') {
      clearInterval(pollTimer);
      clearInterval(countdownTimer);
      uiState.value = 'success';
      startCloseCountdown();
    } else if (res.data.trangThai === 'DaHuy') {
      uiState.value = 'expired';
    } else {
      showCheck('⏳ Chưa nhận được thanh toán. Vui lòng thử lại sau.', 'warn');
    }
  } catch {
    showCheck('❌ Kiểm tra thất bại. Thử lại sau.', 'error');
  } finally {
    checking.value = false;
  }
};

// ===== CANCEL =====
const confirmCancel = async () => {
  if (!confirm('Bạn có chắc muốn hủy đơn hàng này không?')) return;
  try {
    await axios.post(`/api/Subscription/cancel/${orderId}`);
    clearInterval(pollTimer);
    clearInterval(countdownTimer);
    uiState.value = 'expired';
  } catch (e) {
    showCheck('❌ Hủy thất bại. Thử lại sau.', 'error');
  }
};

// ===== CLOSE PAGE =====
const closePage = () => window.close();

const startCloseCountdown = () => {
  closeCountdown.value = 5;
  closeTimer = setInterval(() => {
    closeCountdown.value--;
    if (closeCountdown.value <= 0) { clearInterval(closeTimer); window.close(); }
  }, 1000);
};

// ===== LIFECYCLE =====
onMounted(loadDetail);
onUnmounted(() => {
  clearInterval(countdownTimer);
  clearInterval(pollTimer);
  clearInterval(closeTimer);
});
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800;900&display=swap');

/* ===== ROOT ===== */
.checkout-root {
  min-height: 100vh;
  background: linear-gradient(135deg, #0f0c29 0%, #1a1a3e 50%, #24243e 100%);
  font-family: 'Inter', sans-serif;
  color: #e2e8f0;
  padding-bottom: 40px;
}

/* ===== HEADER ===== */
.co-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 18px 32px;
  background: rgba(255,255,255,0.04);
  border-bottom: 1px solid rgba(255,255,255,0.08);
  backdrop-filter: blur(10px);
}
.co-brand { display: flex; align-items: center; gap: 8px; }
.co-brand-icon { font-size: 1.4rem; }
.co-brand-name { font-weight: 900; font-size: 1.1rem; color: #f97316; letter-spacing: 1px; }
.co-header-center { text-align: center; }
.co-title { font-size: 1.1rem; font-weight: 700; margin: 0 0 4px; color: #f1f5f9; }
.co-plan-badge {
  display: inline-block;
  background: rgba(249,115,22,0.2);
  border: 1px solid rgba(249,115,22,0.4);
  color: #fb923c;
  font-size: .75rem; font-weight: 700;
  padding: 2px 10px; border-radius: 20px;
}
.co-header-right { text-align: right; }
.co-limit-note { font-size: .75rem; color: #94a3b8; }

/* ===== TIMER BAR ===== */
.timer-bar {
  display: flex; align-items: center; gap: 10px;
  padding: 12px 32px;
  background: rgba(255,255,255,0.03);
  border-bottom: 1px solid rgba(255,255,255,0.06);
  transition: background .4s;
  position: relative;
}
.timer-label { color: #94a3b8; font-size: .85rem; }
.timer-value {
  font-size: 1.5rem; font-weight: 900; letter-spacing: 2px;
  color: #22c55e; font-variant-numeric: tabular-nums;
  transition: color .3s;
}
.timer-bar.timer-warning .timer-value { color: #f59e0b; }
.timer-bar.timer-danger .timer-value { color: #ef4444; animation: pulse-danger 1s infinite; }
@keyframes pulse-danger { 0%,100%{opacity:1} 50%{opacity:.6} }

.timer-progress {
  flex: 1; height: 3px;
  background: rgba(255,255,255,0.08);
  border-radius: 2px; margin-left: 12px;
}
.timer-fill {
  height: 100%; border-radius: 2px;
  background: linear-gradient(90deg, #22c55e, #86efac);
  transition: width .9s linear, background .4s;
}
.timer-bar.timer-warning .timer-fill { background: linear-gradient(90deg, #f59e0b, #fcd34d); }
.timer-bar.timer-danger .timer-fill { background: linear-gradient(90deg, #ef4444, #fca5a5); }

/* ===== MAIN GRID ===== */
.co-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 24px;
  max-width: 960px;
  margin: 32px auto;
  padding: 0 24px;
}
@media (max-width: 680px) { .co-grid { grid-template-columns: 1fr; } }

/* ===== QR CARD ===== */
.qr-card {
  background: #fff;
  border-radius: 20px;
  overflow: hidden;
  box-shadow: 0 20px 60px rgba(0,0,0,.4);
}
.qr-card-header {
  display: flex; justify-content: space-between; align-items: center;
  padding: 16px 20px;
  background: #f8fafc;
  border-bottom: 1px solid #e2e8f0;
  font-weight: 600; font-size: .85rem; color: #475569;
}
.qr-vietqr-badge {
  background: linear-gradient(135deg, #0ea5e9, #6366f1);
  color: #fff; font-size: .7rem; font-weight: 700;
  padding: 2px 8px; border-radius: 10px;
}
.qr-image-wrap { padding: 24px; display: flex; justify-content: center; }
.qr-image { max-width: 240px; width: 100%; border-radius: 8px; }
.qr-amount-display {
  padding: 16px;
  background: #f8fafc;
  border-top: 1px solid #e2e8f0;
  text-align: center;
}
.qr-amount-label { display: block; font-size: .75rem; color: #94a3b8; margin-bottom: 4px; }
.qr-amount-value { font-size: 1.5rem; font-weight: 900; color: #dc2626; }

/* ===== INFO CARD ===== */
.info-card {
  background: rgba(255,255,255,0.05);
  border: 1px solid rgba(255,255,255,0.1);
  border-radius: 16px;
  padding: 20px;
  backdrop-filter: blur(10px);
  margin-bottom: 16px;
}
.info-section-title {
  font-size: .72rem; font-weight: 800;
  color: #94a3b8; text-transform: uppercase; letter-spacing: .5px;
  margin-bottom: 12px;
}
.info-row {
  display: flex; justify-content: space-between; align-items: center;
  padding: 8px 0;
  border-bottom: 1px solid rgba(255,255,255,0.05);
}
.info-row:last-child { border-bottom: none; }
.info-label { font-size: .82rem; color: #94a3b8; }
.info-value { font-size: .9rem; font-weight: 600; color: #e2e8f0; }
.info-value.mono { font-family: monospace; letter-spacing: .5px; }
.info-value.upper { text-transform: uppercase; }
.info-value.amount { font-size: 1.1rem; font-weight: 800; color: #f97316; }
.info-value.accent { color: #60a5fa; }
.info-divider { height: 1px; background: rgba(255,255,255,0.1); margin: 14px 0; }

.copyable {
  cursor: pointer; border-radius: 8px;
  transition: background .15s;
  padding-left: 8px; padding-right: 8px; margin: 0 -8px;
}
.copyable:hover { background: rgba(255,255,255,0.07); }
.highlight-row { background: rgba(96,165,250,0.05); border-radius: 8px; }
.copy-icon { color: #94a3b8; font-size: .8rem; }

/* ===== ACTIONS ===== */
.action-area { display: flex; flex-direction: column; gap: 10px; margin-bottom: 12px; }
.btn-action {
  display: flex; align-items: center; justify-content: center; gap: 6px;
  padding: 13px 20px; border-radius: 12px;
  font-size: .9rem; font-weight: 700; border: none;
  cursor: pointer; transition: .2s; width: 100%;
}
.btn-check {
  background: linear-gradient(135deg, #f97316, #ea580c);
  color: #fff; box-shadow: 0 4px 20px rgba(249,115,22,.35);
}
.btn-check:hover:not(:disabled) { transform: translateY(-1px); box-shadow: 0 8px 30px rgba(249,115,22,.5); }
.btn-check:disabled { opacity: .5; cursor: not-allowed; }
.btn-cancel {
  background: rgba(239,68,68,.12); border: 1px solid rgba(239,68,68,.3);
  color: #ef4444;
}
.btn-cancel:hover { background: rgba(239,68,68,.2); }
.btn-success {
  background: linear-gradient(135deg, #22c55e, #16a34a);
  color: #fff;
}

/* ===== AUTO POLL NOTICE ===== */
.auto-poll-notice {
  display: flex; align-items: center; gap: 8px;
  font-size: .78rem; color: #64748b; padding: 0 4px;
}
.pulse-dot {
  width: 8px; height: 8px; border-radius: 50%;
  background: #22c55e;
  animation: pulse-dot 2s infinite;
  flex-shrink: 0;
}
@keyframes pulse-dot { 0%,100%{opacity:1;transform:scale(1)} 50%{opacity:.4;transform:scale(.7)} }

/* ===== CHECK RESULT ===== */
.check-result {
  margin-top: 10px; padding: 10px 14px;
  border-radius: 10px; font-size: .85rem; font-weight: 500;
}
.check-result.success { background: rgba(34,197,94,.12); border: 1px solid rgba(34,197,94,.3); color: #4ade80; }
.check-result.error   { background: rgba(239,68,68,.12); border: 1px solid rgba(239,68,68,.3); color: #f87171; }
.check-result.warn    { background: rgba(245,158,11,.12); border: 1px solid rgba(245,158,11,.3); color: #fbbf24; }
.check-result.info    { background: rgba(99,102,241,.12); border: 1px solid rgba(99,102,241,.3); color: #a5b4fc; }

/* ===== WARNING BAR ===== */
.co-warning {
  max-width: 960px; margin: 0 auto 20px;
  padding: 12px 24px;
  background: rgba(245,158,11,.08);
  border: 1px solid rgba(245,158,11,.2);
  border-radius: 12px; font-size: .82rem; color: #fcd34d;
  margin-left: 24px; margin-right: 24px;
}
.co-warning code { background: rgba(245,158,11,.2); padding: 1px 6px; border-radius: 4px; font-weight: 700; }

/* ===== SPINNER ===== */
.spinner-sm {
  width: 16px; height: 16px; border-radius: 50%;
  border: 2px solid rgba(255,255,255,.3);
  border-top-color: #fff;
  animation: spin .7s linear infinite;
  display: inline-block;
}
.spinner-ring {
  width: 48px; height: 48px; border-radius: 50%;
  border: 4px solid rgba(255,255,255,.1);
  border-top-color: #f97316;
  animation: spin .9s linear infinite;
}
@keyframes spin { to { transform: rotate(360deg); } }

/* ===== FULLSCREEN CENTER ===== */
.fullscreen-center {
  min-height: 100vh; display: flex;
  flex-direction: column; align-items: center; justify-content: center;
  text-align: center; color: #94a3b8;
}

/* ===== OVERLAYS ===== */
.overlay-screen {
  position: fixed; inset: 0; z-index: 100;
  display: flex; align-items: center; justify-content: center;
}
.success-screen { background: linear-gradient(135deg, #052e16 0%, #14532d 100%); }
.expired-screen { background: linear-gradient(135deg, #1c1917 0%, #292524 100%); }

.overlay-card {
  background: rgba(255,255,255,0.05);
  border: 1px solid rgba(255,255,255,0.12);
  border-radius: 24px; padding: 48px 40px;
  text-align: center; max-width: 420px;
  backdrop-filter: blur(16px);
}
.ov-title { font-size: 1.6rem; font-weight: 800; margin: 16px 0 8px; }
.ov-sub { font-size: 1rem; color: #94a3b8; margin-bottom: 8px; }
.ov-note { font-size: .85rem; color: #64748b; margin-bottom: 24px; }

/* Checkmark SVG animation */
.checkmark-svg { width: 72px; height: 72px; }
.ck-circle {
  stroke: #22c55e; stroke-width: 2;
  stroke-dasharray: 166; stroke-dashoffset: 166;
  animation: stroke 0.6s cubic-bezier(0.65, 0, 0.45, 1) forwards;
}
.ck-path {
  stroke: #22c55e; stroke-width: 2.5;
  stroke-dasharray: 48; stroke-dashoffset: 48;
  animation: stroke 0.4s cubic-bezier(0.65, 0, 0.45, 1) 0.5s forwards;
}
@keyframes stroke { to { stroke-dashoffset: 0; } }

.expired-icon { font-size: 4rem; }
.status-icon { font-size: 3rem; }

/* ===== TRANSITIONS ===== */
.pop-enter-active, .pop-leave-active { transition: opacity .3s, transform .3s; }
.pop-enter-from, .pop-leave-to { opacity: 0; transform: scale(.95); }
.slide-up-enter-active, .slide-up-leave-active { transition: opacity .25s, transform .25s; }
.slide-up-enter-from, .slide-up-leave-to { opacity: 0; transform: translateY(8px); }
.fade-enter-active, .fade-leave-active { transition: opacity .4s; }
.fade-enter-from, .fade-leave-to { opacity: 0; }
</style>
