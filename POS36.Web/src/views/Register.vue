<script setup>
import { ref, inject } from "vue";
import { useRouter } from "vue-router";
import axios from "axios";
import PublicNavbar from "../components/PublicNavbar.vue";

const router = useRouter();
const swal = inject("$swal"); // Dùng SweetAlert2 đã cấu hình ở main.js

const form = ref({
  tenCuaHang: "",
  soDienThoai: "",
  tenDangNhap: "",
  matKhau: "",
});

// Biến lưu trữ lỗi cho từng trường
const errors = ref({});
const isLoading = ref(false);

// Hàm kiểm tra Validation (Hợp lệ hóa dữ liệu)
const validateForm = () => {
  errors.value = {};
  let isValid = true;

  if (!form.value.tenCuaHang.trim()) {
    errors.value.tenCuaHang = "Vui lòng nhập tên cửa hàng!";
    isValid = false;
  }

  if (!form.value.soDienThoai.trim()) {
    errors.value.soDienThoai = "Vui lòng nhập số điện thoại!";
    isValid = false;
  } else if (!/^[0-9]+$/.test(form.value.soDienThoai)) {
    errors.value.soDienThoai = "Số điện thoại không hợp lệ!";
    isValid = false;
  }

  if (!form.value.tenDangNhap.trim()) {
    errors.value.tenDangNhap = "Vui lòng nhập tên đăng nhập!";
    isValid = false;
  }

  if (!form.value.matKhau) {
    errors.value.matKhau = "Vui lòng nhập mật khẩu!";
    isValid = false;
  } else if (form.value.matKhau.length < 6) {
    errors.value.matKhau = "Mật khẩu phải từ 6 ký tự!";
    isValid = false;
  }

  return isValid;
};

const handleRegister = async () => {
  if (!validateForm()) return;

  isLoading.value = true;
  try {
    // Gọi API Đăng ký Cửa hàng & Tài khoản thật
    await axios.post("http://localhost:5198/api/Auth/register", form.value);

    // Thông báo thành công rực rỡ
    swal
      .fire({
        icon: "success",
        title: "Tuyệt vời!",
        text: "Đăng ký cửa hàng thành công. Vui lòng đăng nhập!",
        confirmButtonText: "Đăng nhập ngay",
      })
      .then(() => {
        // Chuyển sang trang Đăng nhập
        router.push("/login");
      });
  } catch (error) {
    // Thông báo lỗi bay ra như POS365
    let msg = "Không thể kết nối máy chủ!";
    if (error.response && error.response.data) {
      msg = error.response.data; // Lấy câu báo lỗi từ Backend C#
    }
    swal.fire("Đăng ký thất bại", msg, "error");
  } finally {
    isLoading.value = false;
  }
};
</script>

<template>
  <div class="register-wrapper">
    <PublicNavbar />

    <div
      class="d-flex align-items-center justify-content-center min-vh-100 py-5"
    >
      <div
        class="card shadow-lg border-0 rounded-4"
        style="width: 100%; max-width: 450px"
      >
        <div class="card-body p-5">
          <div class="text-center mb-4">
            <h2 class="fw-bold text-primary">Tạo Cửa Hàng Mới</h2>
            <p class="text-muted">Dùng thử POS36 miễn phí</p>
          </div>

          <form @submit.prevent="handleRegister">
            <div class="mb-3">
              <label class="form-label fw-bold"
                >Tên cửa hàng (Quán ăn/Cafe)</label
              >
              <input
                type="text"
                class="form-control form-control-lg rounded-3"
                :class="{ 'is-invalid': errors.tenCuaHang }"
                v-model="form.tenCuaHang"
                placeholder="VD: Lẩu Đức"
              />
              <div class="invalid-feedback">{{ errors.tenCuaHang }}</div>
            </div>

            <div class="mb-3">
              <label class="form-label fw-bold">Số điện thoại liên hệ</label>
              <input
                type="text"
                class="form-control form-control-lg rounded-3"
                :class="{ 'is-invalid': errors.soDienThoai }"
                v-model="form.soDienThoai"
                placeholder="VD: 0987654321"
              />
              <div class="invalid-feedback">{{ errors.soDienThoai }}</div>
            </div>

            <div class="mb-3">
              <label class="form-label fw-bold">Tên đăng nhập (Admin)</label>
              <input
                type="text"
                class="form-control form-control-lg rounded-3"
                :class="{ 'is-invalid': errors.tenDangNhap }"
                v-model="form.tenDangNhap"
                placeholder="VD: admin_ duc"
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
                placeholder="Mật khẩu từ 6 ký tự"
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
              {{ isLoading ? "Đang khởi tạo cửa hàng..." : "ĐĂNG KÝ NGAY" }}
            </button>
          </form>

          <div class="text-center mt-4 pt-3 border-top">
            <span class="text-muted">Đã có cửa hàng?</span>
            <router-link to="/login" class="text-decoration-none fw-bold ms-1"
              >Đăng nhập</router-link
            >
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.register-wrapper {
  background-color: #f8f9fa;
  min-height: 100vh;
}
</style>
