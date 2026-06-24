<template>
  <div class="checkout-root">

    <!-- ===== LOADING ===== -->
    <div v-if="uiState === 'loading'" class="fullscreen-center">
      <div class="spinner-ring"></div>
      <p class="mt-4 text-muted fw-medium">Đang tải thông tin thanh toán...</p>
    </div>

    <!-- ===== ERROR ===== -->
    <div v-else-if="uiState === 'error'" class="fullscreen-center">
      <div class="status-icon mb-3" style="font-size: 3rem;">⚠️</div>
      <h3 class="text-dark fw-bold">{{ errorMsg }}</h3>
      <button class="btn btn-outline-danger mt-4 fw-bold px-4" @click="closePage">Đóng tab</button>
    </div>

    <!-- ===== SUCCESS ===== -->
    <transition name="pop">
      <div v-if="uiState === 'success'" class="overlay-screen">
        <div class="overlay-card success-card shadow-lg">
          <svg class="checkmark-svg mb-4" viewBox="0 0 52 52">
            <circle class="ck-circle" cx="26" cy="26" r="25" fill="none"/>
            <path class="ck-path" fill="none" d="M14.1 27.2l7.1 7.2 16.7-16.8"/>
          </svg>
          <h3 class="text-success fw-bold mb-2">Thanh toán thành công! 🎉</h3>
          <p class="text-muted mb-1">Gói <strong class="text-dark">{{ detail.tenGoi }}</strong> đã được kích hoạt tự động.</p>
          <p class="small text-muted mb-4">Tab này sẽ tự đóng sau <strong>{{ closeCountdown }}</strong> giây...</p>
          <button class="btn btn-success px-4 py-2 fw-bold w-100" @click="closePage">Đóng ngay</button>
        </div>
      </div>
    </transition>

    <!-- ===== EXPIRED / CANCELLED ===== -->
    <transition name="pop">
      <div v-if="uiState === 'expired'" class="overlay-screen">
        <div class="overlay-card expired-card shadow-lg">
          <div class="mb-3" style="font-size: 4rem;">⏰</div>
          <h3 class="text-danger fw-bold mb-2">Đơn hàng đã hết hạn</h3>
          <p class="text-muted mb-4">Mã QR hết hiệu lực do không thanh toán hoặc đã bị hủy.</p>
          <button class="btn btn-outline-danger px-4 py-2 fw-bold w-100" @click="closePage">Đóng tab</button>
        </div>
      </div>
    </transition>

    <!-- ===== MAIN CHECKOUT ===== -->
    <div v-if="uiState === 'active' && detail" class="checkout-wrapper">
      
      <!-- HEADER -->
      <header class="co-header">
        <div class="co-brand">
          <i class="bi bi-shop-window"></i> POS36
        </div>
        <div class="text-center">
          <h5 class="mb-1 fw-bold text-dark">Thanh toán mua gói dịch vụ</h5>
          <span class="co-plan-badge">{{ detail.tenGoi }}</span>
        </div>
        <div class="text-end text-muted small d-none d-md-block">
          Cửa hàng: <strong class="text-dark">{{ detail.tenCuaHang }}</strong> <br>
          <span style="font-size: 0.75rem;">(Mã KH: {{ detail.cuaHangId }})</span>
        </div>
      </header>

      <!-- TIMER BAR -->
      <div class="timer-bar">
        <i class="bi bi-hourglass-split text-muted fs-5"></i>
        <span class="text-muted fw-medium">Mã QR hết hạn sau:</span>
        <span class="timer-value" :class="timerClass">{{ timerDisplay }}</span>
        <div class="timer-progress-bg d-none d-sm-block">
          <div class="timer-fill" :style="{ width: timerPercent + '%', backgroundColor: timerColor }"></div>
        </div>
      </div>

      <!-- MAIN CONTENT -->
      <div class="co-grid">
        
        <!-- LEFT: QR -->
        <div class="qr-section">
          <div class="d-flex justify-content-center align-items-center w-100 mb-2 px-2">
            <span class="fw-bold text-secondary text-uppercase" style="letter-spacing: 0.5px; font-size: 0.85rem;">Mã chuyển khoản VietQR</span>
          </div>
          
          <img :src="qrUrl" alt="QR Mã" class="qr-img mb-3">
          
          <div class="text-muted fw-medium small mb-1">Số tiền cần chuyển</div>
          <div class="amount-large mb-3">{{ formatVND(detail.soTienThanhToan) }}</div>
          
          <button class="btn btn-sm btn-outline-primary fw-bold px-4 py-2 w-100" style="border-radius: 10px;" @click="downloadQR" title="Tải mã QR xuống">
            <i class="bi bi-download me-2"></i> Tải ảnh mã QR
          </button>
        </div>

        <!-- RIGHT: INFO -->
        <div>
          <h6 class="fw-bold mb-3 text-secondary text-uppercase" style="letter-spacing: 0.5px; font-size: 0.8rem;">
            <i class="bi bi-bank2 me-2"></i>Tài khoản thụ hưởng
          </h6>
          
          <div class="info-card mb-4">
            <div class="info-row">
              <span class="info-label">Ngân hàng</span>
              <span class="info-value">{{ detail.bankCode }}</span>
            </div>
            <div class="info-row">
              <span class="info-label">Số tài khoản</span>
              <span class="info-value mono">
                {{ detail.bankAccountNo }}
                <div class="copy-btn" @click="copyText(detail.bankAccountNo, 'Số tài khoản')" title="Sao chép">
                  <i class="bi bi-clipboard"></i>
                </div>
              </span>
            </div>
            <div class="info-row">
              <span class="info-label">Chủ tài khoản</span>
              <span class="info-value text-uppercase">{{ detail.bankAccountName }}</span>
            </div>
          </div>

          <h6 class="fw-bold mb-3 text-secondary text-uppercase" style="letter-spacing: 0.5px; font-size: 0.8rem;">
            <i class="bi bi-receipt me-2"></i>Chi tiết thanh toán
          </h6>
          
          <div class="info-card">
            <div class="info-row">
              <span class="info-label">Số tiền</span>
              <span class="info-value text-danger fs-5">{{ formatVND(detail.soTienThanhToan) }}</span>
            </div>
            <div class="info-row highlight-row">
              <span class="info-label">Nội dung CK</span>
              <span class="info-value mono">
                {{ detail.maGiaoDich }}
                <div class="copy-btn" @click="copyText(detail.maGiaoDich, 'Nội dung CK')" title="Sao chép">
                  <i class="bi bi-clipboard"></i>
                </div>
              </span>
            </div>
          </div>
          
          <div class="alert alert-warning mt-3 py-2 px-3 small border-0 d-flex align-items-center" style="background-color: #fffbeb; color: #b45309;">
            <i class="bi bi-exclamation-triangle-fill fs-5 me-3"></i> 
            <div>Bắt buộc nhập chính xác <strong>Nội dung CK</strong> để hệ thống tự động ghi nhận và kích hoạt gói.</div>
          </div>

          <!-- Action area -->
          <div class="action-area">
            <button class="btn btn-cancel btn-custom" @click="confirmCancel">
              <i class="bi bi-x-circle me-1"></i> Hủy
            </button>
            <button class="btn btn-primary-custom btn-custom" @click="checkPaymentManual" :disabled="checking || remainingSecs === 0">
              <span v-if="checking" class="spinner-sm me-2"></span>
              <i v-else class="bi bi-search me-1"></i> 
              {{ checking ? 'Đang kiểm tra...' : 'Kiểm tra thanh toán' }}
            </button>
          </div>

          <!-- Check result message -->
          <transition name="slide-up">
            <div v-if="checkMsg" class="check-result" :class="checkMsgType">
              {{ checkMsg }}
            </div>
          </transition>

          <div class="auto-poll mt-3">
            <span class="pulse-dot"></span> Hệ thống tự động kiểm tra mỗi 5s...
          </div>
          
        </div>
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

const timerClass = computed(() => {
  if (remainingSecs.value < 120) return 'danger';
  if (remainingSecs.value < 300) return 'warning';
  return '';
});

const timerColor = computed(() => {
  if (remainingSecs.value < 120) return '#ef4444';
  if (remainingSecs.value < 300) return '#f59e0b';
  return '#10b981';
});

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

// ===== DOWNLOAD QR =====
const downloadQR = async () => {
  try {
    const response = await fetch(qrUrl.value);
    const blob = await response.blob();
    const link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.download = `QR_ThanhToan_${detail.value?.maGiaoDich || 'POS36'}.png`;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(link.href);
    showCheck('✅ Đã tải mã QR xuống thành công.', 'success');
  } catch (error) {
    // Dự phòng khi bị lỗi CORS
    const link = document.createElement('a');
    link.href = qrUrl.value;
    link.target = '_blank';
    link.download = `QR_ThanhToan_${detail.value?.maGiaoDich || 'POS36'}.png`;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    showCheck('✅ Đã mở tab để tải mã QR.', 'success');
  }
};

// ===== COPY =====
const copyText = async (text, label) => {
  try {
    await navigator.clipboard.writeText(text);
    showCheck(`✅ Đã sao chép ${label}`, 'success');
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
    errorMsg.value = 'Không tìm thấy đơn hàng hoặc đơn không hợp lệ.';
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
      showCheck('⏳ Chưa nhận được thanh toán. Vui lòng kiểm tra lại.', 'warn');
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

/* MÀU CHỦ ĐẠO GIỐNG ADMIN LAYOUT */
:root {
  --bs-primary: #f37021; 
  --bs-primary-rgb: 243, 112, 33;
}

.checkout-root {
  min-height: 100vh;
  background-color: #f0f2f5;
  font-family: 'Inter', sans-serif;
  color: #334155;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 20px;
}

/* OVERLAY STATE (Success / Expired) */
.overlay-screen {
  position: fixed; inset: 0; z-index: 100;
  display: flex; align-items: center; justify-content: center;
  background: rgba(15, 23, 42, 0.75);
  backdrop-filter: blur(8px);
}
.overlay-card {
  background: #ffffff;
  border-radius: 24px; 
  padding: 40px 32px;
  text-align: center; 
  max-width: 400px;
  width: 100%;
}

.checkout-wrapper {
  background: #ffffff;
  border-radius: 20px;
  box-shadow: 0 20px 40px rgba(0,0,0,0.06), 0 1px 3px rgba(0,0,0,0.05);
  overflow: hidden;
  max-width: 880px;
  width: 100%;
  border-top: 5px solid #f37021;
}

/* HEADER */
.co-header {
  padding: 24px 32px;
  border-bottom: 1px solid #e2e8f0;
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.co-brand {
  font-weight: 800;
  font-size: 1.3rem;
  color: #f37021;
  display: flex;
  align-items: center;
  gap: 8px;
}
.co-plan-badge {
  display: inline-block;
  background: rgba(243, 112, 33, 0.12);
  color: #f37021;
  font-weight: 700;
  font-size: 0.85rem;
  padding: 5px 14px;
  border-radius: 20px;
  margin-top: 4px;
}

/* TIMER BAR */
.timer-bar {
  background: #f8fafc;
  padding: 16px 32px;
  border-bottom: 1px solid #e2e8f0;
  display: flex;
  align-items: center;
  gap: 15px;
}
.timer-value {
  font-size: 1.35rem;
  font-weight: 800;
  color: #10b981;
  font-variant-numeric: tabular-nums;
  min-width: 70px;
}
.timer-value.warning { color: #f59e0b; }
.timer-value.danger { color: #ef4444; animation: pulse 1s infinite; }

.timer-progress-bg {
  flex: 1;
  height: 6px;
  background: #e2e8f0;
  border-radius: 6px;
  overflow: hidden;
}
.timer-fill {
  height: 100%;
  background: #10b981;
  transition: width 1s linear, background-color 0.4s;
  border-radius: 6px;
}

/* GRID LAYOUT */
.co-grid {
  display: grid;
  grid-template-columns: 1fr 1.3fr;
  padding: 32px;
  gap: 40px;
}
@media(max-width: 768px) {
  .co-grid { grid-template-columns: 1fr; gap: 24px; padding: 24px; }
  .checkout-root { padding: 0; align-items: flex-start; }
  .checkout-wrapper { border-radius: 0; min-height: 100vh; }
}

/* LEFT: QR SECTION */
.qr-section {
  background: #f8fafc;
  border-radius: 20px;
  padding: 24px;
  text-align: center;
  border: 1px dashed #cbd5e1;
  display: flex;
  flex-direction: column;
  align-items: center;
}
.qr-img {
  width: 100%;
  max-width: 240px;
  border-radius: 16px;
  box-shadow: 0 8px 24px rgba(0,0,0,0.08);
  background: #fff;
  padding: 12px;
  margin-bottom: 20px;
}
.amount-large {
  font-size: 1.8rem;
  font-weight: 800;
  color: #ef4444;
  line-height: 1.2;
}

/* RIGHT: INFO SECTION */
.info-card {
  background: #fff;
  border: 1px solid #e2e8f0;
  border-radius: 16px;
  padding: 0;
  overflow: hidden;
}
.info-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 14px 20px;
  border-bottom: 1px solid #e2e8f0;
}
.info-row:last-child { border-bottom: none; }
.info-label {
  color: #64748b;
  font-size: 0.9rem;
  font-weight: 500;
}
.info-value {
  font-weight: 700;
  color: #0f172a;
  display: flex;
  align-items: center;
}
.info-value.mono {
  font-family: monospace;
  font-size: 1.05rem;
  color: #f37021;
}

.copy-btn {
  background: rgba(243, 112, 33, 0.1);
  color: #f37021;
  width: 28px;
  height: 28px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 6px;
  margin-left: 10px;
  cursor: pointer;
  transition: all 0.2s;
}
.copy-btn:hover {
  background: #f37021;
  color: #fff;
}

.highlight-row {
  background-color: #f8fafc;
}

/* ACTIONS */
.action-area {
  margin-top: 24px;
  display: flex;
  gap: 16px;
}
.btn-custom {
  border-radius: 12px;
  font-weight: 600;
  padding: 12px 20px;
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  transition: all 0.2s ease;
}
.btn-primary-custom {
  background-color: #f37021;
  border-color: #f37021;
  color: #fff;
  box-shadow: 0 4px 14px rgba(243, 112, 33, 0.3);
}
.btn-primary-custom:hover:not(:disabled) {
  background-color: #e06117;
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(243, 112, 33, 0.4);
}
.btn-primary-custom:disabled {
  opacity: 0.6;
}
.btn-cancel {
  background: #fff;
  color: #ef4444;
  border: 1px solid #fca5a5;
}
.btn-cancel:hover {
  background: #fef2f2;
  border-color: #ef4444;
}

.auto-poll {
  font-size: 0.85rem;
  color: #64748b;
  display: flex;
  align-items: center;
  gap: 8px;
  justify-content: center;
  font-weight: 500;
}
.pulse-dot {
  width: 10px; height: 10px; border-radius: 50%;
  background: #10b981;
  animation: pulse-dot 2s infinite;
}
@keyframes pulse-dot { 0%,100%{opacity:1;transform:scale(1)} 50%{opacity:.4;transform:scale(.7)} }
@keyframes pulse { 0%,100%{opacity:1} 50%{opacity:.6} }

/* CHECK RESULT */
.check-result {
  margin-top: 14px; padding: 12px 16px;
  border-radius: 10px; font-size: 0.9rem; font-weight: 500;
  text-align: center;
}
.check-result.success { background: #dcfce7; color: #166534; }
.check-result.error   { background: #fee2e2; color: #991b1b; }
.check-result.warn    { background: #fef3c7; color: #92400e; }
.check-result.info    { background: #e0e7ff; color: #3730a3; }

/* SPINNER */
.spinner-sm {
  width: 18px; height: 18px; border-radius: 50%;
  border: 2px solid rgba(255,255,255,0.4);
  border-top-color: #fff;
  animation: spin .8s linear infinite;
  display: inline-block;
}
.spinner-ring {
  width: 48px; height: 48px; border-radius: 50%;
  border: 4px solid rgba(243, 112, 33, 0.2);
  border-top-color: #f37021;
  animation: spin .9s linear infinite;
}
@keyframes spin { to { transform: rotate(360deg); } }

/* FULLSCREEN CENTER */
.fullscreen-center {
  min-height: 100vh; display: flex;
  flex-direction: column; align-items: center; justify-content: center;
  text-align: center; color: #64748b;
  width: 100%;
}

/* Checkmark SVG animation */
.checkmark-svg { width: 80px; height: 80px; margin: 0 auto; display: block; }
.ck-circle {
  stroke: #10b981; stroke-width: 3;
  stroke-dasharray: 166; stroke-dashoffset: 166;
  animation: stroke 0.6s cubic-bezier(0.65, 0, 0.45, 1) forwards;
}
.ck-path {
  stroke: #10b981; stroke-width: 3.5;
  stroke-dasharray: 48; stroke-dashoffset: 48;
  animation: stroke 0.4s cubic-bezier(0.65, 0, 0.45, 1) 0.5s forwards;
}
@keyframes stroke { to { stroke-dashoffset: 0; } }

/* TRANSITIONS */
.pop-enter-active, .pop-leave-active { transition: opacity .3s, transform .3s; }
.pop-enter-from, .pop-leave-to { opacity: 0; transform: scale(.95); }
.slide-up-enter-active, .slide-up-leave-active { transition: opacity .3s, transform .3s; }
.slide-up-enter-from, .slide-up-leave-to { opacity: 0; transform: translateY(10px); }
</style>
