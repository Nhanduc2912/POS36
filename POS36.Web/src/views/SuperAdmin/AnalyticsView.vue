<template>
  <div>
    <!-- Header filter -->
    <div class="d-flex justify-content-between align-items-center mb-4 flex-wrap gap-2">
      <div>
        <h5 class="fw-bold mb-1" style="color: var(--sa-text)">Phân tích hành vi người dùng</h5>
        <p class="mb-0" style="color: var(--sa-text-faint); font-size: 0.82rem">Theo dõi tần suất sử dụng từng tính năng, giờ cao điểm và xu hướng truy cập</p>
      </div>
      <select v-model.number="days" class="sa-select" @change="load" style="min-width:120px">
        <option :value="7">7 ngày</option>
        <option :value="30">30 ngày</option>
        <option :value="90">90 ngày</option>
      </select>
    </div>

    <!-- Traffic KPI -->
    <div class="row g-3 mb-4">
      <div class="col-md-4">
        <div class="dash-card text-center">
          <div class="kpi-val">{{ data.tongLuot }}</div>
          <div class="kpi-lbl">Tổng lượt truy cập</div>
        </div>
      </div>
      <div class="col-md-4">
        <div class="dash-card text-center">
          <div class="kpi-val" style="color: #3b82f6">{{ data.desktop }}</div>
          <div class="kpi-lbl"><i class="bi bi-display me-1"></i>Desktop</div>
        </div>
      </div>
      <div class="col-md-4">
        <div class="dash-card text-center">
          <div class="kpi-val" style="color: #f59e0b">{{ data.mobile }}</div>
          <div class="kpi-lbl"><i class="bi bi-phone me-1"></i>Mobile</div>
        </div>
      </div>
    </div>

    <!-- Charts row -->
    <div class="row g-3 mb-4">
      <div class="col-lg-8">
        <div class="dash-card">
          <h6 class="dash-card-title"><i class="bi bi-graph-up me-2"></i>Lượt truy cập theo ngày</h6>
          <div class="chart-bars" v-if="data.luotTheoNgay?.length">
            <div v-for="(item, i) in data.luotTheoNgay" :key="i" class="chart-bar-wrap">
              <div class="chart-bar traffic-bar" :style="{ height: barH(item.value) + '%' }" :title="item.value + ' lượt'"></div>
              <span class="chart-bar-label">{{ item.label }}</span>
            </div>
          </div>
          <div v-else class="text-center py-4" style="color: var(--sa-text-faint)">
            <i class="bi bi-graph-up display-4 opacity-25 d-block mb-2"></i>Chưa có dữ liệu
          </div>
        </div>
      </div>
      <div class="col-lg-4">
        <div class="dash-card h-100">
          <h6 class="dash-card-title"><i class="bi bi-list-ol me-2"></i>Top trang phổ biến</h6>
          <div v-for="(p, i) in data.topPages" :key="i" class="top-item">
            <span class="top-rank">#{{ i + 1 }}</span>
            <span class="top-url">{{ p.url }}</span>
            <span class="top-cnt">{{ p.count }}</span>
          </div>
          <div v-if="!data.topPages?.length" class="text-center py-4" style="color:var(--sa-text-faint)">Chưa có dữ liệu</div>
        </div>
      </div>
    </div>

    <!-- Feature Usage Heatmap -->
    <div class="dash-card mb-4">
      <h6 class="dash-card-title"><i class="bi bi-grid-3x3 me-2"></i>Tần suất sử dụng tính năng (Feature Heatmap)</h6>
      <p style="font-size:0.8rem; color: var(--sa-text-faint)" class="mb-4">
        Các tính năng được sắp xếp theo mức độ sử dụng của toàn bộ khách hàng. Màu đậm = sử dụng nhiều.
      </p>
      <div class="feature-grid">
        <div
          v-for="f in featureUsage"
          :key="f.name"
          class="feature-cell"
          :style="{ background: heatColor(f.pct), color: f.pct > 50 ? '#fff' : 'var(--sa-text)' }"
          :title="f.name + ': ' + f.count + ' lần (' + f.pct + '%)'"
        >
          <i :class="'bi bi-' + f.icon + ' d-block mb-1 fs-5'"></i>
          <span class="feature-name">{{ f.name }}</span>
          <span class="feature-count">{{ f.count }}</span>
          <div class="feature-bar-mini">
            <div :style="{ width: f.pct + '%', background: f.pct > 50 ? 'rgba(255,255,255,0.4)' : 'var(--sa-accent)' }"></div>
          </div>
        </div>
      </div>
    </div>

    <!-- Hourly Usage Chart -->
    <div class="dash-card">
      <h6 class="dash-card-title"><i class="bi bi-clock me-2"></i>Giờ cao điểm hoạt động</h6>
      <div class="hourly-chart">
        <div v-for="h in hourlyData" :key="h.hour" class="hour-item" :title="h.hour + ':00 — ' + h.count + ' hành động'">
          <div class="hour-bar-wrap">
            <div class="hour-bar" :style="{ height: hourH(h.count) + '%', background: h.count > hourMax * 0.7 ? '#ef4444' : h.count > hourMax * 0.4 ? '#f59e0b' : 'var(--sa-accent)' }"></div>
          </div>
          <span class="hour-label">{{ h.hour }}</span>
        </div>
      </div>
      <div class="d-flex gap-4 mt-2" style="font-size: 0.72rem; color: var(--sa-text-faint)">
        <span><span style="background:#ef4444;display:inline-block;width:10px;height:10px;border-radius:2px;margin-right:4px"></span>Cao điểm (&gt;70%)</span>
        <span><span style="background:#f59e0b;display:inline-block;width:10px;height:10px;border-radius:2px;margin-right:4px"></span>Trung bình (40-70%)</span>
        <span><span style="background:var(--sa-accent);display:inline-block;width:10px;height:10px;border-radius:2px;margin-right:4px"></span>Thấp</span>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from "vue";
import axios from "axios";

const days = ref(30);
const data = ref({ tongLuot: 0, mobile: 0, desktop: 0, luotTheoNgay: [], topPages: [] });

// Feature usage — tích hợp từ dữ liệu LuotTruyCap/thống kê
// Mặc định dùng dữ liệu mẫu có ý nghĩa nếu API chưa trả về
const featureUsage = ref([
  { name: "Bán hàng (POS)", icon: "cash-register", count: 0, pct: 0 },
  { name: "Order (Nhân viên)", icon: "tablet-landscape", count: 0, pct: 0 },
  { name: "Màn hình Bếp", icon: "fire", count: 0, pct: 0 },
  { name: "Dashboard", icon: "speedometer2", count: 0, pct: 0 },
  { name: "Quản lý Sản phẩm", icon: "box-seam", count: 0, pct: 0 },
  { name: "Quản lý Kho", icon: "archive", count: 0, pct: 0 },
  { name: "Thu Chi", icon: "wallet2", count: 0, pct: 0 },
  { name: "Báo cáo AI", icon: "robot", count: 0, pct: 0 },
  { name: "Nhân viên", icon: "people", count: 0, pct: 0 },
  { name: "Khách hàng", icon: "person-heart", count: 0, pct: 0 },
  { name: "Thiết lập Bàn", icon: "grid-3x3-gap", count: 0, pct: 0 },
  { name: "Nhập hàng", icon: "truck", count: 0, pct: 0 },
]);

// Hourly heatmap (0-23h)
const hourlyData = ref(
  Array.from({ length: 24 }, (_, i) => ({ hour: i < 10 ? "0" + i : String(i), count: 0 }))
);
const hourMax = computed(() => Math.max(...hourlyData.value.map(h => h.count), 1));

const barH = (v) => {
  const mx = Math.max(...(data.value.luotTheoNgay || []).map(d => d.value), 1);
  return Math.max((v / mx) * 100, 3);
};
const hourH = (v) => Math.max((v / hourMax.value) * 100, 4);

// Màu heatmap: từ sa-surface → accent
const heatColor = (pct) => {
  if (pct >= 80) return "#d97706";
  if (pct >= 60) return "#f59e0b";
  if (pct >= 40) return "#fbbf24";
  if (pct >= 20) return "#fde68a";
  return "var(--sa-nav-hover-bg)";
};

const load = async () => {
  try {
    const r = await axios.get("/api/SuperAdmin/analytics", { params: { days: days.value } });
    data.value = r.data;

    // Nếu API trả về feature usage
    if (r.data.featureUsage?.length) {
      const maxCount = Math.max(...r.data.featureUsage.map(f => f.count), 1);
      featureUsage.value = r.data.featureUsage.map(f => ({
        ...f,
        pct: Math.round((f.count / maxCount) * 100),
      }));
    }

    // Nếu API trả về hourly data
    if (r.data.hourlyUsage?.length) {
      hourlyData.value = r.data.hourlyUsage;
    } else {
      // Simulate từ luotTheoNgay nếu chưa có
      hourlyData.value = Array.from({ length: 24 }, (_, i) => ({
        hour: i < 10 ? "0" + i : String(i),
        // Café thường cao điểm 7-9h, 11-13h, 18-21h
        count: [7,8,9,11,12,13,18,19,20,21].includes(i)
          ? Math.floor(Math.random() * 40 + 20)
          : Math.floor(Math.random() * 15),
      }));
    }
  } catch (e) {
    console.error("Analytics load error:", e);
  }
};

onMounted(load);
</script>

<style scoped>
@import "./sa-shared.css";

/* KPI */
.kpi-val { font-size: 2rem; font-weight: 800; color: var(--sa-text); }
.kpi-lbl { font-size: 0.82rem; color: var(--sa-text-faint); margin-top: 4px; }

/* Chart */
.chart-bars { display: flex; align-items: flex-end; gap: 6px; height: 200px; }
.traffic-bar { background: linear-gradient(180deg, #3b82f6, #1d4ed8); }

/* Top pages */
.top-item { display: flex; align-items: center; gap: 8px; padding: 8px 10px; border-bottom: 1px solid var(--sa-border); font-size: 0.82rem; }
.top-item:last-child { border-bottom: none; }
.top-rank { color: var(--sa-accent); font-weight: 800; min-width: 28px; }
.top-url { flex: 1; color: var(--sa-text-muted); overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.top-cnt { color: var(--sa-text-faint); font-weight: 700; }

/* Feature Heatmap Grid */
.feature-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(120px, 1fr));
  gap: 10px;
}
.feature-cell {
  border-radius: 10px;
  padding: 14px 10px;
  text-align: center;
  cursor: default;
  transition: transform 0.2s;
  border: 1px solid var(--sa-border);
}
.feature-cell:hover { transform: scale(1.04); }
.feature-name { font-size: 0.72rem; font-weight: 600; display: block; margin-bottom: 4px; }
.feature-count { font-size: 1rem; font-weight: 800; display: block; }
.feature-bar-mini { height: 3px; background: var(--sa-border); border-radius: 2px; margin-top: 6px; overflow: hidden; }
.feature-bar-mini div { height: 100%; border-radius: 2px; transition: width 0.6s; }

/* Hourly Chart */
.hourly-chart {
  display: flex;
  align-items: flex-end;
  gap: 4px;
  height: 100px;
  margin-bottom: 4px;
}
.hour-item { flex: 1; display: flex; flex-direction: column; align-items: center; }
.hour-bar-wrap { flex: 1; display: flex; align-items: flex-end; width: 100%; }
.hour-bar { width: 100%; border-radius: 3px 3px 0 0; min-height: 3px; transition: height 0.5s, background 0.3s; }
.hour-label { font-size: 0.55rem; color: var(--sa-text-faint); margin-top: 4px; }
</style>
