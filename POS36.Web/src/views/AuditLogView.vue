<template>
  <div class="container-fluid px-4 py-4">
    <div class="card border-0 shadow-sm rounded-4 overflow-hidden bg-white">
      <!-- Card Header -->
      <div class="card-header bg-white border-0 pt-4 pb-3 px-4 d-flex flex-wrap justify-content-between align-items-center gap-3">
        <div>
          <h4 class="fw-bold text-dark mb-1 d-flex align-items-center gap-2">
            <i class="bi bi-shield-check-fill text-primary fs-3"></i> NHẬT KÝ HOẠT ĐỘNG HỆ THỐNG
          </h4>
          <p class="text-secondary mb-0 small">
            Giám sát thời gian thực mọi thao tác gọi món, thanh toán, in bill, chuyển tách bàn và vận hành kho của nhân viên.
          </p>
        </div>
        <button class="btn btn-light rounded-pill px-3 py-2 border fw-bold text-secondary d-flex align-items-center gap-1 hover-shadow" @click="fetchLogs">
          <i class="bi bi-arrow-clockwise fs-5"></i> Làm mới
        </button>
      </div>

      <!-- Filters & Toolbar Section -->
      <div class="card-body bg-light bg-opacity-50 border-top border-bottom border-light p-4">
        <div class="row g-3">
          <!-- Hàng 1: Chi nhánh, Lọc nhanh thời gian, Từ khóa -->
          <div class="col-lg-3 col-md-6 col-12">
            <label class="form-label text-dark small fw-bold">Chi nhánh hoạt động</label>
            <select v-model="filters.branchId" class="form-select bg-white border border-light-subtle rounded-3 py-2">
              <option :value="0">-- Tất cả chi nhánh --</option>
              <option v-for="b in branches" :key="b.id" :value="b.id">{{ b.tenChiNhanh }}</option>
            </select>
          </div>

          <div class="col-lg-3 col-md-6 col-12">
            <label class="form-label text-dark small fw-bold">Khoảng thời gian</label>
            <select v-model="filters.quickDate" class="form-select bg-white border border-light-subtle rounded-3 py-2">
              <option value="today">Hôm nay (Từ 00:00)</option>
              <option value="yesterday">Hôm qua</option>
              <option value="3days">3 ngày gần đây</option>
              <option value="7days">7 ngày gần đây</option>
              <option value="30days">1 tháng gần đây</option>
              <option value="90days">3 tháng gần đây</option>
              <option value="custom">Tùy chỉnh khoảng ngày...</option>
            </select>
          </div>

          <!-- Nhập từ ngày / đến ngày (Chỉ hiện khi chọn Custom) -->
          <div v-if="filters.quickDate === 'custom'" class="col-lg-3 col-md-6 col-12 row g-2 mx-0 px-0">
            <div class="col-6">
              <label class="form-label text-dark small fw-bold">Từ ngày</label>
              <input type="date" v-model="filters.startDate" class="form-control bg-white border border-light-subtle rounded-3 py-2" />
            </div>
            <div class="col-6">
              <label class="form-label text-dark small fw-bold">Đến ngày</label>
              <input type="date" v-model="filters.endDate" class="form-control bg-white border border-light-subtle rounded-3 py-2" />
            </div>
          </div>

          <div class="col-lg-3 col-md-6 col-12" :class="{'col-lg-6': filters.quickDate !== 'custom'}">
            <label class="form-label text-dark small fw-bold">Tìm kiếm từ khóa</label>
            <div class="input-group">
              <span class="input-group-text bg-white border-end-0 border-light-subtle rounded-start-3"><i class="bi bi-search text-muted"></i></span>
              <input type="text" v-model="filters.search" class="form-control bg-white border border-light-subtle border-start-0 rounded-end-3 py-2" placeholder="Tên nhân viên, chi tiết món ăn, IP..." @keyup.enter="fetchLogs" />
            </div>
          </div>
        </div>

        <!-- Hàng 2: Checkbox Nhóm hành động định sẵn -->
        <div class="row mt-4 pt-3 border-top border-light-subtle">
          <div class="col-12">
            <span class="d-block text-dark small fw-bold mb-2">Lọc theo nhóm hành động (Checkbox)</span>
            <div class="d-flex flex-wrap gap-3">
              <label class="checkbox-pill d-flex align-items-center gap-2 cursor-pointer p-2 rounded-3 border bg-white">
                <input type="checkbox" v-model="actionsSelected.thanhtoan" class="form-check-input" />
                <span class="small fw-semibold text-dark"><i class="bi bi-cash-coin text-success"></i> Thanh toán & QR</span>
              </label>

              <label class="checkbox-pill d-flex align-items-center gap-2 cursor-pointer p-2 rounded-3 border bg-white">
                <input type="checkbox" v-model="actionsSelected.goimon" class="form-check-input" />
                <span class="small fw-semibold text-dark"><i class="bi bi-cart-fill text-primary"></i> Gọi & Hủy món</span>
              </label>

              <label class="checkbox-pill d-flex align-items-center gap-2 cursor-pointer p-2 rounded-3 border bg-white">
                <input type="checkbox" v-model="actionsSelected.inbill" class="form-check-input" />
                <span class="small fw-semibold text-dark"><i class="bi bi-printer-fill text-info"></i> In hóa đơn (In bill)</span>
              </label>

              <label class="checkbox-pill d-flex align-items-center gap-2 cursor-pointer p-2 rounded-3 border bg-white">
                <input type="checkbox" v-model="actionsSelected.dieuphoi" class="form-check-input" />
                <span class="small fw-semibold text-dark"><i class="bi bi-arrow-left-right text-secondary"></i> Chuyển/Tách/Ghép</span>
              </label>

              <label class="checkbox-pill d-flex align-items-center gap-2 cursor-pointer p-2 rounded-3 border bg-white">
                <input type="checkbox" v-model="actionsSelected.thongbao" class="form-check-input" />
                <span class="small fw-semibold text-dark"><i class="bi bi-bell-fill text-warning"></i> Báo bếp & Báo thu ngân</span>
              </label>

              <label class="checkbox-pill d-flex align-items-center gap-2 cursor-pointer p-2 rounded-3 border bg-white">
                <input type="checkbox" v-model="actionsSelected.nhapkho" class="form-check-input" />
                <span class="small fw-semibold text-dark"><i class="bi bi-box-seam-fill text-danger"></i> Nhập kho kho bãi</span>
              </label>
            </div>
          </div>
        </div>
      </div>

      <!-- Audit Logs Table -->
      <div class="card-body p-0 table-responsive">
        <table class="table table-hover align-middle mb-0">
          <thead class="table-light text-muted small text-uppercase">
            <tr>
              <th class="ps-4" style="width: 190px">Thời gian (Giờ:Phút:Giây)</th>
              <th style="width: 170px">Người thực hiện</th>
              <th style="width: 130px">Vai trò</th>
              <th style="width: 160px">Hành động</th>
              <th>Chi tiết thao tác nghiệp vụ</th>
              <th style="width: 140px">Địa chỉ IP</th>
              <th style="width: 200px">Thiết bị / User-Agent</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="log in logs" :key="log.id" class="align-middle">
              <!-- Thời gian -->
              <td class="ps-4 fw-bold text-dark font-monospace" style="font-size:0.85rem">
                {{ formatDateTime(log.thoiGian) }}
              </td>

              <!-- Người thực hiện -->
              <td class="fw-bold text-dark">{{ log.nguoiThucHien }}</td>

              <!-- Vai trò -->
              <td>
                <span v-if="log.vaiTro === 'ChuCuaHang'" class="badge bg-danger rounded-pill px-3 py-2 fw-normal">
                  <i class="bi bi-person-fill-lock me-1"></i> Chủ Quán
                </span>
                <span v-else-if="log.vaiTro === 'QuanLy' || log.vaiTro === 'Admin'" class="badge bg-primary rounded-pill px-3 py-2 fw-normal">
                  <i class="bi bi-shield-fill me-1"></i> Quản Lý
                </span>
                <span v-else-if="log.vaiTro === 'ThuNgan'" class="badge bg-success rounded-pill px-3 py-2 fw-normal">
                  <i class="bi bi-cash-coin me-1"></i> Thu Ngân
                </span>
                <span v-else-if="log.vaiTro === 'Order'" class="badge bg-warning text-dark rounded-pill px-3 py-2 fw-normal">
                  <i class="bi bi-journal-text me-1"></i> Order
                </span>
                <span v-else-if="log.vaiTro === 'Bep'" class="badge bg-secondary rounded-pill px-3 py-2 fw-normal">
                  <i class="bi bi-fire me-1"></i> Bếp
                </span>
                <span v-else class="badge bg-light text-dark border rounded-pill px-3 py-2 fw-normal">{{ log.vaiTro }}</span>
              </td>

              <!-- Hành động -->
              <td>
                <span class="fw-bold text-uppercase px-3 py-1 rounded-3 d-inline-block text-center shadow-xs" :class="getActionBadgeClass(log.hanhDong)" style="font-size:0.75rem; min-width: 120px;">
                  {{ log.hanhDong }}
                </span>
              </td>

              <!-- Mô tả -->
              <td class="text-dark fw-semibold" style="max-width: 350px; white-space: normal; word-break: break-word;">
                {{ log.moTa }}
              </td>

              <!-- IP Address -->
              <td class="text-muted small font-monospace">{{ log.ipAddress || '---' }}</td>

              <!-- Thiết bị -->
              <td class="text-muted small text-truncate" style="max-width: 200px;" :title="log.thietBi">
                {{ formatUserAgent(log.thietBi) }}
              </td>
            </tr>

            <!-- Loading State -->
            <tr v-if="loading">
              <td colspan="7" class="text-center py-5">
                <div class="spinner-border text-primary mb-3" role="status"></div>
                <p class="text-muted mb-0 fw-bold">Đang tải lịch sử hoạt động...</p>
              </td>
            </tr>

            <!-- Empty State -->
            <tr v-if="!loading && logs.length === 0">
              <td colspan="7" class="text-center py-5 text-muted bg-light bg-opacity-25">
                <i class="bi bi-journal-x display-4 text-secondary d-block mb-3"></i>
                Chưa có dữ liệu nhật ký nào được ghi nhận cho bộ lọc đã chọn.
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Pagination Footer -->
      <div v-if="!loading && totalCount > 0" class="card-footer bg-white border-top border-light px-4 py-3 d-flex flex-wrap justify-content-between align-items-center gap-3">
        <div class="text-secondary small fw-semibold">
          Hiển thị từ <span class="text-dark fw-bold">{{ startIndex + 1 }}</span> đến 
          <span class="text-dark fw-bold">{{ endIndex }}</span> trên tổng số 
          <span class="text-primary fw-bold">{{ totalCount }}</span> dòng nhật ký
        </div>

        <div class="d-flex align-items-center gap-3">
          <!-- Page Size Selector -->
          <div class="d-flex align-items-center gap-2">
            <span class="text-secondary small text-nowrap">Số dòng hiển thị:</span>
            <select v-model="filters.pageSize" class="form-select form-select-sm bg-white border border-light-subtle rounded-3" style="width: 75px">
              <option :value="10">10</option>
              <option :value="20">20</option>
              <option :value="50">50</option>
              <option :value="100">100</option>
            </select>
          </div>

          <!-- Pagination Navigation -->
          <nav>
            <ul class="pagination pagination-sm mb-0 rounded-3">
              <li class="page-item" :class="{ disabled: filters.page === 1 }">
                <button class="page-link py-2 px-3 border border-light-subtle" @click="filters.page--" aria-label="Trước">
                  <i class="bi bi-chevron-left"></i>
                </button>
              </li>
              <li v-for="p in visiblePages" :key="p" class="page-item" :class="{ active: filters.page === p }">
                <button class="page-link py-2 px-3 border border-light-subtle" @click="filters.page = p">
                  {{ p }}
                </button>
              </li>
              <li class="page-item" :class="{ disabled: filters.page === totalPages }">
                <button class="page-link py-2 px-3 border border-light-subtle" @click="filters.page++" aria-label="Sau">
                  <i class="bi bi-chevron-right"></i>
                </button>
              </li>
            </ul>
          </nav>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, watch, computed } from 'vue';
import axios from 'axios';

const loading = ref(false);
const logs = ref([]);
const totalCount = ref(0);
const branches = ref([]);

const filters = reactive({
  branchId: 0,
  quickDate: 'today', // today, yesterday, 3days, 7days, 30days, 90days, custom
  startDate: '',
  endDate: '',
  search: '',
  page: 1,
  pageSize: 20
});

// Checkbox Lọc nhóm hành động
const actionsSelected = reactive({
  thanhtoan: true,
  goimon: true,
  inbill: true,
  dieuphoi: true,
  thongbao: true,
  nhapkho: true
});

const totalPages = computed(() => Math.ceil(totalCount.value / filters.pageSize) || 1);
const startIndex = computed(() => (filters.page - 1) * filters.pageSize);
const endIndex = computed(() => Math.min(startIndex.value + filters.pageSize, totalCount.value));

// Tính các trang hiển thị trên pagination bar
const visiblePages = computed(() => {
  const current = filters.page;
  const total = totalPages.value;
  const pages = [];
  
  if (total <= 5) {
    for (let i = 1; i <= total; i++) pages.push(i);
  } else {
    let start = Math.max(1, current - 2);
    let end = Math.min(total, current + 2);
    
    if (start === 1) end = 5;
    else if (end === total) start = total - 4;
    
    for (let i = start; i <= end; i++) pages.push(i);
  }
  return pages;
});

const getSelectedActionsString = () => {
  const list = [];
  if (actionsSelected.thanhtoan) {
    list.push("Thanh toán", "Chuyển khoản thành công", "Tạo mã QR", "Hủy QR");
  }
  if (actionsSelected.goimon) {
    list.push("Gọi món", "Hủy món");
  }
  if (actionsSelected.inbill) {
    list.push("In hóa đơn");
  }
  if (actionsSelected.dieuphoi) {
    list.push("Chuyển bàn", "Tách bàn", "Ghép bàn");
  }
  if (actionsSelected.thongbao) {
    list.push("Báo bếp", "Báo xong món", "Báo thu ngân");
  }
  if (actionsSelected.nhapkho) {
    list.push("Tạo phiếu nhập", "Xác nhận nhập");
  }
  return list.join(",");
};

const getActionBadgeClass = (action) => {
  if (action === 'Thanh toán' || action === 'Chuyển khoản thành công') {
    return 'bg-success bg-opacity-10 text-success border border-success border-opacity-25';
  }
  if (action === 'Gọi món') {
    return 'bg-primary bg-opacity-10 text-primary border border-primary border-opacity-25';
  }
  if (action === 'In hóa đơn') {
    return 'bg-info bg-opacity-10 text-info border border-info border-opacity-25';
  }
  if (action === 'Chuyển bàn' || action === 'Tách bàn' || action === 'Ghép bàn') {
    return 'bg-dark bg-opacity-10 text-dark border border-dark border-opacity-25';
  }
  if (action === 'Hủy món' || action === 'Hủy QR') {
    return 'bg-danger bg-opacity-10 text-danger border border-danger border-opacity-25';
  }
  if (action === 'Báo bếp' || action === 'Báo xong món' || action === 'Báo thu ngân' || action === 'Tạo mã QR') {
    return 'bg-warning bg-opacity-10 text-warning border border-warning border-opacity-25';
  }
  return 'bg-secondary bg-opacity-10 text-secondary border border-secondary border-opacity-25';
};

const formatDateTime = (dateStr) => {
  if (!dateStr) return '—';
  const d = new Date(dateStr);
  return d.toLocaleString('vi-VN', {
    year: 'numeric', month: '2-digit', day: '2-digit',
    hour: '2-digit', minute: '2-digit', second: '2-digit'
  });
};

const formatUserAgent = (ua) => {
  if (!ua) return 'Không rõ';
  if (ua.includes('Mobile') || ua.includes('Android') || ua.includes('iPhone')) {
    return '📱 Điện thoại di động (' + (ua.includes('Android') ? 'Android' : 'iOS') + ')';
  }
  if (ua.includes('Windows')) return '💻 Máy tính (Windows)';
  if (ua.includes('Macintosh')) return '💻 Máy tính (MacOS)';
  if (ua.includes('Linux')) return '💻 Máy tính (Linux)';
  return ua;
};

const fetchBranches = async () => {
  try {
    const res = await axios.get('/api/ThietLap/chinhanh');
    branches.value = res.data;
  } catch (error) {
    console.error('Lỗi tải danh sách chi nhánh', error);
  }
};

const fetchLogs = async () => {
  loading.value = true;
  try {
    const params = {
      chiNhanhId: filters.branchId,
      search: filters.search,
      quickFilter: filters.quickDate,
      actions: getSelectedActionsString(),
      page: filters.page,
      pageSize: filters.pageSize
    };

    if (filters.quickDate === 'custom') {
      params.startDate = filters.startDate;
      params.endDate = filters.endDate;
    }

    const res = await axios.get('/api/NhatKy', { params });
    if (res.data) {
      logs.value = res.data.items || [];
      totalCount.value = res.data.totalCount || 0;
    }
  } catch (error) {
    console.error('Lỗi tải nhật ký hoạt động', error);
  } finally {
    loading.value = false;
  }
};

onMounted(async () => {
  // Mặc định ngày hôm nay cho lọc custom
  const today = new Date().toISOString().split('T')[0];
  filters.startDate = today;
  filters.endDate = today;

  await fetchBranches();
  await fetchLogs();
});

// Lọc lại khi thay đổi chi nhánh, bộ lọc nhanh, số dòng hiển thị, hoặc checkboxes
watch(() => [
  filters.branchId,
  filters.quickDate,
  filters.startDate,
  filters.endDate,
  filters.pageSize,
  actionsSelected.thanhtoan,
  actionsSelected.goimon,
  actionsSelected.inbill,
  actionsSelected.dieuphoi,
  actionsSelected.thongbao,
  actionsSelected.nhapkho
], () => {
  filters.page = 1; // Reset về trang 1
  fetchLogs();
});

// Watch đổi trang riêng biệt
watch(() => filters.page, () => {
  fetchLogs();
});
</script>

<style scoped>
.table th {
  font-weight: 700;
  font-size: 13px;
  background-color: #f8fafc;
}
.table td {
  font-size: 14px;
}
.cursor-pointer {
  cursor: pointer;
}
.checkbox-pill {
  transition: all 0.2s ease;
  user-select: none;
}
.checkbox-pill:hover {
  background-color: #f1f5f9 !important;
  border-color: #cbd5e1 !important;
}
.checkbox-pill input:checked + span {
  font-weight: bold;
}
.page-link {
  color: #475569;
  background-color: #fff;
  transition: all 0.2s ease;
}
.page-item.active .page-link {
  background-color: #0d6efd;
  border-color: #0d6efd;
  color: #fff;
}
.bg-purple {
  background-color: #faf5ff;
}
.text-purple {
  color: #7e22ce;
}
.border-purple {
  border-color: #c084fc;
}
</style>
