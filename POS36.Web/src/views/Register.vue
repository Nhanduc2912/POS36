<script setup>
import { ref, inject } from "vue";
import { useRouter } from "vue-router";
import axios from "axios";

const router = useRouter();
const swal = inject("$swal");

const form = ref({
  tenCuaHang: "",
  soDienThoai: "",
  email: "", // ĐÃ THÊM TRƯỜNG EMAIL
  tenDangNhap: "",
  matKhau: "",
});

const agreeTerms = ref(false);
const errors = ref({});
const isLoading = ref(false);

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

  // VALIDATE EMAIL
  if (!form.value.email.trim()) {
    errors.value.email = "Vui lòng nhập Email!";
    isValid = false;
  } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.value.email)) {
    errors.value.email = "Email không đúng định dạng!";
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

  if (!agreeTerms.value) {
    swal.fire("Chú ý", "Bạn cần đồng ý với Điều khoản dịch vụ!", "warning");
    isValid = false;
  }

  return isValid;
};

const handleRegister = async () => {
  if (!validateForm()) return;

  isLoading.value = true;
  try {
    await axios.post("/api/Auth/register", form.value);

    swal
      .fire({
        icon: "success",
        title: "Tuyệt vời!",
        text: "Đăng ký cửa hàng thành công. Vui lòng đăng nhập!",
        confirmButtonText: "Đăng nhập ngay",
        confirmButtonColor: "#e65c00",
      })
      .then(() => {
        router.push("/login");
      });
  } catch (error) {
    let msg = "Không thể kết nối máy chủ!";
    if (error.response && error.response.data) msg = error.response.data;
    swal.fire("Đăng ký thất bại", msg, "error");
  } finally {
    isLoading.value = false;
  }
};
</script>

<template>
  <div class="container-fluid p-0 register-container">
    <div class="row g-0 min-vh-100">
      <div class="col-lg-5 d-none d-lg-flex position-relative left-panel">
        <div
          class="overlay d-flex flex-column justify-content-between p-5 w-100 h-100 text-white"
        >
          <div class="logo-box">
            <router-link to="/" class="text-decoration-none">
              <h2 class="fw-bold tracking-tight">POS36</h2>
            </router-link>
          </div>
          <div class="content-box pe-4 mb-4">
            <h1 class="display-5 fw-bolder mb-4 lh-sm text-shadow">
              Nâng tầm quản lý <br /><span class="text-white-50"
                >Cửa hàng của bạn</span
              >
            </h1>
            <p class="fs-6 opacity-90 lh-lg pe-3">
              Tham gia cùng 5,000+ chủ nhà hàng đã tối ưu hóa dịch vụ và tăng
              trưởng doanh thu với hệ thống quản lý thông minh của POS36.
            </p>
            <div
              class="mt-5 bg-white bg-opacity-10 p-4 rounded-4 border border-light border-opacity-25 backdrop-blur"
            >
              <div class="d-flex align-items-center gap-3 mb-3">
                <img
                  src="https://images.unsplash.com/photo-1583394838336-acd977736f90?q=80&w=100&auto=format&fit=crop"
                  class="rounded-circle border border-2 border-white"
                  width="45"
                  height="45"
                  alt="Avatar"
                />
                <div>
                  <div class="fw-bold fs-6">Anh Hoàng</div>
                  <div class="small text-white-50">Chủ chuỗi Le Café</div>
                </div>
              </div>
              <p class="small fst-italic mb-0 opacity-75">
                "POS36 không chỉ là một phần mềm, đó là người bạn đồng hành giúp
                tối ưu hóa mọi quy trình từ bếp đến bàn."
              </p>
            </div>
          </div>
        </div>
      </div>

      <div
        class="col-12 col-lg-7 d-flex align-items-center justify-content-center bg-white right-panel"
      >
        <div class="form-wrapper w-100" style="max-width: 500px; padding: 2rem">
          <div class="d-lg-none mb-4 text-center">
            <router-link to="/" class="text-decoration-none">
              <h2 class="fw-bold text-orange">POS36</h2>
            </router-link>
          </div>

          <div class="mb-5 text-center text-lg-start">
            <h2 class="fw-bold text-dark mb-2">Bắt đầu ngay hôm nay</h2>
            <p class="text-muted small">
              Thiết lập tài khoản quản lý của bạn trong 60 giây.
            </p>
          </div>

          <form @submit.prevent="handleRegister">
            <div class="mb-4">
              <label class="form-label fw-semibold small text-muted mb-2"
                >Tên cửa hàng</label
              >
              <div
                class="input-group custom-input-group"
                :class="{ 'border-danger': errors.tenCuaHang }"
              >
                <span
                  class="input-group-text bg-transparent border-0 text-muted ps-3"
                  ><i class="bi bi-shop"></i
                ></span>
                <input
                  type="text"
                  class="form-control border-0 bg-transparent shadow-none py-2.5"
                  v-model="form.tenCuaHang"
                  placeholder="Ví dụ: Highland Coffee"
                />
              </div>
              <div
                class="text-danger small mt-1 fw-bold"
                v-if="errors.tenCuaHang"
              >
                {{ errors.tenCuaHang }}
              </div>
            </div>

            <div class="row g-3 mb-4">
              <div class="col-md-6">
                <label class="form-label fw-semibold small text-muted mb-2"
                  >Email chủ quán</label
                >
                <div
                  class="input-group custom-input-group"
                  :class="{ 'border-danger': errors.email }"
                >
                  <span
                    class="input-group-text bg-transparent border-0 text-muted ps-3"
                    ><i class="bi bi-envelope"></i
                  ></span>
                  <input
                    type="email"
                    class="form-control border-0 bg-transparent shadow-none py-2.5"
                    v-model="form.email"
                    placeholder="name@company.com"
                  />
                </div>
                <div class="text-danger small mt-1 fw-bold" v-if="errors.email">
                  {{ errors.email }}
                </div>
              </div>

              <div class="col-md-6">
                <label class="form-label fw-semibold small text-muted mb-2"
                  >Số điện thoại</label
                >
                <div
                  class="input-group custom-input-group"
                  :class="{ 'border-danger': errors.soDienThoai }"
                >
                  <span
                    class="input-group-text bg-transparent border-0 text-muted ps-3"
                    ><i class="bi bi-telephone"></i
                  ></span>
                  <input
                    type="tel"
                    class="form-control border-0 bg-transparent shadow-none py-2.5"
                    v-model="form.soDienThoai"
                    placeholder="0987xxxxxx"
                  />
                </div>
                <div
                  class="text-danger small mt-1 fw-bold"
                  v-if="errors.soDienThoai"
                >
                  {{ errors.soDienThoai }}
                </div>
              </div>
            </div>

            <div class="row g-3 mb-4">
              <div class="col-md-6">
                <label class="form-label fw-semibold small text-muted mb-2"
                  >Tên đăng nhập</label
                >
                <div
                  class="input-group custom-input-group"
                  :class="{ 'border-danger': errors.tenDangNhap }"
                >
                  <span
                    class="input-group-text bg-transparent border-0 text-muted ps-3"
                    ><i class="bi bi-person"></i
                  ></span>
                  <input
                    type="text"
                    class="form-control border-0 bg-transparent shadow-none py-2.5"
                    v-model="form.tenDangNhap"
                    placeholder="admin123"
                  />
                </div>
                <div
                  class="text-danger small mt-1 fw-bold"
                  v-if="errors.tenDangNhap"
                >
                  {{ errors.tenDangNhap }}
                </div>
              </div>

              <div class="col-md-6">
                <label class="form-label fw-semibold small text-muted mb-2"
                  >Mật khẩu</label
                >
                <div
                  class="input-group custom-input-group"
                  :class="{ 'border-danger': errors.matKhau }"
                >
                  <span
                    class="input-group-text bg-transparent border-0 text-muted ps-3"
                    ><i class="bi bi-lock"></i
                  ></span>
                  <input
                    type="password"
                    class="form-control border-0 bg-transparent shadow-none py-2.5"
                    v-model="form.matKhau"
                    placeholder="••••••••"
                  />
                </div>
                <div
                  class="text-danger small mt-1 fw-bold"
                  v-if="errors.matKhau"
                >
                  {{ errors.matKhau }}
                </div>
              </div>
            </div>

            <div class="form-check mb-4 mt-2">
              <input
                class="form-check-input cursor-pointer"
                type="checkbox"
                id="terms"
                v-model="agreeTerms"
              />
              <label
                class="form-check-label small text-muted cursor-pointer"
                for="terms"
              >
                Tôi đồng ý với
                <a href="#" class="text-orange fw-bold text-decoration-none"
                  >Điều khoản dịch vụ</a
                >
                và
                <a href="#" class="text-orange fw-bold text-decoration-none"
                  >Chính sách bảo mật</a
                >
                của POS36.
              </label>
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
              {{ isLoading ? "ĐANG KHỞI TẠO CỬA HÀNG..." : "Đăng ký ngay" }}
            </button>

            <div class="text-center">
              <span class="text-muted small fw-medium">Đã có tài khoản? </span>
              <router-link
                to="/login"
                class="text-orange fw-bold text-decoration-none small"
                >Đăng nhập</router-link
              >
            </div>
          </form>

          <div
            class="mt-5 pt-4 border-top d-flex justify-content-center gap-4 opacity-50"
          >
            <div class="d-flex align-items-center gap-1 small fw-bold">
              <i class="bi bi-shield-check fs-5"></i> BẢO MẬT SSL
            </div>
            <div class="d-flex align-items-center gap-1 small fw-bold">
              <i class="bi bi-headset fs-5"></i> HỖ TRỢ 24/7
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
@import url("https://fonts.googleapis.com/css2?family=Plus+Jakarta+Sans:wght@500;600;700;800&display=swap");
.register-container {
  font-family: "Plus Jakarta Sans", sans-serif;
  background-color: #ffffff;
}
.left-panel {
  background-image: url("https://images.unsplash.com/photo-1514933651103-005eec06c04b?q=80&w=1974&auto=format&fit=crop");
  background-size: cover;
  background-position: center;
}
.overlay {
  background: linear-gradient(
    135deg,
    rgba(200, 80, 0, 0.6) 0%,
    rgba(204, 85, 0, 0.95) 100%
  );
  z-index: 1;
}
.text-shadow {
  text-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}
.backdrop-blur {
  backdrop-filter: blur(10px);
}
.custom-input-group {
  background-color: #f7f7f9;
  border-radius: 10px;
  border: 1px solid transparent;
  transition: all 0.2s ease-in-out;
}
.custom-input-group:focus-within {
  border-color: #e65c00;
  background-color: #fff;
  box-shadow: 0 0 0 3px rgba(230, 92, 0, 0.1);
}
.cursor-pointer {
  cursor: pointer;
}
.btn-orange {
  background: linear-gradient(135deg, #994700 0%, #ff7a00 100%);
  color: white;
  border-radius: 10px;
  border: none;
  transition: all 0.3s ease;
  box-shadow: 0 10px 20px -10px rgba(255, 122, 0, 0.5);
}
.btn-orange:hover {
  opacity: 0.9;
  color: white;
  transform: translateY(-2px);
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
