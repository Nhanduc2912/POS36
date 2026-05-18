<template>
  <div>
    <div class="d-flex justify-content-between align-items-center mb-4 flex-wrap gap-2">
      <h6 class="fw-bold mb-0" style="color:var(--sa-text-faint);font-size:.75rem;text-transform:uppercase;letter-spacing:.5px">
        <i class="bi bi-shop me-2"></i>DANH SÁCH CỬA HÀNG
      </h6>
      <div class="d-flex gap-2 flex-wrap align-items-center">
        <select v-model="filterStatus" class="sa-select" @change="loadStores">
          <option value="">Tất cả trạng thái</option>
          <option value="HoatDong">Hoạt động</option>
          <option value="DungThu">Dùng thử</option>
          <option value="ChiDoc">Chỉ đọc</option>
          <option value="BiKhoa">Bị khóa</option>
        </select>
        <input v-model="searchText" class="sa-input" placeholder="🔍 Tìm cửa hàng..." @input="loadStores" style="min-width:200px" />
        <span style="color:var(--sa-text-faint);font-size:.82rem">
          Tổng: <strong style="color:var(--sa-accent)">{{ stores.length }}</strong> cửa hàng
        </span>
      </div>
    </div>

    <!-- Table -->
    <div class="sa-table-wrap">
      <table class="sa-table">
        <thead>
          <tr>
            <th>ID</th><th>Tên cửa hàng</th><th>SĐT</th><th>Email</th>
            <th>Gói</th><th>Trạng thái</th><th>Hết hạn</th><th>Còn lại</th><th>Thao tác</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="s in stores" :key="s.id">
            <td style="color:var(--sa-text-faint)">#{{ s.id }}</td>
            <td class="fw-bold" style="color:var(--sa-text)">{{ s.tenCuaHang }}</td>
            <td style="color:var(--sa-text-muted)">{{ s.soDienThoai }}</td>
            <td style="color:var(--sa-text-faint);font-size:.8rem">{{ s.email || '—' }}</td>
            <td><span class="sa-goi-badge">{{ s.goiDichVu || 'Trial' }}</span></td>
            <td><span class="sa-status-badge" :class="'status-' + s.trangThai">{{ statusMap[s.trangThai] || s.trangThai }}</span></td>
            <td style="color:var(--sa-text-muted);font-size:.82rem">{{ formatDate(s.ngayHetHan) }}</td>
            <td>
              <span :class="s.soNgayConLai <= 7 ? 'text-danger fw-bold' : 'text-success'" style="font-size:.85rem">
                {{ s.soNgayConLai }} ngày
              </span>
            </td>
            <td>
              <div class="d-flex gap-1">
                <button class="sa-btn-sm sa-btn-info" @click="viewDetail(s.id)" title="Chi tiết"><i class="bi bi-eye"></i></button>
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
            <td colspan="9" class="text-center py-5" style="color:var(--sa-text-faint)">
              <i class="bi bi-shop display-4 opacity-25 d-block mb-2"></i>Không tìm thấy cửa hàng nào
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Modal: Chi tiết -->
    <div class="sa-modal-overlay" v-if="showDetail" @click.self="showDetail = false">
      <div class="sa-modal">
        <div class="sa-modal-header">
          <h5><i class="bi bi-shop me-2"></i>{{ detail.store?.tenCuaHang }}</h5>
          <button class="sa-modal-close" @click="showDetail = false">&times;</button>
        </div>
        <div class="sa-modal-body" v-if="detail.store">
          <div class="row g-3 mb-4">
            <div class="col-6"><label>SĐT</label><p>{{ detail.store.soDienThoai }}</p></div>
            <div class="col-6"><label>Email</label><p>{{ detail.store.email || '—' }}</p></div>
            <div class="col-6"><label>Trạng thái</label>
              <p><span class="sa-status-badge" :class="'status-' + detail.store.trangThai">{{ statusMap[detail.store.trangThai] }}</span></p>
            </div>
            <div class="col-6"><label>Gói</label><p><span class="sa-goi-badge">{{ detail.store.goiDichVu || 'Trial' }}</span></p></div>
            <div class="col-6"><label>Ngày đăng ký</label><p>{{ formatDate(detail.store.ngayDangKy) }}</p></div>
            <div class="col-6"><label>Hết hạn</label><p>{{ formatDate(detail.store.ngayHetHan) }}</p></div>
          </div>
          <div class="row g-2 mb-4">
            <div class="col-3 text-center">
              <div class="kpi-mini">{{ detail.thongKe?.soNhanVien }}</div>
              <small style="color:var(--sa-text-faint)">Nhân viên</small>
            </div>
            <div class="col-3 text-center">
              <div class="kpi-mini">{{ detail.thongKe?.soSanPham }}</div>
              <small style="color:var(--sa-text-faint)">Sản phẩm</small>
            </div>
            <div class="col-3 text-center">
              <div class="kpi-mini">{{ detail.thongKe?.soHoaDon }}</div>
              <small style="color:var(--sa-text-faint)">Hóa đơn</small>
            </div>
            <div class="col-3 text-center">
              <div class="kpi-mini" style="color:#22c55e">{{ formatVND(detail.thongKe?.doanhThu) }}</div>
              <small style="color:var(--sa-text-faint)">Doanh thu</small>
            </div>
          </div>
          <h6 style="color:var(--sa-text-faint);font-size:.72rem;text-transform:uppercase;letter-spacing:.5px" class="mb-2">
            <i class="bi bi-clock-history me-1"></i>Lịch sử gói
          </h6>
          <div class="sa-table-wrap">
            <table class="sa-table sa-table-sm">
              <thead><tr><th>Gói</th><th>Số tiền</th><th>Trạng thái</th><th>Ngày</th></tr></thead>
              <tbody>
                <tr v-for="l in detail.lichSuGoi" :key="l.id">
                  <td>{{ l.tenGoi }}</td>
                  <td>{{ formatVND(l.soTienThanhToan) }}</td>
                  <td><span class="sa-status-badge" :class="'status-' + l.trangThai">{{ l.trangThai }}</span></td>
                  <td style="font-size:.78rem;color:var(--sa-text-faint)">{{ formatDate(l.ngayTao) }}</td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>

    <!-- Modal: Gia hạn -->
    <div class="sa-modal-overlay" v-if="showExtend" @click.self="showExtend = false">
      <div class="sa-modal" style="max-width:420px">
        <div class="sa-modal-header">
          <h5><i class="bi bi-plus-circle me-2"></i>Gia hạn: {{ extendStore?.tenCuaHang }}</h5>
          <button class="sa-modal-close" @click="showExtend = false">&times;</button>
        </div>
        <div class="sa-modal-body">
          <div class="mb-3">
            <label>Gói dịch vụ</label>
            <select v-model="extendGoiDichVu" class="sa-select w-100">
              <option value="">Giữ nguyên</option>
              <option value="starter">Starter</option>
              <option value="pro">Pro</option>
              <option value="ultimate">Ultimate</option>
            </select>
          </div>
          <div class="mb-4">
            <label>Số tháng gia hạn</label>
            <select v-model.number="extendMonths" class="sa-select w-100">
              <option :value="1">1 tháng</option>
              <option :value="3">3 tháng</option>
              <option :value="6">6 tháng</option>
              <option :value="12">12 tháng</option>
              <option :value="24">24 tháng</option>
            </select>
          </div>
          <button style="background:var(--sa-accent);border:none;color:#fff;padding:12px;border-radius:8px;font-weight:700;width:100%;cursor:pointer" @click="doExtend">
            <i class="bi bi-check-lg me-1"></i>Xác nhận gia hạn
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
const extendGoiDichVu = ref("");

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
  const s = stores.value.find(x => x.id === id);
  const action = s?.trangThai === "BiKhoa" ? "mở khóa" : "khóa";
  const confirm = await swal.fire({
    title: `Xác nhận ${action}?`,
    text: `${action.charAt(0).toUpperCase() + action.slice(1)} cửa hàng "${s?.tenCuaHang}"?`,
    icon: "question", showCancelButton: true,
    confirmButtonText: "Xác nhận", cancelButtonText: "Hủy",
  });
  if (!confirm.isConfirmed) return;
  try {
    await axios.put(`/api/SuperAdmin/stores/${id}/toggle-status`);
    swal.fire({ toast: true, position: "top-end", icon: "success", title: "Đã cập nhật!", timer: 1500, showConfirmButton: false });
    loadStores();
  } catch (e) { swal.fire("Lỗi", "Không thể cập nhật", "error"); }
};

const openExtend = (store) => {
  extendStore.value = store;
  extendMonths.value = 12;
  extendGoiDichVu.value = "";
  showExtend.value = true;
};

const doExtend = async () => {
  try {
    const res = await axios.post(`/api/SuperAdmin/stores/${extendStore.value.id}/extend`, {
      soThang: extendMonths.value,
      goiDichVu: extendGoiDichVu.value || undefined,
    });
    swal.fire("Thành công!", res.data.message, "success");
    showExtend.value = false;
    loadStores();
  } catch (e) { swal.fire("Lỗi", "Gia hạn thất bại", "error"); }
};

onMounted(loadStores);
</script>

<style scoped>
@import "./sa-shared.css";
</style>
