<template>
  <div class="cashbook-container d-flex bg-white h-100 flex-column">

    <!-- ═══ SUMMARY BAR ═══ -->
    <div class="summary-bar border-bottom px-3 py-2 d-flex gap-3 bg-white">
      <div class="sum-card">
        <div class="sum-label">{{ startDate ? 'Số dư đầu kỳ' : 'Tồn quỹ trước kỳ' }}</div>
        <div class="sum-value text-secondary">{{ formatPrice(summary.dauKy) }}</div>
      </div>
      <div class="sum-divider"></div>
      <div class="sum-card">
        <div class="sum-label">Tổng thu</div>
        <div class="sum-value text-primary">+ {{ formatPrice(summary.tongThu) }}</div>
      </div>
      <div class="sum-divider"></div>
      <div class="sum-card">
        <div class="sum-label">Tổng chi</div>
        <div class="sum-value text-danger">- {{ formatPrice(summary.tongChi) }}</div>
      </div>
      <div class="sum-divider"></div>
      <div class="sum-card sum-card-main">
        <div class="sum-label">Tồn quỹ cuối kỳ</div>
        <div class="sum-value fw-bolder" :class="summary.tonQuy >= 0 ? 'text-success' : 'text-danger'">
          {{ formatPrice(summary.tonQuy) }}
        </div>
      </div>
    </div>

    <!-- ═══ MAIN ═══ -->
    <div class="d-flex flex-grow-1 overflow-hidden">

      <!-- ─── SIDEBAR ─── -->
      <div class="sidebar border-end bg-light d-flex flex-column" style="width:230px;min-width:230px;overflow-y:auto">

        <!-- Khoảng thời gian -->
        <div class="sb-section">
          <div class="sb-title">Khoảng thời gian</div>
          <div class="d-flex flex-wrap gap-1 mb-2">
            <button @click="setDateRange('all')"   :class="activePeriod==='all'   ? 'btn-secondary' : 'btn-outline-secondary'" class="btn btn-xs">Tất cả</button>
            <button @click="setDateRange('today')" :class="activePeriod==='today' ? 'btn-secondary' : 'btn-outline-secondary'" class="btn btn-xs">Hôm nay</button>
            <button @click="setDateRange('week')"  :class="activePeriod==='week'  ? 'btn-secondary' : 'btn-outline-secondary'" class="btn btn-xs">Tuần này</button>
            <button @click="setDateRange('month')" :class="activePeriod==='month' ? 'btn-secondary' : 'btn-outline-secondary'" class="btn btn-xs">Tháng này</button>
          </div>
          <div class="mb-1">
            <label class="sb-label">Từ ngày</label>
            <input v-model="startDate" @change="fetchTransactions" type="date" class="form-control form-control-sm shadow-none" />
          </div>
          <div>
            <label class="sb-label">Đến ngày</label>
            <input v-model="endDate" @change="fetchTransactions" type="date" class="form-control form-control-sm shadow-none" />
          </div>
        </div>

        <!-- Tìm kiếm -->
        <div class="sb-section">
          <div class="sb-title">Tìm kiếm</div>
          <input v-model="searchMa"     type="text" class="form-control form-control-sm mb-2 shadow-none" placeholder="Mã chứng từ" />
          <input v-model="searchDoiTac" type="text" class="form-control form-control-sm shadow-none"       placeholder="Người nộp / nhận" />
        </div>

        <!-- Loại phiếu -->
        <div class="sb-section">
          <div class="sb-title">Loại phiếu</div>
          <div class="d-flex flex-column gap-1">
            <button @click="filterLoaiPhieu=''"    :class="filterLoaiPhieu===''    ? 'btn-secondary active' : 'btn-outline-secondary'" class="btn btn-sm text-start">Tất cả</button>
            <button @click="filterLoaiPhieu='Thu'" :class="filterLoaiPhieu==='Thu' ? 'btn-primary'          : 'btn-outline-secondary'" class="btn btn-sm text-start">Phiếu Thu</button>
            <button @click="filterLoaiPhieu='Chi'" :class="filterLoaiPhieu==='Chi' ? 'btn-danger'           : 'btn-outline-secondary'" class="btn btn-sm text-start">Phiếu Chi</button>
          </div>
        </div>

        <!-- Phương thức -->
        <div class="sb-section">
          <div class="sb-title">Phương thức</div>
          <div class="d-flex flex-column gap-1">
            <button @click="filterTaiKhoan=''"              :class="filterTaiKhoan===''             ? 'btn-secondary' : 'btn-outline-secondary'" class="btn btn-sm text-start">Tất cả</button>
            <button @click="filterTaiKhoan='Tiền mặt'"     :class="filterTaiKhoan==='Tiền mặt'    ? 'btn-secondary' : 'btn-outline-secondary'" class="btn btn-sm text-start">Tiền mặt</button>
            <button @click="filterTaiKhoan='Chuyển khoản'" :class="filterTaiKhoan==='Chuyển khoản'? 'btn-secondary' : 'btn-outline-secondary'" class="btn btn-sm text-start">Chuyển khoản</button>
          </div>
        </div>

      </div>

      <!-- ─── CONTENT ─── -->
      <div class="flex-grow-1 d-flex flex-column overflow-hidden">

        <!-- Toolbar -->
        <div class="toolbar border-bottom px-3 py-2 d-flex justify-content-between align-items-center bg-white">
          <div class="text-muted" style="font-size:0.83rem">
            <span v-if="!startDate && !endDate">Toàn bộ lịch sử</span>
            <span v-else>{{ startDate || '...' }} → {{ endDate || '...' }}</span>
            &nbsp;—&nbsp;<strong>{{ filteredTransactions.length }}</strong> giao dịch
          </div>
          <div class="d-flex gap-2">
            <button @click="openThuModal" class="btn btn-sm btn-primary px-3">
              <i class="bi bi-plus me-1"></i>Phiếu Thu
            </button>
            <button @click="openChiModal" class="btn btn-sm btn-danger px-3">
              <i class="bi bi-dash me-1"></i>Phiếu Chi
            </button>
          </div>
        </div>

        <!-- Thông báo lỗi -->
        <div v-if="errorMsg" class="alert alert-danger m-3 py-2 small mb-0">
          <i class="bi bi-exclamation-triangle me-1"></i>{{ errorMsg }}
        </div>

        <!-- Table -->
        <div class="flex-grow-1 overflow-auto p-3">
          <table class="table table-hover table-bordered align-middle mb-0" style="font-size:0.84rem">
            <thead class="table-light">
              <tr>
                <th style="width:32px"></th>
                <th style="width:145px">Mã chứng từ</th>
                <th>Người nộp/nhận</th>
                <th style="width:190px">Hạng mục</th>
                <th>Diễn giải</th>
                <th style="width:120px" class="text-center">Ngày GD</th>
                <th style="width:135px" class="text-end">Giá trị (₫)</th>
              </tr>
            </thead>
            <tbody>
              <tr v-if="loading">
                <td colspan="7" class="text-center py-4 text-muted">
                  <span class="spinner-border spinner-border-sm me-2"></span>Đang tải dữ liệu...
                </td>
              </tr>
              <tr v-else-if="filteredTransactions.length === 0 && !errorMsg">
                <td colspan="7" class="text-center py-5 text-muted">
                  <div class="mb-1">Không có giao dịch nào</div>
                  <div class="small" v-if="startDate || endDate">
                    Thử chọn "<strong>Tất cả</strong>" để xem toàn bộ lịch sử.
                  </div>
                </td>
              </tr>

              <template v-else v-for="t in filteredTransactions" :key="t.id">
                <!-- ROW CHÍNH -->
                <tr @click="toggleDetail(t.id)" style="cursor:pointer">
                  <td class="text-center text-muted">
                    <i class="bi" :class="expandedRowId===t.id ? 'bi-chevron-down' : 'bi-chevron-right'"></i>
                  </td>
                  <td>
                    <div class="fw-semibold" style="font-size:0.82rem">{{ t.maChungTu }}</div>
                    <span class="badge mt-1" style="font-size:0.68rem"
                      :class="t.loaiPhieu==='Thu' ? 'bg-primary bg-opacity-10 text-primary' : 'bg-danger bg-opacity-10 text-danger'">
                      {{ t.loaiPhieu==='Thu' ? 'Phiếu Thu' : 'Phiếu Chi' }}
                    </span>
                  </td>
                  <td class="fw-semibold">{{ t.nguoiNopNhan || '—' }}</td>
                  <td class="text-muted" style="font-size:0.82rem">{{ t.hangMuc }}</td>
                  <td class="text-muted text-truncate" style="max-width:220px;font-size:0.82rem">{{ t.lyDo || '—' }}</td>
                  <td class="text-center" style="font-size:0.82rem">{{ formatDate(t.ngayGiaoDich) }}</td>
                  <td class="text-end fw-bold" :class="t.loaiPhieu==='Thu' ? 'text-primary' : 'text-danger'">
                    {{ t.loaiPhieu==='Thu' ? '+' : '-' }}{{ formatPrice(t.giaTri) }}
                  </td>
                </tr>

                <!-- ROW CHI TIẾT -->
                <tr v-if="expandedRowId === t.id">
                  <td colspan="7" class="p-0">
                    <div class="detail-panel border-start border-4 p-3 bg-light"
                      :class="t.loaiPhieu==='Thu' ? 'border-primary' : 'border-danger'">
                      <div class="row g-0">
                        <div class="col-md-5">
                          <table class="detail-table">
                            <tr><td>Mã chứng từ</td><td class="fw-semibold">{{ t.maChungTu }}</td></tr>
                            <tr><td>Loại phiếu</td>
                              <td><span class="badge" :class="t.loaiPhieu==='Thu' ? 'bg-primary' : 'bg-danger'">Phiếu {{ t.loaiPhieu }}</span></td>
                            </tr>
                            <tr><td>Hạng mục</td><td>{{ t.hangMuc }}</td></tr>
                            <tr><td>Người nộp/nhận</td><td>{{ t.nguoiNopNhan || '—' }}</td></tr>
                          </table>
                        </div>
                        <div class="col-md-4">
                          <table class="detail-table">
                            <tr><td>Phương thức</td><td>{{ t.phuongThuc }}</td></tr>
                            <tr><td>Ngày giao dịch</td><td>{{ formatDateFull(t.ngayGiaoDich) }}</td></tr>
                            <tr><td>Người tạo</td><td>{{ t.nguoiTao }}</td></tr>
                          </table>
                        </div>
                        <div class="col-md-3 d-flex flex-column align-items-end justify-content-between ps-2">
                          <div class="text-end">
                            <div class="text-muted small mb-1">Giá trị</div>
                            <div class="fs-5 fw-bold" :class="t.loaiPhieu==='Thu' ? 'text-primary' : 'text-danger'">
                              {{ t.loaiPhieu==='Thu' ? '+' : '-' }}{{ formatPrice(t.giaTri) }} ₫
                            </div>
                          </div>
                          <button class="btn btn-sm btn-outline-secondary mt-2">
                            <i class="bi bi-printer me-1"></i>In phiếu
                          </button>
                        </div>
                        <div class="col-12 mt-2 border-top pt-2" v-if="t.lyDo">
                          <span class="text-muted small me-1">Diễn giải:</span>
                          <span class="small">{{ t.lyDo }}</span>
                        </div>
                      </div>
                    </div>
                  </td>
                </tr>
              </template>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch, inject } from "vue";
import axios from "axios";
import { globalState } from "../store";

const swal = inject("$swal");

// ─── State ───────────────────────────────────────────────────────
const transactions    = ref([]);
const summary         = ref({ dauKy: 0, tongThu: 0, tongChi: 0, tonQuy: 0 });
const loading         = ref(false);
const errorMsg        = ref("");
const expandedRowId   = ref(null);

// Filters
const searchMa        = ref("");
const searchDoiTac    = ref("");
const filterLoaiPhieu = ref("");
const filterTaiKhoan  = ref("");

// Date range
const startDate       = ref("");
const endDate         = ref("");
const activePeriod    = ref("all");

// ─── Date quick-select ───────────────────────────────────────────
const fmt = (d) => d.toISOString().split("T")[0];

const setDateRange = (range) => {
  activePeriod.value = range;
  const now = new Date();
  if (range === "all") {
    startDate.value = "";
    endDate.value   = "";
  } else if (range === "today") {
    startDate.value = fmt(now);
    endDate.value   = fmt(now);
  } else if (range === "week") {
    const day = now.getDay() || 7;
    const mon = new Date(now);
    mon.setDate(now.getDate() - day + 1);
    startDate.value = fmt(mon);
    endDate.value   = fmt(now);
  } else if (range === "month") {
    startDate.value = `${now.getFullYear()}-${String(now.getMonth()+1).padStart(2,"0")}-01`;
    endDate.value   = fmt(now);
  }
  fetchTransactions();
};

// ─── Client-side filter ──────────────────────────────────────────
const filteredTransactions = computed(() =>
  transactions.value.filter((t) => {
    if (searchMa.value     && !t.maChungTu?.toLowerCase().includes(searchMa.value.toLowerCase()))     return false;
    if (searchDoiTac.value && !t.nguoiNopNhan?.toLowerCase().includes(searchDoiTac.value.toLowerCase())) return false;
    if (filterLoaiPhieu.value && t.loaiPhieu  !== filterLoaiPhieu.value) return false;
    if (filterTaiKhoan.value  && t.phuongThuc !== filterTaiKhoan.value)  return false;
    return true;
  })
);

// ─── Formatters ──────────────────────────────────────────────────
const formatPrice    = (v) => new Intl.NumberFormat("vi-VN").format(v || 0);
const formatDate     = (s) => {
  if (!s) return "—";
  const d = new Date(s);
  return `${String(d.getDate()).padStart(2,"0")}/${String(d.getMonth()+1).padStart(2,"0")}/${d.getFullYear()}`;
};
const formatDateFull = (s) => {
  if (!s) return "—";
  const d = new Date(s);
  return `${String(d.getDate()).padStart(2,"0")}/${String(d.getMonth()+1).padStart(2,"0")}/${d.getFullYear()} ${String(d.getHours()).padStart(2,"0")}:${String(d.getMinutes()).padStart(2,"0")}`;
};
const todayStr = () => new Date().toISOString().split("T")[0];

// ─── API ─────────────────────────────────────────────────────────
const fetchTransactions = async () => {
  loading.value  = true;
  errorMsg.value = "";
  try {
    // Luôn dùng chiNhanhId=0 để lấy TẤT CẢ giao dịch của cửa hàng
    // (không lọc theo chi nhánh vì Sổ Quỹ là tổng quan toàn store)
    let url = `/api/ThuChi/danh-sach?chiNhanhId=0`;
    if (startDate.value) url += `&startDate=${startDate.value}`;
    if (endDate.value)   url += `&endDate=${endDate.value}`;

    const res = await axios.get(url);
    transactions.value = res.data.danhSach ?? [];
    summary.value      = res.data.thongKe  ?? { dauKy: 0, tongThu: 0, tongChi: 0, tonQuy: 0 };
  } catch (e) {
    console.error("Lỗi tải sổ quỹ:", e);
    if (e.response?.status === 403) {
      errorMsg.value = "Bạn không có quyền xem Sổ Quỹ. Chỉ Chủ cửa hàng hoặc Admin mới được phép.";
    } else if (e.response?.status === 401) {
      errorMsg.value = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.";
    } else {
      errorMsg.value = "Không thể tải dữ liệu. Vui lòng thử lại (F5) hoặc kiểm tra server.";
    }
    transactions.value = [];
  } finally {
    loading.value = false;
  }
};

const submitPhieu = async (payload) => {
  try {
    await axios.post("/api/ThuChi", payload);
    await swal.fire({ icon: "success", title: "Lưu thành công!", timer: 1200, showConfirmButton: false });
    fetchTransactions();
  } catch (e) {
    const msg = typeof e.response?.data === "string" ? e.response.data : "Không thể tạo phiếu.";
    swal.fire("Lỗi", msg, "error");
  }
};

const toggleDetail = (id) => {
  expandedRowId.value = expandedRowId.value === id ? null : id;
};

// ─── Modal template ───────────────────────────────────────────────
const formHtml = (type) => {
  const isThu = type === "Thu";
  const hangMucOptions = isThu
    ? `<option value="Thu tiền bán hàng">Thu tiền bán hàng</option>
       <option value="Thu nợ khách hàng">Thu nợ khách hàng</option>
       <option value="Thu hoa hồng">Thu hoa hồng</option>
       <option value="Thu vốn góp">Thu vốn góp / Chủ bỏ tiền vào</option>
       <option value="Thu khác" selected>Thu khác</option>`
    : `<option value="Chi trả tiền nhập hàng">Chi trả tiền nhập hàng</option>
       <option value="Chi lương nhân viên">Chi lương nhân viên</option>
       <option value="Chi thuê mặt bằng">Chi thuê mặt bằng</option>
       <option value="Chi điện / nước">Chi điện / nước</option>
       <option value="Chi vận hành">Chi vận hành khác</option>
       <option value="Chi quảng cáo">Chi quảng cáo / marketing</option>
       <option value="Chi khác" selected>Chi khác</option>`;

  return `
    <div class="text-start" style="font-size:0.9rem">
      <div class="mb-2">
        <label class="form-label fw-semibold mb-1">Hạng mục ${isThu ? "thu" : "chi"} <span class="text-danger">*</span></label>
        <select id="f-hangmuc" class="form-select form-select-sm">${hangMucOptions}</select>
      </div>
      <div class="mb-2">
        <label class="form-label fw-semibold mb-1">${isThu ? "Người nộp" : "Người nhận"} <span class="text-danger">*</span></label>
        <input id="f-nguoi" class="form-control form-control-sm" placeholder="Nhập tên người ${isThu ? "nộp" : "nhận"} tiền">
      </div>
      <div class="row g-2 mb-2">
        <div class="col-7">
          <label class="form-label fw-semibold mb-1">Số tiền (VNĐ) <span class="text-danger">*</span></label>
          <input id="f-giatri" type="number" class="form-control form-control-sm" placeholder="0" min="1">
        </div>
        <div class="col-5">
          <label class="form-label fw-semibold mb-1">Phương thức</label>
          <select id="f-phuongthuc" class="form-select form-select-sm">
            <option value="Tiền mặt">Tiền mặt</option>
            <option value="Chuyển khoản">Chuyển khoản</option>
          </select>
        </div>
      </div>
      <div class="mb-2">
        <label class="form-label fw-semibold mb-1">Ngày giao dịch</label>
        <input id="f-ngay" type="date" class="form-control form-control-sm">
      </div>
      <div>
        <label class="form-label fw-semibold mb-1">Diễn giải / Ghi chú</label>
        <textarea id="f-lydo" class="form-control form-control-sm" rows="2" placeholder="${isThu ? "Ví dụ: Thu hộ tiền dịch vụ tháng 4..." : "Ví dụ: Thanh toán lương tháng 4..."}"></textarea>
      </div>
    </div>`;
};

// ─── Phiếu Thu ───────────────────────────────────────────────────
const openThuModal = async () => {
  const { value: form } = await swal.fire({
    title: "Tạo Phiếu Thu",
    width: 500,
    html: formHtml("Thu"),
    showCancelButton: true,
    cancelButtonText: "Hủy",
    confirmButtonText: "Xác nhận",
    confirmButtonColor: "#0d6efd",
    didOpen: () => { document.getElementById("f-ngay").value = todayStr(); },
    preConfirm: () => {
      const hangMuc      = document.getElementById("f-hangmuc").value;
      const nguoiNopNhan = document.getElementById("f-nguoi").value.trim();
      const giatri       = parseFloat(document.getElementById("f-giatri").value);
      const phuongThuc   = document.getElementById("f-phuongthuc").value;
      const ngay         = document.getElementById("f-ngay").value;
      const lyDo         = document.getElementById("f-lydo").value.trim();
      if (!nguoiNopNhan)          { swal.showValidationMessage("Vui lòng nhập tên người nộp"); return false; }
      if (!giatri || giatri <= 0) { swal.showValidationMessage("Số tiền phải lớn hơn 0");      return false; }
      return { loaiPhieu: "Thu", hangMuc, nguoiNopNhan, giaTri: giatri, phuongThuc, ngayGiaoDich: ngay || todayStr(), lyDo, chiNhanhId: 0 };
    },
  });
  if (form) await submitPhieu(form);
};

// ─── Phiếu Chi ───────────────────────────────────────────────────
const openChiModal = async () => {
  const { value: form } = await swal.fire({
    title: "Tạo Phiếu Chi",
    width: 500,
    html: formHtml("Chi"),
    showCancelButton: true,
    cancelButtonText: "Hủy",
    confirmButtonText: "Xác nhận",
    confirmButtonColor: "#dc3545",
    didOpen: () => { document.getElementById("f-ngay").value = todayStr(); },
    preConfirm: () => {
      const hangMuc      = document.getElementById("f-hangmuc").value;
      const nguoiNopNhan = document.getElementById("f-nguoi").value.trim();
      const giatri       = parseFloat(document.getElementById("f-giatri").value);
      const phuongThuc   = document.getElementById("f-phuongthuc").value;
      const ngay         = document.getElementById("f-ngay").value;
      const lyDo         = document.getElementById("f-lydo").value.trim();
      if (!nguoiNopNhan)          { swal.showValidationMessage("Vui lòng nhập tên người nhận"); return false; }
      if (!giatri || giatri <= 0) { swal.showValidationMessage("Số tiền phải lớn hơn 0");       return false; }
      return { loaiPhieu: "Chi", hangMuc, nguoiNopNhan, giaTri: giatri, phuongThuc, ngayGiaoDich: ngay || todayStr(), lyDo, chiNhanhId: 0 };
    },
  });
  if (form) await submitPhieu(form);
};

// ─── Lifecycle ────────────────────────────────────────────────────
watch(() => globalState.value.activeBranchId, fetchTransactions);
onMounted(fetchTransactions);
</script>

<style scoped>
/* Summary bar */
.summary-bar   { flex-shrink: 0; }
.sum-card      { display: flex; flex-direction: column; padding: 6px 20px; }
.sum-card-main { background: #f8f9fa; border-radius: 4px; }
.sum-label     { font-size: 0.71rem; color: #6c757d; font-weight: 600; text-transform: uppercase; letter-spacing: 0.04em; margin-bottom: 1px; }
.sum-value     { font-size: 1.1rem; font-weight: 700; }
.sum-divider   { width: 1px; background: #dee2e6; margin: 4px 4px; flex-shrink: 0; }

/* Sidebar */
.sidebar       { flex-shrink: 0; }
.sb-section    { padding: 10px 12px; border-bottom: 1px solid #e9ecef; }
.sb-title      { font-size: 0.71rem; font-weight: 700; text-transform: uppercase; letter-spacing: 0.05em; color: #495057; margin-bottom: 7px; }
.sb-label      { font-size: 0.78rem; color: #6c757d; display: block; margin-bottom: 2px; }
.btn-xs        { font-size: 0.74rem; padding: 2px 7px; border-radius: 3px; }

/* Toolbar */
.toolbar { flex-shrink: 0; }

/* Detail panel */
.detail-panel {
  animation: slideDown 0.12s ease;
}
@keyframes slideDown {
  from { opacity: 0; transform: translateY(-3px); }
  to   { opacity: 1; transform: translateY(0); }
}
.detail-table              { width: 100%; font-size: 0.82rem; border-collapse: collapse; }
.detail-table td           { padding: 3px 8px 3px 0; vertical-align: top; }
.detail-table td:first-child { color: #6c757d; font-weight: 600; white-space: nowrap; width: 44%; }
</style>
