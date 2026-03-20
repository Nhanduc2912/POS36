<script setup>
import { ref, onMounted, watch } from "vue";
import axios from "axios";
import { Bar } from "vue-chartjs";
import {
  Chart as ChartJS,
  Title,
  Tooltip,
  Legend,
  BarElement,
  CategoryScale,
  LinearScale,
} from "chart.js";
import { globalState } from "../store"; // Nhúng kho lưu trữ chi nhánh

ChartJS.register(
  Title,
  Tooltip,
  Legend,
  BarElement,
  CategoryScale,
  LinearScale,
);

const isLoading = ref(true);
const summary = ref({
  tongDonHang: 0,
  doanhThu: 0,
  donHuy: 0,
  banDangDung: 0,
  tongBan: 0,
});
const chartData = ref({
  labels: [],
  datasets: [
    {
      label: "Doanh thu",
      backgroundColor: "#f37021",
      borderRadius: 4,
      data: [],
    },
  ],
});
const chartOptions = ref({
  responsive: true,
  maintainAspectRatio: false,
  plugins: { legend: { display: false } },
  scales: { y: { beginAtZero: true } },
});

// HÀM GỌI API LẤY THỐNG KÊ (CÓ TRUYỀN CHI NHÁNH ID)
const fetchDashboardData = async () => {
  if (!globalState.value.activeBranchId) return; // Nếu chưa có chi nhánh thì thôi không gọi API

  isLoading.value = true;
  try {
    // Vì đã có Axios Interceptor kẹp Token ở main.js, giờ gọi API cực kỳ ngắn gọn
    const response = await axios.get(
      `/api/Dashboard/summary?chiNhanhId=${globalState.value.activeBranchId}`,
    );
    summary.value = response.data.summary;

    chartData.value = {
      labels: response.data.chart.labels,
      datasets: [
        {
          label: "Doanh thu",
          backgroundColor: "#f37021",
          borderRadius: 4,
          data: response.data.chart.data,
        },
      ],
    };
  } catch (error) {
    console.error("Lỗi lấy dữ liệu dashboard:", error);
  } finally {
    isLoading.value = false;
  }
};

// 1. Chạy hàm khi vừa vào trang
onMounted(() => {
  fetchDashboardData();
});

// 2. LẮNG NGHE SỰ THAY ĐỔI CHI NHÁNH TỪ NAVBAR
watch(
  () => globalState.value.activeBranchId,
  (newId) => {
    if (newId) {
      fetchDashboardData(); // Gọi lại API lập tức khi đổi chi nhánh
    }
  },
);
</script>

<template>
  <div class="container-fluid px-4 py-4">
    <div class="d-flex justify-content-between align-items-center mb-4">
      <h5 class="fw-bold text-secondary mb-0">TỔNG QUAN HÔM NAY</h5>
      <div
        v-if="isLoading"
        class="spinner-border text-primary spinner-border-sm"
      ></div>
    </div>

    <div class="row g-3 mb-4">
      <div class="col-md-3">
        <div class="card border-0 shadow-sm h-100">
          <div class="card-body d-flex align-items-center">
            <div class="icon-box bg-primary text-white rounded-circle me-3">
              <i class="bi bi-tag-fill fs-4"></i>
            </div>
            <div>
              <h3 class="mb-0 fw-bold">{{ summary.tongDonHang }}</h3>
              <span class="text-muted small">ĐƠN HÀNG</span>
            </div>
          </div>
        </div>
      </div>
      <div class="col-md-3">
        <div class="card border-0 shadow-sm h-100">
          <div class="card-body d-flex align-items-center">
            <div class="icon-box bg-success text-white rounded-circle me-3">
              <i class="bi bi-cash-stack fs-4"></i>
            </div>
            <div>
              <h3 class="mb-0 fw-bold">
                {{ summary.doanhThu.toLocaleString("vi-VN") }}
              </h3>
              <span class="text-muted small">DOANH THU (VNĐ)</span>
            </div>
          </div>
        </div>
      </div>
      <div class="col-md-3">
        <div class="card border-0 shadow-sm h-100">
          <div class="card-body d-flex align-items-center">
            <div class="icon-box bg-danger text-white rounded-circle me-3">
              <i class="bi bi-arrow-return-left fs-4"></i>
            </div>
            <div>
              <h3 class="mb-0 fw-bold">{{ summary.donHuy }}</h3>
              <span class="text-muted small">HỦY/TRẢ ĐỒ</span>
            </div>
          </div>
        </div>
      </div>
      <div class="col-md-3">
        <div class="card border-0 shadow-sm h-100">
          <div class="card-body d-flex align-items-center">
            <div class="icon-box bg-info text-white rounded-circle me-3">
              <i class="bi bi-grid-fill fs-4"></i>
            </div>
            <div>
              <h3 class="mb-0 fw-bold">
                {{ summary.banDangDung }} / {{ summary.tongBan }}
              </h3>
              <span class="text-muted small">BÀN ĐANG SỬ DỤNG</span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div class="row g-4">
      <div class="col-lg-8">
        <div class="card border-0 shadow-sm h-100">
          <div class="card-header bg-white border-0 pt-4 pb-0">
            <h6 class="fw-bold text-danger mb-0">DOANH THU 7 NGÀY QUA</h6>
          </div>
          <div class="card-body">
            <div v-if="chartData.labels.length > 0" style="height: 350px">
              <Bar :data="chartData" :options="chartOptions" />
            </div>
            <div
              v-else
              class="d-flex justify-content-center align-items-center"
              style="
                height: 350px;
                background-color: #f9f9f9;
                border: 1px dashed #ddd;
                border-radius: 8px;
              "
            >
              <p class="text-muted">
                <i class="bi bi-inbox fs-4 d-block text-center mb-2"></i> Chưa
                có dữ liệu giao dịch
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.icon-box {
  width: 50px;
  height: 50px;
  display: flex;
  align-items: center;
  justify-content: center;
}
</style>
