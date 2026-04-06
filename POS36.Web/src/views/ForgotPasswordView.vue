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
            <div class="step-label mt-2">Nhập Email</div>
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
          >
            Email đăng ký của Chủ cửa hàng
          </label>
          <div class="input-group input-group-lg">
            <span class="input-group-text bg-light border-end-0">
              <i class="bi bi-envelope text-muted"></i>
            </span>
            <input
              type="email"
              class="form-control bg-light border-start-0 ps-0 fs-6"
              v-model="form.email"
              placeholder="VD: name@company.com"
              required
              autofocus
            />
          </div>
        </div>
        <p class="text-muted small mb-4">
          Chúng tôi sẽ gửi một mã xác minh gồm 6 chữ số đến địa chỉ Email này.
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
          >
            Nhập mã gồm 6 chữ số đã gửi
          </label>
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
import { ref, computed, inject } from "vue";
import { useRouter } from "vue-router";
import axios from "axios";
import emailjs from "@emailjs/browser";

const router = useRouter();
const swal = inject("$swal");

const step = ref(1);
const isLoading = ref(false);

const form = ref({
  email: "",
  username: "",
  otp: "",
  newPassword: "",
  confirmPassword: "",
});

const progressWidth = computed(() => {
  if (step.value === 1) return "0%";
  if (step.value === 2) return "33.33%";
  if (step.value === 3) return "66.66%";
  return "100%";
});

// ==========================================
// BƯỚC 1: LẤY OTP & BẮN EMAIL
// ==========================================
const requestOtp = async () => {
  if (!form.value.email.trim()) {
    swal.fire("Chú ý", "Vui lòng nhập Email!", "warning");
    return;
  }

  isLoading.value = true;
  try {
    const res = await axios.post("/api/Auth/forgot-password", {
      Email: form.value.email,
    });

    form.value.username = res.data.tenDangNhap;
    const otpCode = res.data.otp;
    const fullName = res.data.tenNhanVien;

    // Gửi Email thật qua EmailJS
    await emailjs.send(
      "service_65xya5u",
      "template_a63e1vv",
      {
        to_email: form.value.email,
        to_name: fullName,
        otp_code: otpCode,
      },
      "Zjm65dyIcuEbthcT3",
    );

    step.value = 2;
  } catch (error) {
    console.error("Chi tiết lỗi:", error);
    swal.fire(
      "Lỗi",
      error.response?.data ||
        "Không thể gửi Email lúc này. Vui lòng kiểm tra lại cấu hình EmailJS!",
      "error",
    );
  } finally {
    isLoading.value = false;
  }
};

// ==========================================
// BƯỚC 2: CHUYỂN SANG ĐẶT MẬT KHẨU
// ==========================================
const verifyOtp = () => {
  if (form.value.otp.length < 6) {
    swal.fire("Chú ý", "Vui lòng nhập đủ 6 số OTP!", "warning");
    return;
  }
  step.value = 3;
};

// ==========================================
// BƯỚC 3: ĐẶT LẠI MẬT KHẨU
// ==========================================
const resetPassword = async () => {
  if (form.value.newPassword !== form.value.confirmPassword) {
    swal.fire("Chú ý", "Mật khẩu xác nhận không khớp!", "warning");
    return;
  }

  isLoading.value = true;
  try {
    await axios.post("/api/Auth/reset-password", {
      TenDangNhap: form.value.username,
      OtpCode: form.value.otp,
      MatKhauMoi: form.value.newPassword,
    });

    step.value = 4;
  } catch (error) {
    swal
      .fire({
        icon: "error",
        title: "Từ chối",
        text: error.response?.data || "Mã OTP không hợp lệ hoặc đã hết hạn!",
        confirmButtonColor: "#e65c00",
      })
      .then(() => {
        step.value = 2;
        form.value.otp = "";
      });
  } finally {
    isLoading.value = false;
  }
};
</script>

<style scoped>
.forgot-password-bg {
  background: radial-gradient(circle at center, #ffffff 0%, #faf8f5 100%);
}

.main-card {
  max-width: 500px;
  width: 100%;
  border-radius: 20px;
}

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

.tracking-widest {
  letter-spacing: 0.5em;
}

.back-link:hover {
  color: #e65c00 !important;
}
</style>
