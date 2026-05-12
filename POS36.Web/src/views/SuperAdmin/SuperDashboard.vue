<template>
  <div>
    <!-- KPI Cards -->
    <div class="row g-3 mb-4">
      <div class="col-6 col-xl-3" v-for="card in kpiCards" :key="card.label">
        <div class="kpi-card" :style="{ borderColor: card.color }">
          <div class="kpi-icon" :style="{ background: card.color + '18', color: card.color }">
            <i :class="'bi bi-' + card.icon"></i>
          </div>
          <div>
            <div class="kpi-value">{{ card.value }}</div>
            <div class="kpi-label">{{ card.label }}</div>
          </div>
        </div>
      </div>
    </div>

    <!-- Row 2: Revenue + Status Breakdown -->
    <div class="row g-3 mb-4">
      <div class="col-lg-8">
        <div class="dash-card">
          <h6 class="dash-card-title"><i class="bi bi-graph-up me-2"></i>Doanh thu nền tảng 12 tháng</h6>
          <div class="chart-placeholder" v-if="chartLabels.length">
            <div class="chart-bars">
              <div
                v-for="(item, i) in chartData"
                :key="i"
                class="chart-bar-wrap"
              >
                <div
                  class="chart-bar"
                  :style="{ height: getBarHeight(item.value) + '%' }"
                  :title="formatVND(item.value)"
                ></div>
                <span class="chart-bar-label">{{ item.label }}</span>
              </div>
            </div>
          </div>
          <div v-else class="text-muted text-center py-5">Chưa có dữ liệu</div>
        </div>
      </div>

      <div class="col-lg-4">
        <div class="dash-card h-100">
          <h6 class="dash-card-title"><i class="bi bi-pie-chart me-2"></i>Phân bổ trạng thái</h6>
          <div class="status-list">
            <div class="status-item" v-for="s in statusBreakdown" :key="s.label">
              <span class="status-dot" :style="{ background: s.color }"></span>
              <span class="status-label">{{ s.label }}</span>
              <span class="status-count">{{ s.value }}</span>
            </div>
          </div>

          <h6 class="dash-card-title mt-4"><i class="bi bi-exclamation-triangle me-2 text-warning"></i>Cảnh báo</h6>
          <div class="alert-box">
            <div class="alert-item">
              <i class="bi bi-clock-history text-warning"></i>
              <span><strong>{{ data.sapHetHan }}</strong> cửa hàng sắp hết hạn (7 ngày)</span>
            </div>
            <div class="alert-item">
              <i class="bi bi-hourglass-split text-info"></i>
              <span><strong>{{ data.donChoXuLy }}</strong> đơn chờ xử lý</span>
            </div>
            <div class="alert-item">
              <i class="bi bi-person-plus text-success"></i>
              <span><strong>{{ data.dangKyMoi30Ngay }}</strong> đăng ký mới (30 ngày)</span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Row 3: Quick Stats -->
    <div class="row g-3">
      <div class="col-md-4">
        <div class="dash-card text-center">
          <i class="bi bi-cash-stack fs-1 text-success mb-2 d-block"></i>
          <div class="kpi-value">{{ formatVND(data.doanhThuThang) }}</div>
          <div class="kpi-label">Doanh thu tháng này</div>
        </div>
      </div>
      <div class="col-md-4">
        <div class="dash-card text-center">
          <i class="bi bi-wallet2 fs-1 text-warning mb-2 d-block"></i>
          <div class="kpi-value">{{ formatVND(data.doanhThuTong) }}</div>
          <div class="kpi-label">Tổng doanh thu</div>
        </div>
      </div>
      <div class="col-md-4">
        <div class="dash-card text-center">
          <i class="bi bi-cursor-fill fs-1 text-info mb-2 d-block"></i>
          <div class="kpi-value">{{ data.luotTruyCapHomNay }}</div>
          <div class="kpi-label">Lượt truy cập hôm nay</div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from "vue";
import axios from "axios";

const data = ref({
  tongCuaHang: 0, dangHoatDong: 0, dangDungThu: 0, daHetHan: 0, biKhoa: 0,
  sapHetHan: 0, doanhThuThang: 0, doanhThuTong: 0, luotTruyCapHomNay: 0,
  donChoXuLy: 0, dangKyMoi30Ngay: 0, doanhThu12Thang: [],
});

const kpiCards = computed(() => [
  { label: "Tổng cửa hàng", value: data.value.tongCuaHang, icon: "shop", color: "#f59e0b" },
  { label: "Đang hoạt động", value: data.value.dangHoatDong, icon: "check-circle", color: "#22c55e" },
  { label: "Đang dùng thử", value: data.value.dangDungThu, icon: "hourglass-split", color: "#3b82f6" },
  { label: "Hết hạn / Bị khóa", value: data.value.daHetHan + data.value.biKhoa, icon: "lock", color: "#ef4444" },
]);

const statusBreakdown = computed(() => [
  { label: "Hoạt động", value: data.value.dangHoatDong, color: "#22c55e" },
  { label: "Dùng thử", value: data.value.dangDungThu, color: "#3b82f6" },
  { label: "Chỉ đọc", value: data.value.daHetHan, color: "#f59e0b" },
  { label: "Bị khóa", value: data.value.biKhoa, color: "#ef4444" },
]);

const chartData = computed(() => data.value.doanhThu12Thang || []);
const chartLabels = computed(() => chartData.value.map((d) => d.label));

const getBarHeight = (val) => {
  const max = Math.max(...chartData.value.map((d) => d.value), 1);
  return Math.max((val / max) * 100, 3);
};

const formatVND = (n) => {
  if (!n) return "0đ";
  return Number(n).toLocaleString("vi-VN") + "đ";
};

onMounted(async () => {
  try {
    const res = await axios.get("/api/SuperAdmin/dashboard");
    data.value = res.data;
  } catch (e) {
    console.error("Lỗi load dashboard:", e);
  }
});
</script>

<style scoped>
.kpi-card {
  background: #1a1c23;
  border: 1px solid rgba(255, 255, 255, 0.06);
  border-left: 3px solid;
  border-radius: 12px;
  padding: 18px 20px;
  display: flex;
  align-items: center;
  gap: 16px;
  transition: transform 0.2s, box-shadow 0.2s;
}
.kpi-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.3);
}
.kpi-icon {
  width: 44px; height: 44px;
  border-radius: 10px;
  display: flex; align-items: center; justify-content: center;
  font-size: 1.2rem;
}
.kpi-value {
  font-size: 1.4rem; font-weight: 800; color: #f4f4f5;
}
.kpi-label {
  font-size: 0.78rem; color: #6b7280; font-weight: 500;
}

.dash-card {
  background: #1a1c23;
  border: 1px solid rgba(255, 255, 255, 0.06);
  border-radius: 14px;
  padding: 22px;
}
.dash-card-title {
  font-weight: 700; font-size: 0.85rem; color: #9ca3af;
  margin-bottom: 16px; text-transform: uppercase; letter-spacing: 0.5px;
}

/* Chart */
.chart-bars {
  display: flex; align-items: flex-end; gap: 8px; height: 180px;
  padding-top: 10px;
}
.chart-bar-wrap {
  flex: 1; display: flex; flex-direction: column; align-items: center;
}
.chart-bar {
  width: 100%; max-width: 40px;
  background: linear-gradient(180deg, #f59e0b, #d97706);
  border-radius: 6px 6px 0 0;
  transition: height 0.6s ease;
  min-height: 4px;
}
.chart-bar-label {
  font-size: 0.65rem; color: #6b7280; margin-top: 6px;
}

/* Status */
.status-list {
  display: flex; flex-direction: column; gap: 10px;
}
.status-item {
  display: flex; align-items: center; gap: 10px;
  padding: 8px 12px;
  background: rgba(255, 255, 255, 0.03);
  border-radius: 8px;
}
.status-dot {
  width: 10px; height: 10px; border-radius: 50%; flex-shrink: 0;
}
.status-label {
  flex: 1; font-size: 0.85rem; color: #d1d5db;
}
.status-count {
  font-weight: 700; font-size: 1rem; color: #f4f4f5;
}

/* Alerts */
.alert-box {
  display: flex; flex-direction: column; gap: 8px;
}
.alert-item {
  display: flex; align-items: center; gap: 10px;
  font-size: 0.82rem; color: #d1d5db;
  padding: 8px 12px;
  background: rgba(255, 255, 255, 0.03);
  border-radius: 8px;
}
.alert-item i {
  font-size: 1rem;
}
</style>
