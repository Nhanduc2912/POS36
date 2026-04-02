<template>
  <div class="container-fluid px-4 py-4">
    <div class="d-flex justify-content-between align-items-center mb-4">
      <div class="d-flex align-items-center">
        <i class="bi bi-sliders fs-3 text-primary me-2"></i>
        <h3 class="mb-0 fw-bold text-dark">Thiết lập tính năng</h3>
      </div>
      <button
        @click="saveSettings"
        class="btn btn-primary px-4 py-2 shadow-sm fw-medium"
        :disabled="isLoading"
      >
        <span
          v-if="isLoading"
          class="spinner-border spinner-border-sm me-2"
        ></span>
        <i v-else class="bi bi-floppy-fill me-2"></i> Lưu thiết lập
      </button>
    </div>

    <div class="card border-0 shadow-sm rounded-4 overflow-hidden mb-5">
      <div class="card-body p-0">
        <div
          class="bg-light px-4 py-2 fw-bold text-muted border-bottom text-uppercase"
          style="font-size: 0.85rem"
        >
          <i class="bi bi-gear-wide-connected me-1"></i> Vận hành hệ thống
        </div>
        <div class="px-4">
          <div
            class="d-flex justify-content-between align-items-center border-bottom py-3"
          >
            <label class="fw-medium mb-0 text-dark">Cho phép bán âm kho?</label>
            <div class="form-check form-switch fs-4 mb-0">
              <input
                class="form-check-input cursor-pointer shadow-sm"
                type="checkbox"
                role="switch"
                v-model="settings.allowNegativeStock"
              />
            </div>
          </div>
          <div
            class="d-flex justify-content-between align-items-center border-bottom py-3"
          >
            <label class="fw-medium mb-0 text-dark"
              >Kiểm soát lý do Hủy/Trả món?</label
            >
            <div class="form-check form-switch fs-4 mb-0">
              <input
                class="form-check-input cursor-pointer shadow-sm"
                type="checkbox"
                role="switch"
                v-model="settings.requireDeleteReason"
              />
            </div>
          </div>
          <div class="d-flex justify-content-between align-items-center py-3">
            <label class="fw-medium mb-0 text-dark"
              >Múi giờ hệ thống báo cáo</label
            >
            <select
              class="form-select w-auto form-select-sm fw-medium"
              v-model="settings.timezone"
            >
              <option value="SE Asia Standard Time">
                (GMT+07:00) Bangkok, Hanoi, Jakarta
              </option>
            </select>
          </div>
        </div>

        <div
          class="bg-light px-4 py-2 fw-bold text-muted border-top border-bottom text-uppercase"
          style="font-size: 0.85rem"
        >
          <i class="bi bi-cart-check me-1"></i> Bán hàng & Thu phí
        </div>
        <div class="px-4">
          <div
            class="d-flex justify-content-between align-items-center border-bottom py-3"
          >
            <label class="fw-medium mb-0 text-dark"
              >Cho phép nhân viên thay đổi giá khi bán hàng?</label
            >
            <div class="form-check form-switch fs-4 mb-0">
              <input
                class="form-check-input cursor-pointer shadow-sm"
                type="checkbox"
                role="switch"
                v-model="settings.allowEmpChangePrice"
              />
            </div>
          </div>
          <div
            class="d-flex justify-content-between align-items-center border-bottom py-3"
          >
            <label class="fw-medium mb-0 text-dark"
              >Cho phép nhân viên chiết khấu/giảm giá hóa đơn?</label
            >
            <div class="form-check form-switch fs-4 mb-0">
              <input
                class="form-check-input cursor-pointer shadow-sm"
                type="checkbox"
                role="switch"
                v-model="settings.allowEmpDiscount"
              />
            </div>
          </div>
          <div
            class="d-flex justify-content-between align-items-center border-bottom py-3"
          >
            <label class="fw-medium mb-0 text-dark"
              >Tính thuế VAT sau khi đã chiết khấu?</label
            >
            <div class="form-check form-switch fs-4 mb-0">
              <input
                class="form-check-input cursor-pointer shadow-sm"
                type="checkbox"
                role="switch"
                v-model="settings.vatAfterDiscount"
              />
            </div>
          </div>
          <div class="d-flex justify-content-between align-items-center py-3">
            <label class="fw-medium mb-0 text-dark"
              >Mức Thuế VAT (%) mặc định</label
            >
            <input
              type="number"
              class="form-control text-center fw-bold"
              style="width: 100px"
              v-model="settings.vat"
              min="0"
              max="100"
            />
          </div>
        </div>

        <div
          class="bg-light px-4 py-2 fw-bold text-muted border-top border-bottom text-uppercase"
          style="font-size: 0.85rem"
        >
          <i class="bi bi-printer me-1"></i> In ấn & Bar/Bếp
        </div>
        <div class="px-4">
          <div
            class="d-flex justify-content-between align-items-center border-bottom py-3"
          >
            <label class="fw-medium mb-0 text-dark"
              >Cho phép in tạm tính trước khi thanh toán?</label
            >
            <div class="form-check form-switch fs-4 mb-0">
              <input
                class="form-check-input cursor-pointer shadow-sm"
                type="checkbox"
                role="switch"
                v-model="settings.printTempBill"
              />
            </div>
          </div>
          <div
            class="d-flex justify-content-between align-items-center border-bottom py-3"
          >
            <label class="fw-medium mb-0 text-dark"
              >Tự động in hóa đơn sau khi bấm thanh toán?</label
            >
            <div class="form-check form-switch fs-4 mb-0">
              <input
                class="form-check-input cursor-pointer shadow-sm"
                type="checkbox"
                role="switch"
                v-model="settings.autoPrintBill"
              />
            </div>
          </div>
          <div class="d-flex flex-column py-4">
            <label class="fw-medium mb-2 text-dark"
              >Lời chào/Cảm ơn in cuối hóa đơn</label
            >
            <textarea
              class="form-control bg-light"
              rows="3"
              v-model="settings.billNote"
              placeholder="Ví dụ: Cảm ơn Quý khách! Hẹn gặp lại!"
            ></textarea>
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

const settings = ref({
  allowNegativeStock: false,
  requireDeleteReason: true,
  timezone: "SE Asia Standard Time",
  allowEmpChangePrice: false,
  allowEmpDiscount: true,
  vat: 8,
  vatAfterDiscount: true,
  printTempBill: true,
  autoPrintBill: true,
  billNote: "Cảm ơn Quý khách. Hẹn gặp lại!",
});

const isLoading = ref(false);

const fetchSettings = async () => {
  try {
    const token = localStorage.getItem("pos36_token");
    const res = await axios.get(
      "http://localhost:5198/api/ThietLap/CauHinhChung",
      {
        headers: { Authorization: `Bearer ${token}` },
      },
    );
    if (res.data && res.data.duLieu) {
      settings.value = JSON.parse(res.data.duLieu);
    }
  } catch (error) {
    console.error("Lỗi tải cấu hình", error);
  }
};

const saveSettings = async () => {
  isLoading.value = true;
  try {
    const token = localStorage.getItem("pos36_token");
    await axios.post(
      "http://localhost:5198/api/ThietLap",
      {
        MaThietLap: "CauHinhChung",
        DuLieu: JSON.stringify(settings.value),
      },
      {
        headers: { Authorization: `Bearer ${token}` },
      },
    );

    Swal.fire({
      icon: "success",
      title: "Đã lưu thiết lập!",
      text: "Cấu hình đã được đồng bộ lên hệ thống.",
      timer: 1500,
      showConfirmButton: false,
    });
  } catch (error) {
    Swal.fire({
      icon: "error",
      title: "Cập nhật thất bại",
      text: "Vui lòng kiểm tra lại kết nối mạng!",
      confirmButtonColor: "var(--bs-primary)",
    });
  } finally {
    isLoading.value = false;
  }
};

onMounted(fetchSettings);
</script>

<style scoped>
/* Đồng bộ màu Switch với Theme hiện tại */
.form-check-input:checked {
  background-color: var(--bs-primary);
  border-color: var(--bs-primary);
}
.form-control:focus,
.form-select:focus {
  border-color: var(--bs-primary);
  box-shadow: 0 0 0 0.25rem rgba(var(--bs-primary-rgb), 0.25);
}
.cursor-pointer {
  cursor: pointer;
}
</style>
