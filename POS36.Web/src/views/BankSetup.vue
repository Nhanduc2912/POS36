<template>
  <div class="container-fluid px-4 py-4 bg-light min-vh-100">
    <div class="d-flex justify-content-between align-items-center mb-4">
      <h5 class="fw-bold text-danger mb-0">
        <i class="bi bi-qr-code-scan me-2"></i> THIẾT LẬP CHUYỂN KHOẢN (VIETQR)
      </h5>
      <button
        @click="saveBankConfig"
        class="btn btn-primary fw-bold px-4 shadow-sm"
      >
        <i class="bi bi-save me-2"></i> Lưu thiết lập
      </button>
    </div>

    <div class="row g-4">
      <div class="col-lg-6">
        <div class="card border-0 shadow-sm h-100">
          <div class="card-header bg-white border-bottom py-3">
            <h6 class="fw-bold text-dark mb-0">
              <i class="bi bi-bank me-2"></i> THÔNG TIN TÀI KHOẢN NHẬN TIỀN
            </h6>
          </div>
          <div class="card-body p-4">
            <div class="alert alert-info small fw-bold mb-4">
              <i class="bi bi-info-circle-fill me-2"></i> Thông tin này sẽ được
              dùng để tạo mã QR Code động cho khách hàng quét lúc thanh toán.
            </div>

            <div class="mb-3">
              <label class="form-label fw-bold text-secondary"
                >Ngân hàng thụ hưởng <span class="text-danger">*</span></label
              >
              <select
                v-model="bankConfig.bankId"
                class="form-select border-secondary shadow-none fw-bold text-dark"
              >
                <option value="" disabled>-- Chọn Ngân hàng --</option>
                <option value="MB">MBBank - Ngân hàng Quân Đội</option>
                <option value="VCB">
                  Vietcombank - Ngân hàng Ngoại Thương
                </option>
                <option value="TCB">Techcombank - Ngân hàng Kỹ Thương</option>
                <option value="VPB">
                  VPBank - Ngân hàng Việt Nam Thịnh Vượng
                </option>
                <option value="ACB">ACB - Ngân hàng Á Châu</option>
                <option value="ICB">VietinBank - Ngân hàng Công Thương</option>
                <option value="BIDV">
                  BIDV - Ngân hàng Đầu tư và Phát triển
                </option>
                <option value="TPB">TPBank - Ngân hàng Tiên Phong</option>
              </select>
            </div>

            <div class="mb-3">
              <label class="form-label fw-bold text-secondary"
                >Số tài khoản <span class="text-danger">*</span></label
              >
              <input
                type="text"
                v-model="bankConfig.accountNo"
                class="form-control border-secondary shadow-none fw-bold fs-5 text-primary"
                placeholder="Nhập số tài khoản..."
              />
            </div>

            <div class="mb-4">
              <label class="form-label fw-bold text-secondary"
                >Tên chủ tài khoản <span class="text-danger">*</span></label
              >
              <input
                type="text"
                v-model="bankConfig.accountName"
                class="form-control border-secondary shadow-none fw-bold text-uppercase"
                placeholder="VD: NGUYEN VAN A"
              />
            </div>

            <div class="mb-3">
              <label class="form-label fw-bold text-secondary"
                >Mẫu hiển thị QR</label
              >
              <select
                v-model="bankConfig.template"
                class="form-select border-secondary shadow-none"
              >
                <option value="compact">Cơ bản (Chỉ hiện QR)</option>
                <option value="compact2">
                  Chi tiết (Hiện QR + Tên + Số TK)
                </option>
                <option value="print">Mẫu in (Dùng để in ra giấy)</option>
              </select>
            </div>
          </div>
        </div>
      </div>

      <div class="col-lg-6">
        <div class="card border-0 shadow-sm h-100 bg-secondary bg-opacity-10">
          <div
            class="card-header bg-white border-bottom py-3 d-flex justify-content-between align-items-center"
          >
            <h6 class="fw-bold text-dark mb-0">
              <i class="bi bi-eye-fill text-primary me-2"></i> XEM TRƯỚC MÃ QR
            </h6>
            <span class="badge bg-success">Tự động cập nhật</span>
          </div>
          <div
            class="card-body d-flex flex-column justify-content-center align-items-center p-4"
          >
            <div
              v-if="isValidConfig"
              class="text-center bg-white p-4 rounded-4 shadow-lg"
              style="width: 100%; max-width: 350px"
            >
              <h5 class="fw-bold text-danger mb-3">QUÉT ĐỂ THANH TOÁN</h5>

              <img
                :src="qrDemoUrl"
                alt="VietQR"
                class="img-fluid rounded border p-2 mb-3"
                style="min-height: 250px"
              />

              <div class="bg-light p-3 rounded-3 text-start">
                <div class="d-flex justify-content-between mb-2">
                  <span class="text-muted small">Số tiền:</span>
                  <span class="fw-bold fs-5 text-primary">125.000 đ</span>
                </div>
                <div class="d-flex justify-content-between">
                  <span class="text-muted small">Nội dung:</span>
                  <span class="fw-bold font-monospace">HD260324001</span>
                </div>
              </div>
            </div>

            <div v-else class="text-center text-muted p-5">
              <i
                class="bi bi-qr-code text-secondary opacity-25"
                style="font-size: 5rem"
              ></i>
              <p class="mt-3 fw-bold">
                Vui lòng nhập đầy đủ Ngân hàng và Số tài khoản để xem trước mã
                QR.
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed, inject } from "vue";
import axios from "axios";
const swal = inject("$swal");

// Trạng thái cấu hình ngân hàng
const bankConfig = ref({
  bankId: "",
  accountNo: "",
  accountName: "",
  template: "compact2",
});

// Load từ CSDL lúc mở trang
onMounted(async () => {
  try {
    const res = await axios.get("/api/ThietLap/BankConfig");
    if (res.data && res.data.duLieu) {
      bankConfig.value = JSON.parse(res.data.duLieu);
      localStorage.setItem("pos36_bank_config", res.data.duLieu); // Lưu dự phòng vào cache
    }
  } catch (e) {
    console.error("Chưa có cấu hình NH");
  }
});

// Hàm lưu đẩy thẳng lên CSDL
const saveBankConfig = async () => {
  if (!isValidConfig.value)
    return swal.fire(
      "Lỗi",
      "Vui lòng nhập Ngân hàng và Số tài khoản!",
      "warning",
    );

  const duLieuString = JSON.stringify(bankConfig.value);
  try {
    await axios.post("/api/ThietLap", {
      maThietLap: "BankConfig",
      duLieu: duLieuString,
    });
    localStorage.setItem("pos36_bank_config", duLieuString); // Cập nhật luôn cache hiện tại
    swal.fire({
      toast: true,
      position: "top-end",
      icon: "success",
      title: "Đã lưu đồng bộ lên CSDL!",
      timer: 1500,
      showConfirmButton: false,
    });
  } catch (e) {
    swal.fire("Lỗi", "Lưu thiết lập thất bại", "error");
  }
};

// Kiểm tra xem user đã nhập đủ thông tin chưa
const isValidConfig = computed(() => {
  return bankConfig.value.bankId && bankConfig.value.accountNo;
});

// Hàm tạo URL QR Code Động (Gọi thẳng API của VietQR - Miễn phí và Cực nhanh)
const qrDemoUrl = computed(() => {
  if (!isValidConfig.value) return "";

  const amount = 125000; // Số tiền demo
  const addInfo = "HD260324001"; // Nội dung demo
  const accountName = encodeURIComponent(bankConfig.value.accountName); // Chuẩn hóa tên có dấu/khoảng trắng

  // Cú pháp chuẩn của VietQR API
  return `https://img.vietqr.io/image/${bankConfig.value.bankId}-${bankConfig.value.accountNo}-${bankConfig.value.template}.png?amount=${amount}&addInfo=${addInfo}&accountName=${accountName}`;
});
</script>

<style scoped>
.form-select,
.form-control {
  padding: 0.75rem 1rem;
}
</style>
