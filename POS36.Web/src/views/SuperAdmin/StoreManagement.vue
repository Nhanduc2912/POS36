<template>
  <div>
    <!-- Toolbar -->
    <div class="d-flex justify-content-between align-items-center mb-4 flex-wrap gap-2">
      <div class="d-flex gap-2 flex-wrap">
        <select v-model="filterStatus" class="sa-select" @change="loadStores">
          <option value="">Tất cả trạng thái</option>
          <option value="HoatDong">Hoạt động</option>
          <option value="DungThu">Dùng thử</option>
          <option value="ChiDoc">Chỉ đọc</option>
          <option value="BiKhoa">Bị khóa</option>
        </select>
        <input v-model="searchText" class="sa-input" placeholder="Tìm cửa hàng..." @input="loadStores" />
      </div>
      <span class="text-muted small">Tổng: <strong class="text-warning">{{ stores.length }}</strong> cửa hàng</span>
    </div>

    <!-- Table -->
    <div class="sa-table-wrap">
      <table class="sa-table">
        <thead>
          <tr>
            <th>ID</th>
            <th>Tên cửa hàng</th>
            <th>SĐT</th>
            <th>Email</th>
            <th>Gói</th>
            <th>Trạng thái</th>
            <th>Hết hạn</th>
            <th>Còn lại</th>
            <th>Thao tác</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="s in stores" :key="s.id">
            <td class="text-muted">#{{ s.id }}</td>
            <td class="fw-bold">{{ s.tenCuaHang }}</td>
            <td>{{ s.soDienThoai }}</td>
            <td class="text-muted small">{{ s.email || "—" }}</td>
            <td><span class="sa-goi-badge">{{ s.goiDichVu || "Trial" }}</span></td>
            <td>
              <span class="sa-status-badge" :class="'status-' + s.trangThai">
                {{ statusMap[s.trangThai] || s.trangThai }}
              </span>
            </td>
            <td class="small">{{ formatDate(s.ngayHetHan) }}</td>
            <td>
              <span :class="s.soNgayConLai <= 7 ? 'text-danger fw-bold' : 'text-success'">
                {{ s.soNgayConLai }} ngày
              </span>
            </td>
            <td>
              <div class="d-flex gap-1">
                <button class="sa-btn-sm sa-btn-info" @click="viewDetail(s.id)" title="Chi tiết">
                  <i class="bi bi-eye"></i>
                </button>
                <button class="sa-btn-sm" :class="s.trangThai === 'BiKhoa' ? 'sa-btn-success' : 'sa-btn-danger'"
                  @click="toggleStatus(s.id)" :title="s.trangThai === 'BiKhoa' ? 'Mở khóa' : 'Khóa'">
                  <i :class="s.trangThai === 'BiKhoa' ? 'bi bi-unlock' : 'bi bi-lock'"></i>
                </button>
                <button class="sa-btn-sm sa-btn-warning" @click="openExtend(s)" title="Gia hạn">
                  <i class="bi bi-plus-circle"></i>
                </button>
              </div>
            </td>
          </tr>
          <tr v-if="!stores.length">
            <td colspan="9" class="text-center text-muted py-4">Không tìm thấy cửa hàng nào.</td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Modal: Detail -->
    <div class="sa-modal-overlay" v-if="showDetail" @click.self="showDetail = false">
      <div class="sa-modal">
        <div class="sa-modal-header">
          <h5><i class="bi bi-shop me-2"></i>{{ detail.store?.tenCuaHang }}</h5>
          <button class="sa-modal-close" @click="showDetail = false">&times;</button>
        </div>
        <div class="sa-modal-body" v-if="detail.store">
          <div class="row g-3 mb-3">
            <div class="col-6"><label>SĐT</label><p>{{ detail.store.soDienThoai }}</p></div>
            <div class="col-6"><label>Email</label><p>{{ detail.store.email || "—" }}</p></div>
            <div class="col-6"><label>Trạng thái</label><p>{{ statusMap[detail.store.trangThai] }}</p></div>
            <div class="col-6"><label>Gói</label><p>{{ detail.store.goiDichVu || "Trial" }}</p></div>
            <div class="col-6"><label>Ngày đăng ký</label><p>{{ formatDate(detail.store.ngayDangKy) }}</p></div>
            <div class="col-6"><label>Hết hạn</label><p>{{ formatDate(detail.store.ngayHetHan) }}</p></div>
          </div>
          <div class="row g-3 mb-3">
            <div class="col-3 text-center"><div class="kpi-mini">{{ detail.thongKe?.soNhanVien }}</div><small>Nhân viên</small></div>
            <div class="col-3 text-center"><div class="kpi-mini">{{ detail.thongKe?.soSanPham }}</div><small>Sản phẩm</small></div>
            <div class="col-3 text-center"><div class="kpi-mini">{{ detail.thongKe?.soHoaDon }}</div><small>Hóa đơn</small></div>
            <div class="col-3 text-center"><div class="kpi-mini text-success">{{ formatVND(detail.thongKe?.doanhThu) }}</div><small>Doanh thu</small></div>
          </div>
          <h6 class="text-muted mt-3 mb-2"><i class="bi bi-clock-history me-1"></i>Lịch sử gói</h6>
          <div class="sa-table-wrap">
            <table class="sa-table sa-table-sm">
              <thead><tr><th>Gói</th><th>Số tiền</th><th>Trạng thái</th><th>Ngày</th></tr></thead>
              <tbody>
                <tr v-for="l in detail.lichSuGoi" :key="l.id">
                  <td>{{ l.tenGoi }}</td>
                  <td>{{ formatVND(l.soTienThanhToan) }}</td>
                  <td><span class="sa-status-badge" :class="'status-' + l.trangThai">{{ l.trangThai }}</span></td>
                  <td class="small">{{ formatDate(l.ngayTao) }}</td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>

    <!-- Modal: Extend -->
    <div class="sa-modal-overlay" v-if="showExtend" @click.self="showExtend = false">
      <div class="sa-modal" style="max-width: 420px">
        <div class="sa-modal-header">
          <h5><i class="bi bi-plus-circle me-2"></i>Gia hạn: {{ extendStore?.tenCuaHang }}</h5>
          <button class="sa-modal-close" @click="showExtend = false">&times;</button>
        </div>
        <div class="sa-modal-body">
          <div class="mb-3">
            <label class="form-label small fw-bold text-muted">Số tháng gia hạn</label>
            <select v-model.number="extendMonths" class="sa-select w-100">
              <option :value="6">6 tháng</option>
              <option :value="12">12 tháng</option>
              <option :value="24">24 tháng</option>
            </select>
          </div>
          <button class="btn btn-warning w-100 fw-bold" @click="doExtend">
            <i class="bi bi-check-lg me-1"></i> Xác nhận gia hạn
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, inject } from "vue";
import axios from "axios";
const swal = inject("$swal");

const stores = ref([]);
const filterStatus = ref("");
const searchText = ref("");
const showDetail = ref(false);
const detail = ref({});
const showExtend = ref(false);
const extendStore = ref(null);
const extendMonths = ref(12);

const statusMap = {
  HoatDong: "Hoạt động", DungThu: "Dùng thử",
  ChiDoc: "Chỉ đọc", BiKhoa: "Bị khóa",
};

const formatDate = (d) => d ? new Date(d).toLocaleDateString("vi-VN") : "—";
const formatVND = (n) => n ? Number(n).toLocaleString("vi-VN") + "đ" : "0đ";

const loadStores = async () => {
  try {
    const params = {};
    if (filterStatus.value) params.trangThai = filterStatus.value;
    if (searchText.value) params.search = searchText.value;
    const res = await axios.get("/api/SuperAdmin/stores", { params });
    stores.value = res.data;
  } catch (e) { console.error(e); }
};

const viewDetail = async (id) => {
  try {
    const res = await axios.get(`/api/SuperAdmin/stores/${id}`);
    detail.value = res.data;
    showDetail.value = true;
  } catch (e) { console.error(e); }
};

const toggleStatus = async (id) => {
  try {
    await axios.put(`/api/SuperAdmin/stores/${id}/toggle-status`);
    swal.fire({ toast: true, position: "top-end", icon: "success", title: "Đã cập nhật!", timer: 1500, showConfirmButton: false });
    loadStores();
  } catch (e) { swal.fire("Lỗi", "Không thể cập nhật", "error"); }
};

const openExtend = (store) => {
  extendStore.value = store;
  extendMonths.value = 12;
  showExtend.value = true;
};

const doExtend = async () => {
  try {
    const res = await axios.post(`/api/SuperAdmin/stores/${extendStore.value.id}/extend`, { soThang: extendMonths.value });
    swal.fire("Thành công!", res.data.message, "success");
    showExtend.value = false;
    loadStores();
  } catch (e) { swal.fire("Lỗi", "Gia hạn thất bại", "error"); }
};

onMounted(loadStores);
</script>

<style scoped>
.sa-select { background: #1a1c23; color: #e4e4e7; border: 1px solid rgba(255,255,255,0.1); border-radius: 8px; padding: 8px 14px; font-size: 0.85rem; }
.sa-input { background: #1a1c23; color: #e4e4e7; border: 1px solid rgba(255,255,255,0.1); border-radius: 8px; padding: 8px 14px; font-size: 0.85rem; min-width: 200px; }
.sa-input::placeholder { color: #6b7280; }
.sa-table-wrap { overflow-x: auto; }
.sa-table { width: 100%; border-collapse: separate; border-spacing: 0; }
.sa-table th { font-size: 0.75rem; text-transform: uppercase; color: #6b7280; padding: 10px 14px; border-bottom: 1px solid rgba(255,255,255,0.06); letter-spacing: 0.5px; font-weight: 600; }
.sa-table td { padding: 12px 14px; border-bottom: 1px solid rgba(255,255,255,0.04); font-size: 0.88rem; vertical-align: middle; }
.sa-table tbody tr:hover { background: rgba(245, 158, 11, 0.04); }
.sa-table-sm td, .sa-table-sm th { padding: 8px 10px; font-size: 0.8rem; }
.sa-goi-badge { background: rgba(245,158,11,0.15); color: #f59e0b; padding: 3px 10px; border-radius: 6px; font-size: 0.75rem; font-weight: 700; }
.sa-status-badge { padding: 3px 10px; border-radius: 6px; font-size: 0.75rem; font-weight: 700; }
.status-HoatDong { background: rgba(34,197,94,0.15); color: #22c55e; }
.status-DungThu { background: rgba(59,130,246,0.15); color: #3b82f6; }
.status-ChiDoc { background: rgba(245,158,11,0.15); color: #f59e0b; }
.status-BiKhoa { background: rgba(239,68,68,0.15); color: #ef4444; }
.status-DaThanhToan { background: rgba(34,197,94,0.15); color: #22c55e; }
.status-ChoThanhToan { background: rgba(245,158,11,0.15); color: #f59e0b; }
.sa-btn-sm { width: 32px; height: 32px; border: none; border-radius: 8px; display: flex; align-items: center; justify-content: center; cursor: pointer; transition: 0.2s; font-size: 0.85rem; }
.sa-btn-info { background: rgba(59,130,246,0.15); color: #3b82f6; }
.sa-btn-info:hover { background: rgba(59,130,246,0.3); }
.sa-btn-danger { background: rgba(239,68,68,0.15); color: #ef4444; }
.sa-btn-danger:hover { background: rgba(239,68,68,0.3); }
.sa-btn-success { background: rgba(34,197,94,0.15); color: #22c55e; }
.sa-btn-success:hover { background: rgba(34,197,94,0.3); }
.sa-btn-warning { background: rgba(245,158,11,0.15); color: #f59e0b; }
.sa-btn-warning:hover { background: rgba(245,158,11,0.3); }

/* Modals */
.sa-modal-overlay { position: fixed; top: 0; left: 0; right: 0; bottom: 0; background: rgba(0,0,0,0.6); backdrop-filter: blur(4px); z-index: 1000; display: flex; align-items: center; justify-content: center; }
.sa-modal { background: #1a1c23; border: 1px solid rgba(255,255,255,0.08); border-radius: 16px; width: 90%; max-width: 700px; max-height: 80vh; overflow-y: auto; }
.sa-modal-header { display: flex; justify-content: space-between; align-items: center; padding: 18px 24px; border-bottom: 1px solid rgba(255,255,255,0.06); }
.sa-modal-header h5 { margin: 0; font-weight: 700; color: #f4f4f5; font-size: 1rem; }
.sa-modal-close { background: none; border: none; color: #6b7280; font-size: 1.5rem; cursor: pointer; }
.sa-modal-body { padding: 20px 24px; }
.sa-modal-body label { font-size: 0.75rem; color: #6b7280; font-weight: 600; text-transform: uppercase; letter-spacing: 0.3px; }
.sa-modal-body p { color: #e4e4e7; font-weight: 500; margin-bottom: 0; }
.kpi-mini { font-size: 1.3rem; font-weight: 800; color: #f4f4f5; }
</style>
