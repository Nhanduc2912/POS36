<template>
  <div class="container-fluid px-4 py-4">
    <div class="card border-0 shadow-sm rounded-3">
      <!-- Card Header -->
      <div class="card-header bg-white border-bottom py-3 d-flex flex-wrap justify-content-between align-items-center gap-3">
        <div>
          <h5 class="fw-bold text-dark mb-1">
            <i class="bi bi-clock-history text-primary me-2"></i> NHẬT KÝ HOẠT ĐỘNG HỆ THỐNG
          </h5>
          <p class="text-muted mb-0 small">Giám sát và ghi nhận toàn bộ thao tác vận hành của nhân viên và chủ cửa hàng theo thời gian thực.</p>
        </div>
        <button class="btn btn-light btn-sm fw-bold border" @click="fetchLogs">
          <i class="bi bi-arrow-clockwise me-1"></i> Làm mới
        </button>
      </div>

      <!-- Filters Toolbar -->
      <div class="card-body bg-light border-bottom p-3">
        <div class="row g-3 align-items-end">
          <!-- Chọn chi nhánh -->
          <div class="col-lg-3 col-md-6 col-12">
            <label class="form-label text-secondary small fw-bold">Chi nhánh</label>
            <select v-model="filters.branchId" class="form-select bg-white border">
              <option :value="0">-- Tất cả chi nhánh --</option>
              <option v-for="b in branches" :key="b.id" :value="b.id">{{ b.tenChiNhanh }}</option>
            </select>
          </div>

          <!-- Từ ngày -->
          <div class="col-lg-2 col-md-3 col-6">
            <label class="form-label text-secondary small fw-bold">Từ ngày</label>
            <input type="date" v-model="filters.startDate" class="form-control bg-white border" />
          </div>

          <!-- Đến ngày -->
          <div class="col-lg-2 col-md-3 col-6">
            <label class="form-label text-secondary small fw-bold">Đến ngày</label>
            <input type="date" v-model="filters.endDate" class="form-control bg-white border" />
          </div>

          <!-- Tìm kiếm từ khóa -->
          <div class="col-lg-3 col-md-8 col-12">
            <label class="form-label text-secondary small fw-bold">Tìm kiếm từ khóa</label>
            <div class="input-group">
              <span class="input-group-text bg-white border-end-0"><i class="bi bi-search text-muted"></i></span>
              <input type="text" v-model="filters.search" class="form-control bg-white border border-start-0" placeholder="Người dùng, hành động, mô tả..." @keyup.enter="fetchLogs" />
            </div>
          </div>

          <!-- Nút lọc nhanh -->
          <div class="col-lg-2 col-md-4 col-12">
            <button class="btn btn-primary w-100 fw-bold" @click="fetchLogs" :disabled="loading">
              <span v-if="loading" class="spinner-border spinner-border-sm me-1"></span>
              <i v-else class="bi bi-filter me-1"></i> Lọc dữ liệu
            </button>
          </div>
        </div>
      </div>

      <!-- Audit Logs Table -->
      <div class="card-body p-0 table-responsive">
        <table class="table table-hover align-middle mb-0">
          <thead class="table-light text-muted small text-uppercase">
            <tr>
              <th class="ps-4" style="width: 170px">Thời gian (Giây)</th>
              <th style="width: 140px">Người thực hiện</th>
              <th style="width: 120px">Vai trò</th>
              <th style="width: 140px">Hành động</th>
              <th>Chi tiết hoạt động</th>
              <th style="width: 150px">Địa chỉ IP</th>
              <th style="width: 200px">Thiết bị sử dụng</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="log in logs" :key="log.id">
              <!-- Thời gian -->
              <td class="ps-4 fw-bold text-dark small">
                {{ formatDateTime(log.thoiGian) }}
              </td>

              <!-- Người thực hiện -->
              <td class="fw-bold text-primary">{{ log.nguoiThucHien }}</td>

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
                <span class="fw-bold text-uppercase small px-2 py-1 rounded bg-blue-light text-blue">
                  {{ log.hanhDong }}
                </span>
              </td>

              <!-- Mô tả -->
              <td class="text-dark fw-medium" style="max-width: 320px; white-space: normal; word-break: break-word;">
                {{ log.moTa }}
              </td>

              <!-- IP Address -->
              <td class="text-secondary small">{{ log.ipAddress || '---' }}</td>

              <!-- Thiết bị -->
              <td class="text-muted small text-truncate" style="max-width: 200px;" :title="log.thietBi">
                {{ formatUserAgent(log.thietBi) }}
              </td>
            </tr>

            <!-- Loading State -->
            <tr v-if="loading">
              <td colspan="7" class="text-center py-5">
                <div class="spinner-border text-primary mb-3"></div>
                <p class="text-muted mb-0 fw-semibold">Đang truy vấn nhật ký hệ thống...</p>
              </td>
            </tr>

            <!-- Empty State -->
            <tr v-if="!loading && logs.length === 0">
              <td colspan="7" class="text-center py-5 text-muted">
                <i class="bi bi-inbox fs-1 d-block mb-2"></i>
                Không tìm thấy nhật ký hoạt động nào khớp với bộ lọc của bạn.
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, watch } from 'vue';
import axios from 'axios';

const loading = ref(false);
const logs = ref([]);
const branches = ref([]);

const filters = reactive({
  branchId: 0,
  startDate: '',
  endDate: '',
  search: ''
});

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
    const res = await axios.get('/api/NhatKy', {
      params: {
        chiNhanhId: filters.branchId,
        startDate: filters.startDate,
        endDate: filters.endDate,
        search: filters.search
      }
    });
    logs.value = res.data;
  } catch (error) {
    console.error('Lỗi tải nhật ký hoạt động', error);
  } finally {
    loading.value = false;
  }
};

onMounted(async () => {
  // Đặt ngày mặc định là hôm nay
  const today = new Date().toISOString().split('T')[0];
  filters.startDate = today;
  filters.endDate = today;

  await fetchBranches();
  await fetchLogs();
});

// Lọc lại khi thay đổi chi nhánh hoặc khoảng ngày
watch(() => [filters.branchId, filters.startDate, filters.endDate], () => {
  fetchLogs();
});
</script>

<style scoped>
.table th {
  font-weight: 700;
  font-size: 13px;
}
.table td {
  font-size: 14px;
}

.bg-blue-light {
  background-color: #eff6ff;
}
.text-blue {
  color: #1d4ed8;
}

/* Custom styles for Mitglied ranks */
.bg-amber-light { background-color: #fffbeb; }
.text-amber-dark { color: #b45309; }
</style>
