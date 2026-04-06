<template>
  <div
    class="forgot-password-bg d-flex align-items-center justify-content-center min-vh-100"
  >
    <div class="card border-0 shadow-lg p-4 p-md-5 main-card">
      <div class="text-center mb-5">
        <h2 class="fw-bold mb-2 text-dark">Quên mật khẩu?</h2>
        <p class="text-muted" style="font-size: 0.95rem">
          Đừng lo lắng, chúng tôi sẽ giúp bạn thiết lập lại mật khẩu chỉ trong
          vài phút.
        </p>
      </div>

      <div class="position-relative mb-5 px-2">
        <div class="progress-line-container">
          <div class="progress-line" :style="{ width: progressWidth }"></div>
        </div>

        <div class="d-flex justify-content-between position-relative z-2">
          <div
            class="stepper-item"
            :class="{ active: step === 1, completed: step > 1 }"
          >
            <div class="step-circle">1</div>
            <div class="step-label mt-2">Nhập Email/SĐT</div>
          </div>
          <div
            class="stepper-item"
            :class="{ active: step === 2, completed: step > 2 }"
          >
            <div class="step-circle">2</div>
            <div class="step-label mt-2">Xác minh OTP</div>
          </div>
          <div
            class="stepper-item"
            :class="{ active: step === 3, completed: step > 3 }"
          >
            <div class="step-circle">3</div>
            <div class="step-label mt-2">Đặt lại mật khẩu</div>
          </div>
          <div
            class="stepper-item"
            :class="{ active: step === 4, completed: step > 4 }"
          >
            <div class="step-circle">4</div>
            <div class="step-label mt-2">Hoàn tất</div>
          </div>
        </div>
      </div>

      <form v-if="step === 1" @submit.prevent="requestOtp">
        <div class="mb-2">
          <label
            class="form-label fw-bold text-secondary"
            style="font-size: 0.85rem"
            >Email, Số điện thoại hoặc Tên đăng nhập</label
          >
          <div class="input-group input-group-lg">
            <span class="input-group-text bg-light border-end-0"
              ><i class="bi bi-envelope text-muted"></i
            ></span>
            <input
              type="text"
              class="form-control bg-light border-start-0 ps-0 fs-6"
              v-model="form.username"
              placeholder="name@company.com hoặc 09xx xxx xxx"
              required
              autofocus
            />
          </div>
        </div>
        <p class="text-muted small mb-4">
          Chúng tôi sẽ gửi một mã xác minh gồm 6 chữ số đến địa chỉ này.
        </p>

        <button
          type="submit"
          class="btn btn-brand w-100 py-3 fw-bold mb-4"
          :disabled="isLoading"
        >
          <span
            v-if="isLoading"
            class="spinner-border spinner-border-sm me-2"
          ></span>
          Tiếp theo <i class="bi bi-arrow-right ms-1"></i>
        </button>
      </form>

      <form v-if="step === 2" @submit.prevent="verifyOtp">
        <div class="mb-4 text-center">
          <label
            class="form-label fw-bold text-secondary mb-3"
            style="font-size: 0.85rem"
            >Nhập mã gồm 6 chữ số đã gửi</label
          >
          <input
            type="text"
            class="form-control form-control-lg bg-light text-center fw-bold tracking-widest fs-4 mx-auto"
            style="max-width: 250px"
            v-model="form.otp"
            placeholder="_ _ _ _ _ _"
            maxlength="6"
            required
            autofocus
          />
        </div>

        <button
          type="submit"
          class="btn btn-brand w-100 py-3 fw-bold mb-4"
          :disabled="form.otp.length < 6"
        >
          Tiếp theo <i class="bi bi-arrow-right ms-1"></i>
        </button>
      </form>

      <form v-if="step === 3" @submit.prevent="resetPassword">
        <div class="mb-3">
          <label
            class="form-label fw-bold text-secondary"
            style="font-size: 0.85rem"
            >Mật khẩu mới</label
          >
          <input
            type="password"
            class="form-control form-control-lg bg-light fs-6"
            v-model="form.newPassword"
            placeholder="Nhập mật khẩu mới"
            minlength="6"
            required
            autofocus
          />
        </div>
        <div class="mb-4">
          <label
            class="form-label fw-bold text-secondary"
            style="font-size: 0.85rem"
            >Xác nhận mật khẩu</label
          >
          <input
            type="password"
            class="form-control form-control-lg bg-light fs-6"
            v-model="form.confirmPassword"
            placeholder="Nhập lại mật khẩu mới"
            minlength="6"
            required
          />
          <small
            v-if="form.newPassword && form.newPassword !== form.confirmPassword"
            class="text-danger mt-1 d-block"
          >
            * Mật khẩu xác nhận không khớp
          </small>
        </div>

        <button
          type="submit"
          class="btn btn-brand w-100 py-3 fw-bold mb-4"
          :disabled="isLoading || form.newPassword !== form.confirmPassword"
        >
          <span
            v-if="isLoading"
            class="spinner-border spinner-border-sm me-2"
          ></span>
          Cập nhật mật khẩu
        </button>
      </form>

      <div v-if="step === 4" class="text-center py-2">
        <div class="mb-4">
          <i
            class="bi bi-check-circle-fill text-success"
            style="font-size: 4.5rem"
          ></i>
        </div>
        <h4 class="fw-bold mb-2">Hoàn tất!</h4>
        <p class="text-muted mb-4">
          Mật khẩu của bạn đã được thiết lập lại thành công. Vui lòng đăng nhập
          lại hệ thống.
        </p>
        <router-link to="/login" class="btn btn-brand w-100 py-3 fw-bold">
          Quay lại đăng nhập
        </router-link>
      </div>

      <div class="text-center" v-if="step < 4">
        <router-link
          to="/login"
          class="text-decoration-none text-muted fw-medium back-link"
        >
          <i class="bi bi-arrow-left me-1"></i> Quay lại đăng nhập
        </router-link>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from "vue";
import axios from "axios";
import Swal from "sweetalert2";

// Quản lý biến trạng thái
const step = ref(1);
const isLoading = ref(false);

const form = ref({
  username: "",
  otp: "",
  newPassword: "",
  confirmPassword: "",
});

// Tính toán độ dài thanh Cam tùy theo Bước hiện tại
const progressWidth = computed(() => {
  if (step.value === 1) return "0%";
  if (step.value === 2) return "33.33%";
  if (step.value === 3) return "66.66%";
  return "100%";
});

// [BƯỚC 1] Gọi API yêu cầu cấp OTP
const requestOtp = async () => {
  isLoading.value = true;
  try {
    await axios.post("http://localhost:5198/api/Auth/forgot-password", {
      TenDangNhap: form.value.username,
    });
    // Gọi API xong thì đẩy sang Bước 2 (Nhập OTP)
    step.value = 2;
  } catch (error) {
    Swal.fire({ icon: "error", title: "Lỗi", text: "Không thể gửi yêu cầu." });
  } finally {
    isLoading.value = false;
  }
};

// [BƯỚC 2] Bấm nút Tiếp theo ở màn OTP
const verifyOtp = () => {
  // Chỉ chuyển sang giao diện nhập Mật khẩu mới
  // Việc xác thực sẽ gom chung ở Bước 3 khi gọi API reset
  step.value = 3;
};

// [BƯỚC 3] Gọi API Đổi mật khẩu
const resetPassword = async () => {
  if (form.value.newPassword !== form.value.confirmPassword) return;

  isLoading.value = true;
  try {
    await axios.post("http://localhost:5198/api/Auth/reset-password", {
      TenDangNhap: form.value.username,
      OtpCode: form.value.otp,
      MatKhauMoi: form.value.newPassword,
    });
    // Thành công thì nhảy sang Bước 4 (Hoàn tất)
    step.value = 4;
  } catch (error) {
    Swal.fire({
      icon: "error",
      title: "Từ chối",
      text: error.response?.data || "Mã OTP không hợp lệ hoặc đã hết hạn!",
      confirmButtonColor: "#e65c00",
    }).then(() => {
      // Nếu OTP sai, cho người dùng lùi lại Bước 2 để nhập lại OTP
      step.value = 2;
      form.value.otp = "";
    });
  } finally {
    isLoading.value = false;
  }
};
</script>

<style scoped>
/* Nền màu sương sương cực sang */
.forgot-password-bg {
  background: radial-gradient(circle at center, #ffffff 0%, #faf8f5 100%);
}

.main-card {
  max-width: 500px;
  width: 100%;
  border-radius: 20px;
}

/* Nút bấm Cam chuẩn hình sếp gửi */
.btn-brand {
  background-color: #e65c00;
  color: white;
  border: none;
  transition: all 0.3s;
}
.btn-brand:hover {
  background-color: #cc5200;
  color: white;
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(230, 92, 0, 0.2);
}
.btn-brand:disabled {
  background-color: #f0a675;
  transform: none;
  box-shadow: none;
}

/* ==========================================
   CSS CHO THANH TIẾN TRÌNH (STEPPER)
   ========================================== */
.progress-line-container {
  position: absolute;
  top: 15px;
  left: 12.5%;
  width: 75%;
  height: 2px;
  background-color: #e0e0e0;
  z-index: 1;
}
.progress-line {
  height: 100%;
  background-color: #e65c00;
  transition: width 0.4s ease-in-out;
}
.stepper-item {
  position: relative;
  z-index: 2;
  display: flex;
  flex-direction: column;
  align-items: center;
  width: 25%;
}
.step-circle {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  background-color: #e0e0e0;
  color: #fff;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: bold;
  font-size: 0.9rem;
  transition: all 0.4s ease;
}
.stepper-item.active .step-circle,
.stepper-item.completed .step-circle {
  background-color: #e65c00;
  box-shadow: 0 0 0 4px rgba(230, 92, 0, 0.15);
}
.step-label {
  font-size: 0.75rem;
  color: #a0a0a0;
  text-align: center;
  font-weight: 500;
  transition: color 0.3s;
}
.stepper-item.active .step-label {
  color: #e65c00;
  font-weight: bold;
}
.stepper-item.completed .step-label {
  color: #333;
}

/* Nhập OTP dãn chữ */
.tracking-widest {
  letter-spacing: 0.5em;
}

/* Link quay lại */
.back-link:hover {
  color: #e65c00 !important;
}
</style>
