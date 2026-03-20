<script setup>
import { ref, inject } from "vue";
import { useRouter } from "vue-router";
import axios from "axios";
import PublicNavbar from "../components/PublicNavbar.vue";

const router = useRouter();
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
    const res = await axios.post(
      "http://localhost:5198/api/Auth/login",
      form.value,
    );

    const data = res.data;

    // 1. LƯU THÔNG TIN VÀO LOCALSTORAGE
    // (Giữ nguyên key pos36_token của em để không hỏng cấu hình cũ)
    localStorage.setItem("pos36_token", data.token);
    localStorage.setItem("pos36_role", data.role);

    // Lưu thêm Tên nhân viên và ID cửa hàng để các màn hình khác dùng
    localStorage.setItem(
      "tenNhanVien",
      data.tenNhanVien || form.value.tenDangNhap,
    );
    if (data.cuaHangId) localStorage.setItem("cuaHangId", data.cuaHangId);

    // 2. THÔNG BÁO ĐĂNG NHẬP THÀNH CÔNG
    swal.fire({
      icon: "success",
      title: "Đăng nhập thành công!",
      text: `Xin chào ${data.tenNhanVien || form.value.tenDangNhap}`,
      timer: 1500,
      showConfirmButton: false,
    });

    // 3. CHIA LUỒNG THEO VAI TRÒ (ROLE-BASED ROUTING)
    const role = data.role;

    if (role === "Admin" || role === "QuanLy") {
      // Quản lý/Chủ quán thì vào trang quản trị tổng
      router.push("/admin");
    } else if (role === "ThuNgan") {
      // Thu ngân hoặc Order thì bay thẳng ra mặt trận Bán hàng
      router.push("/pos");
    } else if (role === "Order") {
      router.push("/order");
    } else if (role === "Bep") {
      // Bếp thì bay thẳng vào màn hình làm món
      router.push("/kitchen");
    } else {
      // Mặc định nếu chưa rõ quyền
      router.push("/admin");
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
  <div class="login-wrapper">
    <PublicNavbar />

    <div
      class="d-flex align-items-center justify-content-center min-vh-100 py-5"
    >
      <div
        class="card shadow-lg border-0 rounded-4"
        style="width: 100%; max-width: 400px"
      >
        <div class="card-body p-5">
          <div class="text-center mb-4">
            <h2 class="fw-bold text-primary">
              <i class="bi bi-shop"></i> POS36
            </h2>
            <p class="text-muted">Quản lý cửa hàng</p>
          </div>

          <form @submit.prevent="handleLogin">
            <div class="mb-3">
              <label class="form-label fw-bold">Tên đăng nhập</label>
              <input
                type="text"
                class="form-control form-control-lg rounded-3"
                :class="{ 'is-invalid': errors.tenDangNhap }"
                v-model="form.tenDangNhap"
                placeholder="Nhập tài khoản Admin hoặc Nhân viên"
              />
              <div class="invalid-feedback">{{ errors.tenDangNhap }}</div>
            </div>

            <div class="mb-4">
              <label class="form-label fw-bold">Mật khẩu</label>
              <input
                type="password"
                class="form-control form-control-lg rounded-3"
                :class="{ 'is-invalid': errors.matKhau }"
                v-model="form.matKhau"
                placeholder="Nhập mật khẩu"
              />
              <div class="invalid-feedback">{{ errors.matKhau }}</div>
            </div>

            <button
              type="submit"
              class="btn btn-primary btn-lg w-100 py-3 rounded-3 fw-bold fs-5 shadow-sm"
              :disabled="isLoading"
            >
              <span
                v-if="isLoading"
                class="spinner-border spinner-border-sm me-2"
              ></span>
              {{ isLoading ? "Đang xác thực..." : "ĐĂNG NHẬP" }}
            </button>
          </form>

          <div class="text-center mt-4 pt-3 border-top">
            <span class="text-muted">Chưa có cửa hàng?</span>
            <router-link
              to="/register"
              class="text-decoration-none fw-bold ms-1"
              >Đăng ký mới</router-link
            >
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.login-wrapper {
  background-color: #f8f9fa;
  min-height: 100vh;
}
</style>
