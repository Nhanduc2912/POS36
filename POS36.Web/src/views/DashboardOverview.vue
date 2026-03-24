<template>
  <div class="container-fluid px-4 py-4 bg-light min-vh-100">
    <div class="d-flex justify-content-between align-items-center mb-4">
      <h5 class="fw-bold text-danger mb-0">
        <i class="bi bi-pie-chart-fill me-2"></i> TỔNG QUAN HÔM NAY
      </h5>
      <div
        v-if="isLoading"
        class="spinner-border text-primary spinner-border-sm"
      ></div>
    </div>

    <div class="row g-3 mb-3">
      <div class="col-xl-3 col-md-6">
        <div
          class="card border-0 shadow-sm h-100 card-hover"
          @click="router.push('/admin/orders')"
        >
          <div
            class="card-body d-flex align-items-center justify-content-between"
          >
            <div>
              <span class="text-muted fw-bold small d-block mb-1"
                >DOANH THU (VNĐ)</span
              >
              <h3 class="mb-0 fw-bolder text-dark">
                {{ summary.doanhThu.toLocaleString("vi-VN") }}
              </h3>
            </div>
            <div
              class="icon-box bg-success bg-opacity-10 text-success rounded-3 fs-3"
            >
              <i class="bi bi-cash-stack"></i>
            </div>
          </div>
        </div>
      </div>

      <div class="col-xl-3 col-md-6">
        <div
          class="card border-0 shadow-sm h-100 card-hover"
          @click="router.push('/admin/orders')"
        >
          <div
            class="card-body d-flex align-items-center justify-content-between"
          >
            <div>
              <span class="text-muted fw-bold small d-block mb-1"
                >TẠM TÍNH HIỆN TẠI</span
              >
              <h3 class="mb-0 fw-bolder text-primary">
                {{ summary.tamTinh.toLocaleString("vi-VN") }}
              </h3>
            </div>
            <div
              class="icon-box bg-primary bg-opacity-10 text-primary rounded-3 fs-3"
            >
              <i class="bi bi-calculator"></i>
            </div>
          </div>
        </div>
      </div>

      <div class="col-xl-3 col-md-6">
        <div
          class="card border-0 shadow-sm h-100 card-hover"
          @click="router.push('/admin/orders')"
        >
          <div
            class="card-body d-flex align-items-center justify-content-between"
          >
            <div>
              <span class="text-muted fw-bold small d-block mb-1"
                >SỐ ĐƠN HÀNG</span
              >
              <h3 class="mb-0 fw-bolder text-dark">
                {{ summary.tongDonHang }}
              </h3>
            </div>
            <div
              class="icon-box bg-info bg-opacity-10 text-info rounded-3 fs-3"
            >
              <i class="bi bi-receipt"></i>
            </div>
          </div>
        </div>
      </div>

      <div class="col-xl-3 col-md-6">
        <div
          class="card border-0 shadow-sm h-100 card-hover"
          @click="router.push('/admin/tables')"
        >
          <div
            class="card-body d-flex align-items-center justify-content-between"
          >
            <div>
              <span class="text-muted fw-bold small d-block mb-1"
                >BÀN ĐANG PHỤC VỤ</span
              >
              <h3 class="mb-0 fw-bolder text-dark">
                {{ summary.banDangDung }}
                <span class="fs-6 text-muted fw-normal"
                  >/ {{ summary.tongBan }}</span
                >
              </h3>
            </div>
            <div
              class="icon-box bg-warning bg-opacity-10 text-warning rounded-3 fs-3"
            >
              <i class="bi bi-grid-fill"></i>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div class="row g-3 mb-4">
      <div class="col-xl-3 col-md-6">
        <div
          class="card border-0 shadow-sm h-100 card-hover"
          @click="router.push('/admin/cashbook')"
        >
          <div class="card-body d-flex align-items-center p-3">
            <i class="bi bi-wallet2 fs-2 text-secondary me-3"></i>
            <div>
              <h5 class="mb-0 fw-bold">
                {{ summary.tienMat.toLocaleString("vi-VN") }}
              </h5>
              <small class="text-muted fw-bold">TIỀN MẶT</small>
            </div>
          </div>
        </div>
      </div>

      <div class="col-xl-3 col-md-6">
        <div
          class="card border-0 shadow-sm h-100 card-hover"
          @click="router.push('/admin/cashbook')"
        >
          <div class="card-body d-flex align-items-center p-3">
            <i class="bi bi-qr-code-scan fs-2 text-primary me-3"></i>
            <div>
              <h5 class="mb-0 fw-bold">
                {{ summary.chuyenKhoan.toLocaleString("vi-VN") }}
              </h5>
              <small class="text-muted fw-bold">CHUYỂN KHOẢN</small>
            </div>
          </div>
        </div>
      </div>

      <div class="col-xl-3 col-md-6">
        <div
          class="card border-0 shadow-sm h-100 card-hover"
          @click="router.push('/admin/orders')"
        >
          <div class="card-body d-flex align-items-center p-3">
            <i class="bi bi-arrow-return-left fs-2 text-danger me-3"></i>
            <div>
              <h5 class="mb-0 fw-bold">{{ summary.donHuy }}</h5>
              <small class="text-muted fw-bold">MÓN HỦY/TRẢ</small>
            </div>
          </div>
        </div>
      </div>

      <div class="col-xl-3 col-md-6">
        <div
          class="card border-0 shadow-sm h-100 card-hover border-start border-danger border-4"
          @click="router.push('/admin/inventory')"
        >
          <div class="card-body d-flex align-items-center p-3">
            <i
              class="bi bi-exclamation-triangle-fill fs-2 text-danger me-3 pulse-icon"
            ></i>
            <div>
              <h5 class="mb-0 fw-bold text-danger">
                {{ summary.canhBaoKho }} SP
              </h5>
              <small class="text-danger fw-bold">SẮP HẾT HÀNG</small>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div class="row g-4">
      <div class="col-lg-12">
        <div class="card border-0 shadow-sm h-100 rounded-3 overflow-hidden">
          <div
            class="card-header bg-white border-bottom py-3 d-flex justify-content-between align-items-center"
          >
            <h6 class="fw-bold text-dark mb-0">
              <i class="bi bi-bar-chart-fill text-primary me-2"></i> BIỂU ĐỒ
              DOANH THU 7 NGÀY QUA
            </h6>
            <select
              class="form-select form-select-sm w-auto shadow-none fw-bold text-secondary border-0 bg-light"
            >
              <option>Theo Doanh thu</option>
              <option>Theo Đơn hàng</option>
            </select>
          </div>
          <div class="card-body p-4">
            <div v-if="chartData.labels.length > 0" style="height: 350px">
              <Bar :data="chartData" :options="chartOptions" />
            </div>
            <div
              v-else
              class="d-flex flex-column justify-content-center align-items-center"
              style="
                height: 350px;
                background-color: #fcfcfc;
                border: 2px dashed #eaeaea;
                border-radius: 12px;
              "
            >
              <div class="bg-white p-3 rounded-circle shadow-sm mb-3">
                <i class="bi bi-inbox fs-2 text-muted"></i>
              </div>
              <h6 class="text-muted fw-bold">Chưa có dữ liệu giao dịch</h6>
              <p class="small text-muted">
                Các biểu đồ sẽ xuất hiện khi có phát sinh doanh thu.
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch } from "vue";
import { useRouter } from "vue-router";
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
import { globalState } from "../store";

ChartJS.register(
  Title,
  Tooltip,
  Legend,
  BarElement,
  CategoryScale,
  LinearScale,
);

const router = useRouter();
const isLoading = ref(true);

// Cấu trúc Data mới theo yêu cầu
const summary = ref({
  tongDonHang: 0,
  doanhThu: 0,
  tamTinh: 0,
  tienMat: 0,
  chuyenKhoan: 0,
  donHuy: 0,
  canhBaoKho: 0,
  banDangDung: 0,
  tongBan: 0,
});

const chartData = ref({
  labels: [],
  datasets: [
    {
      label: "Doanh thu",
      backgroundColor: "#4e73df", // Đổi màu chart cho sang trọng
      hoverBackgroundColor: "#2e59d9",
      borderRadius: 6,
      data: [],
    },
  ],
});

const chartOptions = ref({
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: { display: false },
    tooltip: {
      backgroundColor: "rgba(0, 0, 0, 0.8)",
      padding: 12,
      titleFont: { size: 14 },
      bodyFont: { size: 14, weight: "bold" },
      callbacks: {
        label: function (context) {
          let label = context.dataset.label || "";
          if (label) {
            label += ": ";
          }
          if (context.parsed.y !== null) {
            label += new Intl.NumberFormat("vi-VN", {
              style: "currency",
              currency: "VND",
            }).format(context.parsed.y);
          }
          return label;
        },
      },
    },
  },
  scales: {
    y: {
      beginAtZero: true,
      grid: { borderDash: [5, 5], color: "#f0f0f0" },
      ticks: {
        callback: function (value) {
          if (value >= 1000000) return value / 1000000 + " Tr";
          if (value >= 1000) return value / 1000 + " k";
          return value;
        },
      },
    },
    x: { grid: { display: false } },
  },
});

const fetchDashboardData = async () => {
  if (!globalState.value.activeBranchId) return;

  isLoading.value = true;
  try {
    const response = await axios.get(
      `/api/Dashboard/summary?chiNhanhId=${globalState.value.activeBranchId}`,
    );

    // Nối dữ liệu từ API vào UI. Nếu API chưa có đủ cột thì tạm thời để giá trị gốc 0
    summary.value = { ...summary.value, ...response.data.summary };

    chartData.value = {
      labels: response.data.chart.labels,
      datasets: [
        {
          label: "Doanh thu",
          backgroundColor: "#4e73df",
          hoverBackgroundColor: "#2e59d9",
          borderRadius: 6,
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

onMounted(() => {
  fetchDashboardData();
});

watch(
  () => globalState.value.activeBranchId,
  (newId) => {
    if (newId) fetchDashboardData();
  },
);
</script>

<style scoped>
.icon-box {
  width: 55px;
  height: 55px;
  display: flex;
  align-items: center;
  justify-content: center;
}
.card-hover {
  transition: all 0.2s ease-in-out;
  cursor: pointer;
}
.card-hover:hover {
  transform: translateY(-4px);
  box-shadow: 0 0.5rem 1rem rgba(0, 0, 0, 0.1) !important;
  border-color: #dee2e6 !important;
}
.pulse-icon {
  animation: pulse 2s infinite;
}
@keyframes pulse {
  0% {
    transform: scale(1);
    opacity: 1;
  }
  50% {
    transform: scale(1.2);
    opacity: 0.7;
  }
  100% {
    transform: scale(1);
    opacity: 1;
  }
}
</style>
