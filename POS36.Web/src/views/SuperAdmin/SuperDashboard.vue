<template>
  <div>
    <!-- KPI Cards -->
    <div class="row g-3 mb-4">
      <div class="col-6 col-xl-3" v-for="card in kpiCards" :key="card.label">
        <div class="kpi-card" :style="{ borderLeftColor: card.color }">
          <div class="kpi-icon" :style="{ background: card.color + '1a', color: card.color }">
            <i :class="'bi bi-' + card.icon"></i>
          </div>
          <div>
            <div class="kpi-value">{{ card.value }}</div>
            <div class="kpi-label">{{ card.label }}</div>
          </div>
        </div>
      </div>
    </div>

    <!-- Row 2: Revenue + Status -->
    <div class="row g-3 mb-4">
      <div class="col-lg-8">
        <div class="dash-card">
          <div class="d-flex justify-content-between align-items-center mb-3">
            <h6 class="dash-card-title mb-0"><i class="bi bi-graph-up me-2"></i>Doanh thu nền tảng 12 tháng</h6>
            <button class="ai-report-btn" @click="generateAiReport" :disabled="aiLoading">
              <span v-if="aiLoading" class="spinner-border spinner-border-sm me-1"></span>
              <i v-else class="bi bi-robot me-1"></i>
              {{ aiLoading ? 'Đang phân tích...' : 'Báo cáo AI' }}
            </button>
          </div>
          <div class="chart-bars" v-if="chartData.length">
            <div v-for="(item, i) in chartData" :key="i" class="chart-bar-wrap">
              <div class="chart-bar" :style="{ height: getBarHeight(item.value) + '%' }" :title="formatVND(item.value)"></div>
              <span class="chart-bar-label">{{ item.label }}</span>
            </div>
          </div>
          <div v-else class="text-center py-5" style="color: var(--sa-text-faint)">Chưa có dữ liệu</div>
        </div>
      </div>

      <div class="col-lg-4">
        <div class="dash-card h-100">
          <h6 class="dash-card-title"><i class="bi bi-pie-chart me-2"></i>Phân bổ trạng thái</h6>
          <div class="status-list">
            <div class="status-item" v-for="s in statusBreakdown" :key="s.label">
              <span class="status-dot" :style="{ background: s.color }"></span>
              <span class="status-lbl">{{ s.label }}</span>
              <span class="status-val">{{ s.value }}</span>
            </div>
          </div>

          <h6 class="dash-card-title mt-4"><i class="bi bi-exclamation-triangle me-2 text-warning"></i>Cảnh báo</h6>
          <div class="alert-list">
            <div class="alert-item"><i class="bi bi-clock-history text-warning"></i><span><strong>{{ data.sapHetHan }}</strong> cửa hàng sắp hết hạn</span></div>
            <div class="alert-item"><i class="bi bi-hourglass-split text-info"></i><span><strong>{{ data.donChoXuLy }}</strong> đơn chờ xử lý</span></div>
            <div class="alert-item"><i class="bi bi-person-plus text-success"></i><span><strong>{{ data.dangKyMoi30Ngay }}</strong> đăng ký mới (30 ngày)</span></div>
          </div>
        </div>
      </div>
    </div>

    <!-- Row 3: Revenue Quick Stats -->
    <div class="row g-3 mb-4">
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

    <!-- AI Report Panel -->
    <div v-if="aiReport" class="dash-card mb-4">
      <div class="d-flex justify-content-between align-items-center mb-3">
        <h6 class="dash-card-title mb-0"><i class="bi bi-robot me-2 text-warning"></i>Phân tích AI — Gemini 2.5 Flash</h6>
        <button class="sa-btn-sm sa-btn-danger" @click="aiReport = ''" title="Đóng"><i class="bi bi-x"></i></button>
      </div>
      <div class="ai-report-content" v-html="aiReport"></div>
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

const aiReport = ref("");
const aiLoading = ref(false);

const kpiCards = computed(() => [
  { label: "Tổng cửa hàng", value: data.value.tongCuaHang, icon: "shop", color: "#f59e0b" },
  { label: "Đang hoạt động", value: data.value.dangHoatDong, icon: "check-circle", color: "#22c55e" },
  { label: "Đang dùng thử", value: data.value.dangDungThu, icon: "hourglass-split", color: "#3b82f6" },
  { label: "Hết hạn / Bị khóa", value: (data.value.daHetHan || 0) + (data.value.biKhoa || 0), icon: "lock", color: "#ef4444" },
]);

const statusBreakdown = computed(() => [
  { label: "Hoạt động", value: data.value.dangHoatDong, color: "#22c55e" },
  { label: "Dùng thử", value: data.value.dangDungThu, color: "#3b82f6" },
  { label: "Chỉ đọc", value: data.value.daHetHan, color: "#f59e0b" },
  { label: "Bị khóa", value: data.value.biKhoa, color: "#ef4444" },
]);

const chartData = computed(() => data.value.doanhThu12Thang || []);

const getBarHeight = (val) => {
  const max = Math.max(...chartData.value.map((d) => d.value), 1);
  return Math.max((val / max) * 100, 3);
};

const formatVND = (n) => {
  if (!n) return "0đ";
  return Number(n).toLocaleString("vi-VN") + "đ";
};

const generateAiReport = async () => {
  aiLoading.value = true;
  aiReport.value = "";
  try {
    const summary = {
      tongCuaHang: data.value.tongCuaHang,
      dangHoatDong: data.value.dangHoatDong,
      dangDungThu: data.value.dangDungThu,
      biKhoa: data.value.biKhoa,
      doanhThuThang: data.value.doanhThuThang,
      doanhThuTong: data.value.doanhThuTong,
      sapHetHan: data.value.sapHetHan,
      donChoXuLy: data.value.donChoXuLy,
    };
    const res = await axios.post("/api/SuperAdmin/ai-analyze", summary);
    aiReport.value = res.data.htmlReport || "<p>Không có kết quả.</p>";
  } catch (e) {
    aiReport.value = "<p class='text-danger'>❌ Không thể kết nối AI. Kiểm tra GeminiAI:ApiKey trong appsettings.</p>";
  } finally {
    aiLoading.value = false;
  }
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
@import "./sa-shared.css";

.kpi-card {
  background: var(--sa-surface);
  border: 1px solid var(--sa-border);
  border-left: 3px solid;
  border-radius: 12px;
  padding: 18px 20px;
  display: flex; align-items: center; gap: 16px;
  transition: transform 0.2s, box-shadow 0.2s;
}
.kpi-card:hover { transform: translateY(-2px); box-shadow: 0 8px 24px var(--sa-shadow); }
.kpi-icon { width: 44px; height: 44px; border-radius: 10px; display: flex; align-items: center; justify-content: center; font-size: 1.2rem; }
.kpi-value { font-size: 1.4rem; font-weight: 800; color: var(--sa-text); }
.kpi-label { font-size: 0.78rem; color: var(--sa-text-faint); font-weight: 500; }

/* Status */
.status-list { display: flex; flex-direction: column; gap: 8px; }
.status-item { display: flex; align-items: center; gap: 10px; padding: 8px 12px; background: var(--sa-nav-hover-bg); border-radius: 8px; }
.status-dot { width: 10px; height: 10px; border-radius: 50%; flex-shrink: 0; }
.status-lbl { flex: 1; font-size: 0.85rem; color: var(--sa-text-muted); }
.status-val { font-weight: 700; font-size: 1rem; color: var(--sa-text); }

/* Alerts */
.alert-list { display: flex; flex-direction: column; gap: 8px; }
.alert-item { display: flex; align-items: center; gap: 10px; font-size: 0.82rem; color: var(--sa-text-muted); padding: 8px 12px; background: var(--sa-nav-hover-bg); border-radius: 8px; }
.alert-item i { font-size: 1rem; }
.alert-item strong { color: var(--sa-text); }

/* AI Report Button */
.ai-report-btn {
  display: flex; align-items: center; gap: 6px;
  background: rgba(245, 158, 11, 0.12);
  border: 1px solid rgba(245, 158, 11, 0.3);
  color: #f59e0b;
  padding: 6px 14px; border-radius: 8px;
  font-size: 0.8rem; font-weight: 700; cursor: pointer;
  transition: 0.2s;
}
.ai-report-btn:hover { background: rgba(245, 158, 11, 0.22); }
.ai-report-btn:disabled { opacity: 0.6; cursor: not-allowed; }

/* AI Report Content */
.ai-report-content {
  font-size: 0.9rem; line-height: 1.7;
  color: var(--sa-text-muted);
  border-top: 1px solid var(--sa-border);
  padding-top: 16px;
}
.ai-report-content strong, .ai-report-content b { color: var(--sa-text); }
.ai-report-content h2, .ai-report-content h3 { color: var(--sa-text); font-size: 1rem; font-weight: 700; margin-top: 12px; }
.ai-report-content ul { padding-left: 18px; }

/* Charts */
.chart-bars { display: flex; align-items: flex-end; gap: 8px; height: 180px; padding-top: 10px; }
.chart-bar-wrap { flex: 1; display: flex; flex-direction: column; align-items: center; }
.chart-bar { width: 100%; max-width: 40px; background: linear-gradient(180deg, var(--sa-accent), #d97706); border-radius: 6px 6px 0 0; transition: height 0.6s ease; min-height: 4px; }
.chart-bar-label { font-size: 0.65rem; color: var(--sa-text-faint); margin-top: 6px; }
</style>
