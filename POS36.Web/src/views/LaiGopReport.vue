<template>
  <div class="container-fluid px-4 py-4 report-wrapper">
    <!-- Header -->
    <div class="d-flex flex-column flex-md-row justify-content-between align-items-md-center mb-4 pb-3 border-bottom gap-3">
      <div>
        <h4 class="fw-bold text-dark mb-1 d-flex align-items-center">
          <span class="icon-box me-2 bg-success bg-opacity-10 text-success rounded-3 p-2 d-inline-flex">
            <i class="bi bi-graph-up-arrow fs-5"></i>
          </span>
          BÁO CÁO LÃI GỘP
        </h4>
        <p class="text-muted mb-0 small">
          Phân tích chi tiết Doanh thu - Giá vốn = Lãi gộp dựa trên phiếu nhập kho và lịch sử bán hàng thực tế.
        </p>
      </div>

      <!-- Bộ lọc nhanh & Tùy chỉnh kết hợp -->
      <div class="d-flex flex-wrap gap-2 align-items-center">
        <!-- Lọc nhanh presets -->
        <div class="btn-group btn-group-sm rounded-pill shadow-sm p-1 bg-white border" role="group">
          <button
            v-for="p in presets"
            :key="p.value"
            @click="applyPreset(p.value)"
            class="btn rounded-pill border-0 px-3 py-1.5 transition-all text-xs"
            :class="activePreset === p.value ? 'btn-success text-white fw-semibold shadow-sm' : 'btn-light text-secondary'"
          >
            {{ p.label }}
          </button>
        </div>

        <!-- Custom Date Range Picker (Chỉ hiển thị khi activePreset === 'custom') -->
        <div v-if="activePreset === 'custom'" class="d-flex gap-2 align-items-center animate-fade-in">
          <input type="date" v-model="tuNgay" class="form-control form-control-sm rounded-3 shadow-none border" style="width:135px" />
          <span class="text-muted text-xs">→</span>
          <input type="date" v-model="denNgay" class="form-control form-control-sm rounded-3 shadow-none border" style="width:135px" />
          <button class="btn btn-sm btn-success fw-bold px-3 rounded-3" @click="load" :disabled="loading">
            <span v-if="loading" class="spinner-border spinner-border-sm me-1"></span>
            <i v-else class="bi bi-search me-1"></i> Lọc
          </button>
        </div>
      </div>
    </div>

    <!-- Chi nhánh -->
    <div class="mb-4 d-flex justify-content-between align-items-center flex-wrap gap-3" v-if="branches.length > 1">
      <div class="d-flex align-items-center gap-2">
        <span class="text-muted small fw-semibold"><i class="bi bi-geo-alt-fill text-danger"></i> Chi nhánh:</span>
        <select v-model="chiNhanhId" class="form-select form-select-sm rounded-pill border shadow-sm w-auto px-3 py-1 fw-bold text-dark" @change="load">
          <option v-for="b in branches" :key="b.id" :value="b.id">{{ b.tenChiNhanh }}</option>
        </select>
      </div>
      
      <!-- Xuất báo cáo -->
      <button v-if="data && data.theoSanPham.length" @click="exportExcel" class="btn btn-sm btn-outline-success fw-bold px-3 rounded-pill shadow-sm transition-all">
        <i class="bi bi-file-earmark-excel-fill me-1"></i> Xuất Excel
      </button>
    </div>
    
    <div class="mb-4 d-flex justify-content-end" v-else-if="data && data.theoSanPham.length">
      <button @click="exportExcel" class="btn btn-sm btn-outline-success fw-bold px-3 rounded-pill shadow-sm transition-all">
        <i class="bi bi-file-earmark-excel-fill me-1"></i> Xuất Excel
      </button>
    </div>

    <!-- Trạng thái loading / empty -->
    <div v-if="loading" class="text-center py-5">
      <div class="spinner-border text-success" style="width:3rem;height:3rem"></div>
      <div class="mt-3 text-muted">Đang tính toán dữ liệu lãi gộp...</div>
    </div>

    <div v-else-if="!data" class="text-center py-5 text-muted card border-0 shadow-sm rounded-4 bg-white p-5">
      <i class="bi bi-bar-chart-line fs-1 d-block mb-3 text-success opacity-50 animated-bounce"></i>
      <h5>Chưa có dữ liệu phân tích</h5>
      <p class="text-secondary small mb-0">Chọn một khoảng thời gian phía trên để xem phân tích hiệu quả kinh doanh.</p>
    </div>

    <template v-else>
      <!-- CẢNH BÁO nếu có SP chưa có giá vốn -->
      <div v-if="data.tongQuan.sanPhamChuaTinhVon > 0" class="alert alert-warning border-0 shadow-sm rounded-4 d-flex gap-3 align-items-center mb-4 p-3 animated-shake">
        <div class="icon-warning-box bg-warning bg-opacity-25 rounded-circle p-2 text-warning d-inline-flex">
          <i class="bi bi-exclamation-triangle-fill fs-5"></i>
        </div>
        <div>
          <h6 class="fw-bold mb-0 text-dark-orange">Cảnh báo dữ liệu giá vốn</h6>
          <span class="small text-secondary">{{ data.tongQuan.luuY }}</span>
        </div>
      </div>

      <!-- KPI Cards -->
      <div class="row g-3 mb-4">
        <div class="col-md-3">
          <div class="card border-0 shadow-sm rounded-4 p-3 h-100 kpi-card bg-emerald">
            <div class="d-flex justify-content-between align-items-start">
              <div>
                <div class="kpi-label small fw-bold text-success text-uppercase tracking-wider mb-1">Doanh thu</div>
                <div class="fw-bold fs-4 text-dark font-monospace">{{ formatVND(data.tongQuan.tongDoanhThu) }}</div>
              </div>
              <span class="kpi-icon-wrapper rounded-3 bg-success bg-opacity-10 text-success p-2">
                <i class="bi bi-cash-stack fs-4"></i>
              </span>
            </div>
          </div>
        </div>
        <div class="col-md-3">
          <div class="card border-0 shadow-sm rounded-4 p-3 h-100 kpi-card bg-amber">
            <div class="d-flex justify-content-between align-items-start">
              <div>
                <div class="kpi-label small fw-bold text-warning text-uppercase tracking-wider mb-1">Giá vốn hàng bán</div>
                <div class="fw-bold fs-4 text-dark font-monospace">{{ formatVND(data.tongQuan.tongGiaVon) }}</div>
              </div>
              <span class="kpi-icon-wrapper rounded-3 bg-warning bg-opacity-10 text-warning p-2">
                <i class="bi bi-box-seam-fill fs-4"></i>
              </span>
            </div>
          </div>
        </div>
        <div class="col-md-3">
          <div class="card border-0 shadow-sm rounded-4 p-3 h-100 kpi-card" :class="data.tongQuan.tongLaiGop >= 0 ? 'bg-blue' : 'bg-red'">
            <div class="d-flex justify-content-between align-items-start">
              <div>
                <div class="kpi-label small fw-bold text-uppercase tracking-wider mb-1" :class="data.tongQuan.tongLaiGop >= 0 ? 'text-primary' : 'text-danger'">Lãi gộp</div>
                <div class="fw-bold fs-4 text-dark font-monospace">{{ formatVND(data.tongQuan.tongLaiGop) }}</div>
              </div>
              <span class="kpi-icon-wrapper rounded-3 p-2" :class="data.tongQuan.tongLaiGop >= 0 ? 'bg-primary bg-opacity-10 text-primary' : 'bg-danger bg-opacity-10 text-danger'">
                <i class="bi fs-4" :class="data.tongQuan.tongLaiGop >= 0 ? 'bi-graph-up text-primary' : 'bi-graph-down text-danger'"></i>
              </span>
            </div>
          </div>
        </div>
        <div class="col-md-3">
          <div class="card border-0 shadow-sm rounded-4 p-3 h-100 kpi-card bg-purple">
            <div class="d-flex justify-content-between align-items-start">
              <div>
                <div class="kpi-label small fw-bold text-uppercase tracking-wider mb-1" style="color:#7c3aed">Tỷ lệ lãi gộp</div>
                <div class="fw-bold fs-3 text-dark font-monospace">{{ data.tongQuan.tiLeLaiGop }}%</div>
              </div>
              <span class="kpi-icon-wrapper rounded-3 p-2" style="background:#f3e8ff; color:#7c3aed">
                <i class="bi bi-percent fs-4"></i>
              </span>
            </div>
            <div class="progress mt-3 rounded-pill" style="height:6px; background:#e9d5ff">
              <div class="progress-bar rounded-pill" role="progressbar"
                   :style="`width:${Math.min(data.tongQuan.tiLeLaiGop, 100)}%; background:#7c3aed`"></div>
            </div>
          </div>
        </div>
      </div>

      <!-- Biểu đồ theo ngày -->
      <div class="card border-0 shadow-sm rounded-4 p-4 mb-4 bg-white" v-if="data.theoNgay.length">
        <h6 class="fw-bold text-dark mb-4 d-flex align-items-center">
          <i class="bi bi-bar-chart-line me-2 text-success"></i>Lãi gộp theo ngày
        </h6>
        <div class="chart-container d-flex gap-3 align-items-end" style="height:160px; overflow-x:auto; padding-bottom:10px;">
          <div v-for="d in data.theoNgay" :key="d.ngay"
               class="chart-bar-group d-flex flex-column align-items-center flex-shrink-0 position-relative group"
               style="min-width:60px">
            
            <!-- Tooltip khi hover cột -->
            <div class="chart-tooltip opacity-0 group-hover:opacity-100 transition-opacity bg-dark text-white rounded-3 py-1.5 px-2.5 position-absolute text-center shadow-md border border-secondary" 
                 style="bottom: 110%; z-index: 10; min-width: 120px; font-size: 11px; pointer-events: none; transition: 0.15s ease;">
              <div class="fw-bold mb-0.5">{{ d.ngay }}</div>
              <div class="text-success font-monospace" v-if="d.laiGop >= 0">+{{ formatVND(d.laiGop) }}</div>
              <div class="text-danger font-monospace" v-else>{{ formatVND(d.laiGop) }}</div>
              <div class="text-muted" style="font-size:9px">Doanh thu: {{ formatVNDShort(d.doanhThu) }}</div>
            </div>

            <div class="chart-bar-value text-muted font-monospace mb-1.5 fw-bold" style="font-size:10px">
              {{ formatVNDShort(d.laiGop) }}
            </div>
            
            <!-- Cột biểu đồ -->
            <div class="chart-bar rounded-top w-75 hover:scale-x-105 transition-all shadow-sm"
                 :style="`height:${Math.max(8, Math.abs(d.laiGop) / maxDayLaiGop * 100)}px; background: ${d.laiGop >= 0 ? 'linear-gradient(180deg,#4ade80,#22c55e)' : 'linear-gradient(180deg,#f87171,#ef4444)'}`">
            </div>
            
            <div class="text-muted mt-2 small fw-semibold" style="font-size:11px">{{ d.ngay }}</div>
          </div>
        </div>
      </div>

      <!-- Bảng chi tiết theo sản phẩm -->
      <div class="card border-0 shadow-sm rounded-4 overflow-hidden bg-white">
        <div class="card-header bg-white border-0 py-3 px-4 d-flex justify-content-between align-items-center flex-wrap gap-2">
          <h6 class="fw-bold text-dark mb-0"><i class="bi bi-table me-2 text-primary"></i>Lãi gộp theo sản phẩm</h6>
          
          <!-- Tìm kiếm sản phẩm trong bảng -->
          <div class="input-group input-group-sm shadow-none" style="width: 260px;">
            <span class="input-group-text bg-white border-end-0 border-light"><i class="bi bi-search text-muted"></i></span>
            <input type="text" v-model="filterSearch" class="form-control border-start-0 border-light shadow-none" placeholder="Tìm tên sản phẩm..." />
            <button v-if="filterSearch" @click="filterSearch = ''" class="btn btn-outline-secondary border-light border-start-0" type="button">
              <i class="bi bi-x-lg"></i>
            </button>
          </div>
        </div>
        <div class="table-responsive">
          <table class="table table-hover align-middle mb-0">
            <thead class="table-light text-muted small text-uppercase">
              <tr>
                <th class="ps-4" style="width:60px">#</th>
                <th>Sản phẩm</th>
                <th class="text-center" style="width:120px">SL bán</th>
                <th class="text-end" style="width:160px">Doanh thu</th>
                <th class="text-end" style="width:160px">Giá vốn</th>
                <th class="text-end" style="width:160px">Lãi gộp</th>
                <th class="text-center pe-4" style="width:140px">Tỷ lệ Lãi Gộp</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(sp, idx) in filteredProducts" :key="idx" class="transition-all">
                <td class="ps-4 text-secondary">{{ idx + 1 }}</td>
                <td class="fw-bold text-dark">{{ sp.tenSanPham }}</td>
                <td class="text-center font-monospace text-secondary fw-semibold">{{ sp.soLuongBan }}</td>
                <td class="text-end font-monospace fw-semibold text-dark">{{ formatVND(sp.doanhThu) }}</td>
                <td class="text-end font-monospace text-warning">{{ formatVND(sp.giaVon) }}</td>
                <td class="text-end font-monospace fw-bold" :class="sp.laiGop >= 0 ? 'text-success' : 'text-danger'">
                  {{ sp.laiGop >= 0 ? '+' : '' }}{{ formatVND(sp.laiGop) }}
                </td>
                <td class="text-center pe-4">
                  <span class="badge rounded-pill px-3 py-1.5 font-monospace"
                        :class="sp.tiLeLaiGop >= 50 ? 'bg-success bg-opacity-10 text-success' : sp.tiLeLaiGop >= 20 ? 'bg-warning bg-opacity-10 text-dark-orange' : 'bg-danger bg-opacity-10 text-danger'">
                    {{ sp.tiLeLaiGop }}%
                  </span>
                </td>
              </tr>
              <tr v-if="filteredProducts.length === 0">
                <td colspan="7" class="text-center py-4 text-muted fst-italic">
                  <i class="bi bi-inbox d-block fs-3 mb-1"></i> Không tìm thấy sản phẩm nào trùng khớp!
                </td>
              </tr>
            </tbody>
            <tfoot class="table-light fw-bold" v-if="filteredProducts.length > 0">
              <tr>
                <td colspan="2" class="ps-4">TỔNG CỘNG HÀNG</td>
                <td class="text-center font-monospace text-secondary">{{ totalQtySold }}</td>
                <td class="text-end font-monospace text-dark">{{ formatVND(data.tongQuan.tongDoanhThu) }}</td>
                <td class="text-end font-monospace text-warning">{{ formatVND(data.tongQuan.tongGiaVon) }}</td>
                <td class="text-end font-monospace text-success">{{ formatVND(data.tongQuan.tongLaiGop) }}</td>
                <td class="text-center pe-4 text-purple font-monospace" style="color:#7c3aed">{{ data.tongQuan.tiLeLaiGop }}%</td>
              </tr>
            </tfoot>
          </table>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue';
import axios from 'axios';
import * as XLSX from 'xlsx';

const tuNgay = ref(new Date().toISOString().slice(0, 10));
const denNgay = ref(new Date().toISOString().slice(0, 10));
const loading = ref(false);
const data = ref(null);
const branches = ref([]);
const chiNhanhId = ref(null);

// Lọc nhanh & tìm kiếm
const activePreset = ref('today');
const filterSearch = ref('');

const presets = [
  { label: 'Hôm nay', value: 'today' },
  { label: 'Hôm qua', value: 'yesterday' },
  { label: '7 ngày qua', value: '7days' },
  { label: 'Tháng này', value: 'thisMonth' },
  { label: 'Tháng trước', value: 'lastMonth' },
  { label: 'Tùy chỉnh', value: 'custom' }
];

const fmtDate = (d) => d.toISOString().slice(0, 10);

const applyPreset = (preset) => {
  activePreset.value = preset;
  const today = new Date();
  
  if (preset === 'today') {
    tuNgay.value = fmtDate(today);
    denNgay.value = fmtDate(today);
  } else if (preset === 'yesterday') {
    const yesterday = new Date(today);
    yesterday.setDate(today.getDate() - 1);
    tuNgay.value = fmtDate(yesterday);
    denNgay.value = fmtDate(yesterday);
  } else if (preset === '7days') {
    const sevenDaysAgo = new Date(today);
    sevenDaysAgo.setDate(today.getDate() - 6);
    tuNgay.value = fmtDate(sevenDaysAgo);
    denNgay.value = fmtDate(today);
  } else if (preset === 'thisMonth') {
    const start = new Date(today.getFullYear(), today.getMonth(), 1);
    tuNgay.value = fmtDate(start);
    denNgay.value = fmtDate(today);
  } else if (preset === 'lastMonth') {
    const start = new Date(today.getFullYear(), today.getMonth() - 1, 1);
    const end = new Date(today.getFullYear(), today.getMonth(), 0);
    tuNgay.value = fmtDate(start);
    denNgay.value = fmtDate(end);
  } else if (preset === 'custom') {
    return;
  }
  
  load();
};

// Computed property để tìm kiếm sản phẩm trong bảng
const filteredProducts = computed(() => {
  if (!data.value?.theoSanPham) return [];
  if (!filterSearch.value.trim()) return data.value.theoSanPham;
  const s = filterSearch.value.toLowerCase().trim();
  return data.value.theoSanPham.filter(sp => sp.tenSanPham.toLowerCase().includes(s));
});

// Tính tổng số lượng đã bán
const totalQtySold = computed(() => {
  if (!filteredProducts.value.length) return 0;
  return filteredProducts.value.reduce((sum, sp) => sum + sp.soLuongBan, 0);
});

// Lấy danh sách chi nhánh
const fetchBranches = async () => {
  try {
    const res = await axios.get('/api/ThietLap/chinhanh');
    branches.value = res.data || [];
    if (branches.value.length > 0) chiNhanhId.value = branches.value[0].id;
  } catch {}
};

const load = async () => {
  if (!chiNhanhId.value) return;
  loading.value = true;
  try {
    const res = await axios.get('/api/BaoCao/lai-gop', {
      params: { chiNhanhId: chiNhanhId.value, tuNgay: tuNgay.value, denNgay: denNgay.value }
    });
    data.value = res.data;
  } catch (e) {
    console.error(e);
  } finally {
    loading.value = false;
  }
};

// Hàm xuất báo cáo Excel
const exportExcel = () => {
  if (!data.value?.theoSanPham?.length) return;
  
  const headers = ["STT", "Sản phẩm", "Số lượng bán", "Doanh thu (đ)", "Giá vốn (đ)", "Lãi gộp (đ)", "Tỷ lệ lãi gộp (%)"];
  const rows = data.value.theoSanPham.map((sp, idx) => [
    idx + 1,
    sp.tenSanPham,
    sp.soLuongBan,
    sp.doanhThu,
    sp.giaVon,
    sp.laiGop,
    `${sp.tiLeLaiGop}%`
  ]);
  
  // Add total row
  rows.push([
    "",
    "TỔNG CỘNG KỲ",
    data.value.theoSanPham.reduce((sum, sp) => sum + sp.soLuongBan, 0),
    data.value.tongQuan.tongDoanhThu,
    data.value.tongQuan.tongGiaVon,
    data.value.tongQuan.tongLaiGop,
    `${data.value.tongQuan.tiLeLaiGop}%`
  ]);

  const worksheet = XLSX.utils.aoa_to_sheet([headers, ...rows]);
  
  // Set column widths
  worksheet["!cols"] = [
    { wch: 6 },  // STT
    { wch: 30 }, // Sản phẩm
    { wch: 15 }, // Số lượng bán
    { wch: 18 }, // Doanh thu
    { wch: 18 }, // Giá vốn
    { wch: 18 }, // Lãi gộp
    { wch: 18 }  // Tỷ lệ LG
  ];

  const workbook = XLSX.utils.book_new();
  XLSX.utils.book_append_sheet(workbook, worksheet, "Lãi Gộp");
  XLSX.writeFile(workbook, `BaoCao_LaiGop_ChiNhanh_${chiNhanhId.value}_${tuNgay.value}_den_${denNgay.value}.xlsx`);
};

const maxDayLaiGop = computed(() => {
  if (!data.value?.theoNgay?.length) return 1;
  return Math.max(...data.value.theoNgay.map(d => Math.abs(d.laiGop)), 1);
});

const formatVND = (v) => {
  if (v == null) return '0 ₫';
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(v);
};

const formatVNDShort = (v) => {
  if (v == null) return '';
  if (Math.abs(v) >= 1_000_000) return (v / 1_000_000).toFixed(1) + 'M';
  if (Math.abs(v) >= 1_000) return (v / 1_000).toFixed(0) + 'K';
  return String(v);
};

onMounted(async () => {
  await fetchBranches();
  applyPreset('today');
});
</script>

<style scoped>
.report-wrapper {
  font-family: 'Inter', sans-serif;
  color: #334155;
  background-color: #f8fafc;
  min-height: 100vh;
}

.icon-box {
  box-shadow: 0 4px 6px -1px rgb(0 0 0 / 0.05);
}

.transition-all {
  transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
}

.text-xs {
  font-size: 0.75rem;
}

/* KPI Cards */
.kpi-card {
  transition: transform 0.2s, box-shadow 0.2s;
  border: 1px solid #f1f5f9 !important;
}

.kpi-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 10px 15px -3px rgb(0 0 0 / 0.05), 0 4px 6px -4px rgb(0 0 0 / 0.05) !important;
}

.bg-emerald {
  background: linear-gradient(135deg, #f0fdf4, #dcfce7) !important;
}

.bg-amber {
  background: linear-gradient(135deg, #fff7ed, #ffedd5) !important;
}

.bg-blue {
  background: linear-gradient(135deg, #eff6ff, #dbeafe) !important;
}

.bg-red {
  background: linear-gradient(135deg, #fef2f2, #fee2e2) !important;
}

.bg-purple {
  background: linear-gradient(135deg, #f5f3ff, #ede9fe) !important;
}

.tracking-wider {
  letter-spacing: 0.05em;
}

.text-dark-orange {
  color: #ea580c;
}

/* Chart */
.chart-container::-webkit-scrollbar {
  height: 6px;
}
.chart-container::-webkit-scrollbar-thumb {
  background: #cbd5e1;
  border-radius: 4px;
}

.chart-bar-group:hover .chart-tooltip {
  opacity: 1 !important;
  transform: translateY(-5px);
}

.chart-bar {
  transition: transform 0.2s ease, height 0.3s ease;
  cursor: pointer;
}

.chart-bar:hover {
  transform: scaleX(1.1);
  filter: brightness(1.05);
}

/* Table */
.table th {
  font-weight: 600;
  font-size: 0.8rem;
  letter-spacing: 0.03em;
  background-color: #f8fafc;
}

.table tbody tr {
  border-bottom: 1px solid #f1f5f9;
}

.table tbody tr:hover {
  background-color: #f8fafc;
}

.progress-bar {
  transition: width 0.4s cubic-bezier(0.4, 0, 0.2, 1);
}

/* Animations */
.animate-fade-in {
  animation: fadeIn 0.2s ease-out forwards;
}

@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(2px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}
</style>
