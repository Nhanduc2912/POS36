<template>
  <div class="container-fluid px-4 py-4">
    <div class="d-flex align-items-center mb-4">
      <i class="bi bi-shop-window fs-3 text-primary me-2"></i>
      <h3 class="mb-0 fw-bold text-dark">Thông tin cửa hàng</h3>
    </div>

    <div class="row">
      <div class="col-lg-8 mb-4 mx-auto">
        <div class="card border-0 shadow-sm rounded-4 h-100">
          <div class="card-header bg-white border-bottom-0 pt-4 pb-0 px-4">
            <h5 class="fw-bold text-primary mb-0">
              <i class="bi bi-building me-2"></i>Hồ sơ kinh doanh
            </h5>
          </div>
          <div class="card-body p-4">
            <form @submit.prevent="updateStoreInfo">
              <div class="row g-4">
                <div class="col-md-12">
                  <label class="form-label fw-medium"
                    >Tên thương hiệu / Tên cửa hàng
                    <span class="text-danger">*</span></label
                  >
                  <input
                    type="text"
                    class="form-control form-control-lg fs-6"
                    v-model="store.tenCuaHang"
                    placeholder="VD: Cafe Mixi"
                    required
                  />
                </div>

                <div class="col-md-6">
                  <label class="form-label fw-medium"
                    >Số điện thoại liên hệ (In trên bill)
                    <span class="text-danger">*</span></label
                  >
                  <input
                    type="text"
                    class="form-control"
                    v-model="store.soDienThoai"
                    placeholder="Nhập số điện thoại"
                    required
                  />
                </div>

                <div class="col-md-6">
                  <label class="form-label fw-medium text-muted small"
                    >Ngày bắt đầu sử dụng POS36</label
                  >
                  <input
                    type="text"
                    class="form-control bg-light text-muted"
                    :value="formatDate(store.ngayDangKy)"
                    disabled
                  />
                  <small class="text-secondary" style="font-size: 0.75rem"
                    ><i class="bi bi-lock-fill"></i> Dữ liệu hệ thống</small
                  >
                </div>
              </div>

              <div class="mt-5 pt-3 border-top text-end">
                <button
                  type="submit"
                  class="btn btn-primary px-5 py-2 fw-medium rounded-3"
                  :disabled="isLoading"
                >
                  <span
                    v-if="isLoading"
                    class="spinner-border spinner-border-sm me-2"
                  ></span>
                  <i v-else class="bi bi-floppy me-2"></i> Cập nhật thông tin
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
import Swal from "sweetalert2";

const store = ref({
  tenCuaHang: "",
  soDienThoai: "",
  ngayDangKy: "",
});

const isLoading = ref(false);

// 1. LẤY DỮ LIỆU CỬA HÀNG
const fetchStoreInfo = async () => {
  try {
    const token = localStorage.getItem("pos36_token");
    const res = await axios.get("http://localhost:5098/api/CuaHang/info", {
      headers: { Authorization: `Bearer ${token}` },
    });
    store.value = res.data;
  } catch (error) {
    Swal.fire({
      icon: "error",
      title: "Lỗi dữ liệu",
      text: "Không thể lấy thông tin cửa hàng!",
      confirmButtonColor: "var(--bs-primary)",
    });
  }
};

// 2. CẬP NHẬT THÔNG TIN CỬA HÀNG
const updateStoreInfo = async () => {
  isLoading.value = true;
  try {
    const token = localStorage.getItem("pos36_token");
    await axios.put(
      "http://localhost:5098/api/CuaHang/update-info",
      {
        tenCuaHang: store.value.tenCuaHang,
        soDienThoai: store.value.soDienThoai,
      },
      {
        headers: { Authorization: `Bearer ${token}` },
      },
    );

    Swal.fire({
      icon: "success",
      title: "Lưu thành công!",
      text: "Thông tin cửa hàng đã được cập nhật.",
      timer: 2000,
      showConfirmButton: false,
    });
  } catch (error) {
    Swal.fire({
      icon: "error",
      title: "Cập nhật thất bại",
      text: "Đã có lỗi xảy ra, vui lòng thử lại.",
      confirmButtonColor: "var(--bs-primary)",
    });
  } finally {
    isLoading.value = false;
  }
};

// Hàm phụ trợ: Format ngày tháng cho đẹp
const formatDate = (dateString) => {
  if (!dateString) return "";
  const date = new Date(dateString);
  return date.toLocaleDateString("vi-VN", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
};

onMounted(() => {
  fetchStoreInfo();
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
