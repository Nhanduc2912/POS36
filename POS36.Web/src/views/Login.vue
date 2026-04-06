<script setup>
import { ref, inject } from "vue";
import axios from "axios";

const swal = inject("$swal");

const form = ref({
  tenDangNhap: "",
  matKhau: "",
});

const errors = ref({});
const isLoading = ref(false);

const validateForm = () => {
  errors.value = {};
  let isValid = true;

  if (!form.value.tenDangNhap.trim()) {
    errors.value.tenDangNhap = "Vui lòng nhập tên đăng nhập!";
    isValid = false;
  }

  if (!form.value.matKhau) {
    errors.value.matKhau = "Vui lòng nhập mật khẩu!";
    isValid = false;
  }

  return isValid;
};

const handleLogin = async () => {
  if (!validateForm()) return;

  isLoading.value = true;
  try {
    const res = await axios.post("/api/Auth/login", form.value);
    const data = res.data;

    // 1. Quét rác cũ
    localStorage.clear();

    // 2. Lưu Token và thông tin MỚI CÁU
    localStorage.setItem("pos36_token", data.token);
    localStorage.setItem("pos36_role", data.role);
    localStorage.setItem(
      "tenNhanVien",
      data.tenNhanVien || form.value.tenDangNhap,
    );
    if (data.cuaHangId) localStorage.setItem("cuaHangId", data.cuaHangId);

    // 3. THÔNG BÁO THÀNH CÔNG
    swal.fire({
      icon: "success",
      title: "Đăng nhập thành công!",
      text: `Xin chào ${data.tenNhanVien || form.value.tenDangNhap}`,
      timer: 1500,
      showConfirmButton: false,
    });

    // 4. CHIA LUỒNG THEO VAI TRÒ
    const role = data.role;
    if (role === "Admin" || role === "QuanLy" || role === "ChuCuaHang") {
      window.location.href = "/admin/";
    } else if (role === "ThuNgan") {
      window.location.href = "/pos";
    } else if (role === "Order") {
      window.location.href = "/order";
    } else if (role === "Bep") {
      window.location.href = "/kitchen";
    } else {
      window.location.href = "/admin/";
    }
  } catch (error) {
    swal.fire(
      "Đăng nhập thất bại",
      error.response?.data?.message || "Sai tên đăng nhập hoặc mật khẩu!",
      "error",
    );
  } finally {
    isLoading.value = false;
  }
};
</script>

<template>
  <div class="container-fluid p-0 login-container">
    <div class="row g-0 min-vh-100">
      <div class="col-lg-6 d-none d-lg-flex position-relative left-panel">
        <div
          class="overlay d-flex flex-column justify-content-between p-5 w-100 h-100 text-white"
        >
          <div class="logo-box">
            <router-link to="/" class="text-decoration-none">
              <h2 class="fw-bold tracking-tight">POS36</h2>
            </router-link>
          </div>
          <div class="content-box pe-5 mb-5">
            <h1 class="display-5 fw-bolder mb-3 lh-sm text-shadow">
              Nền tảng quản lý ẩm<br />thực hiện đại.
            </h1>
            <p class="fs-6 opacity-75 pe-5 lh-lg">
              Dành riêng cho những chủ nhà hàng khát khao sự hoàn mỹ trong từng
              quy trình vận hành. Tăng tốc phục vụ, tối ưu doanh thu với công
              nghệ từ POS36.
            </p>
          </div>
        </div>
      </div>

      <div
        class="col-12 col-lg-6 d-flex align-items-center justify-content-center bg-white right-panel"
      >
        <div class="form-wrapper w-100" style="max-width: 420px; padding: 2rem">
          <div class="d-lg-none mb-5 text-center">
            <h2 class="fw-bold text-orange">POS36</h2>
          </div>

          <div class="mb-5">
            <h2 class="fw-bold text-dark mb-2">Chào mừng trở lại</h2>
            <p class="text-muted small">
              Đăng nhập để quản lý nhà hàng của bạn
            </p>
          </div>

          <form @submit.prevent="handleLogin">
            <div class="mb-4">
              <label class="form-label fw-semibold small text-dark mb-2"
                >Tên đăng nhập</label
              >
              <div
                class="input-group custom-input-group"
                :class="{ 'border-danger': errors.tenDangNhap }"
              >
                <span
                  class="input-group-text bg-transparent border-0 text-muted ps-3"
                >
                  <i class="bi bi-person"></i>
                </span>
                <input
                  type="text"
                  class="form-control border-0 bg-transparent shadow-none py-2"
                  v-model="form.tenDangNhap"
                  placeholder="Nhập tên đăng nhập của bạn"
                />
              </div>
              <div
                class="text-danger small mt-1 fw-bold"
                v-if="errors.tenDangNhap"
              >
                {{ errors.tenDangNhap }}
              </div>
            </div>

            <div class="mb-2">
              <label class="form-label fw-semibold small text-dark mb-2"
                >Mật khẩu</label
              >
              <div
                class="input-group custom-input-group"
                :class="{ 'border-danger': errors.matKhau }"
              >
                <span
                  class="input-group-text bg-transparent border-0 text-muted ps-3"
                >
                  <i class="bi bi-lock"></i>
                </span>
                <input
                  type="password"
                  class="form-control border-0 bg-transparent shadow-none py-2"
                  v-model="form.matKhau"
                  placeholder="••••••••"
                />
              </div>
              <div class="text-danger small mt-1 fw-bold" v-if="errors.matKhau">
                {{ errors.matKhau }}
              </div>
            </div>

            <div class="text-end mb-4">
              <router-link
                to="/forgot-password"
                class="text-orange fw-bold text-decoration-none small"
              >
                Quên mật khẩu?
              </router-link>
            </div>

            <button
              type="submit"
              class="btn btn-orange w-100 py-3 fw-bold d-flex justify-content-center align-items-center gap-2 mb-4"
              :disabled="isLoading"
            >
              <span
                v-if="isLoading"
                class="spinner-border spinner-border-sm"
              ></span>
              {{ isLoading ? "ĐANG XÁC THỰC..." : "Đăng nhập" }}
              <i v-if="!isLoading" class="bi bi-arrow-right fs-5"></i>
            </button>

            <div class="text-center">
              <span class="text-muted small">Chưa có tài khoản? </span>
              <router-link
                to="/register"
                class="text-orange fw-bold text-decoration-none small"
                >Đăng ký ngay</router-link
              >
            </div>
          </form>

          <div class="mt-5 pt-5">
            <p
              class="text-muted text-uppercase fw-bold mb-2"
              style="font-size: 0.65rem; letter-spacing: 1.5px"
            >
              POS36 Ecosystem
            </p>
            <div class="d-flex gap-2 opacity-50">
              <div class="bg-secondary" style="width: 24px; height: 24px"></div>
              <div class="bg-secondary" style="width: 24px; height: 24px"></div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
@import url("https://fonts.googleapis.com/css2?family=Plus+Jakarta+Sans:wght@500;600;700;800&display=swap");

.login-container {
  font-family: "Plus Jakarta Sans", sans-serif;
  background-color: #ffffff;
}

.left-panel {
  background-image: url("https://images.unsplash.com/photo-1556155092-490a1ba16284?q=80&w=2070&auto=format&fit=crop");
  background-size: cover;
  background-position: center;
}

.overlay {
  background: linear-gradient(
    135deg,
    rgba(200, 80, 0, 0.4) 0%,
    rgba(204, 85, 0, 0.9) 100%
  );
  z-index: 1;
}

.text-shadow {
  text-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}

.custom-input-group {
  background-color: #f7f7f9;
  border-radius: 8px;
  border: 1px solid transparent;
  transition: all 0.2s ease-in-out;
}

.custom-input-group:focus-within {
  border-color: #e65c00;
  background-color: #fff;
  box-shadow: 0 0 0 3px rgba(230, 92, 0, 0.1);
}

.btn-orange {
  background-color: #e65c00;
  color: white;
  border-radius: 8px;
  border: none;
  transition: all 0.2s;
}

.btn-orange:hover {
  background-color: #cc5200;
  color: white;
}

.btn-orange:active {
  transform: scale(0.98);
}

.text-orange {
  color: #e65c00;
}

.text-orange:hover {
  color: #cc5200;
  text-decoration: underline !important;
}
</style>
