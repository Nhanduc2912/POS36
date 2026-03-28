<template>
  <div class="container-fluid p-4 bg-light min-vh-100">
    <div class="d-flex justify-content-between align-items-center mb-4">
      <h3 class="fw-bold text-dark">
        <i class="bi bi-calendar-check me-2 text-warning"></i>Tổng Kết Cuối Ngày
      </h3>
      <div class="d-flex gap-2">
        <input
          type="date"
          class="form-control"
          v-model="selectedDate"
          @change="fetchData"
        />
        <button
          class="btn btn-primary fw-bold"
          @click="fetchData"
          :disabled="isLoading"
        >
          <i class="bi bi-arrow-repeat me-1"></i> Làm mới
        </button>
      </div>
    </div>

    <div v-if="!isLoading" class="row g-3 mb-4">
      <div class="col-md-3">
        <div
          class="card border-0 shadow-sm rounded-4 bg-primary text-white h-100"
        >
          <div class="card-body p-4">
            <h6 class="opacity-75 fw-bold mb-2">TỔNG DOANH THU</h6>
            <h3 class="fw-bold mb-0">
              {{ formatPrice(summaryData.tongDoanhThu) }}
            </h3>
          </div>
        </div>
      </div>
      <div class="col-md-3">
        <div
          class="card border-0 shadow-sm rounded-4 bg-success text-white h-100"
        >
          <div class="card-body p-4">
            <h6 class="opacity-75 fw-bold mb-2">TIỀN MẶT</h6>
            <h3 class="fw-bold mb-0">{{ formatPrice(summaryData.tienMat) }}</h3>
          </div>
        </div>
      </div>
      <div class="col-md-3">
        <div class="card border-0 shadow-sm rounded-4 bg-info text-white h-100">
          <div class="card-body p-4">
            <h6 class="opacity-75 fw-bold mb-2">CHUYỂN KHOẢN (QR)</h6>
            <h3 class="fw-bold mb-0">
              {{ formatPrice(summaryData.chuyenKhoan) }}
            </h3>
          </div>
        </div>
      </div>
      <div class="col-md-3">
        <div
          class="card border-0 shadow-sm rounded-4 bg-warning text-dark h-100"
        >
          <div class="card-body p-4">
            <h6 class="opacity-75 fw-bold mb-2">TỔNG SỐ ĐƠN</h6>
            <h3 class="fw-bold mb-0">{{ summaryData.tongDon }} đơn</h3>
          </div>
        </div>
      </div>
    </div>

    <div class="card border-0 shadow-sm rounded-4">
      <div class="card-header bg-white border-bottom py-3">
        <h6 class="fw-bold mb-0">Chi Tiết Giao Dịch Trong Ngày</h6>
      </div>
      <div class="card-body p-0 table-responsive">
        <div v-if="isLoading" class="text-center py-5">
          <span class="spinner-border text-primary"></span>
        </div>
        <table v-else class="table table-hover align-middle mb-0">
          <thead class="table-light text-muted small">
            <tr>
              <th class="ps-4">MÃ HĐ</th>
              <th>GIỜ VÀO</th>
              <th>BÀN</th>
              <th>PHƯƠNG THỨC</th>
              <th class="text-end pe-4">TỔNG TIỀN</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="summaryData.danhSachDon?.length === 0">
              <td colspan="5" class="text-center py-4 text-muted fst-italic">
                Không có giao dịch nào trong ngày này.
              </td>
            </tr>
            <tr v-for="don in summaryData.danhSachDon" :key="don.id">
              <td class="ps-4 fw-bold text-primary">{{ don.maChungTu }}</td>
              <td>{{ new Date(don.ngayBan).toLocaleTimeString("vi-VN") }}</td>
              <td>{{ don.tenBan }}</td>
              <td>
                <span
                  :class="
                    don.phuongThuc === 'Tiền mặt'
                      ? 'badge bg-success'
                      : 'badge bg-info'
                  "
                >
                  {{ don.phuongThuc || "Chưa rõ" }}
                </span>
              </td>
              <td class="text-end pe-4 fw-bold text-danger">
                {{ formatPrice(don.tongCong) }}
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from "vue";
import axios from "axios";

const selectedDate = ref(new Date().toISOString().split("T")[0]);
const isLoading = ref(false);
const summaryData = ref({
  tongDoanhThu: 0,
  tienMat: 0,
  chuyenKhoan: 0,
  tongDon: 0,
  danhSachDon: [],
});

const formatPrice = (price) =>
  new Intl.NumberFormat("vi-VN", { style: "currency", currency: "VND" }).format(
    price || 0,
  );

const fetchData = async () => {
  isLoading.value = true;
  try {
    const token =
      localStorage.getItem("token") || localStorage.getItem("pos36_token");
    const res = await axios.get(
      `/api/Report/daily?date=${selectedDate.value}`,
      {
        headers: { Authorization: `Bearer ${token}` },
      },
    );
    summaryData.value = res.data;
  } catch (error) {
    console.error("Lỗi lấy báo cáo:", error);
  } finally {
    isLoading.value = false;
  }
};

onMounted(() => fetchData());
</script>
