<template>
  <div class="container-fluid px-4 py-4">
    <div class="d-flex align-items-center mb-4">
      <i class="bi bi-person-vcard fs-3 text-primary me-2"></i>
      <h3 class="mb-0 fw-bold text-dark">Thông tin cá nhân</h3>
    </div>

    <div class="row">
      <div class="col-lg-7 mb-4">
        <div class="card border-0 shadow-sm rounded-4 h-100">
          <div class="card-header bg-white border-bottom-0 pt-4 pb-0 px-4">
            <h5 class="fw-bold text-primary mb-0">
              <i class="bi bi-info-circle me-2"></i>Hồ sơ của bạn
            </h5>
          </div>
          <div class="card-body p-4">
            <form @submit.prevent="updateProfile">
              <div class="row g-3">
                <div class="col-md-6">
                  <label class="form-label fw-medium text-muted small"
                    >Tên đăng nhập</label
                  >
                  <input
                    type="text"
                    class="form-control bg-light text-muted"
                    v-model="profile.username"
                    disabled
                  />
                  <small class="text-danger" style="font-size: 0.75rem"
                    >*Không được phép thay đổi</small
                  >
                </div>

                <div class="col-md-6">
                  <label class="form-label fw-medium text-muted small"
                    >Số điện thoại</label
                  >
                  <input
                    type="text"
                    class="form-control bg-light text-muted"
                    v-model="profile.phone"
                    disabled
                  />
                  <small class="text-danger" style="font-size: 0.75rem"
                    >*Không được phép thay đổi</small
                  >
                </div>

                <div class="col-md-12 mt-4">
                  <label class="form-label fw-medium">Họ và tên hiển thị</label>
                  <input
                    type="text"
                    class="form-control"
                    v-model="profile.fullName"
                    placeholder="Nhập tên của bạn"
                    required
                  />
                </div>

                <div class="col-md-12">
                  <label class="form-label fw-medium">Địa chỉ Email</label>
                  <input
                    type="email"
                    class="form-control"
                    v-model="profile.email"
                    placeholder="example@gmail.com"
                  />
                </div>
              </div>

              <div class="mt-4 text-end">
                <button
                  type="submit"
                  class="btn btn-primary px-4 fw-medium"
                  :disabled="isLoadingProfile"
                >
                  <span
                    v-if="isLoadingProfile"
                    class="spinner-border spinner-border-sm me-2"
                  ></span>
                  <i v-else class="bi bi-floppy me-2"></i> Lưu thay đổi
                </button>
              </div>
            </form>
          </div>
        </div>
      </div>

      <div class="col-lg-5 mb-4">
        <div class="card border-0 shadow-sm rounded-4 h-100">
          <div class="card-header bg-white border-bottom-0 pt-4 pb-0 px-4">
            <h5 class="fw-bold text-danger mb-0">
              <i class="bi bi-shield-lock me-2"></i>Đổi mật khẩu
            </h5>
          </div>
          <div class="card-body p-4">
            <form @submit.prevent="changePassword">
              <div class="mb-3">
                <label class="form-label fw-medium"
                  >Mật khẩu hiện tại <span class="text-danger">*</span></label
                >
                <input
                  type="password"
                  class="form-control"
                  v-model="pwdForm.oldPassword"
                  placeholder="Nhập mật khẩu cũ để xác minh"
                  required
                />
              </div>

              <div class="mb-3">
                <label class="form-label fw-medium"
                  >Mật khẩu mới <span class="text-danger">*</span></label
                >
                <input
                  type="password"
                  class="form-control"
                  v-model="pwdForm.newPassword"
                  placeholder="Nhập mật khẩu mới"
                  required
                  minlength="6"
                />
              </div>

              <div class="mb-4">
                <label class="form-label fw-medium"
                  >Xác nhận mật khẩu mới
                  <span class="text-danger">*</span></label
                >
                <input
                  type="password"
                  class="form-control"
                  v-model="pwdForm.confirmPassword"
                  placeholder="Nhập lại mật khẩu mới"
                  required
                  minlength="6"
                />
                <small
                  v-if="
                    pwdForm.newPassword &&
                    pwdForm.newPassword !== pwdForm.confirmPassword
                  "
                  class="text-danger d-block mt-1"
                >
                  <i class="bi bi-exclamation-triangle-fill me-1"></i> Mật khẩu
                  xác nhận không khớp!
                </small>
              </div>

              <div class="text-end">
                <button
                  type="submit"
                  class="btn btn-danger px-4 fw-medium"
                  :disabled="
                    isLoadingPwd ||
                    pwdForm.newPassword !== pwdForm.confirmPassword
                  "
                >
                  <span
                    v-if="isLoadingPwd"
                    class="spinner-border spinner-border-sm me-2"
                  ></span>
                  <i v-else class="bi bi-key me-2"></i> Cập nhật mật khẩu
                </button>
              </div>
            </form>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from "vue";
import axios from "axios";
import Swal from "sweetalert2"; // Import đúng điệu của Sếp

const profile = ref({
  username: "",
  phone: "",
  fullName: "",
  email: "",
});

const pwdForm = ref({
  oldPassword: "",
  newPassword: "",
  confirmPassword: "",
});

const isLoadingProfile = ref(false);
const isLoadingPwd = ref(false);

// 1. LẤY DỮ LIỆU KHI VỪA VÀO TRANG
const fetchProfile = async () => {
  try {
    const token = localStorage.getItem("pos36_token");
    const res = await axios.get("http://localhost:5098/api/Profile", {
      headers: { Authorization: `Bearer ${token}` },
    });
    profile.value = res.data;
  } catch (error) {
    Swal.fire({
      icon: "error",
      title: "Lỗi kết nối",
      text: "Không thể lấy thông tin cá nhân!",
      confirmButtonColor: "var(--bs-primary)",
    });
  }
};

// 2. CẬP NHẬT TÊN VÀ EMAIL
const updateProfile = async () => {
  isLoadingProfile.value = true;
  try {
    const token = localStorage.getItem("pos36_token");
    await axios.put(
      "http://localhost:5098/api/Profile/update-info",
      {
        fullName: profile.value.fullName,
        email: profile.value.email,
      },
      {
        headers: { Authorization: `Bearer ${token}` },
      },
    );

    // Báo thành công bằng SweetAlert2
    Swal.fire({
      icon: "success",
      title: "Thành công!",
      text: "Đã cập nhật thông tin cá nhân.",
      timer: 2000,
      showConfirmButton: false,
    });
  } catch (error) {
    Swal.fire({
      icon: "error",
      title: "Cập nhật thất bại",
      text: error.response?.data || "Đã có lỗi xảy ra, vui lòng thử lại.",
      confirmButtonColor: "var(--bs-primary)",
    });
  } finally {
    isLoadingProfile.value = false;
  }
};

// 3. ĐỔI MẬT KHẨU CÓ XÁC MINH
const changePassword = async () => {
  if (pwdForm.value.newPassword !== pwdForm.value.confirmPassword) return;

  isLoadingPwd.value = true;
  try {
    const token = localStorage.getItem("pos36_token");
    const res = await axios.post(
      "http://localhost:5098/api/Profile/change-password",
      {
        oldPassword: pwdForm.value.oldPassword,
        newPassword: pwdForm.value.newPassword,
      },
      {
        headers: { Authorization: `Bearer ${token}` },
      },
    );

    // Báo thành công
    Swal.fire({
      icon: "success",
      title: "Bảo mật",
      text: res.data.message,
      confirmButtonColor: "var(--bs-primary)",
    });

    // Xóa trắng form mật khẩu sau khi đổi thành công
    pwdForm.value = { oldPassword: "", newPassword: "", confirmPassword: "" };
  } catch (error) {
    // Báo lỗi (sai mật khẩu cũ)
    Swal.fire({
      icon: "error",
      title: "Từ chối",
      text: error.response?.data || "Mật khẩu hiện tại không chính xác!",
      confirmButtonColor: "#dc3545", // Màu đỏ chót cho cảnh báo
    });
  } finally {
    isLoadingPwd.value = false;
  }
};

onMounted(() => {
  fetchProfile();
});
</script>

<style scoped>
.form-control:disabled {
  background-color: #f8f9fa !important;
  cursor: not-allowed;
  opacity: 0.8;
}
.form-control:focus {
  border-color: var(--bs-primary);
  box-shadow: 0 0 0 0.25rem rgba(var(--bs-primary-rgb), 0.25);
}
</style>
