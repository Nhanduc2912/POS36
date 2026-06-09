<template>
  <div class="container-fluid px-4 py-4">
    <!-- Header -->
    <div class="d-flex justify-content-between align-items-center mb-4 pb-3 border-bottom">
      <div>
        <h4 class="fw-bold text-dark mb-1">
          <i class="bi bi-graph-up-arrow text-success me-2"></i> BÁO CÁO LÃI GỘP
        </h4>
        <p class="text-muted mb-0 small">
          Phân tích Doanh thu - Giá vốn = Lãi gộp. Dữ liệu từ phiếu nhập kho thực tế.
        </p>
      </div>
      <div class="d-flex gap-2 align-items-center">
        <input type="date" v-model="tuNgay" class="form-control" style="width:145px" />
        <span class="text-muted">→</span>
        <input type="date" v-model="denNgay" class="form-control" style="width:145px" />
        <button class="btn btn-success fw-bold px-4 rounded-pill" @click="load" :disabled="loading">
          <span v-if="loading" class="spinner-border spinner-border-sm me-1"></span>
          <i v-else class="bi bi-bar-chart-fill me-1"></i> Xem báo cáo
        </button>
      </div>
    </div>

    <!-- Chi nhánh -->
    <div class="mb-4" v-if="branches.length > 1">
      <select v-model="chiNhanhId" class="form-select form-select-sm w-auto" @change="load">
        <option v-for="b in branches" :key="b.id" :value="b.id">{{ b.tenChiNhanh }}</option>
      </select>
    </div>

    <!-- Trạng thái loading / empty -->
    <div v-if="loading" class="text-center py-5">
      <div class="spinner-border text-success" style="width:3rem;height:3rem"></div>
      <div class="mt-3 text-muted">Đang tính toán lãi gộp...</div>
    </div>

    <div v-else-if="!data" class="text-center py-5 text-muted">
      <i class="bi bi-bar-chart-line fs-1 d-block mb-2 text-success opacity-50"></i>
      Chọn kỳ và bấm <strong>Xem báo cáo</strong> để phân tích lãi gộp.
    </div>

    <template v-else>
      <!-- CẢNH BÁO nếu có SP chưa có giá vốn -->
      <div v-if="data.tongQuan.sanPhamChuaTinhVon > 0" class="alert alert-warning d-flex gap-2 align-items-center mb-4">
        <i class="bi bi-exclamation-triangle-fill fs-5"></i>
        <span>{{ data.tongQuan.luuY }}</span>
      </div>

      <!-- KPI Cards -->
      <div class="row g-3 mb-4">
        <div class="col-md-3">
          <div class="card border-0 shadow-sm rounded-3 p-3 h-100" style="background:linear-gradient(135deg,#f0fdf4,#dcfce7)">
            <div class="d-flex justify-content-between align-items-start">
              <div>
                <div class="text-success small fw-semibold mb-1">Doanh thu</div>
                <div class="fw-bold fs-5 text-dark">{{ formatVND(data.tongQuan.tongDoanhThu) }}</div>
              </div>
              <i class="bi bi-currency-dollar fs-3 text-success opacity-50"></i>
            </div>
          </div>
        </div>
        <div class="col-md-3">
          <div class="card border-0 shadow-sm rounded-3 p-3 h-100" style="background:linear-gradient(135deg,#fff7ed,#ffedd5)">
            <div class="d-flex justify-content-between align-items-start">
              <div>
                <div class="text-warning small fw-semibold mb-1">Giá vốn hàng bán</div>
                <div class="fw-bold fs-5 text-dark">{{ formatVND(data.tongQuan.tongGiaVon) }}</div>
              </div>
              <i class="bi bi-box-seam fs-3 text-warning opacity-50"></i>
            </div>
          </div>
        </div>
        <div class="col-md-3">
          <div class="card border-0 shadow-sm rounded-3 p-3 h-100" :style="data.tongQuan.tongLaiGop >= 0 ? 'background:linear-gradient(135deg,#eff6ff,#dbeafe)' : 'background:linear-gradient(135deg,#fef2f2,#fee2e2)'">
            <div class="d-flex justify-content-between align-items-start">
              <div>
                <div class="small fw-semibold mb-1" :class="data.tongQuan.tongLaiGop >= 0 ? 'text-primary' : 'text-danger'">Lãi gộp</div>
                <div class="fw-bold fs-5 text-dark">{{ formatVND(data.tongQuan.tongLaiGop) }}</div>
              </div>
              <i class="bi bi-trending-up fs-3 opacity-50" :class="data.tongQuan.tongLaiGop >= 0 ? 'text-primary' : 'text-danger'"></i>
            </div>
          </div>
        </div>
        <div class="col-md-3">
          <div class="card border-0 shadow-sm rounded-3 p-3 h-100" style="background:linear-gradient(135deg,#f5f3ff,#ede9fe)">
            <div class="d-flex justify-content-between align-items-start">
              <div>
                <div class="text-purple small fw-semibold mb-1" style="color:#7c3aed">Tỷ lệ lãi gộp</div>
                <div class="fw-bold fs-4 text-dark">{{ data.tongQuan.tiLeLaiGop }}%</div>
              </div>
              <i class="bi bi-pie-chart-fill fs-3 opacity-50" style="color:#7c3aed"></i>
            </div>
            <div class="progress mt-2" style="height:6px">
              <div class="progress-bar" role="progressbar"
                   :style="`width:${Math.min(data.tongQuan.tiLeLaiGop, 100)}%; background:#7c3aed`"></div>
            </div>
          </div>
        </div>
      </div>

      <!-- Biểu đồ theo ngày -->
      <div class="card border-0 shadow-sm rounded-3 p-4 mb-4" v-if="data.theoNgay.length">
        <h6 class="fw-bold text-dark mb-3"><i class="bi bi-bar-chart me-2 text-success"></i>Lãi gộp theo ngày</h6>
        <div class="d-flex gap-1 align-items-end" style="height:120px;overflow-x:auto">
          <div v-for="d in data.theoNgay" :key="d.ngay"
               class="d-flex flex-column align-items-center flex-shrink-0"
               style="min-width:48px">
            <div class="text-muted" style="font-size:10px">{{ formatVNDShort(d.laiGop) }}</div>
            <div class="rounded-top w-100"
                 :style="`height:${Math.max(6, d.laiGop / maxDayLaiGop * 90)}px; background:${d.laiGop >= 0 ? '#22c55e' : '#ef4444'}`"
                 :title="`${d.ngay}: ${formatVND(d.laiGop)}`">
            </div>
            <div class="text-muted mt-1" style="font-size:10px">{{ d.ngay }}</div>
          </div>
        </div>
      </div>

      <!-- Bảng chi tiết theo sản phẩm -->
      <div class="card border-0 shadow-sm rounded-3 overflow-hidden">
        <div class="card-header bg-white border-0 py-3 px-4">
          <h6 class="fw-bold text-dark mb-0"><i class="bi bi-table me-2 text-primary"></i>Lãi gộp theo sản phẩm</h6>
        </div>
        <div class="table-responsive">
          <table class="table table-hover mb-0">
            <thead class="table-light">
              <tr>
                <th class="ps-4 text-muted small">#</th>
                <th class="text-muted small">Sản phẩm</th>
                <th class="text-end text-muted small">SL bán</th>
                <th class="text-end text-muted small">Doanh thu</th>
                <th class="text-end text-muted small">Giá vốn</th>
                <th class="text-end text-muted small">Lãi gộp</th>
                <th class="text-end text-muted small pe-4">Tỷ lệ LG</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(sp, idx) in data.theoSanPham" :key="idx">
                <td class="ps-4 text-muted small">{{ idx + 1 }}</td>
                <td class="fw-semibold text-dark">{{ sp.tenSanPham }}</td>
                <td class="text-end text-muted">{{ sp.soLuongBan }}</td>
                <td class="text-end text-dark">{{ formatVND(sp.doanhThu) }}</td>
                <td class="text-end text-warning">{{ formatVND(sp.giaVon) }}</td>
                <td class="text-end fw-bold" :class="sp.laiGop >= 0 ? 'text-success' : 'text-danger'">
                  {{ formatVND(sp.laiGop) }}
                </td>
                <td class="text-end pe-4">
                  <span class="badge rounded-pill"
                        :class="sp.tiLeLaiGop >= 50 ? 'bg-success' : sp.tiLeLaiGop >= 20 ? 'bg-warning text-dark' : 'bg-danger'">
                    {{ sp.tiLeLaiGop }}%
                  </span>
                </td>
              </tr>
            </tbody>
            <tfoot class="table-light fw-bold">
              <tr>
                <td colspan="3" class="ps-4">TỔNG KỲ</td>
                <td class="text-end text-dark">{{ formatVND(data.tongQuan.tongDoanhThu) }}</td>
                <td class="text-end text-warning">{{ formatVND(data.tongQuan.tongGiaVon) }}</td>
                <td class="text-end text-success">{{ formatVND(data.tongQuan.tongLaiGop) }}</td>
                <td class="text-end pe-4 text-purple" style="color:#7c3aed">{{ data.tongQuan.tiLeLaiGop }}%</td>
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

const tuNgay = ref(new Date().toISOString().slice(0, 10));
const denNgay = ref(new Date().toISOString().slice(0, 10));
const loading = ref(false);
const data = ref(null);
const branches = ref([]);
const chiNhanhId = ref(null);

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

const maxDayLaiGop = computed(() => {
  if (!data.value?.theoNgay?.length) return 1;
  return Math.max(...data.value.theoNgay.map(d => Math.abs(d.laiGop)), 1);
});

const formatVND = (v) => {
  if (v == null) return '—';
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
  await load();
});
</script>

<style scoped>
.table tbody tr:hover {
  background: #f8faff;
}
tfoot td {
  border-top: 2px solid #e2e8f0;
}
</style>
